/*
 * RAF Library - Plus (RAFlib-Plus)
 * Copyright 2012 Adrian Astley
 *
 *This file is part of RAFlib-Plus.
 *
 *RAFlib-Plus is free software: you can redistribute it and/or modify
 *it under the terms of the GNU General Public License as published by
 *the Free Software Foundation, either version 3 of the License, or
 *(at your option) any later version.

 *RAFlib-Plus is distributed in the hope that it will be useful,
 *but WITHOUT ANY WARRANTY; without even the implied warranty of
 *MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *GNU General Public License for more details.

 *You should have received a copy of the GNU General Public License
 *along with RAFlib-Plus.  If not, see <http://www.gnu.org/licenses/>
*/

/*
 * This class is designed to help extract and 
 * inject files from the League of Legends game files.
 * http://www.leagueoflegends.com 
 * 
 * This class is a modification of the original 
 * RAFlib generously created and provided by ItzWarty
 * and found here http://code.google.com/p/raf-manager/source/browse/#svn%2FProjects%2FRAFLib
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ItzWarty;
using zlib = ComponentAce.Compression.Libs.zlib;
using System.Threading;
using System.Globalization;

namespace RAFlibPlus
{
    /// <summary>
    /// A class that represents a file within an RAF archive
    /// </summary>
    public class RAFFileListEntry
    {
        private RAFArchive raf = null;

        private UInt32 fileOffset = UInt32.MaxValue;  //It is assumed that LoL archive files will never reach 4 gigs of size.
        private UInt32 fileSize = UInt32.MaxValue;
        private string fileName = null;

        /// <summary>
        /// A class that represents a file within an RAF archive
        /// </summary>
        /// <param name="raf">Pointer to the owning RAFArchive</param>
        /// <param name="directoryFileContent">Pointer to the content of the .raf.dat file</param>
        /// <param name="offsetDirectoryEntry">Offset to the entry data offsets</param>
        /// <param name="offsetStringTable">Offset to the entry's file name</param>
        public RAFFileListEntry(RAFArchive raf, ref byte[] directoryFileContent, UInt32 offsetDirectoryEntry, UInt32 offsetStringTable)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            
            this.raf = raf;

            this.fileOffset = BitConverter.ToUInt32(directoryFileContent, (int)offsetDirectoryEntry + 4); ;
            this.fileSize = BitConverter.ToUInt32(directoryFileContent, (int)offsetDirectoryEntry + 8);

            UInt32 strIndex = BitConverter.ToUInt32(directoryFileContent, (int)offsetDirectoryEntry + 12);
            UInt32 entryOffset = offsetStringTable + 8 + strIndex * 8;

            UInt32 entryValueOffset = BitConverter.ToUInt32(directoryFileContent, (int)entryOffset);
            UInt32 entryValueSize = BitConverter.ToUInt32(directoryFileContent, (int)entryOffset + 4);

            byte[] stringBytes = directoryFileContent.SubArray((int)(entryValueOffset + offsetStringTable), (int)entryValueSize - 1);

            this.fileName = Encoding.ASCII.GetString(stringBytes);
        }

        /// <summary>
        /// A class that represents a file within an RAF archive. Creates an entry that only exists in memory.
        /// </summary>
        /// <param name="raf">Pointer to the owning RAFArchive</param>
        /// <param name="fileName">Full path of the file, ie. DATA/Characters/Ahri/Ahri.skn</param>
        /// <param name="offsetDatFile">Offset to the entry data offsets</param>
        /// <param name="fileSize">Length of the file in bytes</param>
        public RAFFileListEntry(RAFArchive raf, string fileName, UInt32 offsetDatFile, UInt32 fileSize)
        {
            this.raf = raf;
            this.fileName = fileName;
            this.fileOffset = offsetDatFile;
            this.fileSize = fileSize;
        }

        /// <summary>
        /// Returns the content of the file
        /// </summary>
        public byte[] GetContent()
        {
            // Open .dat file
            FileStream fStream = new FileStream(this.raf.RAFFilePath + ".dat", FileMode.Open);

            byte[] content = getContentHelperFunc(fStream);

            fStream.Close();

            return content;
        }

        /// <summary>
        /// Returns the content of the file
        /// </summary>
        /// <param name="fStream">FileStream to the RAF .dat file</param>
        /// <returns></returns>
        public byte[] GetContent(FileStream fStream)
        {
            return getContentHelperFunc(fStream);
        }

        private byte[] getContentHelperFunc(FileStream fStream)
        {
            byte[] buffer = new byte[this.FileSize];            //Will contain compressed data
            fStream.Seek(this.FileOffset, SeekOrigin.Begin);
            fStream.Read(buffer, 0, (int)this.FileSize);

            try
            {
                MemoryStream mStream = new MemoryStream(buffer);
                zlib.ZInputStream zinput = new zlib.ZInputStream(mStream);

                List<byte> dBuffer = new List<byte>(); //decompressed buffer, arraylist to my knowledge...

                //This could be optimized in the future by reading a block and adding it to our arraylist..
                //which would be much faster, obviously
                int data = 0;
                while ((data = zinput.Read()) != -1)
                    dBuffer.Add((byte)data);

                return dBuffer.ToArray();
            }
            catch
            {
                //it's not compressed, just return original content
                return buffer;
            }
        }

        /// <summary>
        /// Returns the raw, still compressed, contents of the file. 
        /// Doesn't really have a use, but included from old RAFlib
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawContent()
        {
            // Open .dat file
            FileStream fStream = new FileStream(this.raf.RAFFilePath + ".dat", FileMode.Open);

            byte[] buffer = new byte[this.FileSize];            //Will contain compressed data
            fStream.Seek(this.FileOffset, SeekOrigin.Begin);
            fStream.Read(buffer, 0, (int)this.FileSize);

            fStream.Close();

            return buffer;
        }

        /// <summary>
        /// Filename of the entry
        /// </summary>
        public String FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                fileName = value;
            }
        }

        /// <summary>
        /// Offset to the start of the archived file in the .dat file
        /// </summary>
        public UInt32 FileOffset
        {
            get
            {
               return fileOffset;
            }
            set
            {
                this.fileOffset = value;
            }
        }
        /// <summary>
        /// Size of this archived file
        /// </summary>
        public UInt32 FileSize
        {
            get
            {
                return fileSize;
            }
            set
            {
                fileSize = value;
            }
        }
        /// <summary>
        /// Hash of the string name
        /// </summary>
        public UInt32 StringNameHash
        {
            get
            {
                return RAFHashManager.GetHash(FileName);
            }
        }

        /// <summary>
        /// Returns the entry's corresponding RAFArchive
        /// </summary>
        public RAFArchive RAFArchive
        {
            get
            {
                return raf;
            }
        }

        /// <summary>
        /// Replace the content of the RAFFileListEntry and update memory of this new data.
        /// You HAVE to call &lt;RAFArchive&gt;.SaveRAFFile() after you finish all the inserts.
        /// </summary>
        /// <param name="content">Content to overwrite the previous file data</param>
        /// <returns></returns>
        public bool ReplaceContent(byte[] content)
        {
            // Open the .dat file
            FileStream datFileStream = new FileStream(this.RAFArchive.RAFFilePath + ".dat", FileMode.Open);

            bool returnVal = replaceContentHelperFunc(content, datFileStream);

            // Close the steam since we're done with it
            datFileStream.Close();

            return returnVal;
        }

        /// <summary>
        /// Replace the content of the RAFFileListEntry and update memory of this new data.
        /// You HAVE to call &lt;RAFArchive&gt;.SaveRAFFile() after you finish all the inserts.
        /// </summary>
        /// <param name="content">Content to overwrite the previous file data</param>
        /// <param name="datFileStream">FileStream to the RAF .dat file</param>
        /// <returns></returns>
        public bool ReplaceContent(byte[] content, FileStream datFileStream)
        {
            return replaceContentHelperFunc(content, datFileStream);
        }

        private bool replaceContentHelperFunc(byte[] content, FileStream datFileStream)
        {
            // Store the old offsets just in case we need to perform a restore.
            // This actually isn't necessary currently, since the raf directory file is saved after packing.
            UInt32 oldOffset = (UInt32)this.FileOffset;
            UInt32 oldSize = (UInt32)this.FileSize;

            try
            {
                // Navigate to the end of it
                datFileStream.Seek(0, SeekOrigin.End);
                UInt32 offset = (UInt32)datFileStream.Length;

                FileInfo fInfo = new FileInfo(fileName);

                // .fsb, .fev, and .gfx files aren't compressed
                byte[] finalContent;
                if (fInfo.Extension == ".fsb" || fInfo.Extension == ".fev" || fInfo.Extension == ".gfx")
                {
                    finalContent = content;
                }
                else
                {
                    // Start of compression
                    MemoryStream mStream = new MemoryStream();
                    zlib.ZOutputStream oStream = new zlib.ZOutputStream(mStream, zlib.zlibConst.Z_DEFAULT_COMPRESSION); //using default compression level
                    oStream.Write(content, 0, content.Length);
                    oStream.finish();
                    finalContent = mStream.ToArray();
                }

                // Write the data to the end of the .dat file
                datFileStream.Write(finalContent, 0, finalContent.Length);

                // Update entry values to represent the new changes
                this.FileOffset = offset;
                this.FileSize = (UInt32)finalContent.Length;

                return true;
            }
            catch (Exception)
            {
                this.FileOffset = oldOffset;
                this.FileSize = oldSize;

                return false;
            }
        }
    }
}
