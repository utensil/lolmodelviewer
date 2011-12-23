


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
// Stores the contents of a bone from an .anm file.
//



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLViewer
{
    class ANMBone
    {
        public const int BONE_NAME_LENGTH = 32;
        public String name;
        public UInt32 flag;
        public List<ANMFrame> frames;

        public ANMBone()
        {
            name = String.Empty;
            flag = 0;
            frames = new List<ANMFrame>();
        }
    }
}
