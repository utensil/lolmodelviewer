


//
// Logs events.
//



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace LOLViewer
{
    public class EventLogger
    {
        private TextWriter file;

        public EventLogger() {}

        public bool Open( string fileName )
        {
            bool result = true;

            try
            {
                file = new StreamWriter(fileName);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public void Close()
        {
            if (file != null)
            {
                file.Close();
            }
        }

        public void LogError(string error)
        {
            if (file != null)
            {
                file.WriteLine("Error: " + error);
            }
        }

        public void LogWarning(string warning)
        {
            if (file != null)
            {
                file.WriteLine("Warning: " + warning);
            }
        }

        public void LogEvent(string e)
        {
            if (file != null)
            {
                file.WriteLine("Event: " + e);
            }
        }
    }
}
