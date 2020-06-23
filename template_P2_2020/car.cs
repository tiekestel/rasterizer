using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace Template
{
    class car : gameObject
    {
        wheel[] wheels;
        float speed = 0;
        public car(ParentMesh carbody, ParentMesh leftWheel, ParentMesh rightWheel)
        {
            parent = carbody;
            wheels = new wheel[2]
            {
                new wheel(leftWheel),
                new wheel(rightWheel)
            };
        }

		//update car position
        public override void Update(Stopwatch gameTime, KeyboardState state)
        {
			// change car direction left right
            if(state.IsKeyDown(Key.Left))
            {
                for(int i = 0; i < 2; ++i)
                {
                    wheels[i].Turn((float)(0.25 * Math.PI));
                }
                if(speed != 0)
                {
                    parent.localRotation *= Matrix4.CreateRotationY((float)gameTime.Elapsed.TotalSeconds * 5 * speed / 2);
                } 
            }
            else if(state.IsKeyDown(Key.Right))
            {
                for(int i = 0; i < 2; ++i)
                {
                    wheels[i].Turn((float)(-0.25 * Math.PI));
                }
                if(speed != 0)
                {
                    parent.localRotation *= Matrix4.CreateRotationY((float)gameTime.Elapsed.TotalSeconds * -5 * speed / 2);
                }
                
            }
            else
            {
                for (int i = 0; i < 2; ++i)
                {
                    wheels[i].Turn(0);
                }
            }

			// change car position 
            if(state.IsKeyDown(Key.Up))
            {
                speed += 0.05f;
                speed = speed > 2 ? 2 : speed;

            }
            else if(state.IsKeyDown(Key.Down))
            {
                speed -= 0.05f;
                speed = speed < -1 ? -1 : speed;
            }
            else
            {
                if(speed > 0)
                {
                    speed -= 0.1f;
                    speed = speed < 0 ? 0 : speed;
                }
                else
                {
                    speed += 0.1f;
                    speed = speed > 0 ? 0 : speed;
                }
            }
            parent.localTranslation *= Matrix4.CreateTranslation((Matrix4.Invert(parent.localRotation) * Vector4.UnitZ).Xyz * speed * (float)gameTime.Elapsed.TotalSeconds);
        }
    }
}
