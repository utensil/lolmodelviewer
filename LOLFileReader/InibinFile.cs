
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

namespace LOLFileReader
{
    public class ModelDefinition
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

    public class InibinFile
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
            def.anmListKey = GetAnimationList(directory);
          
            bool modelStringsResult = GetModelStrings((long)InibinHashID.SKIN_ONE_NAME, 
                    (long)InibinHashID.SKIN_ONE_SKN,
                    (long)InibinHashID.SKIN_ONE_SKL,
                    (long)InibinHashID.SKIN_ONE_TEXTURE, ref def);

            if (modelStringsResult == true)
            {
                def.skin = 1;
                result.Add(def);
            }

            // Read in model 2
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            modelStringsResult = GetModelStrings((long)InibinHashID.SKIN_TWO_NAME,
                (long)InibinHashID.SKIN_TWO_SKN,
                (long)InibinHashID.SKIN_TWO_SKL,
                (long)InibinHashID.SKIN_TWO_TEXTURE, ref def);

            if (modelStringsResult == true)
            {
                def.skin = 2;
                result.Add(def);
            }

            // Read in model 3
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            modelStringsResult = GetModelStrings((long)InibinHashID.SKIN_THREE_NAME,
                (long)InibinHashID.SKIN_THREE_SKN,
                (long)InibinHashID.SKIN_THREE_SKL,
                (long)InibinHashID.SKIN_THREE_TEXTURE, ref def);

            if (modelStringsResult == true)
            {
                def.skin = 3;
                result.Add(def);
            }

            // Read in model 4
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            modelStringsResult = GetModelStrings((long)InibinHashID.SKIN_FOUR_NAME,
                (long)InibinHashID.SKIN_FOUR_SKN,
                (long)InibinHashID.SKIN_FOUR_SKL,
                (long)InibinHashID.SKIN_FOUR_TEXTURE, ref def);

            if (modelStringsResult == true)
            {
                def.skin = 4;
                result.Add(def);
            }

            // Read in model 5 
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            modelStringsResult = GetModelStrings((long)InibinHashID.SKIN_FIVE_NAME,
                (long)InibinHashID.SKIN_FIVE_SKN,
                (long)InibinHashID.SKIN_FIVE_SKL,
                (long)InibinHashID.SKIN_FIVE_TEXTURE, ref def);

            if (modelStringsResult == true)
            {
                def.skin = 5;
                result.Add(def);
            }

            // Read in model 6
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            modelStringsResult = GetModelStrings((long)InibinHashID.SKIN_SIX_NAME,
                (long)InibinHashID.SKIN_SIX_SKN,
                (long)InibinHashID.SKIN_SIX_SKL,
                (long)InibinHashID.SKIN_SIX_TEXTURE, ref def);

            if (modelStringsResult == true)
            {
                def.skin = 6;
                result.Add(def);
            }

            // Read in model 7
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            modelStringsResult = GetModelStrings((long)InibinHashID.SKIN_SEVEN_NAME,
                (long)InibinHashID.SKIN_SEVEN_SKN,
                (long)InibinHashID.SKIN_SEVEN_SKL,
                (long)InibinHashID.SKIN_SEVEN_TEXTURE, ref def);

            if (modelStringsResult == true)
            {
                def.skin = 7;
                result.Add(def);
            }

            // Read in model 8
            def = new ModelDefinition();
            def.anmListKey = directory.Name;
            modelStringsResult = GetModelStrings((long)InibinHashID.SKIN_EIGHT_NAME,
                (long)InibinHashID.SKIN_EIGHT_SKN,
                (long)InibinHashID.SKIN_EIGHT_SKL,
                (long)InibinHashID.SKIN_EIGHT_TEXTURE, ref def);

            if (modelStringsResult == true)
            {
                def.skin = 8;
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

            // Sometimes the first skin is named by the root directory.
            if (properties.ContainsKey(nameID))
            {
                m.name = (String)properties[nameID];
            }

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

            return result;
        }

        private string GetAnimationList(DirectoryInfo directory)
        {
            string result = string.Empty;

            // Riot changed their directory structure for some skins.
            // Originally, champion .inibin files were stored in a directory structure like
            // "*/ChampionName/*.inibin".  Now, some are stored like
            // "*/ChampionName/Skins/Skin01/*inibin".

            if (directory.Name.ToLower().Contains("skin") == false &&
                directory.Name.ToLower().Contains("base") == false)
            {
                // Original Case.
                result = directory.Name;
            }
            else
            {
                // Newer Case.
                string path = directory.ToString();
                string[] splitPath = path.Split('/');

                // Sanity
                if (splitPath.Length > 2)
                {
                    result = splitPath[splitPath.Length - 3];
                }
            }

            return result;
        }
    }
}
