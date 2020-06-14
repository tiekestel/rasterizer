using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Template
{

    public class DirectionalLight
    {
        public Vector4 direction, finalDirection;
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

        public void CalcFinalDirection(Matrix4 camera)
        {
            if(parent != null)
            {
                finalDirection = parent.CalcFinalTransform() * camera.ClearProjection() * direction;
            }
            else
            {
                finalDirection = camera.ClearProjection() * direction;
            }
        }
    }

    public class Pointlight
    {
        public Vector4 position, finalPostion;
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

        public void CalcFinalPosition(Matrix4 camera)
        {
            if(parent != null)
            {
                finalPostion = position * parent.CalcFinalTransform() * camera;
            }
            else
            {
                finalPostion = position * camera;
            }
        }
    }

    public class Spotlight
    {
        public Vector4 position, finalPosition, direction, finalDirection;
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

        public void CalcFinal(Matrix4 camera)
        {
            if(parent != null)
            {
                Matrix4 parentTransform = parent.CalcFinalTransform();
                finalPosition = parentTransform * camera * position;
                finalDirection = parentTransform * camera * direction;
            } 
            else
            {
                finalPosition = position * camera;
                finalDirection = camera.ClearProjection() * direction;
            }
        }
    }

    class SceneGraph
	{
		ParentMesh world;
		ParentMesh teapot;
        List<Pointlight> pointlights;
        List<DirectionalLight> directionalLights;
        List<Spotlight> spotlights;

		public SceneGraph()
		{
			world = new ParentMesh(new Mesh("../../assets/floor.obj"), new Texture("../../assets/black.jpg"), Matrix4.CreateScale(4.0f), new Texture("../../normalMaps/crystal.jpg"));
			teapot = new ParentMesh(new Mesh("../../assets/teapot.obj"), new Texture("../../assets/wood.jpg"), Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(new Vector3(5, 0, 0)));
            directionalLights = new List<DirectionalLight>();
            directionalLights.Add(new DirectionalLight(new Vector4(-1, 1, 0, 1), 0.1f, new Vector3(1, 1, 1), null));
            pointlights = new List<Pointlight>();
            pointlights.Add(new Pointlight(new Vector4(0, 1, 0, 1), 50, new Vector3(1, 0, 0), null));
            spotlights = new List<Spotlight>();
            spotlights.Add(new Spotlight(new Vector4(0,8, 0, 1), new Vector4(0, -1, 0, 1), 100, new Vector3(0, 0, 1), null, 0.8f));
		}

		public void Render(Matrix4 camera, Shader shader)
		{
            foreach(DirectionalLight d in directionalLights)
            {
                d.CalcFinalDirection(camera);
            }

            foreach(Pointlight p in pointlights)
            {
                p.CalcFinalPosition(camera);
            }

            foreach(Spotlight s in spotlights)
            {
                s.CalcFinal(camera);
            }

			world.Render(Matrix4.Identity, camera, shader, pointlights, directionalLights, spotlights);
			teapot.Render(Matrix4.Identity, camera, shader, pointlights, directionalLights, spotlights);
		}
    }
}
