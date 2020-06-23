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
		int framebuffer, renderbuffer, quadVertexbuffer, quadTexbuffer;
        int[] colorbuffer, hdrFramebuffer, hdrColorbuffer;

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
            cameraRotation = Matrix4.CreateRotationX((float)(0.5 * Math.PI));
            cameraFOV = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI * 0.5), screen.width / screen.height, 0.01f, 1000);

            scene = new SceneGraph();

			framebuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

			renderbuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screen.width, screen.height);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderbuffer);

            colorbuffer = new int[2];
            GL.GenTextures(2, colorbuffer);
            for(int i = 0; i < 2; ++i)
            {
                GL.BindTexture(TextureTarget.Texture2D, colorbuffer[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, screen.width, screen.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)All.Nearest });
                GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)All.Nearest });
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, colorbuffer[i], 0);

            }
            DrawBuffersEnum[] attachments = new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 };
            GL.DrawBuffers(2, attachments);


            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			{
				Console.WriteLine("Framebuffer not set up correctly");
			}


            hdrFramebuffer = new int[2];
            hdrColorbuffer = new int[2];

            GL.GenFramebuffers(2, hdrFramebuffer);
            GL.GenTextures(2, hdrColorbuffer);
			for(int i = 0; i < 2; ++i)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, hdrFramebuffer[i]);
                GL.BindTexture(TextureTarget.Texture2D, hdrColorbuffer[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, programValues.screenwidth, programValues.screenheight, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, hdrColorbuffer[i], 0);
            }
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
        MouseState prevmouse, mouse;
		// tick for background surface
		public void Tick()
		{
            // measure frame duration

            KeyboardState keys = Keyboard.GetState();
            prevmouse = mouse;
            mouse = Mouse.GetState();

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

            if(mouse.ScrollWheelValue != prevmouse.ScrollWheelValue)
            {
                camera.Y += mouse.ScrollWheelValue - prevmouse.ScrollWheelValue;
            }

            //if (keys.IsKeyDown(Key.Down))
            //{
            //    cameraRotation *= Matrix4.CreateRotationX((float)timer.Elapsed.TotalSeconds);
            //}
            //else if (keys.IsKeyDown(Key.Up))
            //{
            //    cameraRotation *= Matrix4.CreateRotationX(-(float)timer.Elapsed.TotalSeconds);
            //}
            //else if (keys.IsKeyDown(Key.Right))
            //{
            //    cameraRotation *= Matrix4.CreateRotationY((float)timer.Elapsed.TotalSeconds);
            //}
            //else if (keys.IsKeyDown(Key.Left))
            //{
            //    cameraRotation *= Matrix4.CreateRotationY(-(float)timer.Elapsed.TotalSeconds);
            //}

            scene.Update(timer);
            timer.Restart();
        }

		// tick for OpenGL rendering code
		public void RenderGL()
		{

            // update rotation
            scene.FullRender(cameraPosition, cameraRotation, cameraFOV, shader, framebuffer);

            //Post processing
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.Enable(EnableCap.Texture2D);
            GL.UseProgram(postproc.programID);
            int horizontal = 1;
            bool first = true;
            for (int i = 0; i < 10; ++i)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, hdrFramebuffer[horizontal]);
                GL.Uniform1(GL.GetUniformLocation(postproc.programID, "horizontal"), horizontal);
                GL.BindTexture(TextureTarget.Texture2D, first ? colorbuffer[1] : hdrColorbuffer[horizontal == 0 ? 1 : 0]);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                horizontal = horizontal == 0 ? 1 : 0;
                if (first) first = false;
            }
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
            //GL.BindTexture(TextureTarget.Texture2D, colorbuffer[0]);

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


            GL.UseProgram(rendershader.programID);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, colorbuffer[0]);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, hdrColorbuffer[0]);
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