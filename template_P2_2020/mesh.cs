﻿using System;
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
		public void Render( Shader shader, Matrix4 transform, Texture texture, Texture normalMap, List<Pointlight> pointlights, List<DirectionalLight> directionalLights, List<Spotlight> spotlights )
		{
			// on first run, prepare buffers
			Prepare( shader );

			// safety dance
			GL.PushClientAttrib( ClientAttribMask.ClientVertexArrayBit );

			// enable texture
			int texLoc = GL.GetUniformLocation( shader.programID, "pixels" );
			GL.Uniform1( texLoc, 0 );
			GL.ActiveTexture( TextureUnit.Texture0 );
			GL.BindTexture( TextureTarget.Texture2D, texture.id );

            // enable normal map

            if(normalMap != null)
            {
                int normalLoc = GL.GetUniformLocation(shader.programID, "normalMap");
                GL.Uniform1(normalLoc, 1);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, normalMap.id);
            }

            // enable shader
            GL.UseProgram( shader.programID );

            // pass transform to vertex shader
			GL.UniformMatrix4( shader.uniform_mview, false, ref transform );

            int isNormal = GL.GetUniformLocation(shader.programID, "isNormalMap");
            if(normalMap != null)
            {
                GL.Uniform1(isNormal, 1);
            }
            else
            {
                GL.Uniform1(isNormal, 0);
            }

            //Directional lights
            for (int i = 0; i < directionalLights.Count; ++i)
            {
                int direction = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].direction");
                int color = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].color");
                int strength = GL.GetUniformLocation(shader.programID, "directionalLights[" + i + "].strength");
                GL.Uniform3(direction, Vector3.Normalize(directionalLights[i].finalDirection.Xyz));
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
                GL.Uniform3(position, pointlights[i].finalPostion.Xyz);
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
                GL.Uniform3(position, spotlights[i].finalPosition.Xyz);
                GL.Uniform3(direction, Vector3.Normalize(spotlights[i].finalDirection.Xyz));
                GL.Uniform3(color, spotlights[i].color);
                GL.Uniform1(strength, spotlights[i].strength);
                GL.Uniform1(angle, spotlights[i].angle);
            }
            location = GL.GetUniformLocation(shader.programID, "spotlightCount");
            GL.Uniform1(location, spotlights.Count);


            // enable position, normal and uv attributes
            GL.EnableVertexAttribArray( shader.attribute_vpos );
			GL.EnableVertexAttribArray( shader.attribute_vnrm );
			GL.EnableVertexAttribArray( shader.attribute_vuvs );

			// bind interleaved vertex data
			GL.EnableClientState( ArrayCap.VertexArray );
			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBufferId );
			GL.InterleavedArrays( InterleavedArrayFormat.T2fN3fV3f, Marshal.SizeOf( typeof( ObjVertex ) ), IntPtr.Zero );

			// link vertex attributes to shader parameters 
			GL.VertexAttribPointer( shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 32, 0 );
			GL.VertexAttribPointer( shader.attribute_vnrm, 3, VertexAttribPointerType.Float, true, 32, 2 * 4 );
			GL.VertexAttribPointer( shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 32, 5 * 4 );

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

		// layout of a single vertex
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjVertex
		{
			public Vector2 TexCoord;
			public Vector3 Normal;
			public Vector3 Vertex;
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