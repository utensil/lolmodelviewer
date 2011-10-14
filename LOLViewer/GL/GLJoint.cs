
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
// Represents a joint from the default/binding pose
// of a model.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace LOLViewer
{
    class GLJoint
    {
        public int parent;
        public float scale;

        public Vector3 worldPosition;
        public Quaternion worldOrientation;
        public Matrix4 worldTransform;

        public GLJoint()
        {
            // -1 reserved for root
            parent = -2;
            scale = 1.0f;
            worldOrientation = Quaternion.Identity;
            worldTransform = Matrix4.Identity;
        }
    }
}
