using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	public class Texture
	{
		// data members
		public int id;

		// constructor
		public Texture( string filename )
		{
			if( String.IsNullOrEmpty( filename ) ) throw new ArgumentException( filename );
			id = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, id );
			// We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
			// We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
			// mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );
			Bitmap bmp = new Bitmap( filename );
			BitmapData bmp_data = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0 );
			bmp.UnlockBits( bmp_data );
		}

        public void setHDR(float intensity)
        {
            int framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

            int newid = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, newid);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, newid, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                Console.WriteLine("hdrbuffer not set up correctly");


            float[] vertices = new float[] {
                -1f, -1f,
                1f, -1f,
                -1f, 1f,
                -1f, 1f,
                1f, -1f,
                1f, 1
            };
            float[] texCoords = new float[]
            {
                0, 0,
                1f, 0,
                0, 1f,
                0, 1f,
                1f, 0,
                1f, 1f
            };

            int quadVertexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVertexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);
            int quadTexbuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadTexbuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Length * sizeof(float), texCoords, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);

            int width, height;
            GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureWidth, out width);
            GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureHeight, out height);
            GL.Viewport(0, 0, width, height);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.UseProgram(programValues.hdrtargetshader.programID);

            GL.BindVertexArray(quadVertexbuffer);
            GL.BindVertexArray(quadTexbuffer);
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.UseProgram(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);

            GL.UseProgram(0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, programValues.screenwidth, programValues.screenheight);

            id = newid;
        }
	}
}
