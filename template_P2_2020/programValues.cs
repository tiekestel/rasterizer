using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
	//global variables
	public static class programValues
	{
        public static Shader cubemapshader, skyboxshader, depthmapshader, depthcubemapshader;
        public static int screenwidth, screenheight, cubemapres = 512, depthmapres = 8192, cubedepthmapres = 512, shadowmap, cubemap;

        public static int tex;
	}
}
