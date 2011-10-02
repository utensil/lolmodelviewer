
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
// Encapsulates the contents of a character *.inibin file.
//
// Based on the work of ItzWarty and Engberg.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Diagnostics;

namespace LOLViewer.IO
{
    class InibinFile
    {
        public Dictionary<long, Object> properties;

        public InibinFile()
        {
            properties =   new Dictionary<long, Object>();
        }

        public void AddProperty(long key, Object val)
        {
            if (properties.ContainsKey(key) == false)
            {
                properties.Add(key, val);
            }
        }

        public List<String> GetModelStrings()
        {
            List<String> result = new List<String>();

            // Read in model 1
            result.AddRange(GetModelStrings((long)InibinHashID.SKIN_ONE_SKN,
                (long)InibinHashID.SKIN_ONE_SKL,
                (long)InibinHashID.SKIN_ONE_TEXTURE));

            // Read in model 2
            result.AddRange(GetModelStrings((long)InibinHashID.SKIN_TWO_SKN,
                (long)InibinHashID.SKIN_TWO_SKL,
                (long)InibinHashID.SKIN_TWO_TEXTURE));

            // Read in model 3
            result.AddRange(GetModelStrings((long)InibinHashID.SKIN_THREE_SKN,
                (long)InibinHashID.SKIN_THREE_SKL,
                (long)InibinHashID.SKIN_THREE_TEXTURE));

            // Read in model 4
            result.AddRange(GetModelStrings((long)InibinHashID.SKIN_FOUR_SKN,
                (long)InibinHashID.SKIN_FOUR_SKL,
                (long)InibinHashID.SKIN_FOUR_TEXTURE));

            // Read in model 5 
            result.AddRange(GetModelStrings((long)InibinHashID.SKIN_FIVE_SKN,
                (long)InibinHashID.SKIN_FIVE_SKL,
                (long)InibinHashID.SKIN_FIVE_TEXTURE));

            return result;
        }


        //
        // Helper Function
        //
        private List<String> GetModelStrings(long sknID, long sklID, long textureID)
        {
            List<String> result = new List<String>();

            if( properties.ContainsKey(sknID) &&
                properties.ContainsKey(sklID) &&
                properties.ContainsKey(textureID) )
            {
                result.Add( (String)properties[sknID] );
                result.Add( (String)properties[sklID] );
                result.Add( (String)properties[textureID] );
            }

            return result;
        }
    }
}
