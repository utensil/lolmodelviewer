

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
// Stores the FileInfo's for the files
// making up League of Legends' model.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RAFLib;

namespace LOLViewer.IO
{
    class LOLModel
    {
        public FileInfo fileSkn;
        public FileInfo fileSkl;
        public FileInfo fileTexture;

        public RAFFileListEntry rafSkn;
        public RAFFileListEntry rafSkl;
        public RAFFileListEntry rafTexture;

        public LOLModel() 
        {
            fileSkn = null;
            fileSkl = null;
            fileTexture = null;

            rafSkn = null;
            rafSkl = null;
            rafTexture = null;
        }
    }
}
