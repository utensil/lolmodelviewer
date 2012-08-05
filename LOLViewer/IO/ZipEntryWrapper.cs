using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RAFlibPlus;
using Ionic.Zip;
using Ionic.Crc;

namespace LOLViewer.IO
{
    public class ZIPEntryWrapper : IFileEntry
    {
        public ZIPEntryWrapper(ZipEntry ze, String zipFilePath)
        {
            e = ze;
            rootPath = zipFilePath;
        }

        public String FileName 
        {   get
            {
                return e.FileName;
            }
        }
        public byte[] GetContent()
        {
            byte[] buffer = new byte[e.UncompressedSize];

            using (CrcCalculatorStream s = e.OpenReader())
            {
                int n, totalBytesRead= 0;
                do 
                {
                    n = s.Read(buffer,0, buffer.Length);
                    totalBytesRead+=n;
                }
                while (n>0);

                if (s.Crc != e.Crc)
                    throw new Exception(string.Format("The Zip Entry failed the CRC Check. (0x{0:X8}!=0x{1:X8})", s.Crc, e.Crc));
                if (totalBytesRead != e.UncompressedSize)
                    throw new Exception(string.Format("We read an unexpected number of bytes. ({0}!={1})", totalBytesRead, e.UncompressedSize));
            }

            return buffer;
        }

        public String GetRootPath()
        {
            return rootPath;
        }

        private ZipEntry e;
        private String rootPath;
    }
}
