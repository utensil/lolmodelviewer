

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
// Abrstraction to read .skn files.
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
    public class SKNReader
    {
        /// <summary>
        /// Read in binary .skn file from RAF.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="data">The contents of the file are stored in here.</param>
        /// <returns></returns>
        public static bool Read(IFileEntry file, ref SKNFile data, Logger logger)
        {
            bool result = true;

            logger.Event("Reading skn: " + file.FileName);

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

        private static bool ReadBinary(MemoryStream input, ref SKNFile data, Logger logger)
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

        private static bool ReadData(BinaryReader file, ref SKNFile data, Logger logger)
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

                        vertex.position[0] = file.ReadSingle(); // x
                        vertex.position[1] = file.ReadSingle(); // y
                        vertex.position[2] = file.ReadSingle(); // z

                        for (int j = 0; j < SKNVertex.BONE_INDEX_SIZE; ++j)
                        {
                            int bone = (int)file.ReadByte();
                            vertex.boneIndex[j] = bone;
                        }

                        vertex.weights[0] = file.ReadSingle();
                        vertex.weights[1] = file.ReadSingle();
                        vertex.weights[2] = file.ReadSingle();
                        vertex.weights[3] = file.ReadSingle();

                        vertex.normal[0] = file.ReadSingle(); // x
                        vertex.normal[1] = file.ReadSingle(); // y
                        vertex.normal[2] = file.ReadSingle(); // z

                        vertex.texCoords[0] = file.ReadSingle(); // u
                        vertex.texCoords[1] = file.ReadSingle(); // v

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
                    logger.Error("Unknown skn version: " + data.version);
                    result = false;
                }
            }
            catch(Exception e)
            {
                logger.Error("Skn reading error.");
                logger.Error(e.Message);
                result = false;
            }

            logger.Event("Magic: " + data.magic);
            logger.Event("Version: " + data.version);
            logger.Event("Number of Objects: " + data.numObjects);
            logger.Event("Number of Material Headers: " + data.numMaterialHeaders);
            logger.Event("Number of Vertices: " + data.numVertices);
            logger.Event("Number of Indices: " + data.numIndices);

            return result;
        }
    }
}
