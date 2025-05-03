using System.Runtime.InteropServices;
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

        public void setData<T>(T[] data, BufferUsageHint hint) where T : struct {
            Bind();

            GL.BufferData(
                buffer_target,
                (IntPtr)(data.Length * Marshal.SizeOf(typeof(T))),
                data,
                hint
            );
        }

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