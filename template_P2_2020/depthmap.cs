using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
namespace Template
{
    public class depthmap
    {
        public int framebuffer, id, type;
        public Matrix4 camera;
        public Matrix4 position;
        public depthmap(DirectionalLight light)
        {
            framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, programValues.depthmapres, programValues.depthmapres, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureWrapMode.ClampToEdge);


            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, id, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                Console.WriteLine("depthbuffer not set up correctly");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            position = Matrix4.CreateTranslation(light.direction.Xyz * -200);
            camera = position * Matrix4.LookAt(Vector3.Zero, -light.direction.Xyz, Vector3.UnitY) * Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI * 0.5), 1, 0.01f, 1000);
        }
        public depthmap(Spotlight light)
        {
            framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, programValues.depthmapres, programValues.depthmapres, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureWrapMode.Repeat);


            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, id, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                Console.WriteLine("depthbuffer not set up correctly");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            position = Matrix4.CreateTranslation(-light.position.Xyz);
            camera = Matrix4.LookAt(light.position.Xyz, light.direction.Xyz, Vector3.UnitY) * Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI * 0.5), 1, 0.01f, 1000);

            programValues.shadowmap = id;
        }
        public void Render(Shader shader, SceneGraph scene)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, id);


            GL.Viewport(0, 0, programValues.depthmapres, programValues.depthmapres);
            GL.Enable(EnableCap.DepthTest);
            

            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.CullFace(CullFaceMode.Front);
            scene.SimpleRender(camera, position, shader);
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.DepthTest);
            GL.Viewport(0, 0, programValues.screenwidth, programValues.screenheight);
        }
    }
}
