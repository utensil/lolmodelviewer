


//
// Logs events.
//



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows.Forms;

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
            //MessageBox.Show(null, error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void LogWarning(string warning)
        {
            if (file != null)
            {
                file.WriteLine("Warning: " + warning);
            }

            //MessageBox.Show(null, warning, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void LogEvent(string e)
        {
            if (file != null)
            {
                file.WriteLine("Event: " + e);
            }

            //MessageBox.Show(null, e, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
