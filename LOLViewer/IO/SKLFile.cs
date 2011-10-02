﻿

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
// Stores the contents of an .skl file.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLViewer
{
    class SKLFile
    {
        // Not sure what the first eight bytes represent
        public int              magicOne;
        public int              magicTwo;

        public uint             version;
        public uint             designerID;

        public uint             numBones;
        public List<SKLBone>    bones;

        public uint             numBoneIDs;
        public List<uint>       boneIDs;

        public SKLFile()
        {
            magicOne = magicTwo = 0;

            version = designerID = numBones = 0;
            bones = new List<SKLBone>();
            
            numBoneIDs = 0;
            boneIDs = new List<uint>();
        }
    }
}
