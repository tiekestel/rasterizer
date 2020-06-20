using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
namespace Template
{
    public class cubemap
    {
        public int framebuffer, renderbuffer, id, type;

        Matrix4[] cameraRotations = new Matrix4[]
        {
            Matrix4.CreateRotationY((float) Math.PI * 0.5f) * Matrix4.CreateRotationZ((float) Math.PI),
            Matrix4.CreateRotationY(-(float) Math.PI * 0.5f) * Matrix4.CreateRotationZ((float) Math.PI),
            Matrix4.CreateRotationX(-(float) Math.PI * 0.5f),
            Matrix4.CreateRotationX((float) Math.PI * 0.5f),
            Matrix4.CreateRotationY((float) Math.PI) * Matrix4.CreateRotationZ((float) Math.PI),
            Matrix4.CreateRotationZ((float) Math.PI)
        };
        Matrix4 cameraFOV = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI * 0.5), 1, 0.01f, 1000);
        public cubemap(int _type)
        {
            type = _type;
            framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);


            renderbuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, programValues.cubemapres, programValues.cubemapres);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderbuffer);

            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, id);
            for (int i = 0; i < 6; ++i)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, programValues.cubemapres, programValues.cubemapres, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            }

            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, new int[] { (int)All.Linear });
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, new int[] { (int)All.Linear });
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, new int[] { (int)All.ClampToEdge });
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, new int[] { (int)All.ClampToEdge });
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, new int[] { (int)All.ClampToEdge });
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                Console.WriteLine("CubemapBuffer not set up correctly");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        public void Render(Shader shader, Vector3 transform, SceneGraph scene, ParentMesh parent)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, id);
            Matrix4 cameraPosition = Matrix4.CreateTranslation(-transform);
            GL.Viewport(0, 0, programValues.cubemapres, programValues.cubemapres);
            GL.Enable(EnableCap.DepthTest);
            for (int i = 0; i < 6; ++i)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i, id, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.ClearColor(0, 0, 0, 0);
                scene.RenderSkyBox(cameraPosition, cameraRotations[i], cameraFOV, programValues.skyboxshader);    
                scene.SimpleRender(cameraPosition * cameraRotations[i] * cameraFOV, cameraPosition, programValues.cubemapshader, parent);
            }
            GL.Disable(EnableCap.DepthTest);
            GL.Viewport(0, 0, programValues.screenwidth, programValues.screenheight);
        }
    }
}
