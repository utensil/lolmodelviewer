

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
// Abrstraction to read .skl files.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using OpenTK;
using RAFLib;

namespace LOLViewer
{
    class SKLReader
    {
        public static bool Read(FileInfo file, ref SKLFile data)
        {
            bool result = true;

            try
            {
                FileStream myInput = new FileStream(file.FullName, FileMode.Open);
                result = ReadBinary(myInput, ref data);
                myInput.Close();
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public static bool Read(RAFFileListEntry file, ref SKLFile data)
        {
            bool result = true;

            try
            {
                MemoryStream myInput = new MemoryStream(file.GetContent());
                result = ReadBinary(myInput, ref data);
                myInput.Close();
            }
            catch
            {
                result = false;
            }

            return result;
        }

        //
        // Helper Functions. 
        // (Because nested Try/Catch looks nasty in one function block.)
        //

        private static bool ReadBinary(FileStream input, ref SKLFile data)
        {
            bool result = true;

            try
            {
                BinaryReader myFile = new BinaryReader(input);
                result = ReadData(myFile, ref data);
                myFile.Close();
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private static bool ReadBinary(MemoryStream input, ref SKLFile data)
        {
            bool result = true;

            try
            {
                BinaryReader myFile = new BinaryReader(input);
                result = ReadData(myFile, ref data);
                myFile.Close();
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private static bool ReadData(BinaryReader file, ref SKLFile data)
        {
            bool result = true;

            try
            {
                // File Header Information.
                data.magicOne = file.ReadInt32();
                data.magicTwo = file.ReadInt32();

                data.version = file.ReadUInt32();
                data.designerID = file.ReadUInt32();

                if (data.version > 0)
                {
                    // Read in the bones.
                    data.numBones = file.ReadUInt32();
                    for (int i = 0; i < data.numBones; ++i)
                    {
                        SKLBone bone = new SKLBone();

                        bone.name = new String(
                            file.ReadChars(SKLBone.BONE_NAME_SIZE));

                        bone.parentID = file.ReadInt32();
                        bone.scale = file.ReadSingle();

                        // Read in transform matrix.
                        float[] matrix = new float[SKLBone.TRANSFORM_SIZE];
                        for (int j = 0; j < SKLBone.TRANSFORM_SIZE; ++j)
                        {
                            matrix[j] = file.ReadSingle();
                        }

                        Matrix4 orientationTransform = Matrix4.Identity;

                        orientationTransform.M11 = matrix[0]; //
                        orientationTransform.M21 = matrix[1]; // Column 1
                        orientationTransform.M31 = matrix[2]; //
                        
                        orientationTransform.M12 = matrix[4]; //
                        orientationTransform.M22 = matrix[5]; // Column 2
                        orientationTransform.M32 = matrix[6]; //
                        
                        orientationTransform.M13 = matrix[8]; //
                        orientationTransform.M23 = matrix[9]; // Column 3
                        orientationTransform.M33 = matrix[10]; //

                        bone.orientation = OpenTKExtras.Matrix4.CreateQuatFromMatrix(orientationTransform);

                        // Position from matrix.
                        Vector3 position = new Vector3(matrix[3], matrix[7], matrix[11]);
                        bone.position = position;

                        data.bones.Add(bone);
                    }
                }

                // Version two contains bone IDs.
                if (data.version == 2)
                {
                    data.numBoneIDs = file.ReadUInt32();
                    for (uint i = 0; i < data.numBoneIDs; ++i)
                    {
                        data.boneIDs.Add(file.ReadUInt32());
                    }
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}
