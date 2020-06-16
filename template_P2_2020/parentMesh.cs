using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Template
{
    public class ParentMesh
    {
        List<ParentMesh> child_meshes = new List<ParentMesh>();
        ParentMesh parent;
        Mesh mesh;
        Texture texture, normalMap;
        public Matrix4 localTransform;
        cubemap cubemap;

        public ParentMesh(Mesh _mesh, Texture _texture, Matrix4 _localTransform, int _cubemap = 0, Texture _normalMap = null, ParentMesh _parent = null)
        {
            mesh = _mesh;
            texture = _texture;
            localTransform = _localTransform;
            normalMap = _normalMap;
            parent = _parent;
            if (_cubemap != 0)
            {
                cubemap = new cubemap(_cubemap);
            }
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

            mesh.Render(shader, finalTransform * camera, texture, normalMap, cubemap, pointlights, directionalLights, spotlights);

            //Render child meshes
            foreach (ParentMesh p in child_meshes)
            {
                p.Render(finalTransform, camera, shader, pointlights, directionalLights, spotlights);
            }
        }

        public void SimpleRender(Matrix4 parentMatrix, Matrix4 camera, Shader shader, List<Pointlight> pointlights, List<DirectionalLight> directionalLights, List<Spotlight> spotlights, ParentMesh parentMesh)
        {
            //Combine matrices
            Matrix4 finalTransform = localTransform * parentMatrix;

            //dont render the object because it cant reflect/refract itself
            if(parentMesh != this)
            {
                mesh.Render(shader, finalTransform * camera, texture, pointlights, directionalLights, spotlights);
            }

            //Render child meshes
            foreach (ParentMesh p in child_meshes)
            {
                p.SimpleRender(finalTransform, camera, shader, pointlights, directionalLights, spotlights, parentMesh);
            }
        }

        public void RenderCubemap(Matrix4 parentMatrix, SceneGraph scene)
        {
            Matrix4 finalTransform = localTransform * parentMatrix;
            if (cubemap != null)
            {
                Vector3 transform = finalTransform.Row3.Xyz;
                cubemap.Render(programValues.cubemapshader, transform, scene, this);
                programValues.cubemap = cubemap.id;
            }

            foreach (ParentMesh p in child_meshes)
            {
                p.RenderCubemap(finalTransform, scene);
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
