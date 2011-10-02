
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

namespace LOLViewer.IO
{
    class InibinReader
    {
        public static bool ReadCharacterInibin(FileInfo f, out InibinFile file)
        {
            bool result = true;

            // Try to open the inibin file.
            try
            {
                FileStream fStream = new FileStream(f.FullName, FileMode.Open, 
                    FileAccess.Read);

                result = ReadCharacterInibin(fStream, out file);
            }
            catch
            {
                result = false;
                file = null;
            }

            return result;
        }

        public static bool ReadCharacterInibin(FileStream stream, out InibinFile file)
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
                foreach (long key in ReadSegmentKeys( ref stream )) 
                {
                    long val = (long) ReadInt32(ref stream);
#if VERBOSE
                    DebugOut("U32 prop(" + key + ")", val);
#endif

                    file.AddProperty(key, val);
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
                foreach (long key in ReadSegmentKeys(ref stream))
                {
                    float val = ReadFloat(ref stream);
#if VERBOSE
                    DebugOut("float prop(" + key + ")", val);
#endif

                    file.AddProperty(key, val);
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
                foreach (long key in ReadSegmentKeys(ref stream))
                {
                    float val = stream.ReadByte() * 0.1F;
#if VERBOSE
                    DebugOut("U8/10 prop(" + key + ")", val);
#endif

                    file.AddProperty(key, val);
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
                foreach (long key in ReadSegmentKeys(ref stream))
                {
                    int val = (int) ReadShort(ref stream);
#if VERBOSE
                    DebugOut("U16 prop(" + key + ")", val);
#endif

                    file.AddProperty(key, val);
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
                foreach (long key in ReadSegmentKeys(ref stream))
                {
                    int val = 0xff & stream.ReadByte();
#if VERBOSE
                    DebugOut("U8 prop(" + key + ")", val);
#endif

                    file.AddProperty(key, val);
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
                    foreach (long key in ReadSegmentKeys(ref stream))
                    {
                        long val = (long) ReadInt32(ref stream);
#if VERBOSE
                        DebugOut("U32 color prop(" + key + ")", val);
#endif

                        file.AddProperty(key, val);
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
                    foreach (long key in ReadSegmentKeys(ref stream))
                    {
                        int offset = (int) ReadShort(ref stream);
#if VERBOSE
                        DebugOut("String offset(" + key + ")", offset);
#endif
                        String val = ReadNulTerminatedString(ref stream, 
                            oldStyleOffset + offset);
#if VERBOSE
                        DebugOut("String prop(" + key + ")", val);
#endif

                        file.AddProperty(key, val);
                        
                        lastOffset = offset;
                    }
                }

#if VERBOSE
                // Debuging Code
                Debug.WriteLine("Skin #1 SKN: " + file.properties[(long) InibinHashID.SKIN_ONE_SKN]);
                Debug.WriteLine("Skin #1 SKL: " + file.properties[(long) InibinHashID.SKIN_ONE_SKL]);
                Debug.WriteLine("Skin #1 DDS: " + file.properties[(long) InibinHashID.SKIN_ONE_TEXTURE]);

                Debug.WriteLine("Skin #2 SKN: " + file.properties[(long) InibinHashID.SKIN_TWO_SKN]);
                Debug.WriteLine("Skin #2 SKL: " + file.properties[(long) InibinHashID.SKIN_TWO_SKL]);
                Debug.WriteLine("Skin #2 DDS: " + file.properties[(long) InibinHashID.SKIN_TWO_TEXTURE]);

                Debug.WriteLine("Skin #3 SKN: " + file.properties[(long) InibinHashID.SKIN_THREE_SKN]);
                Debug.WriteLine("Skin #3 SKL: " + file.properties[(long) InibinHashID.SKIN_THREE_SKL]);
                Debug.WriteLine("Skin #3 DDS: " + file.properties[(long) InibinHashID.SKIN_THREE_TEXTURE]);

                Debug.WriteLine("Skin #4 SKN: " + file.properties[(long) InibinHashID.SKIN_FOUR_SKN]);
                Debug.WriteLine("Skin #4 SKL: " + file.properties[(long) InibinHashID.SKIN_FOUR_SKL]);
                Debug.WriteLine("Skin #4 DDS: " + file.properties[(long) InibinHashID.SKIN_FOUR_TEXTURE]);

                Debug.WriteLine("Skin #5 SKN: " + file.properties[(long) InibinHashID.SKIN_FIVE_SKN]);
                Debug.WriteLine("Skin #5 SKL: " + file.properties[(long) InibinHashID.SKIN_FIVE_SKL]);
                Debug.WriteLine("Skin #5 DDS: " + file.properties[(long) InibinHashID.SKIN_FIVE_TEXTURE]); 
#endif
            }

            return result;
        }
        
        //
        // Helper stream reading funtions.
        //

        private static short ReadShort(ref FileStream s)
        {
	        int b1 = s.ReadByte();
	        int b2 = s.ReadByte();
	        return (short)(((0xff & b2) << 8) | (0xff & b1));
        }

        private static Int32 ReadInt32(ref FileStream s)
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

        private static float ReadFloat(ref FileStream s)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt32(ref s)), 0);
        }

        private static long[] ReadSegmentKeys(ref FileStream s)
        {
            int count = (int) ReadShort(ref s);
#if VERBOSE
            DebugOut("segment key count", count);
#endif

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

        public static String ReadNulTerminatedString(ref FileStream s, int atOffset)
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
