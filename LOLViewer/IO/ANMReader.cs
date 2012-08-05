

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
// Abrstraction to read .anm files.
//



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows.Forms;

using OpenTK;
using RAFlibPlus;

namespace LOLViewer.IO
{
    class ANMReader
    {
        /// <summary>
        /// Read in binary .anm file from RAF.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="data">The contents of the file are stored in here.</param>
        /// <returns></returns>
        public static bool Read(IFileEntry file, ref ANMFile data, EventLogger logger)
        {
            bool result = true;

            logger.LogEvent("Reading anm: " + file.FileName);

            try
            {
                // Get the data from the archive
                MemoryStream myInput = new MemoryStream( file.GetContent() );
                result = ReadBinary(myInput, ref data, logger);
                myInput.Close();
            }
            catch(Exception e)
            {
                logger.LogError("Unable to open memory stream: " + file.FileName);
                logger.LogError(e.Message);
                result = false;
            }

            return result;
        }

        //
        // Helper Functions. 
        // (Because nested Try/Catch looks nasty in one function block.)
        //

        private static bool ReadBinary(MemoryStream input, ref ANMFile data, EventLogger logger)
        {
            bool result = true;

            try
            {
                BinaryReader myFile = new BinaryReader(input);
                result = ReadData(myFile, ref data, logger);
                myFile.Close();
            }
            catch(Exception e)
            {
                logger.LogError("Unable to open binary reader.");
                logger.LogError(e.Message);
                result = false;
            }

            return result;
        }

        private static bool ReadData(BinaryReader file, ref ANMFile data, EventLogger logger)
        {
            bool result = true;

            try
            {
                // File Header Information.
                data.magicOne = file.ReadUInt32();
                data.magicTwo = file.ReadUInt32();

                data.version = file.ReadUInt32();

                data.magicThree = file.ReadUInt32();

                // Version 0, 1, 2, 3 Code
                if (data.version == 0 ||
                    data.version == 1 ||
                    data.version == 2 ||
                    data.version == 3)
                {
                    data.numberOfBones = file.ReadUInt32();
                    data.numberOfFrames = file.ReadUInt32();

                    data.playbackFPS = file.ReadUInt32();

                    // Read in all the bones
                    for (UInt32 i = 0; i < data.numberOfBones; ++i)
                    {
                        ANMBone bone = new ANMBone();
                        bone.name = new String(file.ReadChars(ANMBone.BONE_NAME_LENGTH));
                        bone.name = RemoveAnimationNamePadding(bone.name);
                        bone.name = bone.name.ToLower();

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
                // Version 4 Code
                else if (data.version == 4)
                {
                    //
                    // TODO: Still working on reverse engineering this.
                    // For now, just bail.
                    //

                    result = false;
                }
                // Unknown version
                else
                {
                    logger.LogError("Unknown anm version: " + data.version);
#if DEBUG 
                    MessageBox.Show("New .anm version.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    result = false; 
                }
            }
            catch(Exception e)
            {
                logger.LogError("Anm reading error.");
                logger.LogError(e.Message);
                result = false;
            }

            logger.LogEvent("Magic One: " + data.magicOne);
            logger.LogEvent("Magic Two: " + data.magicTwo);
            logger.LogEvent("Magic Three: " + data.magicThree);
            logger.LogEvent("Version: " + data.version);
            logger.LogEvent("Number of Bones: " + data.numberOfBones);
            logger.LogEvent("Number of Frames: " + data.numberOfFrames);
            logger.LogEvent("Playback FPS: " + data.playbackFPS);

            return result;
        }

        //
        // Helper Functions
        //

        private static String RemoveAnimationNamePadding(String s)
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

