using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RAFlibPlus;
using Ionic.Zip;
using CSharpLogger;

namespace LOLFileReader
{
    public class ZIPArchiveWrapper : IArchive
    {
        public ZIPArchiveWrapper(ZipFile zip)
        {
            z = zip;
            //logger = new Logger("ZIPArchiveWrapper.log");
        }

        public List<IFileEntry> SearchFileEntries(String[] endsWiths)
        {
            List<IFileEntry> rw = new List<IFileEntry>();

            foreach (String endsWith in endsWiths)
            {
                IEnumerable<ZipEntry> zl = z.Where(e => e.FileName.ToLower().Contains(endsWith.ToLower()));
            
                foreach (ZipEntry e in zl)
                {
                    //logger.Event("ZipEntry: " + e.FileName);
                    rw.Add(new ZIPEntryWrapper(e, z.Name, endsWith));
                }
            }           

            return rw;
        }

        private ZipFile z;
        //private Logger logger;

    }
}
