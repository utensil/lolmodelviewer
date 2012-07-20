


/*
LOLViewer
Copyright 2011-2012 James Lammlein 

 

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

//
// Handles the initiale loading at program start.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using LOLViewer.IO;

namespace LOLViewer
{
    public partial class LoadingModelsWindow : Form
    {
        public DialogResult result;
        public LOLDirectoryReader reader;
        public EventLogger logger;

        public LoadingModelsWindow()
        {
            result = DialogResult.OK;

            InitializeComponent();

            loadingBackgroundWorker.WorkerReportsProgress = true;
            loadingBackgroundWorker.WorkerSupportsCancellation = true;

            Shown += new EventHandler(OnLoadingModelsWindowShown);

            loadingBackgroundWorker.DoWork += new DoWorkEventHandler(OnWorkerDoWork);
            loadingBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnWorkerWorkCompleted);

            loadingCancelButton.Click += new EventHandler(OnLoadingCancelButtonClick);
        }

        //
        // Window Callbacks
        //

        private void OnLoadingModelsWindowShown(object sender, EventArgs e)
        {
            if (loadingBackgroundWorker.IsBusy == false)
            {
                loadingProgressBar.MarqueeAnimationSpeed = 50;
                loadingBackgroundWorker.RunWorkerAsync();
            }
        }

        //
        // Worker Callbacks
        //

        private void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            bool readResult = reader.Read( logger );
            if (readResult == true)
            {
                logger.LogEvent("Sorting models.");
                result = DialogResult.OK;
                reader.SortModelNames();
            }
            else
            {
                logger.LogError("Failed to read models.");
                result = DialogResult.Abort;
            }
        }

        private void OnWorkerWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        //
        // Button Callbacks
        //

        void OnLoadingCancelButtonClick(object sender, EventArgs e)
        {
            result = DialogResult.Cancel;
            this.Close();
        }
    }
}
