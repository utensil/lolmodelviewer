
/*
LOLViewer
Copyright 2011-2012 James Lammlein, Adrian Astley 

 

This file is part of LOLViewer.

LOLViewer is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

LOLViewer is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with LOLViewer.  If not, see <http://www.gnu.org/licenses/>.

*/

//
// Abstracts the funtions to load a texture into openGL
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using RAFlibPlus;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using LOLFileReader;

using CSharpLogger;

namespace LOLViewer.Graphics
{
    class GLTexture
    {
        public enum SupportedImageEncodings
        {
            BMP,
            GIF,
            JPEG,
            PNG,
            TIFF,
            DDS
        };

        private int textureBuffer;
        private TextureTarget target;

        public GLTexture()
        {
            textureBuffer = 0;
        }

        /// <summary>
        /// Load texture from RAF.
        /// </summary>
        /// <param name="file">The file entry of the texture.</param>
        /// <param name="target">Only supports 2D.</param>
        /// <returns></returns>
        public bool Create(IFileEntry file, TextureTarget target, SupportedImageEncodings encoding, Logger logger)
        {
            bool result = true;

            this.target = target;

            // Hacky sanity check
            if (target != TextureTarget.Texture2D)
            {
                return false;
            }

            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // Create a System.Drawing.Bitmap.
            Bitmap bitmap = null;
            if (encoding == SupportedImageEncodings.DDS)
            {
                // Special case.
                result = DDSReader.Read(file, ref bitmap, logger);
            }
            else
            {
                // Normal case.
                result = CreateBitmap(file.GetContent(), ref bitmap);
            }

            // Pass the Bitmap into OpenGL.
            if (result == true &&
                bitmap != null)
            {
                result = CreateTexture(bitmap, target);
            }

            if (result == true)
            {
                // Set up texture parameters.
                GL.TexEnv(TextureEnvTarget.TextureEnv,
                    TextureEnvParameter.TextureEnvMode,
                    (Int32)TextureEnvModeCombine.Modulate);

                GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMinFilter,
                    (Int32)TextureMinFilter.Linear);

                GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMagFilter,
                    (Int32)TextureMagFilter.Linear);

                GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapS,
                    (Int32)TextureWrapMode.Repeat);

                GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapT,
                    (Int32)TextureWrapMode.Repeat);
            }

            return result;
        }

        public void Destroy()
        {
            if (textureBuffer != 0)
            {
                GL.DeleteTextures(1, ref textureBuffer);
            }
        }

        public bool Bind()
        {
            bool result = true;

            // Bind the texture.
            if (textureBuffer != 0)
            {
                GL.BindTexture(target, textureBuffer);
            }

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            return result;
        }

        //
        // Helper Functions
        //

        private bool CreateBitmap(byte[] data, ref Bitmap bitmap)
        {
            bool result = true;

            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(data);
                bitmap = new Bitmap(stream);
            }
            catch
            {
                result = false;
            }

            if (stream != null)
            {
                stream.Close();
            }

            return result;
        }

        private bool CreateTexture(Bitmap bitmap, TextureTarget target)
        {
            bool result = true;

            // Create the texture buffer.
            if (result == true)
            {
                GL.GenTextures(1, out textureBuffer);
            }

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Bind the texture.
            if (result == true)
            {
                result = Bind();
            }

            // Set the texture data.
            if (result == true)
            {
                try
                {
                    // Get the pixel data from the bitmap.
                    BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0,
                        bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    // Load it into OpenGL
                    GL.TexImage2D(TextureTarget.Texture2D, 0,
                        PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                        PixelType.UnsignedByte, data.Scan0);

                    // Release it
                    bitmap.UnlockBits(data);
                }
                catch
                {
                    result = false;
                }
            }

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            return result;
        }
    }
}
