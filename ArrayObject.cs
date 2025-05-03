using OpenTK.Graphics.OpenGL4;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RayTracing {
    public class ArrayObject {

        public int vao_handle;
        private bool is_binded = false;

        public ArrayObject() {
            vao_handle = GL.GenVertexArray();
        }

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

        // Приклеплеие буфферов к VAO
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