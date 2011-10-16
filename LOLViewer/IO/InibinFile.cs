
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
    class ModelDefinition
    {
        public int skin;
        public String anmListKey; // for finding animation files.
        public String name;
        public String skn;
        public String skl;
        public String tex;

        public ModelDefinition()
        {
            skin = -1;
            anmListKey = String.Empty;
            name = String.Empty;
            skn = String.Empty;
            skl = String.Empty;
            tex = String.Empty;
        }
    };

    class InibinFile
    {
        public DirectoryInfo directory;
        public Dictionary<long, Object> properties;

        public InibinFile()
        {
            properties =   new Dictionary<long, Object>();
            directory = null;
        }

        public void AddProperty(long key, Object val)
        {
            if (properties.ContainsKey(key) == false)
            {
                properties.Add(key, val);
            }
        }

        public List<ModelDefinition> GetModelStrings()
        {
            List<ModelDefinition> result = new List<ModelDefinition>();

            // Read in model 1
            ModelDefinition def = new ModelDefinition();
            def.anmListKey = directory.Name;

            bool flag = GetModelStrings(0, (long)InibinHashID.SKIN_ONE_SKN,
                (long)InibinHashID.SKIN_ONE_SKL,
                (long)InibinHashID.SKIN_ONE_TEXTURE, ref def);

            if (flag == true)
            {
                def.skin = 1;
                result.Add(def);
            }

            // Read in model 2
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            flag = GetModelStrings((long)InibinHashID.SKIN_TWO_NAME,
                (long)InibinHashID.SKIN_TWO_SKN,
                (long)InibinHashID.SKIN_TWO_SKL,
                (long)InibinHashID.SKIN_TWO_TEXTURE, ref def);

            if (flag == true)
            {
                def.skin = 2;
                result.Add(def);
            }

            // Read in model 3
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            flag = GetModelStrings((long)InibinHashID.SKIN_THREE_NAME,
                (long)InibinHashID.SKIN_THREE_SKN,
                (long)InibinHashID.SKIN_THREE_SKL,
                (long)InibinHashID.SKIN_THREE_TEXTURE, ref def);

            if (flag == true)
            {
                def.skin = 3;
                result.Add(def);
            }

            // Read in model 4
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            flag = GetModelStrings((long)InibinHashID.SKIN_FOUR_NAME,
                (long)InibinHashID.SKIN_FOUR_SKN,
                (long)InibinHashID.SKIN_FOUR_SKL,
                (long)InibinHashID.SKIN_FOUR_TEXTURE, ref def);

            if (flag == true)
            {
                def.skin = 4;
                result.Add(def);
            }

            // Read in model 5 
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            flag = GetModelStrings((long)InibinHashID.SKIN_FIVE_NAME,
                (long)InibinHashID.SKIN_FIVE_SKN,
                (long)InibinHashID.SKIN_FIVE_SKL,
                (long)InibinHashID.SKIN_FIVE_TEXTURE, ref def);

            if (flag == true)
            {
                def.skin = 5;
                result.Add(def);
            }

            return result;
        }


        //
        // Helper Function
        //
        private bool GetModelStrings(long nameID, long sknID, long sklID, 
            long textureID, ref ModelDefinition m)
        {
            bool result = false;

            // For skin 1
            if (nameID == 0)
            {
                if (properties.ContainsKey(sknID) &&
                    properties.ContainsKey(sklID) &&
                    properties.ContainsKey(textureID))
                {
                    m.skn = (String)properties[sknID];
                    m.skn = m.skn.ToLower();
                    m.skl = (String)properties[sklID];
                    m.skl = m.skl.ToLower();
                    m.tex = (String)properties[textureID];
                    m.tex = m.tex.ToLower();

                    result = true;
                }
            }
            // For other skins.
            else
            {
                if (properties.ContainsKey(nameID) &&
                    properties.ContainsKey(sknID) &&
                    properties.ContainsKey(sklID) &&
                    properties.ContainsKey(textureID))
                {
                    m.name = (String)properties[nameID];
                    m.skn = (String)properties[sknID];
                    m.skn = m.skn.ToLower();
                    m.skl = (String)properties[sklID];
                    m.skl = m.skl.ToLower();
                    m.tex = (String)properties[textureID];
                    m.tex = m.tex.ToLower();

                    result = true;
                }
            }

            return result;
        }
    }
}
