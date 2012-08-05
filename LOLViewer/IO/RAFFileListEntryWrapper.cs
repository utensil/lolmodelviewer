using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RAFlibPlus;

namespace LOLViewer.IO
{
    public class RAFFileListEntryWrapper : IFileEntry
    {
        public RAFFileListEntryWrapper(RAFFileListEntry rafFile)
        {
            e = rafFile;
        }

        public String FileName {
            get
            {
                return e.FileName;
            }        
        }

        public byte[] GetContent()
        {
            try
            {
                return e.GetContent();
            }
            catch (Exception)
            {
            }

            return null;
        }

        public String GetRootPath()
        {
            return e.RAFArchive.RAFFilePath;
        }

        private RAFFileListEntry e;
    }
}
