


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

        // Don't clear me. This is a bug work around.
        public Dictionary<String, RAFArchive> rafArchives;

        public List<FileInfo> inibinFiles;
        public List<RAFFileListEntry> rafInibins;
        public Dictionary<String, FileInfo> anmListFiles;
        public Dictionary<String, RAFFileListEntry> anmListrafs;
        public Dictionary<String, FileInfo> animationFiles;
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

            rafArchives = new Dictionary<String, RAFArchive>();
            rafInibins = new List<RAFFileListEntry>();

            inibinFiles = new List<FileInfo>();
            anmListFiles = new Dictionary<String, FileInfo>();
            anmListrafs = new Dictionary<String, RAFFileListEntry>();
            animationFiles = new Dictionary<String, FileInfo>();
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
            rafInibins.Clear();
            rafSkls.Clear();
            rafSkns.Clear();
            rafTextures.Clear();
            models.Clear();
            animationFiles.Clear();
            anmListFiles.Clear();
            anmListrafs.Clear();
            

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
            //foreach (RAFFileListEntry f in rafInibins)
            //{
            //    InibinFile iniFile = new InibinFile();
            //    bool readResult = InibinReader.ReadCharacterInibin(f, ref iniFile);

            //    if (readResult == true)
            //    {
            //        // Add the models from this .inibin file
            //        List<ModelDefinition> modelDefs = iniFile.GetModelStrings();
            //        for (int i = 0; i < modelDefs.Count; ++i)
            //        {
            //            try
            //            {
            //                LOLModel model;

            //                bool storeResult = StoreModel(modelDefs[i], out model);
            //                if (storeResult == true)
            //                {
            //                    // Try to store animations for model as well
            //                    //storeResult = StoreAnimations(ref model);
            //                }

            //                if (storeResult == true)
            //                {
            //                    // Name the model the name of the texture -
            //                    // its extension.
            //                    String name = modelDefs[i].tex;
            //                    name = name.Substring(0, name.Length - 4);

            //                    if (models.ContainsKey(name) == false)
            //                        models.Add(name, model);
            //                }
            //            }
            //            catch { }
            //        }
            //    }
            //}

            // Generate model difinitions from the *.inibin files.
            foreach (FileInfo f in inibinFiles)
            {
                InibinFile iniFile = new InibinFile();
                bool readResult = InibinReader.ReadCharacterInibin(f, ref iniFile);

                if (readResult == true)
                {
                    // Add the models from this .inibin file
                    List<ModelDefinition> modelDefs = iniFile.GetModelStrings();
                    for (int i = 0; i < modelDefs.Count; ++i)
                    {
                        try
                        {
                            LOLModel model; 
                                
                            bool storeResult = StoreModel(modelDefs[i], out model);
                            if (storeResult == true)
                            {
                                // Try to store animations for model as well
                                storeResult = StoreAnimations(ref model);
                            }

                            if (storeResult == true)
                            {
                                // Name the model the name of the texture -
                                // its extension.
                                String name = modelDefs[i].tex;
                                name = name.Substring(0, name.Length - 4);

                                if (models.ContainsKey(name) == false)
                                    models.Add(name, model);
                            }
                        }
                        catch {}
                    }
                }
            }

            return result;
        }

        private bool StoreModel(ModelDefinition def, out LOLModel model)
        {
            model = new LOLModel();
            model.skinNumber = def.skin;
            model.anmList = def.anmListKey;

            // Find the skn.
            if (skns.ContainsKey(def.skn))
            {
                model.fileSkn = skns[def.skn];
            }
            else if (rafSkns.ContainsKey(def.skn))
            {
                model.rafSkn = rafSkns[def.skn];
            }
            else
            {
               return false;
            }

            // Find the skl.
            if (skls.ContainsKey(def.skl))
            {
                model.fileSkl = skls[def.skl];
            }
            else if (rafSkls.ContainsKey(def.skl))
            {
                model.rafSkl = rafSkls[def.skl];
            }
            else
            {
                return false;
            }

            // Find the texture.
            if (textures.ContainsKey(def.tex))
            {
                model.fileTexture = textures[def.tex];
            }
            else if (rafTextures.ContainsKey(def.tex))
            {
                model.rafTexture = rafTextures[def.tex];
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool StoreAnimations(ref LOLModel model)
        {
            bool result = true;

            Dictionary<String, String> animations =
                new Dictionary<String, String>();

            result = ANMListReader.ReadAnimationList(model.skinNumber,
               anmListFiles[ model.anmList ], ref animations);

            if (result == true)
            {
                // Store the animations in the model.
                foreach (var a in animations)
                {
                    if( animationFiles.ContainsKey(a.Value) == true &&
                        model.animations.ContainsKey(a.Key) == false )
                    {
                        model.animations.Add(a.Key, animationFiles[a.Value]);
                    }
                }
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

                    // Sanity
                    if (di.Exists == false)
                        return true;

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
                // Read all files in the directory.
                DirectoryInfo di = new DirectoryInfo(dir.FullName);
                foreach (FileInfo f in di.GetFiles())
                {
                    ReadFile(f);
                }

                // Read in animations from the "Animations" subdirectory.
                foreach (DirectoryInfo d in di.GetDirectories())
                {
                    if (d.Name == "Animations" || d.Name == "animations")
                    {
                        foreach (FileInfo f in d.GetFiles())
                        {
                            ReadFile(f);
                        }
                    }
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
                        String name = f.Name;
                        name = name.ToLower();
                        if (skls.ContainsKey(name) == false)
                        {
                            skls.Add(name, f);
                        }
                        break;
                    }
                case ".skn":
                    {
                        String name = f.Name;
                        name = name.ToLower();
                        if (skns.ContainsKey(name) == false)
                        {
                            skns.Add(name, f);
                        }
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
                            String name = f.Name;
                            name = name.ToLower();
                            if (textures.ContainsKey(name) == false)
                            {
                                textures.Add(name, f);
                            }
                        }
                        break;
                    }
                case ".inibin":
                    {
                        inibinFiles.Add(f);
                        break;
                    }
                case ".list":
                    {
                        // Sanity. TODO: What do we do on error? Just ignore?
                        if (anmListFiles.ContainsKey(f.Directory.Name) == false)
                        {
                            anmListFiles.Add(f.Directory.Name, f);
                        }
                        break;
                    }
                case ".anm":
                    {
                        // Remove the file extension for the key.
                        // This way it matches the values in Animations.list.
                        String key = f.Name;
                        key = key.Remove(key.Length - 4);
                        key = key.ToLower();

                        if (animationFiles.ContainsKey(key) == false)
                        {
                            animationFiles.Add(key, f);
                        }
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

                // TODO: Bug. These archives don't release their file handle and
                // there's no function to close them.
                // So, for now, let's just hold onto them incase we need them later.

                RAFArchive archive = null;
                if( rafArchives.ContainsKey(f.FullName) == true )
                {
                    archive = rafArchives[f.FullName];
                }
                else
                {
                    archive = new RAFArchive(f.FullName);
                    rafArchives.Add(f.FullName, archive);
                }
                 

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
                        name = name.ToLower();

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
                    name = name.ToLower();

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
                    name = name.ToLower();

                    if (rafSkls.ContainsKey(name) == false)
                        rafSkls.Add(name, e);
                }

                // There's .inibin files in here too.
                files = fileList.SearchFileEntries(".inibin");
                foreach (RAFFileListEntry e in files)
                {
                    String name = e.FileName;
                    if (name.Contains("Characters") == true && // try to only read required files
                        name.Contains("Scripts") == false && 
                        name.Contains("RecItems") == false )
                    {
                        rafInibins.Add(e);
                    }

                }

                files = fileList.SearchFileEntries("Animations.list");
                foreach (RAFFileListEntry e in files)
                {
                    String name = e.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);
                    name = name.ToLower();

                    if( anmListrafs.ContainsKey(name) == false )
                        anmListrafs.Add(name, e);
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
