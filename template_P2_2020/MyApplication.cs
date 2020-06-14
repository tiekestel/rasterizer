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
		Shader shader;                          // shader to use for rendering
		Shader postproc;                        // shader to use for post processing
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing
		SceneGraph scene;
		Matrix4 camera;
		int framebuffer, colorbuffer, renderbuffer, quadVertexbuffer, quadTexbuffer, skyboxbuffer;
		Shader skyboxshader;

		// initialize
		public void Init()
		{
			timer = new Stopwatch();
			timer.Reset();
			timer.Start();
			// create shaders
			shader = new Shader( "../../shaders/vs.glsl", "../../shaders/fs.glsl" );
			postproc = new Shader( "../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl" );
			skyboxshader = new Shader( "../../shaders/vs_skybox.glsl", "../../shaders/fs_skybox.glsl" );

			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			
            camera = new Matrix4();
			scene = new SceneGraph();
			programValues.scene = scene;

			framebuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			colorbuffer = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, colorbuffer);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screen.width, screen.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
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
			programValues.cubemapbuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			programValues.cubemapColorbuffer = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, programValues.cubemapColorbuffer);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 400, 400, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)All.Nearest });
			GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)All.Nearest });
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			renderbuffer = GL.GenRenderbuffer();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, 400, 400);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderbuffer);
			GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, programValues.cubemapColorbuffer, 0);
			GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			{
				Console.WriteLine("Cubemap framebuffer not set up correctly");
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

			float[] skyboxVertices = new float[] {
				// positions          
				-1.0f,  1.0f, -1.0f,
				-1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				 1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,

				-1.0f, -1.0f,  1.0f,
				-1.0f, -1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f, -1.0f,
				-1.0f,  1.0f,  1.0f,
				-1.0f, -1.0f,  1.0f,

				 1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,

				-1.0f, -1.0f,  1.0f,
				-1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f, -1.0f,  1.0f,
				-1.0f, -1.0f,  1.0f,

				-1.0f,  1.0f, -1.0f,
				 1.0f,  1.0f, -1.0f,
				 1.0f,  1.0f,  1.0f,
				 1.0f,  1.0f,  1.0f,
				-1.0f,  1.0f,  1.0f,
				-1.0f,  1.0f, -1.0f,

				-1.0f, -1.0f, -1.0f,
				-1.0f, -1.0f,  1.0f,
				 1.0f, -1.0f, -1.0f,
				 1.0f, -1.0f, -1.0f,
				-1.0f, -1.0f,  1.0f,
				 1.0f, -1.0f,  1.0f
			};

			skyboxbuffer = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, skyboxbuffer);
			GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.StaticDraw);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

			string[] faces = new string[]
			{
				"../../assets/bluecloud_rt.jpg",
				"../../assets/bluecloud_lf.jpg",
				"../../assets/bluecloud_up.jpg",
				"../../assets/bluecloud_dn.jpg",
				"../../assets/bluecloud_ft.jpg",
				"../../assets/bluecloud_bk.jpg"
			};
			programValues.skybox = LoadCubemap(faces);
		}

        Vector3 cameraPosition = new Vector3(0, -14.5f, 0);
		// tick for background surface
		public void Tick()
		{
            // measure frame duration

            KeyboardState keys = Keyboard.GetState();

			if (keys.IsKeyDown(Key.A))
			{
                cameraPosition.X += (float)timer.Elapsed.TotalSeconds * 10;
			}
			else if (keys.IsKeyDown(Key.D))
			{
                cameraPosition.X -= (float)timer.Elapsed.TotalSeconds * 10;
            }
			if (keys.IsKeyDown(Key.W))
			{
                cameraPosition.Z += (float)timer.Elapsed.TotalSeconds * 10;
            }
			else if (keys.IsKeyDown(Key.S))
			{
                cameraPosition.Z -= (float)timer.Elapsed.TotalSeconds * 10;
            }
            camera = Matrix4.CreateTranslation(cameraPosition) * Matrix4.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2) * Matrix4.CreatePerspectiveFieldOfView(1.2f, 1.3f, .1f, 1000);
            timer.Restart();
        }

		// tick for OpenGL rendering code
		public void RenderGL()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
			GL.ClearColor(Color.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			GL.Clear(ClearBufferMask.DepthBufferBit);

			GL.EnableVertexAttribArray(0);
			GL.Enable(EnableCap.TextureCubeMap);
			GL.UseProgram(skyboxshader.programID);
			GL.BindVertexArray(skyboxbuffer);
			Matrix4 view = camera.ClearProjection().ClearTranslation();
			Matrix4 projection = camera.ClearRotation().ClearTranslation();
			GL.UniformMatrix4(GL.GetUniformLocation(skyboxshader.programID, "view"), false, ref view);
			GL.UniformMatrix4(GL.GetUniformLocation(skyboxshader.programID, "projection"), false, ref projection);
			GL.BindTexture(TextureTarget.TextureCubeMap, programValues.skybox);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

			GL.DisableVertexAttribArray(0);
			GL.Disable(EnableCap.TextureCubeMap);
			GL.Enable(EnableCap.DepthTest);

			// update rotation
			scene.FullRender(camera, shader);

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
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
		}

		int LoadCubemap(string[] faces)
		{
			int textureID = GL.GenTexture();
			GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

			for(int i = 0; i < faces.Length; ++i)
			{
				Bitmap bmp = new Bitmap(faces[i]);
				bmp.RotateFlip(RotateFlipType.Rotate180FlipY);	
				System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
				bmp.UnlockBits(bmp_data);
			}

			GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, new int[] { (int)All.Linear });
			GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, new int[] { (int)All.Linear });
			GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, new int[] { (int)All.ClampToEdge });
			GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, new int[] { (int)All.ClampToEdge });
			GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, new int[] { (int)All.ClampToEdge });

			return textureID;
		}
	}
}