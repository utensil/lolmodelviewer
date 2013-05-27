using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RAFlibPlus;

namespace LOLFileReader
{
    public class RAFFileListEntryWrapper : IFileEntry
    {
        public RAFFileListEntryWrapper(RAFMasterFileList.RAFSearchResult result)
        {
            e = result.value;
            SearchPhrase = result.searchPhrase;
        }

        public String SearchPhrase { get; private set; }

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
