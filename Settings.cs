using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using RayTracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RayTracing {
    static class Settings {
        public static int WINDOW_WIDTH  = 1920;
        public static int WINDOW_HEIGHT = 1080;

        public static Vector3 BASE_CAMERA_POSITION = new Vector3(0.0f, 0.0f, -5.0f);

        public static float AMBIENT = 0.25f;

        public static string WINDOW_NAME = "RayTracing";

        public static string SHADER_FRAG_PATH = "../../../Shaders/shader.frag";
        public static string SHADER_VERT_PATH = "../../../Shaders/shader.vert";
    };
}
