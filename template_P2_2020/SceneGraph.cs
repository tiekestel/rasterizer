using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Template
{
    public class DirectionalLight
    {
        public Vector4 direction;
        public float strength;
        public Vector3 color;
        public ParentMesh parent;
        public depthmap shadowMap;

        public DirectionalLight(Vector4 _direction, float _strength, Vector3 _color, ParentMesh _parent)
        {
            direction = _direction;
            strength = _strength;
            color = _color;
            parent = _parent;
            shadowMap = new depthmap(this);
        }
    }

    public class Pointlight
    {
        public Vector4 position, localPosition;
        public float strength;
        public Vector3 color;
        public ParentMesh parent;
        public cubeDepthmap shadowMap;

        public Pointlight(Vector4 _position, float _strength, Vector3 _color, ParentMesh _parent)
        {
            localPosition = _position;
            strength = _strength;
            color = _color;
            parent = _parent;
            shadowMap = new cubeDepthmap();
        }

        public void calc()
        {
            if (parent != null)
            {
                Matrix4 parentmatrix = parent.CalcFinalTransform();
                position = localPosition * parentmatrix.ClearScale();
            }
        }
    }

    public class Spotlight
    {
        public Vector4 position, direction, localPosition, localDirection;
        public float strength;
        public Vector3 color;
        public ParentMesh parent;
        public float angle;

        public Spotlight(Vector4 _position, Vector4 _direction, float _strength, Vector3 _color, ParentMesh _parent, float _angle)
        {
            localPosition = _position;
            localDirection = _direction;
            strength = _strength;
            color = _color;
            parent = _parent;
            angle = _angle;
        }

        public void calc()
        {
            if(parent != null)
            {
                Matrix4 parentmatrix = parent.CalcFinalTransform();
                position =  localPosition*parentmatrix.ClearScale();
                direction = localDirection*parentmatrix;
            }
        }
    }

    public class SceneGraph
	{
        List<ParentMesh> primaryMeshes;
        List<Pointlight> pointlights;
        List<DirectionalLight> directionalLights;
        List<Spotlight> spotlights;
        List<gameObject> gameObjects;
        ParentMesh carbody;
        int skyboxbuffer, skybox;
        bool headlight = false;

        public SceneGraph()
        {
            primaryMeshes = new List<ParentMesh>();
            gameObjects = new List<gameObject>();
            //floor
            primaryMeshes.Add(new ParentMesh(new Mesh("../../assets/floor.obj"), new Texture("../../assets/grass/grassfloor.jpg", false), Matrix4.Identity, Matrix4.CreateScale(8f), Matrix4.Identity, null, 0, 0, new Texture("../../assets/grass/grassnormal.jpg", true)));

            // textures
            Texture black = new Texture("../../assets/black.jpg", false);
            
            //traintracks
            Mesh rail = new Mesh("../../assets/traintracks/rail_straight.obj");
            primaryMeshes[0].Add(new ParentMesh(rail, black, Matrix4.CreateTranslation(1, -2, 3), Matrix4.CreateScale(0.1f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(rail, black, Matrix4.CreateTranslation(-1.02f, -2, 3), Matrix4.CreateScale(0.1f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(rail, black, Matrix4.CreateTranslation(-3.04f, -2, 3), Matrix4.CreateScale(0.1f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(rail, black, Matrix4.CreateTranslation(-5.06f, -2, 3), Matrix4.CreateScale(0.1f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(rail, black, Matrix4.CreateTranslation(-7.08f, -2, 3), Matrix4.CreateScale(0.1f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(rail, black, Matrix4.CreateTranslation(3.02f, -2, 3), Matrix4.CreateScale(0.1f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(rail, black, Matrix4.CreateTranslation(5.04f, -2, 3), Matrix4.CreateScale(0.1f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(rail, black, Matrix4.CreateTranslation(7.06f, -2, 3), Matrix4.CreateScale(0.1f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            ParentMesh train = new ParentMesh(new Mesh("../../assets/train/train.obj"), new Texture("../../assets/train/color.png", false), Matrix4.CreateTranslation(10, -2, 3), Matrix4.CreateScale(0.1f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0], 0, 0, new Texture("../../assets/train/normal.png", false));
            gameObjects.Add(new train(train));
            primaryMeshes[0].Add(train);

            //roads
            Mesh road = new Mesh("../../assets/roads/Road.obj");
            Mesh roadtr = new Mesh("../../assets/roads/Road_tr.obj");
            Texture roadTex = new Texture("../../assets/roads/Road.png", false);
            Texture roadtrTex = new Texture("../../assets/roads/Road_tr.png", false);
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-7.5f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-6.5f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(roadtr, roadtrTex, Matrix4.CreateTranslation(-5.6f, -1.99f, 0.99f), Matrix4.CreateScale(0.47f), Matrix4.Identity, primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-4.7f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-3.7f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-2.7f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-1.7f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-0.7f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(0.3f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(1.3f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(2.3f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(3.3f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(new Mesh("../../assets/roads/Road_sq.obj"), new Texture("../../assets/roads/Road_sq.png", false), Matrix4.CreateTranslation(4.2f, -1.99f, 0.99f), Matrix4.CreateScale(0.47f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(5.1f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(6.1f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(7.1f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(8.1f, -1.99f, 1), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, 1.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, 2.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, 3.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, 4.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, 5.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, 6.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, 7.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, 0.1f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, -0.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, -1.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, -2.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, -3.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(roadtr, roadtrTex, Matrix4.CreateTranslation(4.21f, -1.99f, -4.8f), Matrix4.CreateScale(0.47f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, -5.7f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, -6.7f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(4.2f, -1.99f, -7.7f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(3.3f, -1.99f, -4.8f), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(2.3f, -1.99f, -4.8f), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(1.3f, -1.99f, -4.8f), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(0.3f, -1.99f, -4.8f), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-0.7f, -1.99f, -4.8f), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-1.7f, -1.99f, -4.8f), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-2.7f, -1.99f, -4.8f), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-3.7f, -1.99f, -4.8f), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-4.7f, -1.99f, -4.8f), Matrix4.CreateScale(0.5f), Matrix4.CreateRotationY((float)(0.5f * Math.PI)), primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(new Mesh("../../assets/roads/Road_ro.obj"), new Texture("../../assets/roads/Road_ro.png", false), Matrix4.CreateTranslation(-5.6f, -1.99f, -4.8f), Matrix4.CreateScale(0.47f), Matrix4.CreateRotationY((float)(-0.5f * Math.PI)), primaryMeshes[0]));

            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-5.6f, -1.99f, 0.1f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-5.6f, -1.99f, -0.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-5.6f, -1.99f, -1.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-5.6f, -1.99f, -2.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(road, roadTex, Matrix4.CreateTranslation(-5.6f, -1.99f, -3.9f), Matrix4.CreateScale(0.5f), Matrix4.Identity, primaryMeshes[0]));

            //buildings
            Mesh igloo = new Mesh("../../assets/buildings/igloo.obj");
            Texture tiles = new Texture("../../assets/tiles.jpg", false);
            Texture wood = new Texture("../../assets/wood.jpg", false);
            primaryMeshes[0].Add(new ParentMesh(new Mesh("../../assets/buildings/part1model.obj"), black, Matrix4.CreateTranslation(5.1f, -1.99f, -3.4f), Matrix4.CreateScale(0.05f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(new Mesh("../../assets/buildings/KinderGarten.obj"), black, Matrix4.CreateTranslation(3.5f, -1.99f, 5.4f), Matrix4.CreateScale(1f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(igloo, tiles, Matrix4.CreateTranslation(2.5f, -1.99f, -2.8f), Matrix4.CreateScale(0.02f), Matrix4.CreateRotationY((float)(0.25 * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(igloo, wood, Matrix4.CreateTranslation(1.5f, -1.99f, -0.9f), Matrix4.CreateScale(0.02f), Matrix4.CreateRotationY((float)(-0.15 * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(igloo, black, Matrix4.CreateTranslation(0.2f, -1.99f, -3.4f), Matrix4.CreateScale(0.02f), Matrix4.CreateRotationY((float)(0.5 * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(igloo, tiles, Matrix4.CreateTranslation(-1f, -1.99f, -0.4f), Matrix4.CreateScale(0.02f), Matrix4.CreateRotationY((float)(-0.55 * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(igloo, wood, Matrix4.CreateTranslation(-2f, -1.99f, -3.4f), Matrix4.CreateScale(0.02f), Matrix4.CreateRotationY((float)(0.65 * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(igloo, black, Matrix4.CreateTranslation(-3f, -1.99f, -0.9f), Matrix4.CreateScale(0.02f), Matrix4.CreateRotationY((float)(0.65 * Math.PI)), primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(igloo, tiles, Matrix4.CreateTranslation(-4f, -1.99f, -2.8f), Matrix4.CreateScale(0.02f), Matrix4.CreateRotationY((float)(0.25 * Math.PI)), primaryMeshes[0]));

            //campfire
            pointlights = new List<Pointlight>();
            ParentMesh campfire = new ParentMesh(new Mesh("../../assets/campfire.obj"), new Texture("../../assets/campfire.jpg", false), Matrix4.CreateTranslation(-1, -1.99f, -2f), Matrix4.CreateScale(3), Matrix4.Identity, primaryMeshes[0]);
            primaryMeshes[0].Add(campfire);
            pointlights.Add(new Pointlight(new Vector4(0, 1.5f, 0, 1), 500, new Vector3(1, 0.3f, 0.3f), campfire));

            //trees
            Mesh popular = new Mesh("../../assets/trees/Popular_tree.obj");
            Mesh fir = new Mesh("../../assets/trees/Fir_Tree.obj");
            Mesh palm = new Mesh("../../assets/trees/Palm_tree.obj");
            Texture firTex = new Texture("../../assets/trees/Fir_tree.png", false);
            primaryMeshes[0].Add(new ParentMesh(new Mesh("../../assets/trees/Carrot.obj"), new Texture("../../assets/trees/carrot.png", false), Matrix4.CreateTranslation(-1f, -1f, 5.5f), Matrix4.CreateScale(5), Matrix4.CreateRotationZ((float)(0.5 * Math.PI)), primaryMeshes[0],0,1));
            primaryMeshes[0].Add(new ParentMesh(popular, firTex, Matrix4.CreateTranslation(-7f, -1.99f, -2f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(fir, firTex, Matrix4.CreateTranslation(-6.7f, -1.99f, -6.6f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(palm, firTex, Matrix4.CreateTranslation(-4f, -1.99f, -5.5f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(popular, firTex, Matrix4.CreateTranslation(-1f, -1.99f, -6.3f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(popular, firTex, Matrix4.CreateTranslation(-0.5f, -1.99f, -6f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(fir, firTex, Matrix4.CreateTranslation(2f, -1.99f, -6.6f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(palm, firTex, Matrix4.CreateTranslation(5f, -1.99f, -6f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(popular, firTex, Matrix4.CreateTranslation(6.6f, -1.99f, -1.9f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(fir, firTex, Matrix4.CreateTranslation(5.6f, -1.99f, 2f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(palm, firTex, Matrix4.CreateTranslation(6f, -1.99f, 6f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(palm, firTex, Matrix4.CreateTranslation(6.5f, -1.99f, 5.8f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));
            primaryMeshes[0].Add(new ParentMesh(palm, firTex, Matrix4.CreateTranslation(6.7f, -1.99f, 6.5f), Matrix4.CreateScale(0.2f), Matrix4.Identity, primaryMeshes[0]));

            //streetlamps
            Mesh streetlamp = new Mesh("../../assets/streetlamp.obj");
            ParentMesh[] streetlamps = new ParentMesh[5]
            {
                new ParentMesh(streetlamp, black, Matrix4.CreateTranslation(-5.7f, -1.99f, -5.2f), Matrix4.CreateScale(0.3f), Matrix4.CreateRotationY((float)(0.75 * Math.PI)), primaryMeshes[0]),
                new ParentMesh(streetlamp, black, Matrix4.CreateTranslation(-5.5f, -1.99f, 1.5f), Matrix4.CreateScale(0.3f), Matrix4.CreateRotationY((float)(-0.5 * Math.PI)), primaryMeshes[0]),
                new ParentMesh(streetlamp, black, Matrix4.CreateTranslation(4.7f, -1.99f, -4.9f), Matrix4.CreateScale(0.3f), Matrix4.Identity, primaryMeshes[0]),
                new ParentMesh(streetlamp, black, Matrix4.CreateTranslation(-1f, -1.99f, 1.5f), Matrix4.CreateScale(0.3f), Matrix4.CreateRotationY((float)(-0.5 * Math.PI)), primaryMeshes[0]),
                new ParentMesh(streetlamp, black, Matrix4.CreateTranslation(4.7f, -1.99f, 5.5f), Matrix4.CreateScale(0.3f), Matrix4.Identity, primaryMeshes[0])
            };

            spotlights = new List<Spotlight>();
            for(int i = 0; i < streetlamps.Length; ++i)
            {
                spotlights.Add(new Spotlight(new Vector4(0, 4, 0, 1), Vector4.Normalize(new Vector4(-1, -1, 0, 0)), 100, new Vector3(1, 1, 1), streetlamps[i], 0.9f));
                primaryMeshes[0].Add(streetlamps[i]);
            }

            //car
            carbody = new ParentMesh(new Mesh("../../assets/car/Cars.obj"), black, Matrix4.CreateTranslation(0, -1.99f, 1.17f), Matrix4.CreateScale(0.05f), Matrix4.CreateRotationY((float)(0.5 * Math.PI)), primaryMeshes[0]);
            ParentMesh leftWheel = new ParentMesh(new Mesh("../../assets/car/Left_Wheel.obj"), black, Matrix4.CreateTranslation(0.7f, 0.3f, 1.4f), Matrix4.CreateScale(1f), Matrix4.Identity, primaryMeshes[0]);
            ParentMesh rightWheel = new ParentMesh(new Mesh("../../assets/car/Right_Wheel.obj"), black, Matrix4.CreateTranslation(-0.7f, 0.3f, 1.4f), Matrix4.CreateScale(1f), Matrix4.Identity, primaryMeshes[0]);
            
            primaryMeshes[0].Add(carbody);
            carbody.Add(leftWheel);
            carbody.Add(rightWheel);
            gameObjects.Add(new car(carbody, leftWheel, rightWheel));

            //sun
            directionalLights = new List<DirectionalLight>();
            directionalLights.Add(new DirectionalLight(new Vector4(-1f, 1, 0, 1), 1, new Vector3(1, 1, 1), null));

            //create skybox
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

            string[] faces = new string[]
            {
                "../../assets/bluecloud_rt.jpg",
                "../../assets/bluecloud_lf.jpg",
                "../../assets/bluecloud_up.jpg",
                "../../assets/bluecloud_dn.jpg",
                "../../assets/bluecloud_ft.jpg",
                "../../assets/bluecloud_bk.jpg"
            };

            //create skybox buffer
            skyboxbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, skyboxbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);

            skybox = LoadCubemap(faces);

            foreach (ParentMesh p in primaryMeshes)
            {
                p.RenderCubemap(Matrix4.Identity, this);
            }
           
        }

		// normally render scene
		public void Render(Matrix4 camera, Matrix4 cameraPosition, Shader shader)
		{
            foreach (ParentMesh p in primaryMeshes)
            {
                p.Render(Matrix4.Identity, camera, cameraPosition, shader, pointlights, directionalLights, spotlights);
            }
		}

		// simply render scene
        public void SimpleRender(Matrix4 camera, Matrix4 cameraPosition, Shader shader, ParentMesh parentMesh = null)
        {
            foreach(ParentMesh p in primaryMeshes)
            {
                p.SimpleRender(Matrix4.Identity, camera, cameraPosition, shader, pointlights, directionalLights, spotlights, parentMesh);
            }
        }

		// full render scene
        public void FullRender(Matrix4 cameraPosition, Matrix4 cameraRotation, Matrix4 cameraFOV, Shader shader, int framebuffer)
        {
			//render shadowmap
            foreach (DirectionalLight d in directionalLights)
            {
                d.shadowMap.Render(programValues.depthmapshader, this);
            }

            foreach(Pointlight p in pointlights)
            {
                p.calc();
                p.shadowMap.Render(programValues.depthmapshader, p.position.Xyz, this);
            }

            foreach(Spotlight s in spotlights)
            {
                s.calc();
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.UseProgram(shader.programID);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            Render(cameraPosition * cameraRotation * cameraFOV, cameraPosition, shader);
            RenderSkyBox(cameraPosition, cameraRotation, cameraFOV, programValues.skyboxshader);
            GL.UseProgram(0);
        }

		// render skybox
        public void RenderSkyBox(Matrix4 cameraPosition, Matrix4 cameraRotation, Matrix4 cameraFOV, Shader skyboxshader)
        {
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

		// render cubemap
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

        Vector3 cameraheight = new Vector3(0, -30, 0);

        public void Update(Stopwatch gameTime, KeyboardState state, KeyboardState prevstate, ref Matrix4 cameraPosition, ref Matrix4 cameraRotation, MouseState mousestate, MouseState prevmousestate)
        {
            foreach(gameObject g in gameObjects)
            {
                g.Update(gameTime, state);
            }

            cameraheight.Y += mousestate.ScrollWheelValue - prevmousestate.ScrollWheelValue;
			Matrix4 carTransform = carbody.CalcFinalTransform();
			cameraPosition = Matrix4.Invert(carTransform.ClearRotation()) * Matrix4.CreateTranslation(cameraheight);
			cameraRotation = Matrix4.Invert(carTransform.ClearTranslation()) * Matrix4.CreateRotationY((float)Math.PI) * Matrix4.CreateRotationX((float)(0.5 * Math.PI));

			if (state.IsKeyDown(Key.H) && !prevstate.IsKeyDown(Key.H))
            {
                if(headlight)
                {
                    spotlights = spotlights.GetRange(0, spotlights.Count - 4);
                    headlight = false;
                }
                else
                {
                    spotlights.Add(new Spotlight(new Vector4(-0.25f, 0.2f, 0.75f, 1), new Vector4(0, 0, 1, 0), 50, new Vector3(1, 1, 1), carbody, 0.8f));
                    spotlights.Add(new Spotlight(new Vector4(0.25f, 0.2f, 0.75f, 1), new Vector4(0, 0, 1, 0), 50, new Vector3(1, 1, 1), carbody, 0.8f));
                    spotlights.Add(new Spotlight(new Vector4(-0.25f, 0.2f, -0.5f, 1), new Vector4(0, 0, -1, 0), 30, new Vector3(1, 0, 0), carbody, 0.95f));
                    spotlights.Add(new Spotlight(new Vector4(0.25f, 0.2f, -0.5f, 1), new Vector4(0, 0, -1, 0), 30, new Vector3(1, 0, 0), carbody, 0.95f));
                    headlight = true;
                }
            }
        }

		// load cubemap
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
