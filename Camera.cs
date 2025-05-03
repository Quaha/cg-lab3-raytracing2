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

namespace Game {
    public class Camera {

        public Vector3 position = Settings.BASE_CAMERA_POSITION;

        public Vector3 up_direction = Vector3.UnitY;
        public Vector3 front_direction = -Vector3.UnitZ;
        public Vector3 right_direction = Vector3.UnitX;

        float SPEED = 10f;
        float SENSITIVITY = 3f;

        float pitch = 0.0f; // degrees
        float yaw = 0.0f; // degrees

        Vector2 last_mouse_position = new Vector2(0, 0);

        int SCREENWIDTH, SCREENHEIGHT;

        public Camera(int SCREENWIDTH, int SCREENHEIGHT) {
            this.SCREENWIDTH = SCREENWIDTH;
            this.SCREENHEIGHT = SCREENHEIGHT;
        }

        public Matrix4 getViewMatrix() {
            return Matrix4.LookAt(position, position + front_direction, up_direction);
        }

        public Matrix4 getProjection() {
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f),
                SCREENWIDTH / SCREENHEIGHT,
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

        // Получение матрицы вида (переводит все координаты в координаты камеры)
        public Matrix4 GetViewMatrix() {
            return Matrix4.LookAt(position, position + front_direction, up_direction);
        }

        // Получение матрицы проекции (отвечает за перспективу)
        public Matrix4 GetProjectionMatrix() {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), (float)SCREENWIDTH / SCREENHEIGHT, 0.1f, 100f);
        }

        // Получение матрицы модели (перемещение)
        public Matrix4 GetModelMatrix(Vector3 translation) {
            return Matrix4.CreateTranslation(translation);
        }

        public void UpdateProjection(int width, int height) {
            SCREENWIDTH = width;
            SCREENHEIGHT = height;
        }
    }
}