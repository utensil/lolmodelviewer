


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
// Abrstraction for a material used in .skn files.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLFileReader
{
    public class SKNMaterial
    {
        public String       name;
        public const int    MATERIAL_NAME_SIZE = 64; // in bytes

        public int          startVertex;
        public int          numVertices;
        public int          startIndex;
        public int          numIndices;

        public SKNMaterial()
        {
            name = String.Empty;
            startVertex = numVertices = startIndex = numIndices = 0;
        }
    }
}
