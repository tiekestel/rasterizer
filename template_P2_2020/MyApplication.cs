using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
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

            camera = new Matrix4();
			scene = new SceneGraph();
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
			// update rotation
			scene.Render(camera, shader);
		}
	}
}