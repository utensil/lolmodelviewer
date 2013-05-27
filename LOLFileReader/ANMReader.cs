

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
// Abrstraction to read .anm files.
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
    public class ANMReader
    {
        /// <summary>
        /// Read in binary .anm file from RAF.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="data">The contents of the file are stored in here.</param>
        /// <returns></returns>
        public static bool Read(IFileEntry file, ref ANMFile data, Logger logger)
        {
            bool result = true;

            logger.Event("Reading anm: " + file.FileName);

            try
            {
                // Get the data from the archive
                MemoryStream myInput = new MemoryStream( file.GetContent() );
                result = ReadBinary(myInput, ref data, logger);
                myInput.Close();
            }
            catch(Exception e)
            {
                logger.Error("Unable to open memory stream: " + file.FileName);
                logger.Error(e.Message);
                result = false;
            }

            return result;
        }

        //
        // Helper Functions. 
        // (Because nested Try/Catch looks nasty in one function block.)
        //

        private static bool ReadBinary(MemoryStream input, ref ANMFile data, Logger logger)
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
                logger.Error("Unable to open binary reader.");
                logger.Error(e.Message);
                result = false;
            }

            return result;
        }

        private static bool ReadData(BinaryReader file, ref ANMFile data, Logger logger)
        {
            bool result = true;

            try
            {
                // File Header Information.
                data.id = new String(file.ReadChars(ANMFile.ID_SIZE));
                data.version = file.ReadUInt32();
                
                // Version 0, 1, 2, 3 Code
                if (data.version == 0 ||
                    data.version == 1 ||
                    data.version == 2 ||
                    data.version == 3)
                {
                    //
                    // Header information specific to these versions.
                    //
                    
                    data.magic = file.ReadUInt32();
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
                        
                        // Unknown
                        file.ReadUInt32();
                        
                        // For each bone, read in its value at each frame in the animation.
                        for (UInt32 j = 0; j < data.numberOfFrames; ++j)
                        {
                            ANMFrame frame = new ANMFrame();
                            
                            // Read in the frame's quaternion.
                            frame.orientation[0] = file.ReadSingle(); // x
                            frame.orientation[1] = file.ReadSingle(); // y
                            frame.orientation[2] = file.ReadSingle(); // z
                            frame.orientation[3] = file.ReadSingle(); // w
                            
                            // Read in the frame's position.
                            frame.position[0] = file.ReadSingle(); // x
                            frame.position[1] = file.ReadSingle(); // y 
                            frame.position[2] = file.ReadSingle(); // z
                            
                            bone.frames.Add(frame);
                        }
                        
                        data.bones.Add(bone);
                    }
                }
                // Version 4 Code
                else if (data.version == 4)
                {
                    //
                    // Based on the reverse engineering work of Hossein Ahmadi.
                    //
                    // In this version, position vectors and orientation quaternions are
                    // stored separately in sorted, keyed blocks.  The assumption is Riot
                    // is removing duplicate vectors and quaternions by using an indexing scheme
                    // to look up values.
                    //
                    // So, after the header, there are three data sections: a vector section, a quaternion
                    // section, and a look up section.  The number of vectors and quaternions
                    // may not match the expected value based on the number of frames and bones.  However, 
                    // the number of look ups should match this value and can be used to create the animation.
                    
                    //
                    // Header information specific to version 4.
                    //
                    
                    data.magic = file.ReadUInt32();
                    
                    // Not sure what any of these mean.
                    float unknown = file.ReadSingle();
                    unknown = file.ReadSingle();
                    unknown = file.ReadSingle();
                    
                    data.numberOfBones = file.ReadUInt32();
                    data.numberOfFrames = file.ReadUInt32();
                    
                    // Time per frame is stored in this file type.  Need to invert it into FPS.
                    data.playbackFPS = (UInt32) Math.Round(1.0f / file.ReadSingle());
                    
                    // These are offsets to specific data sections in the file.
                    UInt32 unknownOffset = file.ReadUInt32();
                    unknownOffset = file.ReadUInt32();
                    unknownOffset = file.ReadUInt32();
                    
                    UInt32 positionOffset = file.ReadUInt32();
                    UInt32 orientationOffset = file.ReadUInt32();
                    UInt32 indexOffset = file.ReadUInt32();
                    
                    // These last three values are confusing.
                    // They aren't a vector and they throw off the offset values
                    // by 12 bytes. Just ignore them and keep reading.
                    unknownOffset = file.ReadUInt32();
                    unknownOffset = file.ReadUInt32();
                    unknownOffset = file.ReadUInt32();
                    
                    //
                    // Vector section.
                    //
                    
                    List<float> positions = new List<float>();
                    UInt32 numberOfPositions = (orientationOffset - positionOffset) / sizeof(float);
                    for (UInt32 i = 0; i < numberOfPositions; ++i)
                    {
                        positions.Add(file.ReadSingle());
                    }
                    
                    //
                    // Quaternion section.
                    //
                    
                    List<float> orientations = new List<float>();
                    UInt32 numberOfOrientations = (indexOffset - orientationOffset) / sizeof(float);
                    for (UInt32 i = 0; i < numberOfOrientations; ++i)
                    {
                        orientations.Add(file.ReadSingle());
                    }
                    
                    //
                    // Offset section.
                    //
                    // Note: Unlike versions 0-3, data in this version is
                    // Frame 1:
                    //      Bone 1:
                    //      Bone 2:
                    // ...
                    // Frame 2:
                    //      Bone 1:
                    // ...
                    //
                    
                    Dictionary<UInt32, ANMBone> boneMap = new Dictionary<UInt32, ANMBone>();
                    for (Int32 i = 0; i < data.numberOfBones; ++i)
                    {
                        //
                        // The first frame is a special case since we are allocating bones
                        // as we read them in.
                        //
                        
                        // Read in the offset data.
                        UInt32 boneID = file.ReadUInt32();
                        UInt16 positionID = file.ReadUInt16();
                        UInt16 unknownIndex = file.ReadUInt16(); // Unknown.
                        UInt16 orientationID = file.ReadUInt16();
                        unknownIndex = file.ReadUInt16(); // Unknown. Seems to always be zero.
                        
                        // Allocate the bone.
                        ANMBone bone = new ANMBone();
                        bone.id = boneID;
                        
                        // Allocate all the frames for the bone.
                        for (int j = 0; j < data.numberOfFrames; ++j)
                        {
                            bone.frames.Add(new ANMFrame());
                        }
                        
                        // Retrieve the data for the first frame.
                        ANMFrame frame = bone.frames[0];
                        frame.position = LookUpVector(positionID, positions);
                        frame.orientation = LookUpQuaternion(orientationID, orientations);
                        
                        // Store the bone in the dictionary by bone ID.
                        boneMap[boneID] = bone;
                    }
                    
                    Int32 currentFrame = 1;
                    Int32 currentBone = 0;
                    
                    UInt32 numberOfLookUps = (data.numberOfFrames - 1) * data.numberOfBones;
                    for (UInt32 i = 0; i < numberOfLookUps; ++i)
                    {
                        //
                        // Normal case for all frames after the first.
                        //
                        
                        // Read in the offset data.
                        
                        UInt32 boneID = file.ReadUInt32();
                        UInt16 positionID = file.ReadUInt16();
                        UInt16 unknownIndex = file.ReadUInt16(); // Unknown.
                        UInt16 orientationID = file.ReadUInt16();
                        unknownIndex = file.ReadUInt16(); // Unknown. Seems to always be zero.
                        
                        // Retrieve the bone from the dictionary.
                        // Note: The bones appear to be in the same order in every frame.  So, a dictionary
                        // isn't exactly needed and you could probably get away with a list.  However, this way
                        // feels safer just in case something ends up being out of order.
                        ANMBone bone = boneMap[boneID];         
                        ANMFrame frame = bone.frames[currentFrame];
                        frame.position = LookUpVector(positionID, positions);
                        frame.orientation = LookUpQuaternion(orientationID, orientations);
                        
                        // This loop is slightly ambiguous.  
                        //
                        // The problem is previous .anm versions contain data like:
                        // foreach bone
                        //      foreach frame
                        //
                        // However, this version contains data like:
                        // foreach frame
                        //      foreach bone
                        //
                        // So, reading one version is going to be a little goofy.
                        currentBone++;
                        if (currentBone >= data.numberOfBones)
                        {
                            currentBone = 0;
                            currentFrame++;
                        }
                    }
                    
                    // Finally, we need to move all the data from the dictionary into the ANMFile.
                    foreach(var bone in boneMap)
                    {
                        data.bones.Add(bone.Value);
                    }

                    // Currently returning false for this version.  We can not render this version correctly yet.
                    // So, we need to tell the viewer not to try and load it.
                    result = false;
                }
                // Unknown version
                else
                {
                    logger.Error("Unknown anm version: " + data.version);

                    result = false; 
                }
            }
            catch(Exception e)
            {
                logger.Error("Anm reading error.");
                logger.Error(e.Message);
                result = false;
            }

            logger.Event("File ID: " + data.id);
            logger.Event("Magic: " + data.magic);
            logger.Event("Version: " + data.version);
            logger.Event("Number of Bones: " + data.numberOfBones);
            logger.Event("Number of Frames: " + data.numberOfFrames);
            logger.Event("Playback FPS: " + data.playbackFPS);

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

        private static float[] LookUpVector(Int32 id, List<float> vectors)
        {
            float[] result = new float[3];
            
            Int32 startingPosition = id * result.Count();
            for (int i = 0; i < result.Count(); ++i)
            {
                result[i] = vectors[startingPosition + i];
            }
            
            return result;
        }

        private static float[] LookUpQuaternion(Int32 id, List<float> quaternions)
        {
            float[] result = new float[4];
            
            Int32 startingPosition = id * result.Count();
            for (int i = 0; i < result.Count(); ++i)
            {
                result[i] = quaternions[startingPosition + i];
            }
            
            return result;
        }
    }
}

