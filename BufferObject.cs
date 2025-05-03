using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace RayTracing {
    public enum BufferType {
        ArrayBuffer = BufferTarget.ArrayBuffer,
        ElementBuffer = BufferTarget.ElementArrayBuffer
    }

    public class BufferObject {

        public int buffer_handle;
        private BufferTarget buffer_target;
        private bool is_binded = false;

        public BufferObject(BufferType type) {
            buffer_target = (BufferTarget)type;
            buffer_handle = GL.GenBuffer();
        }

        // Передача данных в буффер
        public void setData<T>(T[] data, BufferUsageHint hint) where T : struct {
            Bind();

            GL.BufferData(
                buffer_target,
                (IntPtr)(data.Length * Marshal.SizeOf(typeof(T))),
                data,
                hint
            );
        }

        // Привязка к VAO
        public void Bind() {
            if (!is_binded) {
                is_binded = true;
                GL.BindBuffer(buffer_target, buffer_handle);
            }
        }

        public void unBind() {
            if (is_binded) {
                GL.BindBuffer(buffer_target, 0);
            }
        }

        public void delete() {
            unBind();
            GL.DeleteBuffer(buffer_handle);
        }
    }
}