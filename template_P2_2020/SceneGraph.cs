using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Template
{
	class SceneGraph
	{
		ParentMesh world;
		ParentMesh teapot;

		class ParentMesh
		{
			List<ParentMesh> child_meshes = new List<ParentMesh>();
			Mesh mesh;
			Texture texture;
			Matrix4 localTransform;

			public ParentMesh(Mesh _mesh, Texture _texture, Matrix4 _localTransform)
			{
				mesh = _mesh;
				texture = _texture;
				localTransform = _localTransform;
			}

			//Add child meshes to parent mesh
			public void Add(ParentMesh child_mesh)
			{
				child_meshes.Add(child_mesh);
			}

			public void Render(Matrix4 parentMatrix, Shader shader)
			{
				//Combine matrices
				Matrix4 finalTransform = localTransform * parentMatrix;
				mesh.Render(shader, finalTransform, texture);
				//Render child meshes
				foreach(ParentMesh p in child_meshes)
				{
					p.Render(finalTransform, shader);
				}
			}
		}

		public SceneGraph()
		{
			world = new ParentMesh(new Mesh("../../assets/floor.obj"), new Texture("../../assets/tiles.jpg"), Matrix4.CreateScale(4.0f) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), 0));
			teapot = new ParentMesh(new Mesh("../../assets/teapot.obj"), new Texture("../../assets/wood.jpg"), Matrix4.CreateScale(0.5f) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), 0));
		}

		public void Render(Matrix4 camera, Shader shader)
		{
			world.Render(camera, shader);
			teapot.Render(camera, shader);
		}
	}
}
