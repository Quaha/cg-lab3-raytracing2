using OpenTK.Graphics.OpenGL4;

namespace RayTracing {
    class Shader {

        int shader_handle;

        // Загрузка, компиляция и линковка шейдеров программы
        public void loadShader() {
            shader_handle = GL.CreateProgram();

            int vertex_shader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex_shader, loadShaderSource(Settings.SHADER_VERT_PATH));
            GL.CompileShader(vertex_shader);
            checkShaderCompile(vertex_shader);

            int fragment_shader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment_shader, loadShaderSource(Settings.SHADER_FRAG_PATH));
            GL.CompileShader(fragment_shader);
            checkShaderCompile(fragment_shader);

            // Линковка шейдерной программы
            GL.AttachShader(shader_handle, vertex_shader);
            GL.AttachShader(shader_handle, fragment_shader);

            GL.LinkProgram(shader_handle);

            checkProgramLink();

            GL.DeleteShader(vertex_shader);
            GL.DeleteShader(fragment_shader);
        }

        // Проверка корректности линковки
        public void checkProgramLink() {
            GL.GetProgram(shader_handle, GetProgramParameterName.LinkStatus, out int link_status);
            if (link_status == 0) {
                string info = GL.GetProgramInfoLog(shader_handle);
                Console.WriteLine($"Error linking program: {info}");

                throw new Exception("Program linking failed!");
            }
        }

        // Проверка корректности компиляции
        public void checkShaderCompile(int shader) {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compile_status);
            if (compile_status == 0) {
                string info = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"Error compiling shader: {info}");

                throw new Exception("Shader compilation failed!");
            }
        }

        // Получение исходного кода шейдера
        public static string loadShaderSource(string filepath) {
            string shader_source = "";
            try {
                using (StreamReader reader = new StreamReader(filepath)) {
                    shader_source = reader.ReadToEnd();
                }
            }
            catch (Exception e) {
                Console.WriteLine("Failed to load shader source file: " + e.Message);

                throw new Exception("Shader loading failed");
            }
            return shader_source;
        }

        // Передача информации в шейдеры
        public void setShaderData(int width, int height, Camera camera) {
            GL.Uniform1(GL.GetUniformLocation(shader_handle, "WINDOW_WIDTH"), width);
            GL.Uniform1(GL.GetUniformLocation(shader_handle, "WINDOW_HEIGHT"), height);

            GL.Uniform1(GL.GetUniformLocation(shader_handle, "ambient"), Settings.AMBIENT);

            GL.Uniform3(GL.GetUniformLocation(shader_handle, "camera_POS"), camera.getPosition());
            GL.Uniform3(GL.GetUniformLocation(shader_handle, "camera_VIEW"), camera.getFront());
            GL.Uniform3(GL.GetUniformLocation(shader_handle, "camera_UP"), camera.getUp());
            GL.Uniform3(GL.GetUniformLocation(shader_handle, "camera_RIGHT"), camera.getRight());

            GL.Uniform1(GL.GetUniformLocation(shader_handle, "camera_FOV"), camera.getFOV());
        }

        public void useShader() {
            GL.UseProgram(shader_handle);
        }

        public void delete() {
            GL.DeleteProgram(shader_handle);
        }

        public int getHandle() {
            return shader_handle;
        }
    }
}