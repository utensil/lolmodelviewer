
/*
LOLViewer
Copyright 2011 James Lammlein 

 

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
using RAFLib;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using System.Drawing;
using System.Drawing.Imaging;

namespace LOLViewer
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

        public int textureBuffer;
        private TextureTarget target;

        public GLTexture()
        {
            textureBuffer = 0;
        }

        /// <summary>
        /// Load a texture from file. Supports standard .NET image types 
        /// and .DDS only.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool Create(FileInfo f, TextureTarget target, SupportedImageEncodings encoding)
        {
            bool result = true;

            this.target = target;

            // Hacky sanity check
            if( System.IO.File.Exists(f.FullName) == false )
            {
                return false;
            }

            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // Check file type
            if (encoding != SupportedImageEncodings.DDS)
            {
                // Normal file type.
                // All OpenGL buffers are created in the helper function below.
                result = CreateDotNetTexture(f, target);
            }
            else
            {
                // DDS Compression.
                // All OpenGL buffers are created internally in the OpenTK
                // Utility class.
                result = CreateDDSTexture(f, target);
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

        /// <summary>
        /// Load texture from RAF.
        /// </summary>
        /// <param name="f">The RAF file entry of the texture.</param>
        /// <param name="target">Only supports 2D.</param>
        /// <param name="encoding">Only supports DDS.</param>
        /// <returns></returns>
        public bool Create( RAFFileListEntry f, TextureTarget target, SupportedImageEncodings encoding)
        {
            bool result = true;

            this.target = target;

            // Hacky sanity check
            if (target != TextureTarget.Texture2D &&
                encoding != SupportedImageEncodings.DDS)
            {
                return false;
            }

            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // Only doing DDS at the moment.
            // I don't really have a need at the moment to load
            // other textures from RAF.
            result = CreateDDSTexture(f, target);

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

        //
        // Helper Functions
        //
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

        private bool CreateDotNetTexture(FileInfo f, TextureTarget target)
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
                // File is a normal .NET encoding
                try
                {
                    // Read the texture from file.
                    Bitmap bitmap = new Bitmap(f.FullName);
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

        private bool CreateDDSTexture(FileInfo f, TextureTarget target)
        {
            bool result = true;

            TextureTarget dimension;
            uint handle;
            TextureLoaders.ImageDDS.LoadFromDisk(f.FullName, out handle,
                out dimension);

            if (handle > 0)
            {
                textureBuffer = (int)handle;
            }
            else
            {
                result = false;
            }

            return result;
        }

        private bool CreateDDSTexture(RAFFileListEntry f, TextureTarget target)
        {
            bool result = true;

            TextureTarget dimension;
            uint handle;
            TextureLoaders.ImageDDS.LoadFromMemory(f.GetContent(), out handle,
                out dimension);

            if (handle > 0)
            {
                textureBuffer = (int)handle;
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}
