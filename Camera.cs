using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using RayTracing;

namespace RayTracing {
    public class Camera {

        Vector3 position = Settings.BASE_CAMERA_POSITION;
            
        Vector3 front_direction = -Vector3.UnitZ;
        Vector3 up_direction = Vector3.UnitY;
        Vector3 right_direction = Vector3.UnitX;

        float FOV = Settings.BASE_CAMERA_FOV;

        float SPEED = Settings.BASE_CAMERA_MOVEMENT_SPEED;
        float SENSITIVITY = Settings.BASE_CAMERA_ROTATION_SENSITIVITY;

        float pitch = 0.0f; // degrees
        float yaw = 0.0f; // degrees

        Vector2 last_mouse_position = new Vector2(0, 0);

        int WINDOW_WIDTH, WINDOW_HEIGHT;

        public Camera(int WINDOW_WIDTH, int WINDOW_HEIGHT) {
            this.WINDOW_WIDTH = WINDOW_WIDTH;
            this.WINDOW_HEIGHT = WINDOW_HEIGHT;
        }

        public Matrix4 getViewMatrix() {
            return Matrix4.LookAt(position, position + front_direction, up_direction);
        }

        public Matrix4 getProjection() {
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                WINDOW_WIDTH / WINDOW_HEIGHT,
                0.1f,
                100f
            );
        }

        private void updateDirectionVectors() {
            front_direction.X =
                MathF.Cos(MathHelper.DegreesToRadians(pitch)) *
                MathF.Cos(MathHelper.DegreesToRadians(yaw)
            );

            front_direction.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));

            front_direction.Z =
                MathF.Cos(MathHelper.DegreesToRadians(pitch)) *
                MathF.Sin(MathHelper.DegreesToRadians(yaw)
            );

            front_direction = Vector3.Normalize(front_direction);
            right_direction = Vector3.Normalize(Vector3.Cross(front_direction, Vector3.UnitY));
            up_direction = Vector3.Normalize(Vector3.Cross(right_direction, front_direction));
        }

        public void inputProcessing(KeyboardState input, MouseState mouse, FrameEventArgs e) {
            // Mouse
            var deltaX = mouse.X - last_mouse_position.X;
            var deltaY = mouse.Y - last_mouse_position.Y;

            last_mouse_position = new Vector2(mouse.X, mouse.Y);
            yaw += deltaX * SENSITIVITY * (float)e.Time;
            pitch -= deltaY * SENSITIVITY * (float)e.Time;

            if (pitch <= -89.0f) {
                pitch = -89.0f;
            }

            if (pitch >= 89.0f) {
                pitch = 89.0f;
            }

            // Keys
            if (input.IsKeyDown(Keys.W)) {
                position += front_direction * SPEED * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.A)) {
                position -= right_direction * SPEED * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.S)) {
                position -= front_direction * SPEED * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.D)) {
                position += right_direction * SPEED * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.LeftShift)) {
                position -= up_direction * SPEED * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.Space)) {
                position += up_direction * SPEED * (float)e.Time;
            }

            updateDirectionVectors();
        }

        public void update(KeyboardState input, MouseState mouse, FrameEventArgs e) {
            inputProcessing(input, mouse, e);
        }

        public Vector3 getPosition() {
            return position;
        }

        public Vector3 getFront() {
            return front_direction;
        }

        public Vector3 getUp() {
            return up_direction;
        }

        public Vector3 getRight() {
            return right_direction;
        }

        public float getFOV() {
            return FOV;
        }
    }
}