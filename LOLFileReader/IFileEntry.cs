using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLFileReader
{
    public interface IFileEntry
    {
        /// <summary>
        /// Get the name of the file entry.
        /// </summary>
        /// <returns></returns>
        String FileName { get; }

        String SearchPhrase { get; }

        /// <summary>
        /// Get the content of the file entry as a byte array.
        /// </summary>
        byte[] GetContent();

        /// <summary>
        /// Get the root path of the archive that the file entry belongs to.
        /// </summary>
        String GetRootPath();
    }
}
