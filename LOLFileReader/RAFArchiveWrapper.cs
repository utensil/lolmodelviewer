using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RAFlibPlus;

namespace LOLViewer.IO
{
    public class RAFArchiveWrapper : IArchive
    {
        public RAFArchiveWrapper(RAFArchive rafArch)
        {
            r = rafArch;
        }

        public List<IFileEntry> SearchFileEntries(string endsWith)
        {
            List<RAFFileListEntry> rl = r.SearchFileEntries(endsWith, RAFArchive.RAFSearchType.All);

            List<IFileEntry> rw = new List<IFileEntry>();
            foreach (RAFFileListEntry e in rl)
            {
                rw.Add(new RAFFileListEntryWrapper(e));
            }

            return rw;
        }

        private RAFArchive r;
    }
}
