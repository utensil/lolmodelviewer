using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RAFlibPlus;
using Ionic.Zip;

namespace LOLViewer.IO
{
    public class ZIPArchiveWrapper : IArchive
    {
        public ZIPArchiveWrapper(ZipFile zip)
        {
            z = zip;
        }

        public List<IFileEntry> SearchFileEntries(string endsWith)
        {
            IEnumerable<ZipEntry> zl = z.Where(e => e.FileName.EndsWith(endsWith));

            List<IFileEntry> rw = new List<IFileEntry>();
            foreach (ZipEntry e in zl)
            {
                rw.Add(new ZIPEntryWrapper(e, z.Name));
            }

            return rw;
        }

        private ZipFile z;
    }
}
