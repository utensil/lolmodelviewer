

/*
LOLViewer
Copyright 2011-2012 James Lammlein 

 

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
using System.Windows.Forms;
using OpenTK;
using RAFlibPlus;

namespace LOLViewer
{
    class SKLReader
    {
        public static bool Read(RAFFileListEntry file, ref SKLFile data)
        {
            bool result = true;
            
            try
            {
                // Get the data from the archive
                MemoryStream myInput = new MemoryStream( file.GetContent() );
                result = ReadBinary(myInput, ref data);
                myInput.Close();
            }
            catch
            {
                result = false;
            }

            return result;
        }

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

                if (data.version == 1 || data.version == 2)
                {
                    data.designerID = file.ReadUInt32();

                    // Read in the bones.
                    data.numBones = file.ReadUInt32();
                    for (int i = 0; i < data.numBones; ++i)
                    {
                        SKLBone bone = new SKLBone();

                        bone.name = new String(
                            file.ReadChars(SKLBone.BONE_NAME_SIZE));
                        bone.name = RemoveBoneNamePadding(bone.name);
                        bone.name = bone.name.ToLower();

                        bone.ID = i;
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
                // Newest version so far.
                else if (data.version == 0)
                {
                    // Header
                    Int16 zero = file.ReadInt16(); // ?

                    data.numBones = (uint) file.ReadInt16();

                    data.numBoneIDs = file.ReadUInt32();
                    Int16 offsetToVertexData = file.ReadInt16(); // Should be 64.

                    int unknown = file.ReadInt16(); // ?

                    int offset1 = file.ReadInt32();
                    int offsetToAnimationIndices = file.ReadInt32();
                    int offset2 = file.ReadInt32();
                    int offset3 = file.ReadInt32();
                    int offsetToStrings = file.ReadInt32();

                    // Not sure what this data represents.
                    // I think it's padding incase more header data is required later.
                    file.BaseStream.Position += 20;

                    file.BaseStream.Position = offsetToVertexData;
                    for (int i = 0; i < data.numBones; ++i)
                    {
                        SKLBone bone = new SKLBone();
                        // The old scale was always 0.1.
                        // For now, just go with it.
                        bone.scale = 0.1f;

                        zero            = file.ReadInt16(); // ?
                        bone.ID         = file.ReadInt16();
                        bone.parentID   = file.ReadInt16();
                        unknown         = file.ReadInt16(); // ?

                        int namehash = file.ReadInt32();

                        float twoPointOne = file.ReadSingle();

                        bone.position.X = file.ReadSingle();
                        bone.position.Y = file.ReadSingle();
                        bone.position.Z = file.ReadSingle();

                        float one = file.ReadSingle(); // ? Maybe scales for X, Y, and Z
                        one = file.ReadSingle();
                        one = file.ReadSingle();

                        bone.orientation.X = file.ReadSingle();
                        bone.orientation.Y = file.ReadSingle();
                        bone.orientation.Z = file.ReadSingle();
                        bone.orientation.W = file.ReadSingle();

                        float ctx = file.ReadSingle(); // ctx
                        float cty = file.ReadSingle(); // cty
                        float ctz = file.ReadSingle(); // ctz

                        data.bones.Add(bone);

                        // The rest of the bone data is unknown. Maybe padding?
                        file.BaseStream.Position += 32;
                    }

                    file.BaseStream.Position = offset1;
                    for (int i = 0; i < data.numBones; ++i) // ?
                    {
                        // 8 bytes
                        int valueOne = file.ReadInt32();
                        int valueTwo = file.ReadInt32();
                    }

                    file.BaseStream.Position = offsetToAnimationIndices;
                    for (int i = 0; i < data.numBoneIDs; ++i) // Inds for animation
                    {
                        // 2 bytes
                        UInt16 boneID = file.ReadUInt16();
                        data.boneIDs.Add(boneID);
                    }

                    file.BaseStream.Position = offsetToStrings;
                    for (int i = 0; i < data.numBones; ++i)
                    {
                        // bone names
                        string name = ""; 
                        while( name.Contains( '\0' ) == false )
                        {
                            name += new string(file.ReadChars(4));
                        }
                        name = RemoveBoneNamePadding(name);
                        name = name.ToLower();
                        
                        data.bones[i].name = name;
                    }

                    // Converting to original format.
                    for (int i = 0; i < data.numBones; ++i)
                    {
                        // Only update non root bones.
                        if (data.bones[i].parentID != -1)
                        {
                            // Determine the parent bone.
                            SKLBone parentBone = data.bones[ data.bones[i].parentID ];

                            // Append quaternions for rotation transform B * A
                            data.bones[i].orientation = parentBone.orientation * data.bones[i].orientation;

                            Vector3 localPosition = Vector3.Zero;
                            localPosition.X = data.bones[i].position.X;
                            localPosition.Y = data.bones[i].position.Y;
                            localPosition.Z = data.bones[i].position.Z;

                            data.bones[i].position = parentBone.position +
                                Vector3.Transform(localPosition, parentBone.orientation);
                        }
                    }
                }
                // Unknown Version
                else
                {
#if DEBUG
                    MessageBox.Show("New .skl version.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    result = false;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        //
        // Helper Functions
        //

        private static String RemoveBoneNamePadding(String s)
        {
            int position = s.IndexOf('\0');
            if (position >= 0)
            {
                s = s.Remove(position);
            }

            return s;
        }
    }
}
