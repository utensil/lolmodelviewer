

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
// Abrstraction to read .dds files.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using Tao.DevIl;

using CSharpLogger;

using RAFlibPlus;

namespace LOLFileReader
{
    public class DDSReader
    {
        /// <summary>
        /// Read in binary .dds file from RAF.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="data">The contents of the file are stored in here.</param>
        /// <returns></returns>
        public static bool Read(IFileEntry file, ref Bitmap bitmap, Logger logger)
        {
            bool result = true;

            logger.Event("Reading dds: " + file.FileName);

            try
            {
                // Create image.
                int[] images = new int[1];
                Il.ilGenImages(1, images);

                // Bind image.
                Il.ilBindImage(images[0]);

                // Load the image data into DevIL.
                byte[] data = file.GetContent();
                result = Il.ilLoadL(Il.IL_DDS, data, data.Length);
                if (result == true)
                {
                    int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
                    int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT); ;

                    // Create the bitmap.
                    bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    Rectangle rect = new Rectangle(0, 0, width, height);

                    // Store the DevIL image data into the bitmap.
                    BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                    Il.ilConvertImage(Il.IL_BGRA, Il.IL_UNSIGNED_BYTE);
                    Il.ilCopyPixels(0, 0, 0, width, height, 1, Il.IL_BGRA, Il.IL_UNSIGNED_BYTE, bitmapData.Scan0);

                    bitmap.UnlockBits(bitmapData);
                }

                // Free image.
                Il.ilDeleteImages(1, images);

                if (result == false)
                {
                    throw new System.Exception("Unable to load image data.");
                }
            }
            catch(Exception e)
            {
                logger.Error("Unable to open dds file: " + file.FileName);
                logger.Error(e.Message);
                result = false;
            }

            return result;
        }
    }
}
