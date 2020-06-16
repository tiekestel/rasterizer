using System.IO;
using System.Collections.Generic;
using OpenTK;
using System.Runtime.InteropServices;

namespace Template
{
	// mesh and loader based on work by JTalton; http://www.opentk.com/node/642

	public class MeshLoader
	{
		public bool Load( Mesh mesh, string fileName )
		{
			try
			{
				using( StreamReader streamReader = new StreamReader( fileName ) )
				{
					Load( mesh, streamReader );
					streamReader.Close();
					return true;
				}
			}
			catch { return false; }
		}

		char[] splitCharacters = new char[] { ' ' };

		List<Vector3> vertices;
		List<Vector3> normals;
		List<Vector2> texCoords;
		List<Mesh.ObjVertex> objVertices;
		List<Mesh.ObjTriangle> objTriangles;
		List<Mesh.ObjQuad> objQuads;


        struct SVertex
        {
            public Vector2 TexCoord;
            public Vector3 Normal;
            public Vector3 Vertex;
        }

		void Load( Mesh mesh, TextReader textReader )
		{
			vertices = new List<Vector3>();
			normals = new List<Vector3>();
			texCoords = new List<Vector2>();
			objVertices = new List<Mesh.ObjVertex>();
			objTriangles = new List<Mesh.ObjTriangle>();
			objQuads = new List<Mesh.ObjQuad>();
			string line;
			while( (line = textReader.ReadLine()) != null )
			{
				line = line.Trim( splitCharacters );
				line = line.Replace( "  ", " " );
				string[] parameters = line.Split( splitCharacters );
				switch( parameters[0] )
				{
				case "p": // point
					break;
				case "v": // vertex
					float x = float.Parse( parameters[1] );
					float y = float.Parse( parameters[2] );
					float z = float.Parse( parameters[3] );
					vertices.Add( new Vector3( x, y, z ) );
					break;
				case "vt": // texCoord
					float u = float.Parse( parameters[1] );
					float v = float.Parse( parameters[2] );
					texCoords.Add( new Vector2( u, v ) );
					break;
				case "vn": // normal
					float nx = float.Parse( parameters[1] );
					float ny = float.Parse( parameters[2] );
					float nz = float.Parse( parameters[3] );
					normals.Add( new Vector3( nx, ny, nz ) );
					break;
				case "f":
					switch( parameters.Length )
					{
					case 4:
						Mesh.ObjTriangle objTriangle = new Mesh.ObjTriangle();
						objTriangle.Index0 = ParseFaceParameter( parameters[1] );
						objTriangle.Index1 = ParseFaceParameter( parameters[2] );
						objTriangle.Index2 = ParseFaceParameter( parameters[3] );
						objTriangles.Add( objTriangle );
						break;
					case 5:
						Mesh.ObjQuad objQuad = new Mesh.ObjQuad();
						objQuad.Index0 = ParseFaceParameter( parameters[1] );
						objQuad.Index1 = ParseFaceParameter( parameters[2] );
						objQuad.Index2 = ParseFaceParameter( parameters[3] );
						objQuad.Index3 = ParseFaceParameter( parameters[4] );
						objQuads.Add( objQuad );
						break;
					}
					break;
				}
			}
            foreach(Mesh.ObjTriangle t in objTriangles)
            {
                SetTBN(t.Index0, t.Index1, t.Index2);
            }
			mesh.vertices = objVertices.ToArray();
			mesh.triangles = objTriangles.ToArray();
			mesh.quads = objQuads.ToArray();
			vertices = null;
			normals = null;
			texCoords = null;
			objVertices = null;
			objTriangles = null;
			objQuads = null;
		}

		char[] faceParamaterSplitter = new char[] { '/' };
		
        int ParseFaceParameter( string faceParameter )
		{
			Vector3 vertex = new Vector3();
			Vector2 texCoord = new Vector2();
			Vector3 normal = new Vector3();
			string[] parameters = faceParameter.Split( faceParamaterSplitter );
			int vertexIndex = int.Parse( parameters[0] );
			if( vertexIndex < 0 ) vertexIndex = vertices.Count + vertexIndex;
			else vertexIndex = vertexIndex - 1;
			vertex = vertices[vertexIndex];
			if( parameters.Length > 1 ) if( parameters[1] != "" )
				{
					int texCoordIndex = int.Parse( parameters[1] );
					if( texCoordIndex < 0 ) texCoordIndex = texCoords.Count + texCoordIndex;
					else texCoordIndex = texCoordIndex - 1;
					texCoord = texCoords[texCoordIndex];
				}
			if( parameters.Length > 2 )
			{
				int normalIndex = int.Parse( parameters[2] );
				if( normalIndex < 0 ) normalIndex = normals.Count + normalIndex;
				else normalIndex = normalIndex - 1;
				normal = normals[normalIndex];
			}
			return AddObjVertex( ref vertex, ref texCoord, ref normal );
		}

		int AddObjVertex( ref Vector3 vertex, ref Vector2 texCoord, ref Vector3 normal )
		{
			Mesh.ObjVertex newObjVertex = new Mesh.ObjVertex();
			newObjVertex.Vertex = vertex;
			newObjVertex.TexCoord = texCoord;
			newObjVertex.Normal = normal;
			objVertices.Add( newObjVertex );
			return objVertices.Count - 1;
		}

        void SetTBN(int vertex1, int vertex2, int vertex3)
        {
            Mesh.ObjVertex v1 = objVertices[vertex1];
            Mesh.ObjVertex v2 = objVertices[vertex2];
            Mesh.ObjVertex v3 = objVertices[vertex3];
            Vector2 dUV1 = v2.TexCoord - v1.TexCoord;
            Vector2 dUV2 = v3.TexCoord - v1.TexCoord;
            Vector3 dPos1 = v2.Vertex - v1.Vertex;
            Vector3 dPos2 = v3.Vertex - v1.Vertex;

            float r = 1 / (dUV1.X * dUV2.Y - dUV1.Y * dUV2.X);
            Vector3 tangent = (dPos1 * dUV2.Y - dPos2 * dUV1.Y) * r;
            Vector3 bitangent = (dPos2 * dUV1.X - dPos1 * dUV2.X) * r;
            v1.Tangent = tangent;
            v1.Bitangent = bitangent;
            v2.Tangent = tangent;
            v2.Bitangent = bitangent;
            v3.Tangent = tangent;
            v3.Bitangent = bitangent;

            objVertices[vertex1] = v1;
            objVertices[vertex2] = v2;
            objVertices[vertex3] = v3;
        }
	}
}
