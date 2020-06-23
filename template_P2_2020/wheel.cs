using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Template
{
    class wheel
    {
        ParentMesh parent;

        public wheel(ParentMesh wheel)
        {
            parent = wheel;
        }

		// turn wheels
        public void Turn(float angle)
        {
            parent.localRotation = Matrix4.CreateRotationY(angle);
        }
    }
}
