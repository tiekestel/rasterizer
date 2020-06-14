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
        bool cubemap;

        public ParentMesh(Mesh _mesh, Texture _texture, Matrix4 _localTransform, bool _cubemap = false, Texture _normalMap = null, ParentMesh _parent = null)
        {
            mesh = _mesh;
            texture = _texture;
            localTransform = _localTransform;
            normalMap = _normalMap;
            parent = _parent;
            cubemap = _cubemap;
        }

        //Add child meshes to parent mesh
        public void Add(ParentMesh child_mesh)
        {
            child_meshes.Add(child_mesh);
        }

        public void Render(Matrix4 parentMatrix, Matrix4 camera, Shader shader, List<Pointlight> pointlights, List<DirectionalLight> directionalLights, List<Spotlight> spotlights, ParentMesh parentMesh)
        {
            //Combine matrices
            Matrix4 finalTransform = localTransform * parentMatrix;

            if(parentMesh != this)
            {
                //if(cubemap)
                //{
                //    RenderCubemap(finalTransform.Row3.Xyz, shader);
                //}

                mesh.Render(shader, finalTransform * camera, texture, normalMap, pointlights, directionalLights, spotlights);
            }

            //Render child meshes
            foreach (ParentMesh p in child_meshes)
            {
                p.Render(finalTransform, camera, shader, pointlights, directionalLights, spotlights, parentMesh);
            }
        }

        public void RenderCubemap(Vector3 transform, Shader shader)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, programValues.cubemapbuffer);
            Matrix4 createCameraview = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI * 0.5f), 1, 0.1f, 10);

            for(int i = -1; i < 2; i += 2)
            {
                Matrix4 camera = Matrix4.LookAt(transform, transform + new Vector3(i, 0, 0), Vector3.UnitY);
                programValues.scene.Render(camera * createCameraview, shader, this);
                camera = Matrix4.LookAt(transform, transform + new Vector3(0, i, 0), Vector3.UnitY);
                programValues.scene.Render(camera * createCameraview, shader, this);
                camera = Matrix4.LookAt(transform, transform + new Vector3(0, 0, i), Vector3.UnitY);
                programValues.scene.Render(camera * createCameraview, shader, this);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
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
