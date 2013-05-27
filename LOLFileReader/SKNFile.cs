

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
// Stores the contents of an .skn file.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLFileReader
{
    public class SKNFile
    {
        // File Header Data
        public int                 magic;  // ????? What does this do?????
        public short               version;
        public short               numObjects;

        // Some versions have material header information.
        public int                 numMaterialHeaders;
        public List<SKNMaterial>   materialHeaders;

        // Actual model information.
        public int                 numIndices;
        public int                 numVertices;
        public List<short>         indices;
        public List<SKNVertex>     vertices;

        // Contained in version two.
        public List<int>           endTab; // ???? 

        public SKNFile()
        {
            magic = version = numObjects = 0;

            numMaterialHeaders = 0;
            materialHeaders = new List<SKNMaterial>();

            numIndices = numVertices = 0;
            indices = new List<short>();
            vertices = new List<SKNVertex>();

            endTab = new List<int>();
        }
    }
}
