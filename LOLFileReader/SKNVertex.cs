



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
// Abrstraction for a vertex in a .skn file.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLFileReader
{
    public class SKNVertex
    {
        public float[]      position;
        public int[]        boneIndex;
        public float[]      weights;
        public float[]      normal;
        public float[]      texCoords;

        public const int    BONE_INDEX_SIZE = 4; // in bytes

        public SKNVertex()
        {
            position = new float[3];
            boneIndex = new int[BONE_INDEX_SIZE];
            weights = new float[4];
            normal = new float[3];
            texCoords = new float[2];
        }
    }
}
