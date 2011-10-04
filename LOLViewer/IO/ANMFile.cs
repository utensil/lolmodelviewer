

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
// Stores the contents of an .anm file.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLViewer
{
    class ANMFile
    {
        public UInt32 magicOne;
        public UInt32 magicTwo;

        public UInt32 version;

        public UInt32 magicThree;

        public UInt32 numberOfBones;
        public UInt32 numberOfFrames;

        // I think this is the play back FPS for the animation.
        public UInt32 magicFour;

        public List<ANMBone> bones;

        public ANMFile()
        {
            magicOne = 0;
            magicTwo = 0;

            version = 0;

            magicThree = 0;

            numberOfBones = 0;
            numberOfFrames = 0;

            magicFour = 0;

            bones = new List<ANMBone>();
        }
    }
}
