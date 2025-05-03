using OpenTK.Mathematics;

namespace RayTracing {
    static class Settings {
        public static int WINDOW_WIDTH  = 1920;
        public static int WINDOW_HEIGHT = 1080;

        public static float BASE_CAMERA_FOV = 90.0f; // deg
        public static Vector3 BASE_CAMERA_POSITION = new Vector3(0.0f, 0.0f, -4.5f);

        public static float BASE_CAMERA_MOVEMENT_SPEED = 10.0f;
        public static float BASE_CAMERA_ROTATION_SENSITIVITY = 3.0f;

        public static float AMBIENT = 0.25f;

        public static string WINDOW_NAME = "RayTracing";

        public static string SHADER_FRAG_PATH = "../../../Shaders/shader.frag";
        public static string SHADER_VERT_PATH = "../../../Shaders/shader.vert";
    };
}
