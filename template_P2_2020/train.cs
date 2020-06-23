using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenTK;
namespace Template
{
    class train : gameObject
    {
        float time;
        Matrix4 beginTranslation;
        public train(ParentMesh _parent)
        {
            parent = _parent;
            beginTranslation = parent.localTranslation;
        }
        public override void Update(Stopwatch gameTime)
        {
            parent.localTranslation = Matrix4.CreateTranslation(-Vector3.UnitX * (float)gameTime.Elapsed.TotalSeconds) * parent.localTranslation;
            time += (float)gameTime.Elapsed.TotalSeconds;
            if(time > 20)
            {
                parent.localTranslation = beginTranslation;
                time -= 20;
            }
        }
    }
}
