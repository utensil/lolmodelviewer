
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
// Stores the position and orientation of an .anm bone
// for a give frame.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLFileReader
{
    public class ANMFrame
    {
        public float[] orientation;
        public float[] position;

        public ANMFrame()
        {
            orientation = new float[4];
            position = new float[3];
        }
    }
}
