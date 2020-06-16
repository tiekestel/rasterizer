using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Template
{

    public class DirectionalLight
    {
        public Vector4 direction;
        public float strength;
        public Vector3 color;
        public ParentMesh parent;

        public DirectionalLight(Vector4 _direction, float _strength, Vector3 _color, ParentMesh _parent)
        {
            direction = _direction;
            strength = _strength;
            color = _color;
            parent = _parent;
        }
    }

    public class Pointlight
    {
        public Vector4 position;
        public float strength;
        public Vector3 color;
        public ParentMesh parent;

        public Pointlight(Vector4 _position, float _strength, Vector3 _color, ParentMesh _parent)
        {
            position = _position;
            strength = _strength;
            color = _color;
            parent = _parent;
        }
    }

    public class Spotlight
    {
        public Vector4 position, direction;
        public float strength;
        public Vector3 color;
        public ParentMesh parent;
        public float angle;

        public Spotlight(Vector4 _position, Vector4 _direction, float _strength, Vector3 _color, ParentMesh _parent, float _angle)
        {
            position = _position;
            direction = _direction;
            strength = _strength;
            color = _color;
            parent = _parent;
            angle = _angle;
        }
    }

    public class SceneGraph
	{
        List<ParentMesh> primaryMeshes;
        List<Pointlight> pointlights;
        List<DirectionalLight> directionalLights;
        List<Spotlight> spotlights;
        int skyboxbuffer, skybox;

		public SceneGraph()
		{
            ParentMesh world = new ParentMesh(new Mesh("../../assets/floor.obj"), new Texture("../../assets/black.jpg"), Matrix4.CreateScale(4.0f), 0/*, new Texture("../../normalMaps/crystal.jpg")*/);
            ParentMesh teapot = new ParentMesh(new Mesh("../../assets/teapot.obj"), new Texture("../../assets/wood.jpg"), Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(new Vector3(0, 0, 0)), 0);
            primaryMeshes = new List<ParentMesh>();
            primaryMeshes.Add(world);
            primaryMeshes.Add(teapot);
            directionalLights = new List<DirectionalLight>();
            directionalLights.Add(new DirectionalLight(new Vector4(-1, 1, 0, 1), 1, new Vector3(1, 1, 1), null));
            pointlights = new List<Pointlight>();
            pointlights.Add(new Pointlight(new Vector4(5, 8, 0, 1), 50, new Vector3(1, 1, 1), null));
            spotlights = new List<Spotlight>();
            spotlights.Add(new Spotlight(new Vector4(0,8, 0, 1), new Vector4(0, -1, 0, 1), 1000, new Vector3(0, 0, 1), null, 0.8f));
            float[] skyboxVertices = new float[] {
				// positions          
				-1.0f,  1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f
            };

            skyboxbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, skyboxbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);

            string[] faces = new string[]
            {
                "../../assets/bluecloud_rt.jpg",
                "../../assets/bluecloud_lf.jpg",
                "../../assets/bluecloud_up.jpg",
                "../../assets/bluecloud_dn.jpg",
                "../../assets/bluecloud_ft.jpg",
                "../../assets/bluecloud_bk.jpg"
            };
            skybox = LoadCubemap(faces);

            foreach (ParentMesh p in primaryMeshes)
            {
                p.RenderCubemap(Matrix4.Identity, this);
            }
        }

		public void Render(Matrix4 camera, Matrix4 cameraPosition, Shader shader)
		{
            //move lights to camera space
            //foreach(DirectionalLight d in directionalLights)
            //{
            //    d.CalcFinalDirection(camera);
            //}

            //foreach(Pointlight p in pointlights)
            //{
            //    p.CalcFinalPosition(camera);
            //}

            //foreach(Spotlight s in spotlights)
            //{
            //    s.CalcFinal(camera);
            //}

            foreach(ParentMesh p in primaryMeshes)
            {
                p.Render(Matrix4.Identity, camera, cameraPosition, shader, pointlights, directionalLights, spotlights);
            }

		}

        public void SimpleRender(Matrix4 camera, Shader shader, ParentMesh parentMesh)
        {
            foreach(ParentMesh p in primaryMeshes)
            {
                p.SimpleRender(Matrix4.Identity, camera, shader, pointlights, directionalLights, spotlights, parentMesh);
            }
        }

        public void FullRender(Matrix4 cameraPosition, Matrix4 cameraRotation, Matrix4 cameraFOV, Shader shader, int framebuffer)
        {


            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.UseProgram(shader.programID);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);


            GL.Enable(EnableCap.DepthTest);
            Render(cameraPosition * cameraRotation * cameraFOV, cameraPosition, shader);
            RenderSkyBox(cameraPosition, cameraRotation, cameraFOV, programValues.skyboxshader);
            //RenderCubeMap(cameraPosition, cameraRotation, cameraFOV, programValues.skyboxshader);
            GL.UseProgram(0);
        }

        public void RenderSkyBox(Matrix4 cameraPosition, Matrix4 cameraRotation, Matrix4 cameraFOV, Shader skyboxshader)
        {
            //skybox
            GL.DepthFunc(DepthFunction.Lequal);
            GL.EnableVertexAttribArray(2);
            GL.Enable(EnableCap.TextureCubeMap);
            GL.UseProgram(skyboxshader.programID);
            GL.BindVertexArray(skyboxbuffer);
            GL.UniformMatrix4(GL.GetUniformLocation(skyboxshader.programID, "view"), false, ref cameraRotation);
            GL.UniformMatrix4(GL.GetUniformLocation(skyboxshader.programID, "projection"), false, ref cameraFOV);
            GL.BindTexture(TextureTarget.TextureCubeMap, skybox);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.DisableVertexAttribArray(2);
            GL.Disable(EnableCap.TextureCubeMap);
            GL.DepthFunc(DepthFunction.Less);
            GL.Disable(EnableCap.DepthTest);
        }

        public void RenderCubeMap(Matrix4 cameraPosition, Matrix4 cameraRotation, Matrix4 cameraFOV, Shader skyboxshader)
        {
            //skybox
            GL.DepthFunc(DepthFunction.Lequal);
            GL.EnableVertexAttribArray(2);
            GL.Enable(EnableCap.TextureCubeMap);
            GL.UseProgram(skyboxshader.programID);
            GL.BindVertexArray(skyboxbuffer);
            GL.UniformMatrix4(GL.GetUniformLocation(skyboxshader.programID, "view"), false, ref cameraRotation);
            GL.UniformMatrix4(GL.GetUniformLocation(skyboxshader.programID, "projection"), false, ref cameraFOV);
            GL.BindTexture(TextureTarget.TextureCubeMap, programValues.cubemap);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.DisableVertexAttribArray(2);
            GL.Disable(EnableCap.TextureCubeMap);
            GL.DepthFunc(DepthFunction.Less);
            GL.Disable(EnableCap.DepthTest);

        }

        int LoadCubemap(string[] faces)
        {
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);

            for (int i = 0; i < faces.Length; ++i)
            {
                Bitmap bmp = new Bitmap(faces[i]);
                bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                bmp.UnlockBits(bmp_data);
            }

            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, new int[] { (int)All.Linear });
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, new int[] { (int)All.Linear });
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, new int[] { (int)All.ClampToEdge });
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, new int[] { (int)All.ClampToEdge });
            GL.TexParameterI(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, new int[] { (int)All.ClampToEdge });

            return textureID;
        }
    }
}
