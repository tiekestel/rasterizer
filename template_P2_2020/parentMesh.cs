using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Template
{
    public class ParentMesh
    {
        List<ParentMesh> child_meshes = new List<ParentMesh>();
        ParentMesh parent;
        Mesh mesh;
        Texture texture, normalMap;
        public Matrix4 localTransform;

        public ParentMesh(Mesh _mesh, Texture _texture, Matrix4 _localTransform, Texture _normalMap = null, ParentMesh _parent = null)
        {
            mesh = _mesh;
            texture = _texture;
            localTransform = _localTransform;
            normalMap = _normalMap;
            parent = _parent;
        }

        //Add child meshes to parent mesh
        public void Add(ParentMesh child_mesh)
        {
            child_meshes.Add(child_mesh);
        }

        public void Render(Matrix4 parentMatrix, Matrix4 camera, Shader shader, List<Pointlight> pointlights, List<DirectionalLight> directionalLights, List<Spotlight> spotlights)
        {
            //Combine matrices
            Matrix4 finalTransform = localTransform * parentMatrix;
            mesh.Render(shader, finalTransform * camera, texture, normalMap, pointlights, directionalLights, spotlights);
            //Render child meshes
            foreach (ParentMesh p in child_meshes)
            {
                p.Render(finalTransform, camera, shader, pointlights, directionalLights, spotlights);
            }
        }

        public Matrix4 CalcFinalTransform()
        {
            if(parent != null)
            {
                return localTransform * parent.CalcFinalTransform();
            }
            else
            {
                return localTransform;
            }
        }
    }

}
