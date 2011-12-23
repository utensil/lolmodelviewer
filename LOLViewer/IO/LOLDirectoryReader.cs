


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
// Extracts model and texture information
// from the League of Legends directory
// structure.
//

//#define VERBOSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

using RAFLib;

namespace LOLViewer.IO
{
    public class LOLDirectoryReader
    {
        public const String DEFAULT_ROOT = "C:/Riot Games/League of Legends";
        public const String DEFAULT_MODEL_ROOT = "/DATA/Characters";
        public const String DEFAULT_RAF_DIRECTORY_ONE = "DATA";
        public const String DEFAULT_RAF_DIRECTORY_TWO = "Characters";

        public const String DEFAULT_EXTRACTED_TEXTURES_ROOT = "content/textures/";
        public String root;

        public Dictionary<String, RAFFileListEntry> skls;
        public Dictionary<String, RAFFileListEntry> skns;
        public Dictionary<String, RAFFileListEntry> textures;

        public List<RAFFileListEntry> inibins;
        public Dictionary<String, RAFFileListEntry> animationLists;
        public Dictionary<String, RAFFileListEntry> animations;


        public Dictionary<String, LOLModel> models;

        public LOLDirectoryReader()
        {
            root = DEFAULT_ROOT;

            inibins = new List<RAFFileListEntry>();

            animationLists = new Dictionary<String, RAFFileListEntry>();
            animations = new Dictionary<String, RAFFileListEntry>();
            
            skls = new Dictionary<String, RAFFileListEntry>();
            skns = new Dictionary<String, RAFFileListEntry>();
            textures = new Dictionary<String, RAFFileListEntry>();

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

            // Clear old data.

            models.Clear();

            skls.Clear();
            skns.Clear();
            textures.Clear();

            inibins.Clear();
            animationLists.Clear();
            animations.Clear();           

            DirectoryInfo rootDir = null;
            try
            {
                rootDir = new DirectoryInfo(root);
            }
            catch 
            {
                return false;
            }

            //
            // Directory Guard.
            // Assume the user selected the wrong LOL installation directory.
            //
            // Possibly correct directories:
            // - Name contains "Riot Games"  ...\Riot Games\League of Legends\RADS...
            // - Name contains "League of Legends" ...\League of Legends\RADS...
            // - Child Directory Contains "League of Legends" ...\Renamed\League of Legends\RADS...
            // - Child Directory Contains "RADS" ...\Renamed\RADS...
            //
            // The only case not really handled is something like
            // ...\Riot Games\Renamed\RADS... which no one better have!  And, honestly, this case should fail
            // because someone could have a new Riot game installed which is not LOL.
            //

            bool isRootSelected = false;

            // Deafult directory installations.
            if (rootDir.Name.Contains("League of Legends") == true ||
                rootDir.Name.Contains("Riot Games") == true ||
                rootDir.Name.Contains("RADS") == true ||
                rootDir.Name.Contains("rads") == true )
            {
                isRootSelected = true;
            }
            // Selected a renamed "Riot Games" directory.
            else if (ContainsDirectory(rootDir, "League of Legends") == true)
            {
                isRootSelected = true;
            }
            // Selected a rename "League of Legends" directory.
            else if (ContainsDirectory(rootDir, "RADS") == true || 
                     ContainsDirectory(rootDir, "rads") == true )
            {
                isRootSelected = true;
            }

            // If we didn't find the root, just bail.
            if (isRootSelected == false)
            {
                return false;
            }

            //
            // Start from the root and try to read
            // model files and textures.
            //
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

            foreach( RAFFileListEntry f in inibins )
            {
                InibinFile iniFile = new InibinFile();
                bool readResult = InibinReader.ReadCharacterInibin(f, ref iniFile);

                if (readResult == true)
                {
                    // Add the models from this .inibin file
                    List<ModelDefinition> modelDefs = iniFile.GetModelStrings();
                    for (int j = 0; j < modelDefs.Count; ++j)
                    {
                        try
                        {
                            LOLModel model;
                            bool storeResult = StoreModel(modelDefs[j], out model);

                            if (storeResult == true)
                            {
                                // Try to store animations for model as well
                                storeResult = StoreAnimations(ref model);
                            }

                            if (storeResult == true)
                            {
                                // Name the model after the parent directory
                                // of the .inibin plus the name from the .inibin.
                                // Some things overlap without both.
                                String name = modelDefs[j].name;

                                String directoryName = f.FileName;
                                int pos = directoryName.LastIndexOf("/");
                                directoryName = directoryName.Remove(pos);
                                pos = directoryName.LastIndexOf("/");
                                directoryName = directoryName.Substring(pos + 1);

                                // Sometimes the name from the .inibin file is "".
                                // So, just name it after the directory
                                if (name == "")
                                {
                                    name = directoryName + "/" + directoryName;
                                }
                                else
                                {
                                    name = directoryName + "/" + name;
                                }

                                if (models.ContainsKey(name) == false)
                                    models.Add(name, model);
                            }
                        }
                        catch { }
                    }
                }
            }

            return result;
        }

        private bool StoreModel(ModelDefinition def, out LOLModel model)
        {
            model = new LOLModel();
            model.skinNumber = def.skin;
            model.animationList = def.anmListKey.ToLower();

            // Find the skn.
            if (skns.ContainsKey(def.skn))
            {
                model.skn = skns[def.skn];
            }
            else
            {
               return false;
            }

            // Find the skl.
            if (skls.ContainsKey(def.skl))
            {
                model.skl = skls[def.skl];
            }
            else
            {
                return false;
            }

            // Find the texture.
            if (textures.ContainsKey(def.tex))
            {
                model.texture = textures[def.tex];
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

            Dictionary<String, String> animationStrings =
                new Dictionary<String, String>();

            // Sanity
            if (animationLists.ContainsKey(model.animationList) == true)
            {
                result = ANMListReader.ReadAnimationList(model.skinNumber - 1, // indexing in animations.list assumes the original skin to be -1
                    animationLists[model.animationList], ref animationStrings);
            }

            if (result == true)
            {
                // Store the animations in the model.
                foreach (var a in animationStrings)
                {
                    if (animations.ContainsKey(a.Value) == true &&
                        model.animations.ContainsKey(a.Key) == false)
                    {
                        model.animations.Add(a.Key, animations[a.Value]);
                    }
                }
            }

            return result;
        }

        public void SortModelNames()
        {
            IEnumerable<KeyValuePair<String, LOLModel>> alphabetical = models.OrderBy(model => model.Key);

            Dictionary<String, LOLModel> temp = new Dictionary<String, LOLModel>();
            foreach (var m in alphabetical)
            {
                temp.Add(m.Key, m.Value);
            }

            models.Clear();
            models = temp;
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

            //
            // Parse the directory's name and determine what to do.
            //

            // Odd case
            // US Client: "League of Legends"
            // EU Client: "League of Legends EU"
            // etc.
            if (dir.Name.Contains("League of Legends") == true)
            {
                result = OpenDirectory(dir);
            }
            else
            {
                //
                // Standard Case
                //

                switch (dir.Name)
                {
                    case "rads":
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

            }

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
#if VERBOSE
            catch(Exception e)
            {
                result = false;
                ErrorMessage(e.Message);
            }
#else
            catch
            {
                result = false;
            }
#endif

            return result;
        }

        private bool OpenModelsRoot(DirectoryInfo dir)
        {
            bool result = true;

            // We've arrived at the root of the model folders.
            try
            {
                DirectoryInfo di = new DirectoryInfo(dir.FullName);

                // Read directories in reverse order to prioritize newer files.
                DirectoryInfo[] children = di.GetDirectories();
                for (int i = 1; i <= children.Length; ++i)
                {
                    result = OpenGameClientVersion(children[children.Length - i]);
                    if (result == false)
                        break;
                }
            }
#if VERBOSE
            catch(Exception e)
            {
                result = false;
                ErrorMessage(e.Message);
            }
#else
            catch
            {
                result = false;
            }
#endif

            return result;
        }

        private bool OpenGameClientVersion(DirectoryInfo dir)
        {
            bool result = true;

            // Read in .raf files and look for model information in them.
            RAFArchive archive = null;
            try
            {
                foreach(FileInfo f in dir.GetFiles())
                {
                    // Ignore non RAF files.
                    if (f.Extension != ".raf")
                        continue;
                    
                    // ReadRAF() opens the archive.
                    result = ReadRAF(f, ref archive);
                    if (result == false)
                    {
                        break;
                    }
                }
            }
#if VERBOSE
            catch(Exception e)
            {
                result = false;
                ErrorMessage(e.Message);
            }
#else
            catch
            {
                result = false;
            }
#endif

            // Note: archive will always equal the last RAFArchive in the directory.
            // So, we alawys apend files to the last one if there's more than one in a directory.
            // However, it really shouldn't matter which one they are appended to as long as their
            // in memory.

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
                        result = OpenModelDirectory(d, ref archive, DEFAULT_MODEL_ROOT);
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
#if VERBOSE
                        ErrorMessage(e.Message);
#endif
                    }
                }
            }

            // If the archive was opened, we need to release the file pointer.
            if (archive != null)
            {
                archive.GetDataFileContentStream().Close();
            }

            return result;
        }

        private bool ReadRAF(FileInfo f, ref RAFArchive archive)
        {
            bool result = true;

            try
            {
                // Open the archive
                archive = new RAFArchive(f.FullName);


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
                        e.FileName.Contains("circle") == false &&
                        e.FileName.Contains("square") == false &&
                        e.FileName.Contains("DATA") == true &&
                        e.FileName.Contains("Characters") == true)
                    {
                        String name = e.FileName;
                        int pos = name.LastIndexOf("/");
                        name = name.Substring(pos + 1);
                        name = name.ToLower();

                        if (textures.ContainsKey(name) == false)
                            textures.Add(name, e);
                    }
                }

                files = fileList.SearchFileEntries(".DDS");
                foreach (RAFFileListEntry e in files)
                {
                    // Try to parse out unwanted textures.
                    if (e.FileName.Contains("LoadScreen") == false &&
                        e.FileName.Contains("Loadscreen") == false &&
                        e.FileName.Contains("loadscreen") == false &&
                        e.FileName.Contains("circle") == false &&
                        e.FileName.Contains("square") == false &&
                        e.FileName.Contains("DATA") == true &&
                        e.FileName.Contains("Characters") == true)
                    {
                        String name = e.FileName;
                        int pos = name.LastIndexOf("/");
                        name = name.Substring(pos + 1);
                        name = name.ToLower();

                        if (textures.ContainsKey(name) == false)
                            textures.Add(name, e);
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

                    if (skns.ContainsKey(name) == false)
                        skns.Add(name, e);
                }

                // Get the .skl files.
                files = fileList.SearchFileEntries(".skl");
                foreach (RAFFileListEntry e in files)
                {
                    String name = e.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);
                    name = name.ToLower();

                    if (skls.ContainsKey(name) == false)
                        skls.Add(name, e);
                }

                // There's .inibin files in here too.
                files = fileList.SearchFileEntries(".inibin");
                foreach (RAFFileListEntry e in files)
                {
                    String name = e.FileName;
                    if (name.Contains("Characters") == true) // try to only read required files
                    {
                        inibins.Add(e);
                    }

                }

                // Read in animation lists
                files = fileList.SearchFileEntries("Animations.list");
                foreach (RAFFileListEntry e in files)
                {
                    String name = e.FileName;

                    // Remove the file name.
                    int pos = name.LastIndexOf("/");
                    name = name.Remove(pos);

                    // Remove proceeding directories.
                    pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);
                    name = name.ToLower();

                    // Name is the parent directory.
                    if (animationLists.ContainsKey(name) == false)
                        animationLists.Add(name, e);
                }

                // Read in animations
                files = fileList.SearchFileEntries(".anm");
                foreach (RAFFileListEntry e in files)
                {
                    String name = e.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);
                    name = name.Remove(name.Length - 4);
                    name = name.ToLower();

                    if (animations.ContainsKey(name) == false)
                        animations.Add(name, e);
                }
            }
#if VERBOSE
            catch(Exception e)
            {
                result = false;
                ErrorMessage(e.Message);
            }
#else
            catch
            {
                result = false;
            }
#endif

            return result;
        }

        private bool OpenModelDirectory(DirectoryInfo dir, ref RAFArchive archive, String directoryOffset)
        {
            bool result = true;

            try
            {
                // Read all files in the directory.
                DirectoryInfo di = new DirectoryInfo(dir.FullName);
                foreach (FileInfo f in di.GetFiles())
                {
                    ReadFile(f, ref archive, directoryOffset + "/" + di.Name);
                }

                // Read in animations from the "Animations" subdirectory.
                foreach (DirectoryInfo d in di.GetDirectories())
                {
                    if (d.Name == "Animations" || d.Name == "animations")
                    {
                        foreach (FileInfo f in d.GetFiles())
                        {
                            ReadFile(f, ref archive, directoryOffset + "/" + di.Name + "/" + d.Name);
                        }
                    }
                }
            }
#if VERBOSE
            catch(Exception e)
            {
                result = false;
                ErrorMessage(e.Message);
            }
#else
            catch
            {
                result = false;
            }
#endif

            return result;
        }

        private void ReadFile(FileInfo f, ref RAFArchive archive, String directoryOffset)
        {
            String filePath = directoryOffset + "/" + f.Name;

            // Look for supported extentions.
            switch (f.Extension)
            {
                case ".skl":
                    {
                        bool result = archive.InsertFile(filePath, File.ReadAllBytes(f.FullName), null);
                        if (result == true)
                        {
                            RAFDirectoryFile dir = archive.GetDirectoryFile();
                            RAFFileList list = dir.GetFileList();
                            RAFFileListEntry fileEntry = list.GetFileEntry(filePath);

                            String name = f.Name;
                            name = name.ToLower();

                            if( skls.ContainsKey( name ) == false )
                                skls.Add(name, fileEntry);
                        }
                        break;
                    }
                case ".skn":
                    {
                        bool result = archive.InsertFile(filePath, File.ReadAllBytes(f.FullName), null);
                        if (result == true)
                        {
                            RAFDirectoryFile dir = archive.GetDirectoryFile();
                            RAFFileList list = dir.GetFileList();
                            RAFFileListEntry fileEntry = list.GetFileEntry(filePath);

                            String name = f.Name;
                            name = name.ToLower();

                            if (skns.ContainsKey(name) == false)
                                skns.Add(name, fileEntry);
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
                             f.Name.Contains("loadscreen") == false && 
                             f.Name.Contains("circle") == false &&
                             f.Name.Contains("square") == false )
                        {
                            bool result = archive.InsertFile(filePath, File.ReadAllBytes(f.FullName), null);
                            if (result == true)
                            {
                                RAFDirectoryFile dir = archive.GetDirectoryFile();
                                RAFFileList list = dir.GetFileList();
                                RAFFileListEntry fileEntry = list.GetFileEntry(filePath);

                                String name = f.Name;
                                name = name.ToLower();

                                if (textures.ContainsKey(name) == false)
                                    textures.Add(name, fileEntry);
                            }
                        }
                        break;
                    }
                case ".inibin":
                    {
                        bool result = archive.InsertFile(filePath, File.ReadAllBytes(f.FullName), null);
                        if (result == true)
                        {
                            RAFDirectoryFile dir = archive.GetDirectoryFile();
                            RAFFileList list = dir.GetFileList();
                            RAFFileListEntry fileEntry = list.GetFileEntry(filePath);

                            inibins.Add(fileEntry);
                        }
                        break;
                    }
                case ".list":
                    {
                        bool result = archive.InsertFile(filePath, File.ReadAllBytes(f.FullName), null);
                        if (result == true)
                        {
                            RAFDirectoryFile dir = archive.GetDirectoryFile();
                            RAFFileList list = dir.GetFileList();
                            RAFFileListEntry fileEntry = list.GetFileEntry(filePath);

                            String name = fileEntry.FileName;

                            // Remove the file name.
                            int pos = name.LastIndexOf("/");
                            name = name.Remove(pos);

                            // Remove proceeding directories.
                            pos = name.LastIndexOf("/");
                            name = name.Substring(pos + 1);
                            name = name.ToLower();

                            if (animationLists.ContainsKey(name) == false)
                                animationLists.Add(name, fileEntry);
                        }
                        break;
                    }
                case ".anm":
                    {
                        bool result = archive.InsertFile(filePath, File.ReadAllBytes(f.FullName), null);
                        if (result == true)
                        {
                            RAFDirectoryFile dir = archive.GetDirectoryFile();
                            RAFFileList list = dir.GetFileList();
                            RAFFileListEntry fileEntry = list.GetFileEntry(filePath);

                            String name = f.Name;
                            name = name.ToLower();

                            // Remove the file extension for the key.
                            // This way it matches the values in Animations.list.
                            name = name.Remove(name.Length - 4);

                            if (animations.ContainsKey(name) == false)
                                animations.Add(name, fileEntry);
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

        //
        // Helper Functions
        //
        private bool ContainsDirectory(DirectoryInfo parent, String child)
        {
            bool result = false;

            // Sanity.
            if (parent.Exists == true)
            {
                try
                {
                    foreach (DirectoryInfo d in parent.GetDirectories())
                    {
                        if (d.Name.Contains(child) == true) // contains used for the LOL NA - LOL EU case
                        {
                            result = true;
                            break;
                        }
                    }
                }
                catch { }

            }

            return result;
        }

        //
        // Helper Debugging Funtion
        //
#if VERBOSE
        private void  ErrorMessage(String message)
        {
            MessageBox.Show(message);
        }
#endif
    }
}


