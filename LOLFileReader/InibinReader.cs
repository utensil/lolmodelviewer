
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
// Class to read in character *.inibin files.
//
// This class acts as a static namespace.  Just call the
// 'ReadCharaterInibin' function whenever you need to read a .inibin.
// It didn't make sense to encapsulate a 'reader' object since
// there's really no need to preform operators on a 'reader'.  We
// just want the data.
//
//
// Based on the work of ItzWarty and Engberg.
//

// Define this to output debug strings to the console.
//#define VERBOSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Diagnostics;

using CSharpLogger;

using RAFlibPlus;

namespace LOLFileReader
{
    public class InibinReader
    {
        public static bool Read(IFileEntry file, ref InibinFile data, Logger logger)
        {
            bool result = true;
            
            logger.Event("Reading inibin: " + file.FileName);

            try
            {
                // Get the data from the archive
                MemoryStream myInput = new MemoryStream( file.GetContent() );
                result = ReadCharacterInibin(myInput, ref data, logger);
        
                int end = file.FileName.LastIndexOf("/");
                String directory = file.FileName.Substring(0, end);
                String archive = file.GetRootPath();
                archive = archive.Replace("\\", "/");
                end = archive.LastIndexOf("/");
                archive = archive.Substring(0, end);
        
                data.directory = new DirectoryInfo(archive + "/" + directory);
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

        private static bool ReadCharacterInibin(MemoryStream stream, ref InibinFile file, Logger logger)
        {
 	        bool result = true;

            file = new InibinFile();

            stream.Seek( 0, SeekOrigin.Begin );

            // Header Info
            int version = stream.ReadByte();

#if VERBOSE
            DebugOut("version", version);
#endif

	        int fileLen = (int) stream.Length;
#if VERBOSE
	        DebugOut("file length", fileLen);
#endif

	        int oldLen = (int)ReadShort(ref stream);
#if VERBOSE
	        DebugOut("old style length", oldLen);
#endif

	        int oldStyleOffset = fileLen - oldLen;
#if VERBOSE
	        DebugOut("old style offset", oldStyleOffset);
#endif

	        int format = (int) ReadShort(ref stream);
#if VERBOSE
	        DebugOut("format", format);
#endif

            if ((format & 0x0001) == 0) 
            {
#if VERBOSE
                DebugOut("No U32 segment", "skipping");
#endif
            }
            else
            {
                // Reads values based on the hash keys.
#if VERBOSE
                DebugOut("U32 properties start position", stream.Position);
#endif
                long[] keys = ReadSegmentKeys( ref stream );
                if (keys != null)
                {
                    foreach (long key in keys)
                    {
                        long val = (long)ReadInt32(ref stream);
#if VERBOSE
                        DebugOut("U32 prop(" + key + ")", val);
#endif

                        file.AddProperty(key, val);
                    }
                }
            }

            // float values
            if ((format & 0x0002) == 0)
            {
#if VERBOSE
                DebugOut("No float segment", "skipping");
#endif
            }
            else
            {
#if VERBOSE
                DebugOut("Float properties start position", stream.Position);
#endif
                long[] keys = ReadSegmentKeys( ref stream );
                if (keys != null)
                {
                    foreach (long key in keys)
                    {
                        float val = ReadFloat(ref stream);
#if VERBOSE
                        DebugOut("float prop(" + key + ")", val);
#endif

                        file.AddProperty(key, val);
                    }
                }
            }

            // U8 values
            if ((format & 0x0004) == 0)
            {
#if VERBOSE
                DebugOut("No U8/10 segment", "skipping");
#endif
            }
            else
            {
#if VERBOSE
                DebugOut("U8/10 properties start position", stream.Position);
#endif
                long[] keys = ReadSegmentKeys( ref stream );
                if (keys != null)
                {
                    foreach (long key in keys)
                    {
                        float val = stream.ReadByte() * 0.1F;
#if VERBOSE
                        DebugOut("U8/10 prop(" + key + ")", val);
#endif

                        file.AddProperty(key, val);
                    }
                }
            }

            // U16 values
            if ((format & 0x0008) == 0)
            {
#if VERBOSE
                DebugOut("No U16 segment", "skipping");
#endif
            }
            else
            {
#if VERBOSE
                DebugOut("U16 properties start position", stream.Position);
#endif
                long[] keys = ReadSegmentKeys( ref stream );
                if (keys != null)
                {
                    foreach (long key in keys)
                    {
                        int val = (int)ReadShort(ref stream);
#if VERBOSE
                        DebugOut("U16 prop(" + key + ")", val);
#endif

                        file.AddProperty(key, val);
                    }
                }
            }

            // U8 values
            if ((format & 0x0010) == 0)
            {
#if VERBOSE
                DebugOut("No U8 segment", "skipping");
#endif
            }
            else
            {
#if VERBOSE
                DebugOut("U8 properties start position", stream.Position);
#endif
                long[] keys = ReadSegmentKeys( ref stream );
                if (keys != null)
                {
                    foreach (long key in keys)
                    {
                        int val = 0xff & stream.ReadByte();
#if VERBOSE
                        DebugOut("U8 prop(" + key + ")", val);
#endif

                        file.AddProperty(key, val);
                    }
                }
            }

            // Boolean flags - single bit, ignoring
            if ((format & 0x0020) == 0)
            {
#if VERBOSE
                DebugOut("No boolean segment", "skipping");
#endif
            }
            else
            {
#if VERBOSE
                DebugOut("Boolean flags start position", stream.Position);
#endif
                long[] booleanKeys = ReadSegmentKeys(ref stream);
                if (booleanKeys != null)
                {
#if VERBOSE
                    DebugOut("Boolean keys found", booleanKeys.Length);
#endif
                    int index = 0;
                    for (int i = 0; i < 1 + ((booleanKeys.Length - 1) / 8); ++i)
                    {
                        int bits = stream.ReadByte();
                        for (int b = 0; b < 8; ++b)
                        {
                            long key = booleanKeys[index];
                            int val = 0x1 & bits;
#if VERBOSE
                            DebugOut("Boolean prop(" + key + ")", val);
#endif

                            file.AddProperty(key, val);

                            bits = bits >> 1;
                            if (++index == booleanKeys.Length)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            // 4-byte color values or something?
            if ((format & 0x0400) == 0)
            {
#if VERBOSE
                DebugOut("No 4-byte color segment", "skipping");
#endif
            }
            else
            {
#if VERBOSE
                DebugOut("Color? properties start position", stream.Position);
#endif
                long[] keys = ReadSegmentKeys( ref stream );
                if (keys != null)
                {
                    foreach (long key in keys)
                    {
                        long val = (long)ReadInt32(ref stream);
#if VERBOSE
                        DebugOut("U32 color prop(" + key + ")", val);
#endif

                        file.AddProperty(key, val);
                    }
                }
            }

            // Newer section.
            // I don't know what exactly these values represent.
            // I think it's related to champions with the new rage mechanic.
            // I'm just using it to increment the stream.
            // So, when I get to the part to read in strings, the pointer is at the
            // correct location.
            if ((format & 0x0080) == 0)
            {
#if VERBOSE
                DebugOut("No offsets segment", "skipping");
#endif
            }
            else
            {
#if VERBOSE
                DebugOut("Rage values start position", stream.Position);
#endif
                long[] rageKeys = ReadSegmentKeys(ref stream);
                if (rageKeys != null)
                {
#if VERBOSE
                    DebugOut("Rage keys found", rageKeys.Length);
#endif
                    foreach (long key in rageKeys)
                    {
                        float val1 = ReadFloat(ref stream);
                        float val2 = ReadFloat(ref stream);
                        float val3 = ReadFloat(ref stream);
#if VERBOSE
                        DebugOut("Rage prop 1(" + key + ")", val1);
                        DebugOut("Rage prop 2(" + key + ")", val2);
                        DebugOut("Rage prop 3(" + key + ")", val3);
#endif
                        // If you actually need these values, figure out what 12 byte
                        // structure they represent and add that property.  
                        // It's probably a Vector3.
                        // file.AddProperty(key, MyRageKeyStructure);
                    }
                }
            }

            // Old-style offsets to strings
            if ((format & 0x1000) == 0)
            {
#if VERBOSE
                DebugOut("No offsets segment", "skipping");
#endif
            }
            else
            {
#if VERBOSE
                DebugOut("Old style data position", stream.Position);
#endif
                int lastOffset = -1;
                long[] keys = ReadSegmentKeys( ref stream );

                //
                // New method to read the newer .inibins.
                // Why determine the offset by reading in data from the file header
                // when we can just compute it here?  This seems to fix the problem
                // with newer .inibins.  I'm not sure what the value in the header
                // is used for though.
                //

                if (keys != null)
                {
                    oldStyleOffset = (int)stream.Position + keys.Length * 2;

                    foreach (long key in keys)
                    {
                        int offset = (int)ReadShort(ref stream);
#if VERBOSE
                        DebugOut("String offset(" + key + ")", offset);
#endif
                        String val = ReadNullTerminatedString(ref stream,
                            oldStyleOffset + offset);
#if VERBOSE
                        DebugOut("String prop(" + key + ")", val);
#endif

                        file.AddProperty(key, val);

                        lastOffset = offset;
                    }
                }
            }

#if VERBOSE
            // Debuging Code
            //Debug.WriteLine("Skin #1 Name: " + file.properties[(long) InibinHashID.SKIN_ONE_NAME]);
            Debug.WriteLine("Skin #1 SKN: " + file.properties[(long) InibinHashID.SKIN_ONE_SKN]);
            Debug.WriteLine("Skin #1 SKL: " + file.properties[(long) InibinHashID.SKIN_ONE_SKL]);
            Debug.WriteLine("Skin #1 DDS: " + file.properties[(long) InibinHashID.SKIN_ONE_TEXTURE]);

            Debug.WriteLine("Skin #2 Name: " + file.properties[(long) InibinHashID.SKIN_TWO_NAME]);
            Debug.WriteLine("Skin #2 SKN: " + file.properties[(long) InibinHashID.SKIN_TWO_SKN]);
            Debug.WriteLine("Skin #2 SKL: " + file.properties[(long) InibinHashID.SKIN_TWO_SKL]);
            Debug.WriteLine("Skin #2 DDS: " + file.properties[(long) InibinHashID.SKIN_TWO_TEXTURE]);

            Debug.WriteLine("Skin #3 Name: " + file.properties[(long) InibinHashID.SKIN_THREE_NAME]);
            Debug.WriteLine("Skin #3 SKN: " + file.properties[(long) InibinHashID.SKIN_THREE_SKN]);
            Debug.WriteLine("Skin #3 SKL: " + file.properties[(long) InibinHashID.SKIN_THREE_SKL]);
            Debug.WriteLine("Skin #3 DDS: " + file.properties[(long) InibinHashID.SKIN_THREE_TEXTURE]);

            Debug.WriteLine("Skin #4 Name: " + file.properties[(long) InibinHashID.SKIN_FOUR_NAME]);
            Debug.WriteLine("Skin #4 SKN: " + file.properties[(long) InibinHashID.SKIN_FOUR_SKN]);
            Debug.WriteLine("Skin #4 SKL: " + file.properties[(long) InibinHashID.SKIN_FOUR_SKL]);
            Debug.WriteLine("Skin #4 DDS: " + file.properties[(long) InibinHashID.SKIN_FOUR_TEXTURE]);

            Debug.WriteLine("Skin #5 Name: " + file.properties[(long) InibinHashID.SKIN_FIVE_NAME]);
            Debug.WriteLine("Skin #5 SKN: " + file.properties[(long) InibinHashID.SKIN_FIVE_SKN]);
            Debug.WriteLine("Skin #5 SKL: " + file.properties[(long) InibinHashID.SKIN_FIVE_SKL]);
            Debug.WriteLine("Skin #5 DDS: " + file.properties[(long) InibinHashID.SKIN_FIVE_TEXTURE]); 

            Debug.WriteLine("Skin #6 Name: " + file.properties[(long) InibinHashID.SKIN_SIX_NAME]);
            Debug.WriteLine("Skin #6 SKN: " + file.properties[(long) InibinHashID.SKIN_SIX_SKN]);
            Debug.WriteLine("Skin #6 SKL: " + file.properties[(long) InibinHashID.SKIN_SIX_SKL]);
            Debug.WriteLine("Skin #6 DDS: " + file.properties[(long) InibinHashID.SKIN_SIX_TEXTURE]);

            Debug.WriteLine("Skin #7 Name: " + file.properties[(long)InibinHashID.SKIN_SEVEN_NAME]);
            Debug.WriteLine("Skin #7 SKN: " + file.properties[(long)InibinHashID.SKIN_SEVEN_SKN]);
            Debug.WriteLine("Skin #7 SKL: " + file.properties[(long)InibinHashID.SKIN_SEVEN_SKL]);
            Debug.WriteLine("Skin #7 DDS: " + file.properties[(long)InibinHashID.SKIN_SEVEN_TEXTURE]);

            Debug.WriteLine("Skin #8 Name: " + file.properties[(long)InibinHashID.SKIN_EIGHT_NAME]);
            Debug.WriteLine("Skin #8 SKN: " + file.properties[(long)InibinHashID.SKIN_EIGHT_SKN]);
            Debug.WriteLine("Skin #8 SKL: " + file.properties[(long)InibinHashID.SKIN_EIGHT_SKL]);
            Debug.WriteLine("Skin #8 DDS: " + file.properties[(long)InibinHashID.SKIN_EIGHT_TEXTURE]); 
#endif

            logger.Event("Version: " + version);

            //if (file.properties.ContainsKey((long)InibinHashID.SKIN_ONE_NAME))
                //logger.LogEvent("Skin #1 Name: " + file.properties[(long) InibinHashID.SKIN_ONE_NAME]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_ONE_SKN))
                logger.Event("Skin #1 SKN: " + file.properties[(long)InibinHashID.SKIN_ONE_SKN]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_ONE_SKL))
                logger.Event("Skin #1 SKL: " + file.properties[(long)InibinHashID.SKIN_ONE_SKL]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_ONE_TEXTURE))
                logger.Event("Skin #1 DDS: " + file.properties[(long)InibinHashID.SKIN_ONE_TEXTURE]);

            if (file.properties.ContainsKey((long)InibinHashID.SKIN_TWO_NAME))
                logger.Event("Skin #2 Name: " + file.properties[(long)InibinHashID.SKIN_TWO_NAME]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_TWO_SKN))
                logger.Event("Skin #2 SKN: " + file.properties[(long)InibinHashID.SKIN_TWO_SKN]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_TWO_SKL))
                logger.Event("Skin #2 SKL: " + file.properties[(long)InibinHashID.SKIN_TWO_SKL]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_TWO_TEXTURE))
                logger.Event("Skin #2 DDS: " + file.properties[(long)InibinHashID.SKIN_TWO_TEXTURE]);

            if (file.properties.ContainsKey((long)InibinHashID.SKIN_THREE_NAME))
                logger.Event("Skin #3 Name: " + file.properties[(long)InibinHashID.SKIN_THREE_NAME]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_THREE_SKN))
                logger.Event("Skin #3 SKN: " + file.properties[(long)InibinHashID.SKIN_THREE_SKN]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_THREE_SKL))
                logger.Event("Skin #3 SKL: " + file.properties[(long)InibinHashID.SKIN_THREE_SKL]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_THREE_TEXTURE))
                logger.Event("Skin #3 DDS: " + file.properties[(long)InibinHashID.SKIN_THREE_TEXTURE]);

            if (file.properties.ContainsKey((long)InibinHashID.SKIN_FOUR_NAME))
                logger.Event("Skin #4 Name: " + file.properties[(long)InibinHashID.SKIN_FOUR_NAME]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_FOUR_SKN))
                logger.Event("Skin #4 SKN: " + file.properties[(long)InibinHashID.SKIN_FOUR_SKN]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_FOUR_SKL))
                logger.Event("Skin #4 SKL: " + file.properties[(long)InibinHashID.SKIN_FOUR_SKL]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_FOUR_TEXTURE))
                logger.Event("Skin #4 DDS: " + file.properties[(long)InibinHashID.SKIN_FOUR_TEXTURE]);

            if (file.properties.ContainsKey((long)InibinHashID.SKIN_FIVE_NAME))
                logger.Event("Skin #5 Name: " + file.properties[(long)InibinHashID.SKIN_FIVE_NAME]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_FIVE_SKN))
                logger.Event("Skin #5 SKN: " + file.properties[(long)InibinHashID.SKIN_FIVE_SKN]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_FIVE_SKL))
                logger.Event("Skin #5 SKL: " + file.properties[(long)InibinHashID.SKIN_FIVE_SKL]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_FIVE_TEXTURE))
                logger.Event("Skin #5 DDS: " + file.properties[(long)InibinHashID.SKIN_FIVE_TEXTURE]);

            if (file.properties.ContainsKey((long)InibinHashID.SKIN_SIX_NAME))
                logger.Event("Skin #6 Name: " + file.properties[(long)InibinHashID.SKIN_SIX_NAME]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_SIX_SKN))
                logger.Event("Skin #6 SKN: " + file.properties[(long)InibinHashID.SKIN_SIX_SKN]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_SIX_SKL))
                logger.Event("Skin #6 SKL: " + file.properties[(long)InibinHashID.SKIN_SIX_SKL]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_SIX_TEXTURE))
                logger.Event("Skin #6 DDS: " + file.properties[(long)InibinHashID.SKIN_SIX_TEXTURE]);

            if (file.properties.ContainsKey((long)InibinHashID.SKIN_SEVEN_NAME))
                logger.Event("Skin #7 Name: " + file.properties[(long)InibinHashID.SKIN_SEVEN_NAME]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_SEVEN_SKN))
                logger.Event("Skin #7 SKN: " + file.properties[(long)InibinHashID.SKIN_SEVEN_SKN]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_SEVEN_SKL))
                logger.Event("Skin #7 SKL: " + file.properties[(long)InibinHashID.SKIN_SEVEN_SKL]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_SEVEN_TEXTURE))
                logger.Event("Skin #7 DDS: " + file.properties[(long)InibinHashID.SKIN_SEVEN_TEXTURE]);

            if (file.properties.ContainsKey((long)InibinHashID.SKIN_EIGHT_NAME))
                logger.Event("Skin #8 Name: " + file.properties[(long)InibinHashID.SKIN_EIGHT_NAME]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_EIGHT_SKN))
                logger.Event("Skin #8 SKN: " + file.properties[(long)InibinHashID.SKIN_EIGHT_SKN]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_EIGHT_SKL))
                logger.Event("Skin #8 SKL: " + file.properties[(long)InibinHashID.SKIN_EIGHT_SKL]);
            if (file.properties.ContainsKey((long)InibinHashID.SKIN_EIGHT_TEXTURE))
                logger.Event("Skin #8 DDS: " + file.properties[(long)InibinHashID.SKIN_EIGHT_TEXTURE]); 

            return result;
        }
        
        //
        // Helper stream reading funtions.
        //

        private static short ReadShort(ref MemoryStream s)
        {
	        int b1 = s.ReadByte();
	        int b2 = s.ReadByte();
	        return (short)(((0xff & b2) << 8) | (0xff & b1));
        }

        private static Int32 ReadInt32(ref MemoryStream s)
        {
            Int32 b1 = s.ReadByte();
            Int32 b2 = s.ReadByte();
            Int32 b3 = s.ReadByte();
            Int32 b4 = s.ReadByte();
            return (0xff & b1) |
                ((0xff & b2) << 8) |
                ((0xff & b3) << 16) |
                ((0xff & b4) << 24);
        }

        private static float ReadFloat(ref MemoryStream s)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt32(ref s)), 0);
        }

        private static long[] ReadSegmentKeys(ref MemoryStream s)
        {
            int count = (int) ReadShort(ref s);
#if VERBOSE
            DebugOut("segment key count", count);
#endif
            // Sometimes this happens.
            if (count < 0)
            {
                return null;
            }

            long[] result = new long[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = (long) ReadInt32(ref s);
#if VERBOSE
                DebugOut("key[" + i + "]", result[i]);
#endif
            }

            return result;
        }

        private static String ReadNullTerminatedString(ref MemoryStream s, int atOffset)
        {
            long oldPos = s.Position;
            s.Seek(atOffset, SeekOrigin.Begin);

            StringBuilder sb = new StringBuilder();
            int c;
            while ((c = s.ReadByte()) > 0)
            {
                sb.Append((char)c);
            }

            s.Seek(oldPos, SeekOrigin.Begin);
            return sb.ToString();
        }

        //
        //  Helper Debug Functions 
        //

        private static void DebugOut(String label, String val)
        {
            Debug.WriteLine(label + ": " + val);
        }

        private static void DebugOut(String label, float val)
        {
            DebugOut(label, val.ToString());
        }

        private static void DebugOut(String label, Object val)
        {
            if( val == null )
            {
                DebugOut(label, "null");
            }
            else
            {
                DebugOut(label, val.ToString());
            }
        }

        private static void DebugOut(String label, long val)
        {
            DebugOut(label, "0x" + val.ToString("x") + " (" + val + ")");
        }
    }
}
