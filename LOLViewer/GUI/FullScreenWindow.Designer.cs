
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


namespace LOLViewer.GUI
{
    partial class FullScreenWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.glControlMain = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // glControlMain
            // 
            this.glControlMain.BackColor = System.Drawing.Color.Black;
            this.glControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlMain.Location = new System.Drawing.Point(0, 0);
            this.glControlMain.Name = "glControlMain";
            this.glControlMain.Size = new System.Drawing.Size(362, 289);
            this.glControlMain.TabIndex = 7;
            this.glControlMain.VSync = true;
            // 
            // FullScreenWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 289);
            this.Controls.Add(this.glControlMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FullScreenWindow";
            this.Text = "LOLViewer 1.15";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl glControlMain;
    }
}