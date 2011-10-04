

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
// Stores the contents of an .skn file.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLViewer
{
    class SKNFile
    {
        // File Header Data
        public int                 magic;  // ????? What does this do?????
        public short               version;
        public short               numObjects;

        // Some versions have material header information.
        public int                 numMaterialHeaders;
        public List<SKNMaterial>   materialHeaders;

        // Actual model information.
        public int                 numIndices;
        public int                 numVertices;
        public List<short>         indices;
        public List<SKNVertex>     vertices;

        // Contained in version two.
        public List<int>           endTab; // ???? 

        public SKNFile()
        {
            magic = version = numObjects = 0;

            numMaterialHeaders = 0;
            materialHeaders = new List<SKNMaterial>();

            numIndices = numVertices = 0;
            indices = new List<short>();
            vertices = new List<SKNVertex>();

            endTab = new List<int>();
        }

        /// <summary>
        /// Loads the data in the SKNFile class into
        /// an OpenGL model file.
        /// </summary>
        /// <param name="model">Where to put the data.</param>
        /// <param name="usingDDSTexture">The V coordinate in OpenGL
        /// is different from directX.  If you're using textures on
        /// this model which were intended to be used with directX, you 
        /// need to invert the V coordinate.</param>
        /// <returns></returns>
        public bool ToGLStaticModel(ref GLStaticModel model, bool usingDDSTexture)
        {
            bool result = true;

            List<float> vData = new List<float>();
            List<float> nData = new List<float>();
            List<float> tData = new List<float>();
            for (int i = 0; i < numVertices; ++i)
            {
                vData.Add(vertices[i].position.X);
                vData.Add(vertices[i].position.Y);
                vData.Add(vertices[i].position.Z);

                nData.Add(vertices[i].normal.X);
                nData.Add(vertices[i].normal.Y);
                nData.Add(vertices[i].normal.Z);

                tData.Add(vertices[i].texCoords.X);
                if (usingDDSTexture == false)
                {
                    tData.Add(vertices[i].texCoords.Y);
                }
                else
                {
                    // DDS Texture.
                    tData.Add(1.0f - vertices[i].texCoords.Y);
                }
            }

            List<uint> iData = new List<uint>();
            for (int i = 0; i < numIndices; ++i)
            {
                iData.Add((uint)indices[i]);
            }


            result = model.Create(vData, nData, tData, iData);

            return result;
        }
    }
}
