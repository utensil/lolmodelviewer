

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
// Abstraction to read .skl files.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using CSharpLogger;

using RAFlibPlus;

namespace LOLFileReader
{
    public class SKLReader
    {
        public static bool Read(IFileEntry file, ref SKLFile data, Logger logger)
        {
            bool result = true;

            logger.Event("Reading skl: " + file.FileName);

            try
            {
                // Get the data from the archive
                MemoryStream myInput = new MemoryStream(file.GetContent());
                result = ReadBinary(myInput, ref data, logger);
                myInput.Close();
            }
            catch (Exception e)
            {
                logger.Error("Unable to open memory stream: " + file.FileName);
                logger.Error(e.Message);
                result = false;
            }

            return result;
        }

        
        //Helper Functions. 
        //(Because nested Try/Catch looks nasty in one function block.) 
        private static bool ReadBinary(MemoryStream input, ref SKLFile data, Logger logger)
        {
            bool result = true;

            try
            {
                BinaryReader myFile = new BinaryReader(input, Encoding.ASCII);
                result = ReadData(myFile, ref data, logger);
                myFile.Close();
            }
            catch (Exception e)
            {
                logger.Error("Unable to open binary reader.");
                logger.Error(e.Message);
                result = false;
            }

            return result;
        }

        private static bool ReadData(BinaryReader file, ref SKLFile data, Logger logger)
        {
            bool result = true;

            try
            {
                // File Header Information.
                data.id = new String(file.ReadChars(SKLFile.ID_SIZE));
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
                        float[] orientation = new float[SKLBone.ORIENTATION_SIZE];
                        for (int j = 0; j < SKLBone.ORIENTATION_SIZE; ++j)
                        {
                            orientation[j] = file.ReadSingle();
                        }

                        bone.orientation = orientation;

                        // Position from matrix.
                        bone.position[0] = orientation[3];
                        bone.position[1] = orientation[7];
                        bone.position[2] = orientation[11];

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

                    data.numBones = (uint)file.ReadInt16();

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

                        zero = file.ReadInt16(); // ?
                        bone.ID = file.ReadInt16();
                        bone.parentID = file.ReadInt16();
                        unknown = file.ReadInt16(); // ?

                        int namehash = file.ReadInt32();

                        float twoPointOne = file.ReadSingle();

                        bone.position[0] = file.ReadSingle();
                        bone.position[1] = file.ReadSingle();
                        bone.position[2] = file.ReadSingle();

                        float one = file.ReadSingle(); // ? Maybe scales for X, Y, and Z
                        one = file.ReadSingle();
                        one = file.ReadSingle();

                        bone.orientation[0] = file.ReadSingle();
                        bone.orientation[1] = file.ReadSingle();
                        bone.orientation[2] = file.ReadSingle();
                        bone.orientation[3] = file.ReadSingle();

                        float ctx = file.ReadSingle(); // ctx
                        float cty = file.ReadSingle(); // cty
                        float ctz = file.ReadSingle(); // ctz

                        data.bones.Add(bone);

                        // The rest of the bone data is unknown. Maybe padding?
                        file.BaseStream.Position += 32;
                    }

                    file.BaseStream.Position = offset1;
                    for (int i = 0; i < data.numBones; ++i) // Inds for version 4 animation.
                    {
                        // 8 bytes
                        uint sklID = file.ReadUInt32();
                        uint anmID = file.ReadUInt32();

                        data.boneIDMap[anmID] = sklID;
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
                        while (name.Contains('\0') == false)
                        {
                            name += new string(file.ReadChars(4));
                        }
                        name = RemoveBoneNamePadding(name);
                        name = name.ToLower();

                        data.bones[i].name = name;
                    }
                }
                // Unknown Version
                else
                {
                    logger.Error("Unknown skl version: " + data.version);
                    result = false;
                }
            }
            catch (Exception e)
            {
                logger.Error("Skl reading error.");
                logger.Error(e.Message);
                result = false;
            }

            logger.Event("File ID: " + data.id);
            logger.Event("Version: " + data.version);
            logger.Event("Designer ID: " + data.designerID);
            logger.Event("Number of Bones: " + data.numBones);
            logger.Event("Number of Bone IDs: " + data.numBoneIDs);

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
