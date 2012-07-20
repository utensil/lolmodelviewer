

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
// Abrstraction to read .skn files.
//

//
// TODO: Some of the newer SKLs can not be read properly.  IE Frost Fire Brand
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
    class SKNReader
    {
        /// <summary>
        /// Read in binary .skn file from RAF.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="data">The contents of the file are stored in here.</param>
        /// <returns></returns>
        public static bool Read(RAFFileListEntry file, ref SKNFile data, EventLogger logger)
        {
            bool result = true;

            logger.LogEvent("Reading skn: " + file.FileName);

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

        private static bool ReadBinary(MemoryStream input, ref SKNFile data, EventLogger logger)
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

        private static bool ReadData(BinaryReader file, ref SKNFile data, EventLogger logger)
        {
            bool result = true;

            try
            {
                // File Header Information.
                data.magic       = file.ReadInt32();
                data.version     = file.ReadInt16();
                data.numObjects  = file.ReadInt16();

                if (data.version == 1 || data.version == 2)
                {
                    // Contains material headers.
                    data.numMaterialHeaders = file.ReadInt32();
                    for (int i = 0; i < data.numMaterialHeaders; ++i)
                    {
                        // Read in the headers.
                        SKNMaterial header = new SKNMaterial();

                        header.name = new String(file.ReadChars(SKNMaterial.MATERIAL_NAME_SIZE));
                        header.startVertex = file.ReadInt32();
                        header.numVertices = file.ReadInt32();
                        header.startIndex = file.ReadInt32();
                        header.numIndices = file.ReadInt32();

                        data.materialHeaders.Add(header);
                    }

                    // Read in model data.
                    data.numIndices = file.ReadInt32();
                    data.numVertices = file.ReadInt32();

                    for (int i = 0; i < data.numIndices; ++i)
                    {
                        data.indices.Add(file.ReadInt16());
                    }

                    for (int i = 0; i < data.numVertices; ++i)
                    {
                        SKNVertex vertex = new SKNVertex();

                        vertex.position.X = file.ReadSingle();
                        vertex.position.Y = file.ReadSingle();
                        vertex.position.Z = file.ReadSingle();

                        for (int j = 0; j < SKNVertex.BONE_INDEX_SIZE; ++j)
                        {
                            int bone = (int)file.ReadByte();
                            vertex.boneIndex[j] = bone;
                        }

                        vertex.weights.X = file.ReadSingle();
                        vertex.weights.Y = file.ReadSingle();
                        vertex.weights.Z = file.ReadSingle();
                        vertex.weights.W = file.ReadSingle();

                        vertex.normal.X = file.ReadSingle();
                        vertex.normal.Y = file.ReadSingle();
                        vertex.normal.Z = file.ReadSingle();

                        vertex.texCoords.X = file.ReadSingle();
                        vertex.texCoords.Y = file.ReadSingle();

                        data.vertices.Add(vertex);
                    }

                    // Data exclusive to version two.
                    if (data.version == 2)
                    {
                        data.endTab.Add(file.ReadInt32());
                        data.endTab.Add(file.ReadInt32());
                        data.endTab.Add(file.ReadInt32());
                    }
                }
                // Unknown Version
                else           
                {
                    logger.LogError("Unknown skn version: " + data.version);
#if DEBUG
                    MessageBox.Show("New .skn version.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                    result = false;
                }
            }
            catch(Exception e)
            {
                logger.LogError("Skn reading error.");
                logger.LogError(e.Message);
                result = false;
            }

            logger.LogEvent("Magic: " + data.magic);
            logger.LogEvent("Version: " + data.version);
            logger.LogEvent("Number of Objects: " + data.numObjects);
            logger.LogEvent("Number of Material Headers: " + data.numMaterialHeaders);
            logger.LogEvent("Number of Vertices: " + data.numVertices);
            logger.LogEvent("Number of Indices: " + data.numIndices);
            logger.LogEvent("Magic: " + data.magic);

            return result;
        }
    }
}
