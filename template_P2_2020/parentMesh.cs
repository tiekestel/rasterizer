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
        public Matrix4 localTranslation, localScale, localRotation;
        public float intensity;
        cubemap cubemap;

        public ParentMesh(Mesh _mesh, Texture _texture, Matrix4 _localTransform, Matrix4 _localScale, Matrix4 _localRotation, ParentMesh _parent = null, float hdr = 0, int _cubemap = 0, Texture _normalMap = null)
        {
            mesh = _mesh;
            texture = _texture;
            localTranslation = _localTransform;
            localScale = _localScale;
            localRotation = _localRotation;
            normalMap = _normalMap;
            parent = _parent;
            
            intensity = hdr;
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

        public void Render(Matrix4 parentMatrix, Matrix4 camera, Matrix4 cameraPosition, Shader shader, List<Pointlight> pointlights, List<DirectionalLight> directionalLights, List<Spotlight> spotlights)
        {
            //Combine matrices
            Matrix4 finalTransform = localScale * localRotation * localTranslation * parentMatrix;

            mesh.Render(shader, finalTransform, camera, cameraPosition, intensity, texture, normalMap, cubemap, pointlights, directionalLights, spotlights);

            //Render child meshes
            foreach (ParentMesh p in child_meshes)
            {
                p.Render(finalTransform, camera, cameraPosition, shader, pointlights, directionalLights, spotlights);
            }
        }

        public void SimpleRender(Matrix4 parentMatrix, Matrix4 camera, Matrix4 cameraPosition, Shader shader, List<Pointlight> pointlights, List<DirectionalLight> directionalLights, List<Spotlight> spotlights, ParentMesh parentMesh)
        {
            //Combine matrices
            Matrix4 finalTransform = localScale * localRotation * localTranslation * parentMatrix;

            //dont render the object because it cant reflect/refract itself
            if(parentMesh != this)
            {
                mesh.Render(shader, finalTransform, camera, cameraPosition, texture, pointlights, directionalLights, spotlights);
            }

            //Render child meshes
            foreach (ParentMesh p in child_meshes)
            {
                p.SimpleRender(finalTransform, camera, cameraPosition, shader, pointlights, directionalLights, spotlights, parentMesh);
            }
        }

        public void RenderCubemap(Matrix4 parentMatrix, SceneGraph scene)
        {
            Matrix4 finalTransform = localScale * localRotation * localTranslation * parentMatrix;
            if (cubemap != null)
            {
                Vector3 transform = finalTransform.Row3.Xyz;
                cubemap.Render(programValues.cubemapshader, transform, scene, this);
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
                return localScale * localRotation * localTranslation * parent.CalcFinalTransform();
            }
            else
            {
                return localScale * localRotation * localTranslation;
            }
        }
    }

}
