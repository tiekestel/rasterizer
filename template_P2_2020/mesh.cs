using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	// mesh and loader based on work by JTalton; http://www.opentk.com/node/642

	public class Mesh
	{
		// data members
		public Matrix4 modelView;
		public ObjVertex[] vertices;            // vertex positions, model space
		public ObjTriangle[] triangles;         // triangles (3 vertex indices)
		public ObjQuad[] quads;                 // quads (4 vertex indices)
		int vertexBufferId;                     // vertex buffer
		int triangleBufferId;                   // triangle buffer
		int quadBufferId;                       // quad buffer

		// constructor
		public Mesh( string fileName )
		{
			MeshLoader loader = new MeshLoader();
			loader.Load( this, fileName );
		}

		// initialization; called during first render
		public void Prepare( Shader shader )
		{
			if( vertexBufferId == 0 )
			{
				// generate interleaved vertex data (uv/normal/position (total 8 floats) per vertex)
				GL.GenBuffers( 1, out vertexBufferId );
				GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBufferId );
				GL.BufferData( BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Marshal.SizeOf( typeof( ObjVertex ) )), vertices, BufferUsageHint.StaticDraw );

				// generate triangle index array
				GL.GenBuffers( 1, out triangleBufferId );
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, triangleBufferId );
				GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(triangles.Length * Marshal.SizeOf( typeof( ObjTriangle ) )), triangles, BufferUsageHint.StaticDraw );

				// generate quad index array
				GL.GenBuffers( 1, out quadBufferId );
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, quadBufferId );
				GL.BufferData( BufferTarget.ElementArrayBuffer, (IntPtr)(quads.Length * Marshal.SizeOf( typeof( ObjQuad ) )), quads, BufferUsageHint.StaticDraw );
			}
		}

		// render the mesh using the supplied shader and matrix
		public void Render( Shader shader, Matrix4 transform, Matrix4 camera, Matrix4 cameraPosition, float intensity, Texture texture, Texture normalMap, cubemap cubemap, List<Pointlight> pointlights, List<DirectionalLight> directionalLights, List<Spotlight> spotlights )
		{
			// on first run, prepare buffers
			Prepare( shader );

			// safety dance
			GL.PushClientAttrib( ClientAttribMask.ClientVertexArrayBit );
            // enable normal map




            // enable shader
            GL.UseProgram( shader.programID );

			// pass transform to vertex shader
			int tr = GL.GetUniformLocation(shader.programID, "transform");
			GL.UniformMatrix4( tr, false, ref transform );
			GL.UniformMatrix4( shader.uniform_mview, false, ref camera );
			GL.Uniform4(GL.GetUniformLocation(shader.programID, "viewPos"), cameraPosition.Row3);

            int isNormal = GL.GetUniformLocation(shader.programID, "isNormalMap");
            if(normalMap != null)
            {
                GL.Uniform1(isNormal, 1);
            }
            else
            {
                GL.Uniform1(isNormal, 0);
            }

            int cubeMapType = GL.GetUniformLocation(shader.programID, "cubeMapType");
            if (cubemap != null)
            {
                GL.Uniform1(cubeMapType, cubemap.type);
            }
            else
            {
                GL.Uniform1(cubeMapType, 0);
            }

            //Directional lights
            int depthmaps = 0;
            for (int i = 0; i < directionalLights.Count; ++i)
            {
                int direction = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].direction");
                int color = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].color");
                int strength = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].strength");
                int map = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].shadowMap");
                int mat = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].lightSpace");
                GL.Uniform3(direction, Vector3.Normalize(directionalLights[i].direction.Xyz));
                GL.Uniform3(color, directionalLights[i].color);
                GL.Uniform1(strength, directionalLights[i].strength);
                GL.Uniform1(map, 3 + depthmaps);
                GL.ActiveTexture(TextureUnit.Texture3 + depthmaps);
                GL.BindTexture(TextureTarget.Texture2D, directionalLights[i].shadowMap.id);
                GL.UniformMatrix4(mat, false, ref directionalLights[i].shadowMap.camera);
                depthmaps++;
            }
            int location = GL.GetUniformLocation(shader.programID, "directionalLightCount");
            GL.Uniform1(location, directionalLights.Count);

            int cubes = 0;
            //Pointlights
            for (int i = 0; i < pointlights.Count; ++i)
            {
                int position = GL.GetUniformLocation(shader.programID, "pointlights[" + i + "].position");
                int color = GL.GetUniformLocation(shader.programID, "pointlights[" + i + "].color");
                int strength = GL.GetUniformLocation(shader.programID, "pointlights[" + i + "].strength");
                int map = GL.GetUniformLocation(shader.programID, "pointlights[" + i + "].shadowMap");
                GL.Uniform3(position, pointlights[i].position.Xyz);
                GL.Uniform3(color, pointlights[i].color);
                GL.Uniform1(strength, pointlights[i].strength);
                GL.Uniform1(map, 3 + depthmaps);
                GL.ActiveTexture(TextureUnit.Texture3 + depthmaps);
                GL.BindTexture(TextureTarget.TextureCubeMap, pointlights[i].shadowMap.id);
                depthmaps++;
                cubes++;
            }
            location = GL.GetUniformLocation(shader.programID, "pointlightCount");
            GL.Uniform1(location, pointlights.Count);

            for(int i = cubes; i < 20; ++i)
            {
                int map = GL.GetUniformLocation(shader.programID, "pointlights[" + i + "].shadowMap");
                GL.Uniform1(map, 3 + depthmaps - 1);
                GL.BindTexture(TextureTarget.TextureCubeMap, pointlights[0].shadowMap.id);
            }

            //Spotlights
            for (int i = 0; i < spotlights.Count; ++i)
            {
                int direction = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].direction");
                int position = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].position");
                int color = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].color");
                int strength = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].strength");
                int angle = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].angle");
                GL.Uniform3(position, spotlights[i].position.Xyz);
                GL.Uniform3(direction, Vector3.Normalize(spotlights[i].direction.Xyz));
                GL.Uniform3(color, spotlights[i].color);
                GL.Uniform1(strength, spotlights[i].strength);
                GL.Uniform1(angle, spotlights[i].angle);
                depthmaps++;
            }
            location = GL.GetUniformLocation(shader.programID, "spotlightCount");
            GL.Uniform1(location, spotlights.Count);

            if (normalMap != null)
            {
                int normalLoc = GL.GetUniformLocation(shader.programID, "normalMap");
                GL.Uniform1(normalLoc, 1);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, normalMap.id);
            }

            if (cubemap != null)
            {
                int cubeLoc = GL.GetUniformLocation(shader.programID, "cubeMap");
                GL.Uniform1(cubeLoc, 2);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.TextureCubeMap, cubemap.id);
            }


            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.id);

            GL.Uniform1(GL.GetUniformLocation(shader.programID, "intensity"), intensity);

            // enable position, normal and uv attributes
            GL.EnableVertexAttribArray( shader.attribute_vpos );
			GL.EnableVertexAttribArray( shader.attribute_vnrm );
			GL.EnableVertexAttribArray( shader.attribute_vuvs );
			GL.EnableVertexAttribArray( shader.attribute_vtan );
			GL.EnableVertexAttribArray( shader.attribute_vbit );

			// bind interleaved vertex data
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBufferId );
			GL.InterleavedArrays( InterleavedArrayFormat.T2fN3fV3f, Marshal.SizeOf( typeof( ObjVertex ) ), IntPtr.Zero );

			// link vertex attributes to shader parameters 
			GL.VertexAttribPointer( shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 56, 0 );
			GL.VertexAttribPointer( shader.attribute_vnrm, 3, VertexAttribPointerType.Float, true, 56, 2 * 4 );
			GL.VertexAttribPointer( shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 56, 5 * 4 );
            GL.VertexAttribPointer(shader.attribute_vtan, 3, VertexAttribPointerType.Float, false, 56, 8 * 4);
            GL.VertexAttribPointer(shader.attribute_vbit, 3, VertexAttribPointerType.Float, false, 56, 11 * 4);


			// bind triangle index data and render
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, triangleBufferId );
			GL.DrawArrays( PrimitiveType.Triangles, 0, triangles.Length * 3 );

			// bind quad index data and render
			if( quads.Length > 0 )
			{
				GL.BindBuffer( BufferTarget.ElementArrayBuffer, quadBufferId );
				GL.DrawArrays( PrimitiveType.Quads, 0, quads.Length * 4 );
			}

			// restore previous OpenGL state
			GL.UseProgram( 0 );
			GL.PopClientAttrib();
		}
        public void Render(Shader shader, Matrix4 transform, Matrix4 camera, Matrix4 cameraPosition, Texture texture, List<Pointlight> pointlights, List<DirectionalLight> directionalLights, List<Spotlight> spotlights)
        {
            // on first run, prepare buffers
            Prepare(shader);

            // safety dance
            GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);


            // enable texture
            int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
            GL.Uniform1(texLoc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.id);



            // enable shader
            GL.UseProgram(shader.programID);

            // pass transform to vertex shader
            int tr = GL.GetUniformLocation(shader.programID, "transform");
            GL.UniformMatrix4(tr, false, ref transform);
            GL.UniformMatrix4(shader.uniform_mview, false, ref camera);
            GL.Uniform4(GL.GetUniformLocation(shader.programID, "viewPos"), cameraPosition.Row3);

            //Directional lights
            for (int i = 0; i < directionalLights.Count; ++i)
            {
                int direction = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].direction");
                int color = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].color");
                int strength = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].strength");
                GL.Uniform3(direction, Vector3.Normalize(new Vector3(-directionalLights[i].direction.X, directionalLights[i].direction.Y, directionalLights[i].direction.Z)));
                GL.Uniform3(color, directionalLights[i].color);
                GL.Uniform1(strength, directionalLights[i].strength);
            }
            int location = GL.GetUniformLocation(shader.programID, "directionalLightCount");
            GL.Uniform1(location, directionalLights.Count);

            //Pointlights
            for (int i = 0; i < pointlights.Count; ++i)
            {
                int position = GL.GetUniformLocation(shader.programID, "pointlights[" + i + "].position");
                int color = GL.GetUniformLocation(shader.programID, "pointlights[" + i + "].color");
                int strength = GL.GetUniformLocation(shader.programID, "pointlights[" + i + "].strength");
                GL.Uniform3(position, pointlights[i].position.Xyz);
                GL.Uniform3(color, pointlights[i].color);
                GL.Uniform1(strength, pointlights[i].strength);
            }
            location = GL.GetUniformLocation(shader.programID, "pointlightCount");
            GL.Uniform1(location, pointlights.Count);

            //Spotlights
            for (int i = 0; i < spotlights.Count; ++i)
            {
                int direction = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].direction");
                int position = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].position");
                int color = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].color");
                int strength = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].strength");
                int angle = GL.GetUniformLocation(shader.programID, "spotlights[" + i + "].angle");
                GL.Uniform3(position, spotlights[i].position.Xyz);
                GL.Uniform3(direction, Vector3.Normalize(spotlights[i].direction.Xyz));
                GL.Uniform3(color, spotlights[i].color);
                GL.Uniform1(strength, spotlights[i].strength);
                GL.Uniform1(angle, spotlights[i].angle);
            }
            location = GL.GetUniformLocation(shader.programID, "spotlightCount");
            GL.Uniform1(location, spotlights.Count);


            // enable position, normal and uv attributes
            GL.EnableVertexAttribArray(shader.attribute_vpos);
            GL.EnableVertexAttribArray(shader.attribute_vnrm);
            GL.EnableVertexAttribArray(shader.attribute_vuvs);

            // bind interleaved vertex data
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);
            GL.InterleavedArrays(InterleavedArrayFormat.T2fN3fV3f, Marshal.SizeOf(typeof(ObjVertex)), IntPtr.Zero);

            // link vertex attributes to shader parameters 
            GL.VertexAttribPointer(shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 56, 0);
            GL.VertexAttribPointer(shader.attribute_vnrm, 3, VertexAttribPointerType.Float, true, 56, 2 * 4);
            GL.VertexAttribPointer(shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 56, 5 * 4);


            // bind triangle index data and render
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, triangleBufferId);
            GL.DrawArrays(PrimitiveType.Triangles, 0, triangles.Length * 3);

            // bind quad index data and render
            if (quads.Length > 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, quadBufferId);
                GL.DrawArrays(PrimitiveType.Quads, 0, quads.Length * 4);
            }

            // restore previous OpenGL state
            GL.UseProgram(0);
            GL.PopClientAttrib();
        }

        // layout of a single vertex
        [StructLayout( LayoutKind.Sequential )]
		public struct ObjVertex
		{
			public Vector2 TexCoord;
			public Vector3 Normal;
			public Vector3 Vertex;
            public Vector3 Tangent;
            public Vector3 Bitangent;
		}

		// layout of a single triangle
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjTriangle
		{
			public int Index0, Index1, Index2;
		}

		// layout of a single quad
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjQuad
		{
			public int Index0, Index1, Index2, Index3;
		}
	}
}