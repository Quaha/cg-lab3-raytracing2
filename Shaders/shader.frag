#version 460 core

#define EPS 0.001
#define INF 1000000.0

uniform int WINDOW_WIDTH;
uniform int WINDOW_HEIGHT;

uniform vec3 camera_POS;
uniform vec3 camera_VIEW;
uniform vec3 camera_UP;
uniform vec3 camera_RIGHT;

uniform float camera_FOV;

uniform float ambient;

const int MAX_DEPTH = 5;

const int DIFFUSE = 1;
const int REFLECTION = 2;
const int REFRACTION = 3;

out vec4 frag_color;
in vec3 frag_position;

struct Sphere {
	vec3 center;
	float radius;
	int material_id;
};

struct Triangle {
	vec3 v1, v2, v3;
	int material_id;
};

struct Camera {
	vec3 position;
	vec3 view;
	vec3 up;
	vec3 right;
	vec2 scale;
};

struct Ray {
	vec3 start;
	vec3 direction;
};

struct Light {
	vec3 position;
};

struct Material {
	// Diffuse color
	vec3 color;

	// Ambient, diffuse and specular coeffs
	vec4 light_coeffs;

	// 0.0 - non-reflection, 1.0 - mirror
	float reflection_coef;
	float refraction_coef;
	float refractive_index;

	int material_type;
};

struct Intersection {
	float intersect_dist;
	vec3 point;
	vec3 normal;
	vec3 color;

	// ambient, diffuse and specular coeffs
	vec4 light_coeffs;

	// 0.0 - non-reflection, 1.0 - mirror
	float reflection_coef;
	float refraction_coef;
	float refractive_index;

	int material_type;
};

struct TracingRay {
	Ray ray;
	float contribution;
	int depth;
};

// --- Ray Stack ---

TracingRay stack[10];
int stack_size = 0;

bool isEmpty() {
	return (stack_size == 0);
}

void pushRay(TracingRay ray) {
	stack[stack_size] = ray;
	stack_size = stack_size + 1;
}

TracingRay popRay() {
	stack_size = stack_size - 1;
	return stack[stack_size];
}

// --- End of Stack ---

const int COUNT_OF_TRIANGLES = 12;
const Triangle triangles[COUNT_OF_TRIANGLES] = Triangle[COUNT_OF_TRIANGLES](
    // front wall
    Triangle(vec3(-5.0,-5.0, -5.0), vec3( 5.0,-5.0, -5.0), vec3(-5.0, 5.0, -5.0), 4),
    Triangle(vec3( 5.0, 5.0, -5.0), vec3(-5.0, 5.0, -5.0), vec3( 5.0,-5.0, -5.0), 4),

    // right wall
    Triangle(vec3(5.0, 5.0, 5.0), vec3(5.0, 5.0, -5.0), vec3(5.0, -5.0, 5.0), 3),
    Triangle(vec3(5.0, -5.0, -5.0), vec3(5.0, 5.0, -5.0), vec3(5.0, -5.0, 5.0), 3),

    // back wall
    Triangle(vec3(-5.0,-5.0, 5.0), vec3( 5.0,-5.0, 5.0), vec3(-5.0, 5.0, 5.0), 4),
    Triangle(vec3( 5.0, 5.0, 5.0), vec3(-5.0, 5.0, 5.0), vec3( 5.0,-5.0, 5.0), 4),

    // left wall
    Triangle(vec3(-5.0,-5.0,-5.0), vec3(-5.0, 5.0, 5.0), vec3(-5.0, 5.0,-5.0), 4),
    Triangle(vec3(-5.0,-5.0,-5.0), vec3(-5.0,-5.0, 5.0), vec3(-5.0, 5.0, 5.0), 4),

    // top wall
    Triangle(vec3(-5.0,5.0,-5.0), vec3(5.0, 5.0, -5.0), vec3(-5.0, 5.0,5.0), 4),
    Triangle(vec3(5.0,5.0,5.0), vec3(5.0, 5.0, -5.0), vec3(-5.0, 5.0,5.0), 4),

    // bottom wall
    Triangle(vec3(-5.0,-5.0,-5.0), vec3(5.0, -5.0, -5.0), vec3(-5.0, -5.0,5.0), 4),
    Triangle(vec3(5.0,-5.0,5.0), vec3(5.0,-5.0, -5.0), vec3(-5.0, -5.0,5.0), 4)
);

const int COUNT_OF_SPHERES = 3;
Sphere spheres[COUNT_OF_SPHERES] = Sphere[](
    Sphere(vec3(-1.0, -1.0, -2.0), 2.0, 0),
    Sphere(vec3( 2.0,  1.0,  2.0), 1.0, 1),
    Sphere(vec3( 3.0,  0.0,  0.0), 1.3, 2)
);

const int COUNT_OF_MATERIALS = 5;
const Material materials[COUNT_OF_MATERIALS] = Material[COUNT_OF_MATERIALS](
    Material(vec3(0.9, 0.1, 0.1), vec4(0.0, 0.9, 0.3, 512.0), 0.0, 0.0, 1.66, DIFFUSE),
    Material(vec3(0.1, 0.9, 0.1), vec4(0.0, 0.9, 0.6, 350.0), 0.0, 0.9, 1.66, REFRACTION),
    Material(vec3(0.1, 0.1, 0.9), vec4(0.2, 0.9, 0.1, 5.0),   0.0, 0.0, 1.66, DIFFUSE),
    Material(vec3(0.1, 0.5, 0.4), vec4(0.2, 0.9, 0.6, 250.0), 0.6, 0.0, 1.66, REFLECTION),
    Material(vec3(0.1, 0.5, 0.4), vec4(0.2, 0.9, 0.1, 5.0),   0.0, 0.0, 1.66, DIFFUSE)
);

Light light = Light(vec3(1.0, 2.0, -4.0));
Camera camera;

Ray generateRay(Camera camera) {
	vec2 coords = frag_position.xy * camera.scale;
	vec3 direction = camera.view + camera.right * coords.x + camera.up * coords.y;
	return Ray(camera.position, normalize(direction));
}

bool intersectSphere(Sphere sphere, Ray ray, float start, float final, out float intersect_dist ) {
	/* These equations could be derived from this system:
    *   
    *   / (v - center, v - center) = radius ^ 2
    *  <| 
    *   \ b = start + direction * t, t in R
    */   

    vec3 L = ray.start - sphere.center;

    float a = 1.0; // because direction is normilized
    float b = 2.0 * dot(L, ray.direction);
    float c = dot(L, L) - sphere.radius * sphere.radius;

    float D2 = b * b - 4.0 * c;

    if (D2 < 0) {
        return false;
    }

    float D = sqrt(D2);

    float t1 = (-b - D) / 2.0;
    float t2 = (-b + D) / 2.0;

    if (start <= t1 && t1 <= final) {
        intersect_dist = t1;
        return true;
    }

    if (start <= t2 && t2 <= final) {
        intersect_dist = t2;
        return true;
    }

    return false;
}


bool intersectTriangle(Ray ray, vec3 v1, vec3 v2, vec3 v3, out float intersect_dist) {

    // Thomas Moller and Ben Trumbore algorithm
    vec3 e1 = v2 - v1;
    vec3 e2 = v3 - v1;

    vec3 pvec = cross(ray.direction, e2);
    float det = dot(e1, pvec);

    if (-EPS < det && det < EPS) {
        return false;
    }
	
    float inv_det = 1.0 / det;
    vec3 tvec = ray.start - v1;

    float u = dot(tvec, pvec) * inv_det;
    if (u < 0.0 || u > 1.0) {
        return false;
    }

    vec3 qvec = cross(tvec, e1);
    float v = dot(ray.direction, qvec) * inv_det;
    if (v < 0.0 || u + v > 1.0) {
        return false;
    }

    intersect_dist = dot(e2, qvec) * inv_det;
	return (intersect_dist > 0.0);
}


bool raytrace(Ray ray, Sphere spheres[COUNT_OF_SPHERES], Triangle triangles[COUNT_OF_TRIANGLES], Material materials[COUNT_OF_MATERIALS], float start, float final, inout Intersection intersect) {

	bool result = false;
	float nearest_dist = start;

	intersect.intersect_dist = final;

	for (int i = 0; i < COUNT_OF_SPHERES; i++) {

		Sphere sphere = spheres[i];
		if (intersectSphere(sphere, ray, start, final, nearest_dist) && start < nearest_dist && nearest_dist < intersect.intersect_dist) {

			intersect.intersect_dist = nearest_dist;
			intersect.point = ray.start + ray.direction * nearest_dist;
			intersect.normal = normalize(intersect.point - spheres[i].center);
			if (dot(ray.direction, intersect.normal) > 0.0) {
				intersect.normal = -intersect.normal;
			}

			intersect.color = materials[sphere.material_id].color;
			intersect.light_coeffs = materials[sphere.material_id].light_coeffs;
			intersect.reflection_coef = materials[sphere.material_id].reflection_coef;
			intersect.refraction_coef = materials[sphere.material_id].refraction_coef;
			intersect.refractive_index = materials[sphere.material_id].refractive_index;
			intersect.material_type = materials[sphere.material_id].material_type;

			result = true;
		}
	}

	for (int i = 0; i < COUNT_OF_TRIANGLES; i++) {

		Triangle triangle = triangles[i];

		if (intersectTriangle(ray, triangle.v1, triangle.v2, triangle.v3, nearest_dist) && start < nearest_dist && nearest_dist < intersect.intersect_dist) {
			intersect.intersect_dist = nearest_dist;
			intersect.point = ray.start + ray.direction * nearest_dist;
			intersect.normal = normalize(cross(triangle.v1 - triangle.v2, triangle.v3 - triangle.v2));
			if (dot(ray.direction, intersect.normal) > 0.0) {
				intersect.normal = -intersect.normal;
			}

			intersect.color = materials[triangle.material_id].color;
			intersect.light_coeffs = materials[triangle.material_id].light_coeffs;
			intersect.reflection_coef = materials[triangle.material_id].reflection_coef;
			intersect.refraction_coef = materials[triangle.material_id].refraction_coef;
			intersect.refractive_index = materials[triangle.material_id].refractive_index;
			intersect.material_type = materials[triangle.material_id].material_type;

			result = true;
		}
	}

	return result;
}

vec3 getPhongPart(Intersection intersect, Light curr_light, float shadow) {
	vec3 light = normalize(curr_light.position - intersect.point);
	vec3 view = normalize(camera.position - intersect.point);

	float diffuse = max(dot(light, intersect.normal), 0.0);

	vec3 reflected = reflect(-view, intersect.normal);
	float specular = pow(max(dot(reflected, light), 0.0), intersect.light_coeffs.w);

	return intersect.light_coeffs.x * intersect.color + intersect.light_coeffs.y * diffuse * intersect.color * shadow + intersect.light_coeffs.z * specular;
}

float processShadow(Light light, Intersection hit) {
    vec3 light_dir = normalize(light.position - hit.point);
    
    float light_distance = distance(light.position, hit.point);

    Ray shadow_ray = Ray(hit.point + light_dir * EPS, light_dir);
    
    Intersection shadow_hit;
    shadow_hit.intersect_dist = INF;

    bool in_shadow = raytrace(shadow_ray, spheres, triangles, materials, 0.0, light_distance, shadow_hit);


    return in_shadow ? ambient : 1.0;
}

void main() {

	float start = 0.0;
	float final = INF;

	camera = Camera(camera_POS, camera_VIEW, camera_UP, camera_RIGHT, vec2(1.0));

	float aspect = float(WINDOW_WIDTH) / float(WINDOW_HEIGHT);
	float fov_rad = radians(camera_FOV);
	float scale = tan(fov_rad / 2.0);

	camera.scale = vec2(scale * aspect, scale);

	Ray ray = generateRay(camera);

	Intersection intersect;
	intersect.intersect_dist = INF;

	vec3 result_color = vec3(0.0, 0.0, 0.0);	
	if (raytrace(ray, spheres, triangles, materials, start, final, intersect)) {
		result_color = vec3(0.1, 0.1, 0.1);
	}
	
	TracingRay tracing_ray = TracingRay(ray, 1, 0);
	pushRay(tracing_ray);

	while(!isEmpty()) {
		TracingRay tracing_ray = popRay();
		ray = tracing_ray.ray;

		if (tracing_ray.depth >= MAX_DEPTH) {
			continue;
		}

		Intersection intersect;
		intersect.intersect_dist = INF;

		start = 0.0;
		final = INF;

		if (raytrace(ray, spheres, triangles, materials, start, final, intersect)) {
			if (intersect.material_type == DIFFUSE) {
				float shadowing = processShadow(light, intersect);
				result_color += tracing_ray.contribution * getPhongPart(intersect, light, shadowing);
			}
			else if (intersect.material_type == REFLECTION){
				
				float contribution;

				contribution = tracing_ray.contribution * (1 - intersect.reflection_coef);
				float shadowing = processShadow(light, intersect);
				result_color += contribution * getPhongPart(intersect, light, shadowing);

				vec3 reflect_direction = reflect(ray.direction, intersect.normal);
					
				contribution = tracing_ray.contribution * intersect.reflection_coef;
				TracingRay reflectRay = TracingRay(Ray(intersect.point + reflect_direction * EPS, reflect_direction), contribution, tracing_ray.depth + 1);
				pushRay(reflectRay);
			}
			else if (intersect.material_type == REFRACTION) {
				float contribution;

				contribution = tracing_ray.contribution * (1.0 - intersect.refraction_coef);
				float shadowing = processShadow(light, intersect);
				result_color += contribution * getPhongPart(intersect, light, shadowing);

				float eta = 1.0 / intersect.refractive_index;
				vec3 refract_direction = refract(ray.direction, intersect.normal, eta);

				if (length(refract_direction) > EPS) {
					contribution = tracing_ray.contribution * intersect.refraction_coef;
					TracingRay refractRay = TracingRay(
						Ray(intersect.point + refract_direction * EPS, refract_direction),
						contribution,
						tracing_ray.depth + 1
					);
					pushRay(refractRay);
				}
			}
		} 
	} 
	frag_color = vec4(result_color, 1.0);
}