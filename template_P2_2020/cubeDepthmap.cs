using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Template
{
    public class cubeDepthmap
    {
        public int framebuffer, renderbuffer, id;
        Matrix4[] cameraRotations = new Matrix4[]
        {
            Matrix4.CreateRotationY((float) Math.PI * 0.5f) * Matrix4.CreateRotationZ((float) Math.PI),
            Matrix4.CreateRotationY(-(float) Math.PI * 0.5f) * Matrix4.CreateRotationZ((float) Math.PI),
            Matrix4.CreateRotationX(-(float) Math.PI * 0.5f),
            Matrix4.CreateRotationX((float) Math.PI * 0.5f),
            Matrix4.CreateRotationY((float) Math.PI) * Matrix4.CreateRotationZ((float) Math.PI),
            Matrix4.CreateRotationZ((float) Math.PI)
        };
        Matrix4 cameraFOV = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI * 0.5), 1, 0.01f, 500);
        public cubeDepthmap()
        {
            framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            int renderbuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, programValues.cubedepthmapres, programValues.cubedepthmapres);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderbuffer);

            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, id);
            for(int i = 0; i < 6; ++i)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.DepthComponent, programValues.cubedepthmapres, programValues.cubedepthmapres, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            }
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                Console.WriteLine("cubedepthbuffer not set up correctly");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            programValues.cubemap = id;
        }
        public void Render(Shader shader, Vector3 transform, SceneGraph scene)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, id);
            Matrix4 cameraPosition = Matrix4.CreateTranslation(-transform);
            GL.Viewport(0, 0, programValues.cubedepthmapres, programValues.cubedepthmapres);
            GL.Enable(EnableCap.DepthTest);
            GL.CullFace(CullFaceMode.Front);
            for(int i = 0; i < 6; ++i)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.TextureCubeMapPositiveX + i, id, 0);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                scene.SimpleRender(cameraPosition * cameraRotations[i] * cameraFOV, cameraPosition, programValues.depthcubemapshader);
            }
            GL.CullFace(CullFaceMode.Back);

            GL.Viewport(0, 0, programValues.screenwidth, programValues.screenheight);
        }
    }
}
