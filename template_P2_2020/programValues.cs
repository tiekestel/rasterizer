using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
	public static class programValues
	{
        public static Shader cubemapshader, skyboxshader, depthmapshader, depthcubemapshader, hdrtargetshader;
        public static int screenwidth, screenheight, cubemapres = 128, depthmapres = 4096, cubedepthmapres = 512, shadowmap, cubemap;
	}
}
