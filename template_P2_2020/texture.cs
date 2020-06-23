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
        int width, height;

		// constructor
		public Texture( string filename, bool repeat )
		{
			if( String.IsNullOrEmpty( filename ) ) throw new ArgumentException( filename );
			id = GL.GenTexture();
			GL.BindTexture( TextureTarget.Texture2D, id );
			// We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
			// We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
			// mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
			GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );
            if (repeat)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            }
			Bitmap bmp = new Bitmap( filename );
            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
			BitmapData bmp_data = bmp.LockBits( new Rectangle( 0, 0, bmp.Width, bmp.Height ), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
			GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0 );
            width = bmp.Width;
            height = bmp.Height;
            bmp.UnlockBits( bmp_data );
		}
	}
}
