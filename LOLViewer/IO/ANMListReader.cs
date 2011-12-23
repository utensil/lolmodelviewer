


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
// Parses the text file containing a list of animations for 
// a character.
//
// This code is a hacky disaster.  Even worse, Riot now distributes Animation.list files
// as binary. So, this class ends up parsing binary sometimes.  
// It's not a really big though as the higher abstraction will just throw out non-existing files
// the binary lists will create.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using RAFLib;

namespace LOLViewer.IO
{
    class ANMListReader
    {
        public static bool ReadAnimationList(int skin, RAFFileListEntry file,
            ref Dictionary<String, String> animations)
        {
            bool result = true;

            // This happens when the file does not actually exist in the RAF archive.
            if (file.IsMemoryEntry == true)
            {
                String directoryName = file.RAFArchive.RAFFilePath;
                directoryName = directoryName.Replace("\\", "/");
                int pos = directoryName.LastIndexOf("/");
                directoryName = directoryName.Remove(pos);

                String fileName = directoryName + file.FileName;

                // Read it from the disk.
                return ReadAnimationList(skin, new FileInfo(fileName), ref animations);
            }

            // Create a new archive
            RAFArchive rafArchive = new RAFArchive(file.RAFArchive.RAFFilePath);

            try
            {
                // Get the data from the archive
                MemoryStream myInput 
                    = new MemoryStream(rafArchive.GetDirectoryFile().GetFileList().GetFileEntry(file.FileName).GetContent());
                StreamReader reader = new StreamReader(myInput);

                ParseAnimations(skin, reader, ref animations);

                reader.Close();
                myInput.Close();
            }
            catch
            {
                result = false;
                animations.Clear();
            }

            // Release the archive
            rafArchive.GetDataFileContentStream().Close();

            return result;
        }

        public static bool ReadAnimationList(int skin, FileInfo file,
            ref Dictionary<String, String> animations)
        {
            bool result = true;

            // Sanity. Lowers thrown exceptions
            if (file.Exists == false)
                return true;

            try
            {
                StreamReader myInput = new StreamReader(file.FullName);
                ParseAnimations(skin, myInput, ref animations);
            }
            catch
            {
                result = false;
                animations.Clear();
            }

            return result;
        }

        public static void ParseAnimations(int skin, StreamReader f,
            ref Dictionary<String, String> animations)
        {
            while (f.EndOfStream == false)
            {
                String line = f.ReadLine();
                if (line.Length <= 0)
                    continue;

                if (line[0] != ';' &&
                    line[0] != '\n' &&
                    line[0] != '\r' &&
                    line[0] != ' ')
                {
                    // Denotes animations for a specific skin.
                    if (line[0] == '[')
                    {
                        int skinNumber = -1;
                        if (line[6] == ']') // Seems pretty hacky : /
                        {
                            skinNumber = Int32.Parse(line[5].ToString());
                        }
                        else
                        {
                            skinNumber = Int32.Parse(line[5].ToString() + line[6].ToString());
                        }

                        ParseSkinSpecificAnimations(skin, skinNumber, ref f, ref animations);
                    }
                    // Animations for every skin.
                    else
                    {
                        String[] rawData = line.Split();
                        List<String> data = new List<String>();
                        foreach (String s in rawData)
                        {
                            // Sanity check
                            if (s != "")
                                data.Add(s);
                        }

                        // Sanity
                        if (data.Count > 1)
                        {
                            ParseAnimation(false, data[0], data[1], ref animations);
                        }
                    }
                }
            }
        }

        public static void ParseSkinSpecificAnimations(int modelSkin, int animationSkin, ref StreamReader f,
            ref Dictionary<String, String> animations)
        {
            // We need to read or skip over these animations.
            while (f.EndOfStream == false)
            {
                String line = f.ReadLine();

                // We've reached a gap in skin defitions.
                if (line.Length <= 0)
                    return;

                if (line[0] != ';' &&
                    line[0] != '\n' &&
                    line[0] != '\r' &&
                    line[0] != ' ')
                {
                    // Only read the animation if this is the correct skin.
                    // If this isn't the correct skin, we harmlessly pass over these 
                    // animation definitions.
                    if (modelSkin == animationSkin)
                    {
                        String[] rawData = line.Split();
                        List<String> data = new List<String>();
                        foreach (String s in rawData)
                        {
                            // Sanity check.
                            if (s != "")
                                data.Add(s);
                        }

                        // Sanity
                        if (data.Count > 1)
                        {
                            ParseAnimation(true, data[0], data[1], ref animations);
                        }
                    }
                }
                else
                {
                    // We found a comment/white space line,
                    // this denotes the end of this block of skin animations.
                    return;
                }
            }
        }

        public static void ParseAnimation(bool replace, String animation, String file,
            ref Dictionary<String, String> animations)
        {
            // Letter case does not correlate between animation.list files and the .anm file.
            // So, generate the file key in all lower case.
            file = file.ToLower();

            if (replace == true)
            {
                if (animations.ContainsKey(animation) == true)
                {
                    animations.Remove(animation);
                }
                animations.Add(animation, file);
            }
            else
            {
                if (animations.ContainsKey(animation) == false)
                {
                    animations.Add(animation, file);
                }
            }
        }
    }
}
