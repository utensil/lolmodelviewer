


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
// Extracts model and texture information
// from the League of Legends directory
// structure.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Diagnostics;

using RAFLib;

namespace LOLViewer.IO
{
    class LOLDirectoryReader
    {
        public const String DEFAULT_ROOT = "C:\\Riot Games";
        public const String DEFAULT_MODEL_ROOT = "\\DATA\\Characters";
        public const String DEFAULT_RAF_DIRECTORY_ONE = "DATA";
        public const String DEFAULT_RAF_DIRECTORY_TWO = "Characters";

        public const String DEFAULT_EXTRACTED_TEXTURES_ROOT = "content\\textures\\";
        public String root;

        public List<FileInfo> inibinFiles;
        public Dictionary<String, RAFFileListEntry> rafSkls;
        public Dictionary<String, RAFFileListEntry> rafSkns;
        public Dictionary<String, RAFFileListEntry> rafTextures;

        private Dictionary<String, FileInfo> skls;
        private Dictionary<String, FileInfo> skns;
        private Dictionary<String, FileInfo> textures;

        public Dictionary<String, LOLModel> models;

        public LOLDirectoryReader()
        {
            root = DEFAULT_ROOT;

            inibinFiles = new List<FileInfo>();
            rafSkls = new Dictionary<String, RAFFileListEntry>();
            rafSkns = new Dictionary<String, RAFFileListEntry>();
            rafTextures = new Dictionary<String, RAFFileListEntry>();

            skls = new Dictionary<String, FileInfo>();
            skns = new Dictionary<String, FileInfo>();
            textures = new Dictionary<String, FileInfo>();

            models = new Dictionary<String,LOLModel>();
        }

        /// <summary>
        /// Call this if LOL was installed in a non-default location.
        /// </summary>
        /// <param name="s">Full path to and including the "Riot Games" folder.</param>
        public void SetRoot(String s)
        {
            root = s;
        }

        public bool Read()
        {
            bool result = true;

            skls.Clear();
            skns.Clear(); 
            textures.Clear();
            inibinFiles.Clear();
            rafSkls.Clear();
            rafSkns.Clear();
            rafTextures.Clear();
            models.Clear();

            // Start from the root and try to read
            // model files and textures.
            try
            {
                DirectoryInfo di = new DirectoryInfo(root);
                foreach (DirectoryInfo d in di.GetDirectories())
                {
                    result = ReadDirectory(d);
                }
            }
            catch
            {
                result = false;
            }

            // Generate model difinitions from the *.inibin files.
            foreach (FileInfo f in inibinFiles)
            {
                InibinFile iniFile;
                bool readResult = InibinReader.ReadCharacterInibin(f, out iniFile);

                if (readResult == true)
                {
                    // Add the models from this .inibin file
                    List<String> modelStrings = iniFile.GetModelStrings();
                    for (int i = 0; i < modelStrings.Count; i += 3)
                    {
                        try
                        {
                            LOLModel model; 
                                
                            bool storeResult = StoreModel(modelStrings[i],
                                modelStrings[i+1], modelStrings[i+2],
                                out model);

                            if (storeResult == true)
                            {
                                // Name the model the name of the texture -
                                // its extension.
                                String name = modelStrings[i + 2];
                                name = name.Substring(0, name.Length - 4);

                                if( models.ContainsKey(name) == false )
                                    models.Add(name, model);
                            }
                        }
                        catch {}
                    }
                }
            }

            return result;
        }

        private bool StoreModel(String skn, String skl, 
            String texture, out LOLModel model)
        {
            bool result = true;

            model = new LOLModel();

            // Find the skn.
            if (skns.ContainsKey(skn))
            {
                model.fileSkn = skns[skn];
            }
            else if (rafSkns.ContainsKey(skn))
            {
                model.rafSkn = rafSkns[skn];
            }
            else
            {
                result = false;
            }

            // Find the skl.
            if (skls.ContainsKey(skl))
            {
                model.fileSkl = skls[skl];
            }
            else if (rafSkls.ContainsKey(skl))
            {
                model.rafSkl = rafSkls[skl];
            }
            else
            {
                result = false;
            }

            // Find the texture.
            if (textures.ContainsKey(texture))
            {
                model.fileTexture = textures[texture];
            }
            else if (rafTextures.ContainsKey(texture))
            {
                model.rafTexture = rafTextures[texture];
            }
            else
            {
                result = false;
            }

            return result;
        }

        public List<String> GetModelNames()
        {
            List<String> names = new List<String>();
            
            foreach (var model in models)
            {
                names.Add(model.Key);
            }

            return names;
        }

        public LOLModel GetModel(String name)
        {
            LOLModel result = null;
            
            foreach(var m in models)
            {
                if (m.Key == name)
                {
                    // This is the model we want.
                    result = m.Value;
                    break;
                }
            }

            return result;
        }

        //
        // Helper functions for reading the directory structure.
        //

        private bool ReadDirectory(DirectoryInfo dir)
        {
            bool result = true;

            // Parse the directory's name and determine what to do.
            switch (dir.Name)
            {
                case "League of Legends":
                    {
                        result = OpenDirectory(dir);
                        break;
                    };
                case "RADS":
                    {
                        result = OpenDirectory(dir);
                        break;
                    };
                case "projects":
                    {
                        result = OpenDirectory(dir);
                        break;
                    };
                case "lol_game_client":
                    {
                        result = OpenDirectory(dir);
                        break;
                    };
                case "filearchives":
                    {
                        result = OpenModelsRoot(dir);
                        break;
                    };
                default:
                    {
                        // Just ignore this directory.
                        break;
                    }
            };

            return result;
        }

        private bool OpenDirectory(DirectoryInfo dir)
        {
            bool result = true;

            // Open this directory and keep reading more directories.
            try
            {
                DirectoryInfo di = new DirectoryInfo(dir.FullName);
                foreach (DirectoryInfo d in di.GetDirectories())
                {
                    result = ReadDirectory(d);
                    if (result == false)
                        break;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private bool OpenModelsRoot(DirectoryInfo dir)
        {
            bool result = true;

            // We've arrived at the root of the model folders.
            try
            {
                DirectoryInfo di = new DirectoryInfo(dir.FullName);
                foreach (DirectoryInfo d in di.GetDirectories())
                {
                    result = OpenGameClientVersion(d);
                    if (result == false) 
                        break;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private bool OpenGameClientVersion(DirectoryInfo dir)
        {
            bool result = true;

            // Read in .raf files and look for model information in them.
            try
            {
                foreach(FileInfo f in dir.GetFiles())
                {
                    result = ReadRAF(f);
                    if (result == false)
                        break;
                }

            }
            catch
            {
                result = false;
            }

            // Look for raw model information contain on the hard drive.
            if( result == true )
            {
                try
                {
                    // Reads in character directories.
                    String dirName = dir.FullName + DEFAULT_MODEL_ROOT;
                    DirectoryInfo di = new DirectoryInfo(dirName);
                    foreach (DirectoryInfo d in di.GetDirectories())
                    {
                        result = OpenModelDirectory(d);
                        if (result == false)
                            break;
                    }
                }
                catch ( Exception e )
                {
                    // If the directory was not found, that's alright.
                    // Sometimes there's no character data in a patch.

                    // TODO: Find a better way to test this.  There has to be a way to check against types.
                    // IE if(e.GetType() == System.IO.DirectoryNotFoundException)
                    if (e.Message.Contains("Could not find a part of the path") == false)
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        private bool OpenModelDirectory(DirectoryInfo dir)
        {
            bool result = true;

            try
            {
                DirectoryInfo di = new DirectoryInfo(dir.FullName);
                foreach (FileInfo f in di.GetFiles())
                {
                    ReadFile(f);
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private void ReadFile(FileInfo f)
        {
            // Look for supported extentions.
            switch (f.Extension)
            {
                case ".skl":
                    {
                        if (skls.ContainsKey(f.Name) == false)
                            skls.Add(f.Name, f);
                        break;
                    }
                case ".skn":
                    {
                        if (skns.ContainsKey(f.Name) == false)
                            skns.Add(f.Name, f);
                        break;
                    }
                case ".DDS":
                case ".dds":
                    {
                        // This is a more complicated case because there are a lot
                        // of textures not used on the models. (For the loading screens, store, etc.)
                        // So, we have to try and reduce the irrelevant ones we load in.
                        if ( f.Name.Contains("LoadScreen") == false &&
                             f.Name.Contains("Loadscreen") == false && 
                             f.Name.Contains("loadscreen") == false )
                        {
                            if( textures.ContainsKey(f.Name) == false)
                                textures.Add(f.Name, f);
                        }
                        break;
                    }
                case ".inibin":
                    {
                        inibinFiles.Add(f);
                        break;
                    }
                default:
                    {
                        //Debug.WriteLine("Excluding File: " + f.Name);
                        break;
                    }
            };
        }

        private bool ReadRAF(FileInfo f)
        {
            bool result = true;

            // Ignore non RAF files.
            if (f.Extension != ".raf")
                return result;

            try
            {
                // Open the archive
                RAFArchive archive = new RAFArchive(f.FullName);

                // Get directory
                RAFDirectoryFile directory = archive.GetDirectoryFile();

                // Get the file list.
                RAFFileList fileList = directory.GetFileList();

                // Get the texture files.
                List<RAFFileListEntry> files = fileList.SearchFileEntries(".dds");
                foreach (RAFFileListEntry e in files)
                {
                    // Try to parse out unwanted textures.
                    if (e.FileName.Contains("LoadScreen") == false &&
                        e.FileName.Contains("Loadscreen") == false &&
                        e.FileName.Contains("loadscreen") == false &&
                        e.FileName.Contains("DATA") == true &&
                        e.FileName.Contains("Characters") == true)
                    {
                        String name = e.FileName;
                        int pos = name.LastIndexOf("/");
                        name = name.Substring(pos + 1);

                        if( rafTextures.ContainsKey(name) == false )
                            rafTextures.Add(name, e);
                    }
                }

                // Get the .skn files
                files = fileList.SearchFileEntries(".skn");
                foreach (RAFFileListEntry e in files)
                {
                    String name = e.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);

                    if (rafSkns.ContainsKey(name) == false)
                        rafSkns.Add(name, e);
                }

                // Get the .skl files.
                files = fileList.SearchFileEntries(".skl");
                foreach (RAFFileListEntry e in files)
                {
                    String name = e.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);

                    if (rafSkns.ContainsKey(name) == false)
                        rafSkns.Add(name, e);
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}
