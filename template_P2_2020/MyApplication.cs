using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using OpenTK.Input;
using System.Drawing;

namespace Template
{
	class MyApplication
	{
		// member variables
		public Surface screen;                  // background surface for printing etc.
		Stopwatch timer;                        // timer for measuring frame duration
		Shader shader, postproc, rendershader;                          // shader to use for rendering
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing
		SceneGraph scene;
		Matrix4 cameraPosition, cameraRotation, cameraFOV;
		int framebuffer, hdrFramebuffer, colorbuffer, hdrColorbuffer, renderbuffer, hdrRenderbuffer, quadVertexbuffer, quadTexbuffer;

		// initialize
		public void Init()
		{
            programValues.screenwidth = screen.width;
            programValues.screenheight = screen.height;
            
			timer = new Stopwatch();
			timer.Reset();
			timer.Start();
			// create shaders
			shader = new Shader( "../../shaders/vs.glsl", "../../shaders/fs.glsl" );
			postproc = new Shader( "../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl" );
			rendershader = new Shader( "../../shaders/vs_render.glsl", "../../shaders/fs_render.glsl" );
			//rendershader = new Shader( "../../shaders/vs_render.glsl", "../../shaders/fs_renderDepthmap.glsl" );

			programValues.skyboxshader = new Shader( "../../shaders/vs_skybox.glsl", "../../shaders/fs_skybox.glsl" );
            programValues.cubemapshader = new Shader("../../shaders/vs_cubemap.glsl", "../../shaders/fs_cubemap.glsl");
            programValues.depthmapshader = new Shader("../../shaders/vs_depthmap.glsl", "../../shaders/fs_depthmap.glsl");
            programValues.depthcubemapshader = new Shader("../../shaders/vs_depthmap.glsl", "../../shaders/fs_depthcubemap.glsl");
            programValues.hdrtargetshader = new Shader("../../shaders/vs_hdr.glsl", "../../shaders/fs_hdr.glsl");

			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			
            cameraPosition = new Matrix4();
            cameraRotation = Matrix4.Identity;
            cameraFOV = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI * 0.5), screen.width / screen.height, 0.01f, 1000);

            scene = new SceneGraph();

			framebuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			colorbuffer = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, colorbuffer);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, screen.width, screen.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)All.Nearest });
			GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)All.Nearest });
			renderbuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screen.width, screen.height);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderbuffer);
			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, colorbuffer, 0);
			GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
			if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			{
				Console.WriteLine("Framebuffer not set up correctly");
			}

			hdrFramebuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, hdrFramebuffer);
			hdrColorbuffer = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, hdrColorbuffer);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screen.width, screen.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)All.Nearest });
			GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)All.Nearest });
			hdrRenderbuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, hdrRenderbuffer);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screen.width, screen.height);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, hdrRenderbuffer);
			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, hdrColorbuffer, 0);
			GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			{
				Console.WriteLine("HDRFramebuffer not set up correctly");
			}

			float[] vertices = new float[] {
				-1f, -1f,
				1f, -1f,
				-1f, 1f,
				-1f, 1f,
				1f, -1f,
				1f, 1
			};
			float[] texCoords = new float[]
			{
				0, 0,
				1f, 0,
				0, 1f,
				0, 1f,
				1f, 0,
				1f, 1f
			};
			quadVertexbuffer = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, quadVertexbuffer);
			GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
			quadTexbuffer = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, quadTexbuffer);
			GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Length * sizeof(float), texCoords, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);




		}

        Vector3 camera = new Vector3(0, -14.5f, 0);
		// tick for background surface
		public void Tick()
		{
            // measure frame duration

            KeyboardState keys = Keyboard.GetState();

			if (keys.IsKeyDown(Key.A))
			{
                camera.X += (float)timer.Elapsed.TotalSeconds * 10;
			}
			else if (keys.IsKeyDown(Key.D))
			{
                camera.X -= (float)timer.Elapsed.TotalSeconds * 10;
            }
			if (keys.IsKeyDown(Key.W))
			{
                camera.Z += (float)timer.Elapsed.TotalSeconds * 10;
            }
			else if (keys.IsKeyDown(Key.S))
			{
                camera.Z -= (float)timer.Elapsed.TotalSeconds * 10;
            }
            cameraPosition = Matrix4.CreateTranslation(camera);

            if (keys.IsKeyDown(Key.Down))
            {
                cameraRotation *= Matrix4.CreateRotationX((float)timer.Elapsed.TotalSeconds);
            }
            else if (keys.IsKeyDown(Key.Up))
            {
                cameraRotation *= Matrix4.CreateRotationX(-(float)timer.Elapsed.TotalSeconds);
            }
            else if (keys.IsKeyDown(Key.Right))
            {
                cameraRotation *= Matrix4.CreateRotationY((float)timer.Elapsed.TotalSeconds);
            }
            else if (keys.IsKeyDown(Key.Left))
            {
                cameraRotation *= Matrix4.CreateRotationY(-(float)timer.Elapsed.TotalSeconds);
            }
            timer.Restart();
        }

		// tick for OpenGL rendering code
		public void RenderGL()
		{

            // update rotation
            scene.FullRender(cameraPosition, cameraRotation, cameraFOV, shader, framebuffer);
			
			//Post processing
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, hdrFramebuffer);
			GL.Clear(ClearBufferMask.DepthBufferBit);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.ClearColor(Color.Black);
			GL.EnableVertexAttribArray(0);
			GL.EnableVertexAttribArray(1);
			GL.Enable(EnableCap.Texture2D);
			GL.UseProgram(postproc.programID);
			GL.BindVertexArray(quadVertexbuffer);
			GL.BindVertexArray(quadTexbuffer);
			GL.Disable(EnableCap.DepthTest);
			GL.BindTexture(TextureTarget.Texture2D, colorbuffer);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			GL.UseProgram(0);
			GL.DisableVertexAttribArray(0);
			GL.DisableVertexAttribArray(1);
			GL.Disable(EnableCap.Texture2D);



            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //GL.Clear(ClearBufferMask.DepthBufferBit);
            //GL.Clear(ClearBufferMask.ColorBufferBit);
            //GL.ClearColor(Color.Black);
            //GL.EnableVertexAttribArray(0);
            //GL.EnableVertexAttribArray(1);
            //GL.Enable(EnableCap.Texture2D);
            //GL.ActiveTexture(TextureUnit.Texture1);
            //GL.BindTexture(TextureTarget.Texture2D, hdrColorbuffer);

            //GL.UseProgram(rendershader.programID);

            //GL.BindVertexArray(quadVertexbuffer);
            //GL.BindVertexArray(quadTexbuffer);
            //GL.Disable(EnableCap.DepthTest);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            //GL.UseProgram(0);
            //GL.DisableVertexAttribArray(0);
            //GL.DisableVertexAttribArray(1);
            //GL.Disable(EnableCap.Texture2D);


            //Render
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(Color.Black);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.Enable(EnableCap.Texture2D);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, colorbuffer);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, hdrColorbuffer);

            GL.UseProgram(rendershader.programID);

            GL.BindVertexArray(quadVertexbuffer);
            GL.BindVertexArray(quadTexbuffer);
            GL.Disable(EnableCap.DepthTest);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.UseProgram(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.Disable(EnableCap.Texture2D);
        }
    }
}