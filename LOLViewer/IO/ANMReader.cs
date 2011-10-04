

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
// Abrstraction to read .anm files.
//



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using OpenTK;
using RAFLib;

namespace LOLViewer.IO
{
    class ANMReader
    {
        /// <summary>
        /// Reads in a binary .anm file from disc.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="data">The contents of the file are stored in here.</param>
        /// <returns></returns>
        public static bool Read(FileInfo file, ref ANMFile data)
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

        /// <summary>
        /// Read in binary .anm file from RAF.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="data">The contents of the file are stored in here.</param>
        /// <returns></returns>
        public static bool Read(RAFFileListEntry file, ref ANMFile data)
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

        private static bool ReadBinary(FileStream input, ref ANMFile data)
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

        private static bool ReadBinary(MemoryStream input, ref ANMFile data)
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

        private static bool ReadData(BinaryReader file, ref ANMFile data)
        {
            bool result = true;

            try
            {
                // File Header Information.
                data.magicOne = file.ReadUInt32();
                data.magicTwo = file.ReadUInt32();

                data.version = file.ReadUInt32();

                data.magicThree = file.ReadUInt32();

                data.numberOfBones = file.ReadUInt32();
                data.numberOfFrames = file.ReadUInt32();

                data.magicFour = file.ReadUInt32();

                // Read in all the bones
                for (UInt32 i = 0; i < data.numberOfBones; ++i)
                {
                    ANMBone bone = new ANMBone();
                    bone.name = new String(file.ReadChars(ANMBone.BONE_NAME_LENGTH));
                    bone.flag = file.ReadUInt32();

                    // For each bone, read in its value at each frame in the animation.
                    for (UInt32 j = 0; j < data.numberOfFrames; ++j)
                    {
                        ANMFrame frame = new ANMFrame();

                        // Read in the frame's quaternion.
                        float x = file.ReadSingle();
                        float y = file.ReadSingle();
                        float z = file.ReadSingle();
                        float w = file.ReadSingle();
                        frame.orientation = new Quaternion(x, y, z, w);

                        // Read in the frame's position.
                        x = file.ReadSingle();
                        y = file.ReadSingle();
                        z = file.ReadSingle();
                        frame.position = new Vector3(x, y, z);

                        bone.frames.Add(frame);
                    }

                    data.bones.Add(bone);
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
