

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
// Stores the contents of an .skl file.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLViewer
{
    class SKLFile
    {
        // Not sure what the first eight bytes represent
        public int              magicOne;
        public int              magicTwo;

        public uint             version;
        public uint             designerID;

        public uint             numBones;
        public List<SKLBone>    bones;

        public uint             numBoneIDs;
        public List<uint>       boneIDs;

        public SKLFile()
        {
            magicOne = magicTwo = 0;

            version = designerID = numBones = 0;
            bones = new List<SKLBone>();
            
            numBoneIDs = 0;
            boneIDs = new List<uint>();
        }

        /// <summary>
        /// Loads data from SKN and SKL files into
        /// an OpenGL rigged model.
        /// </summary>
        /// <param name="model">Where to store the data.</param>
        /// <param name="skn">The .skn data.</param>
        /// <param name="usingDDSTexture">The V coordinate in OpenGL
        /// is different from directX.  If you're using textures on
        /// this model which were intended to be used with directX, you 
        /// need to invert the V coordinate.</param>
        /// <returns></returns>
        public bool ToGLRiggedModel(ref GLRiggedModel model, SKNFile skn, bool usingDDSTexture)
        {
            bool result = true;

            List<float> vData = new List<float>();
            List<float> nData = new List<float>();
            List<float> tData = new List<float>();
            List<int> bData = new List<int>();
            List<float> wData = new List<float>();
            List<OpenTK.Matrix4> btData = new List<OpenTK.Matrix4>();
            for (int i = 0; i < skn.numVertices; ++i)
            {
                // Position Information
                vData.Add(skn.vertices[i].position.X);
                vData.Add(skn.vertices[i].position.Y);
                vData.Add(skn.vertices[i].position.Z);

                // Normal Information
                nData.Add(skn.vertices[i].normal.X);
                nData.Add(skn.vertices[i].normal.Y);
                nData.Add(skn.vertices[i].normal.Z);

                // Tex Coords Information
                tData.Add(skn.vertices[i].texCoords.X);
                if (usingDDSTexture == false)
                {
                    tData.Add(skn.vertices[i].texCoords.Y);
                }
                else
                {
                    // DDS Texture.
                    tData.Add(1.0f - skn.vertices[i].texCoords.Y);
                }

                // Bone Index Information
                for (int j = 0; j < SKNVertex.BONE_INDEX_SIZE; ++j)
                {
                    bData.Add(skn.vertices[i].boneIndex[j]);
                }

                // Bone Weight Information
                wData.Add(skn.vertices[i].weights.X);
                wData.Add(skn.vertices[i].weights.Y);
                wData.Add(skn.vertices[i].weights.Z);
                wData.Add(skn.vertices[i].weights.W);
            }

            // Bone Transform Information
            for (int i = 0; i < numBones; ++i)
            {
                btData.Add(bones[i].transform);
            }

            // Index Information
            List<uint> iData = new List<uint>();
            for (int i = 0; i < skn.numIndices; ++i)
            {
                iData.Add((uint)skn.indices[i]);
            }

            result = model.Create(vData, nData, tData, 
                bData, wData, iData, btData);

            return result;
        }
    }
}
