using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Game;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RayTracing {
	public class Window : GameWindow {

		int width, height;

        FrameCounter fps_counter = new FrameCounter();

		Camera camera;

        public List<Vector3> vertices = new List<Vector3>() {
			new Vector3(-1f,1f,-1f),
			new Vector3(1f,1f,-1f),
			new Vector3(1f,-1f,-1f),
			new Vector3(-1f,-1f,-1f),
		};

		public uint[] indices = {
			0, 1, 2,
			2, 3, 0
		};

		ArrayObject VAO;
		BufferObject VBO, EBO;

		Shader shader;

        public Window(int width, int height): base(
			GameWindowSettings.Default,
            new NativeWindowSettings {
				ClientSize = new Vector2i(width, height),
				Title = Settings.WINDOW_NAME
            }
		) 
		{
            CenterWindow(new Vector2i(width, height));
            this.width = width;
            this.height = height;

			camera = new Camera(width, height);
        }

        protected override void OnResize(ResizeEventArgs e) {
			base.OnResize(e);
			GL.Viewport(0, 0, e.Width, e.Height);
			this.width = e.Width;
			this.height = e.Height;
		}

		protected override void OnLoad() {
			base.OnLoad();

            CursorState = CursorState.Grabbed;

            VAO = new ArrayObject();
            VBO = new BufferObject(BufferType.ArrayBuffer);
            EBO = new BufferObject(BufferType.ElementBuffer);

            shader = new Shader();

            VAO.Bind();

			VBO.setData(vertices.ToArray(), BufferUsageHint.StaticDraw);
			VAO.setVertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

			EBO.setData(indices, BufferUsageHint.StaticDraw);

            shader.loadShader();
        }

        protected override void OnUnload() {
            base.OnUnload();

			EBO.delete();
			VBO.delete();
			VAO.delete();

			shader.delete();
        }

        protected override void OnRenderFrame(FrameEventArgs args) {

            MouseState mouse = MouseState;
            KeyboardState input = KeyboardState;

            camera.update(input, mouse, args);

            // Обновление FPS
            fps_counter.updateCounter((float)args.Time);
            if (fps_counter.canGetFPS()) {
                Console.WriteLine($"FPS: {fps_counter.getFPS()}");
            }

            if (KeyboardState.IsKeyDown(Keys.Escape)) {
                Close();
            }

            GL.ClearColor(0.1f, 0.3f, 0.8f, 0.5f);
			GL.Clear(ClearBufferMask.ColorBufferBit);

			shader.useShader();
			shader.setShaderData(width, height, camera);

			VAO.Bind();
			VBO.Bind();
			EBO.Bind();

			GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

			Context.SwapBuffers();
			base.OnRenderFrame(args);
		}

		protected override void OnUpdateFrame(FrameEventArgs args) {
			base.OnUpdateFrame(args);
		}
	}
}



