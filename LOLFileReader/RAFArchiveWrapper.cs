using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RAFlibPlus;

namespace LOLFileReader
{
    public class RAFArchiveWrapper : IArchive
    {
        public RAFArchiveWrapper(RAFMasterFileList rafArch)
        {
            r = rafArch;
        }

        public List<IFileEntry> SearchFileEntries(string[] endsWiths)
        {
            List<RAFMasterFileList.RAFSearchResult> rl = r.SearchFileEntries(endsWiths, RAFMasterFileList.RAFSearchType.All);

            List<IFileEntry> rw = new List<IFileEntry>();
            foreach (RAFMasterFileList.RAFSearchResult e in rl)
            {
                rw.Add(new RAFFileListEntryWrapper(e));
            }

            return rw;
        }

        private RAFMasterFileList r;
    }
}
