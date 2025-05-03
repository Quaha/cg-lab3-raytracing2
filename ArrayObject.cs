using OpenTK.Graphics.OpenGL4;

namespace RayTracing {
    public class ArrayObject {

        public int vao_handle;
        private bool is_binded = false;

        public ArrayObject() {
            vao_handle = GL.GenVertexArray();
        }

        // Привязка к контексту
        public void Bind() {
            if (!is_binded) {
                is_binded = true;
                GL.BindVertexArray(vao_handle);
            }
        }

        public void unBind() {
            if (is_binded) {
                GL.BindVertexArray(0);
                is_binded = false;
            }
        }

        public void delete() {
            unBind();
            GL.DeleteVertexArray(vao_handle);
        }

        // Привязка атрибутов вершин
        public void setVertexAttribPointer(int index,
                                           int size,
                                           VertexAttribPointerType type,
                                           bool normalize,
                                           int stride,
                                           int offset) {
            GL.VertexAttribPointer(index, size, type, normalize, stride, offset);
            GL.EnableVertexAttribArray(index);
        }
    }
}