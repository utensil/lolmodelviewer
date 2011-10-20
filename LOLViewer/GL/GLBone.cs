
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
// Represents a bone from a specific key frame in an animation.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace LOLViewer
{
    class GLBone
    {
        public int parent;
        public float scale;
        public Matrix4 worldTransform;
        public Quaternion worldOrientation;

        public GLBone()
        {
            // -1 reserved for root
            parent = -2;
            scale = 0.0f;
            worldTransform = Matrix4.Identity;
            worldOrientation = Quaternion.Identity;
        }
    }
}
