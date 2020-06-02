using System.Diagnostics;
using OpenTK;
using System;
using OpenTK.Input;

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

		// initialize
		public void Init()
		{
			timer = new Stopwatch();
			timer.Reset();
			timer.Start();
			// create shaders
			shader = new Shader( "../../shaders/vs.glsl", "../../shaders/fs.glsl" );
			postproc = new Shader( "../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl" );

			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			quad = new ScreenQuad();

			camera = Matrix4.CreateTranslation(new Vector3(0, -14.5f, 0)) * Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)Math.PI / 2) * Matrix4.CreatePerspectiveFieldOfView(1.2f, 1.3f, .1f, 1000);

			scene = new SceneGraph();
		}

		// tick for background surface
		public void Tick()
		{
			KeyboardState keys = Keyboard.GetState();

			if (keys.IsKeyDown(Key.A))
			{
				camera *= Matrix4.CreateTranslation(new Vector3(1, 0, 0) * (float)timer.Elapsed.TotalSeconds);
			}
			else if (keys.IsKeyDown(Key.D))
			{
				camera *= Matrix4.CreateTranslation(new Vector3(-1, 0, 0) * (float)timer.Elapsed.TotalSeconds);
			}
			else if (keys.IsKeyDown(Key.W))
			{
				camera *= Matrix4.CreateTranslation(new Vector3(0, -1, 0) * (float)timer.Elapsed.TotalSeconds);
			}
			else if (keys.IsKeyDown(Key.S))
			{
				camera *= Matrix4.CreateTranslation(new Vector3(0, 1, 0) * (float)timer.Elapsed.TotalSeconds);
			}

			timer.Restart();
		}

		// tick for OpenGL rendering code
		public void RenderGL()
		{
			// measure frame duration
			float frameDuration = timer.ElapsedMilliseconds;
			timer.Reset();
			timer.Start();

			// update rotation
			scene.Render(camera, shader);
		}
	}
}