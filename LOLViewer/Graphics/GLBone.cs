
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
// Represents a bone from a specific key frame in an animation.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace LOLViewer.Graphics
{
    class GLBone
    {
        public String name;
        public int parent;

        public Matrix4 transform;
        public List<Matrix4> frames;

        public GLBone()
        {
            name = String.Empty;
            // -1 reserved for root
            parent = -2;

            transform = Matrix4.Identity;
            frames = new List<Matrix4>();
        }
    }
}
