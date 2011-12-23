



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
// Abrstraction for a vertex in a .skn file.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace LOLViewer
{
    class SKNVertex
    {
        public Vector3      position;
        public int[]        boneIndex;
        public Vector4      weights;
        public Vector3      normal;
        public Vector2      texCoords;

        public const int    BONE_INDEX_SIZE = 4; // in bytes

        public SKNVertex()
        {
            position = Vector3.Zero;
            boneIndex = new int[BONE_INDEX_SIZE];
            weights = Vector4.Zero;
            normal = Vector3.Zero;
            texCoords = Vector2.Zero;
        }
    }
}
