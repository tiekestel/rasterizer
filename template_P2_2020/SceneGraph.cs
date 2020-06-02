using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
	class SceneGraph
	{
		ParentMesh camera;
		class ParentMesh
		{
			List<ParentMesh> child_meshes = new List<ParentMesh>();
			Mesh mesh;
			Texture texture;

			public ParentMesh(Mesh _mesh, Texture _texture)
			{
				mesh = _mesh;
				texture = _texture;
			}

			//Add child meshes to parent mesh
			public void Add(ParentMesh child_mesh)
			{
				child_meshes.Add(child_mesh);
			}

			public void Render(Matrix parentMatrix, Shader shader)
			{
				//Render child meshes
				foreach(ParentMesh p in child_meshes)
				{
					p.Render(null, shader);
				}
			}
		}

		public SceneGraph(Matrix camera)
		{
			ParentMesh world = new ParentMesh(new Mesh("../../assets/floor.obj"), new Texture("../../assets/wood.jpg"));
			ParentMesh teapot = new ParentMesh(new Mesh("../../assets/teapot.obj"), new Texture("../../assets/wood.jpg"));
			world.Add(teapot);
			world.Render(camera, shader);
		}
	}
}
