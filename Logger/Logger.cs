

/*
LOLViewer
Copyright 2011-2012 James Lammlein, Adrian Astley

 

This file is part of LOLViewer.

LOLViewer is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

LOLViewer is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with LOLViewer.  If not, see <http://www.gnu.org/licenses/>.

*/




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Timers;
using System.Windows.Forms;

namespace CSharpLogger
{
    public class Logger
    {
        private String filePath;
        private StreamWriter writer;
        private StringBuilder builder;
        private bool flushHold;

        /// <summary>
        /// Allows easy logging of errors, warnings, and events
        /// </summary>
        /// <param name="filePath"></param>
        public Logger(String filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>
        /// Call to close the filestream
        /// </summary>
        public void Close()
        {
            writer.Close();
        }

        private void Flush()
        {
            try
            {
                if (writer == null || writer.BaseStream == null)
                    writer = new StreamWriter(filePath);

                writer.Write(builder);
                builder.Clear();
            }
            catch { }
        }

        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="message">The message to explain the error</param>
        public void Error(String message)
        {
            if (builder == null)
                builder = new StringBuilder();
            builder.AppendLine(DateTime.Now.ToString("MM-dd-yy HH:mm:ss") + " - Error: " + message);
            if (!flushHold)
                Flush();
            //MessageBox.Show(null, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Logs a warning
        /// </summary>
        /// <param name="message">The message to explain the warning</param>
        public void Warning(String message)
        {
            if (builder == null)
                builder = new StringBuilder();
            builder.AppendLine(DateTime.Now.ToString("MM-dd-yy HH:mm:ss") + " - Warning: " + message);
            if (!flushHold)
                Flush();

            //MessageBox.Show(null, message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Logs an event
        /// </summary>
        /// <param name="message">The message to explain the event</param>
        public void Event(String message)
        {
            if (builder == null)
                builder = new StringBuilder();
            builder.AppendLine(DateTime.Now.ToString("MM-dd-yy HH:mm:ss") + " - Event: " + message);
            if (!flushHold)
                Flush();

            //MessageBox.Show(null, e, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Stops all filewriting and holds all logging in a buffer until RestartFlushes() is called
        /// </summary>
        public void HoldFlushes()
        {
            flushHold = true;
        }

        /// <summary>
        /// Logging will flush to file after each log call. Forces an immediate flush
        /// </summary>
        public void RestartFlushes()
        {
            flushHold = false;
            Flush();
        }
    }
}
