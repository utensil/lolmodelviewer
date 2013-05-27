

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

namespace LOLViewer.GUI
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.mainWindowMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMainWindowMenuStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.readMainMenuStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readDirectoryMainMenuStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMainWindowMenuStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainWindowStatusStrip = new System.Windows.Forms.StatusStrip();
            this.mainWindowStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainWindowProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.glControlTabControlSplitContainer = new System.Windows.Forms.SplitContainer();
            this.enableAnimationButton = new System.Windows.Forms.Button();
            this.animationOptionsLabel = new System.Windows.Forms.Label();
            this.currentAnimationComboBox = new System.Windows.Forms.ComboBox();
            this.playAnimationButton = new System.Windows.Forms.Button();
            this.timelineTrackBar = new System.Windows.Forms.TrackBar();
            this.fullscreenButton = new System.Windows.Forms.Button();
            this.backgroundColorButton = new System.Windows.Forms.Button();
            this.resetCameraButton = new System.Windows.Forms.Button();
            this.renderingOptionsLabel = new System.Windows.Forms.Label();
            this.glControlMain = new OpenTK.GLControl();
            this.glTabModelListBoxSplitContainer = new System.Windows.Forms.SplitContainer();
            this.modelListBox = new System.Windows.Forms.ListBox();
            this.modelSearchBox = new System.Windows.Forms.TextBox();
            this.modelSearchLabel = new System.Windows.Forms.Label();
            this.mainWindowMenuStrip.SuspendLayout();
            this.mainWindowStatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.glControlTabControlSplitContainer)).BeginInit();
            this.glControlTabControlSplitContainer.Panel1.SuspendLayout();
            this.glControlTabControlSplitContainer.Panel2.SuspendLayout();
            this.glControlTabControlSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timelineTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.glTabModelListBoxSplitContainer)).BeginInit();
            this.glTabModelListBoxSplitContainer.Panel1.SuspendLayout();
            this.glTabModelListBoxSplitContainer.Panel2.SuspendLayout();
            this.glTabModelListBoxSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainWindowMenuStrip
            // 
            this.mainWindowMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMainWindowMenuStrip,
            this.aboutMainWindowMenuStrip});
            this.mainWindowMenuStrip.Location = new System.Drawing.Point(10, 9);
            this.mainWindowMenuStrip.Name = "mainWindowMenuStrip";
            this.mainWindowMenuStrip.Size = new System.Drawing.Size(772, 24);
            this.mainWindowMenuStrip.TabIndex = 7;
            this.mainWindowMenuStrip.Text = "mainWindowMenuStrip";
            // 
            // fileMainWindowMenuStrip
            // 
            this.fileMainWindowMenuStrip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.readMainMenuStripItem,
            this.readDirectoryMainMenuStripItem,
            this.fileToolStripSeparator,
            this.exitToolStripMenuItem});
            this.fileMainWindowMenuStrip.Name = "fileMainWindowMenuStrip";
            this.fileMainWindowMenuStrip.Size = new System.Drawing.Size(41, 20);
            this.fileMainWindowMenuStrip.Text = "File";
            // 
            // readMainMenuStripItem
            // 
            this.readMainMenuStripItem.Name = "readMainMenuStripItem";
            this.readMainMenuStripItem.Size = new System.Drawing.Size(310, 22);
            this.readMainMenuStripItem.Text = "Read...";
            // 
            // readDirectoryMainMenuStripItem
            // 
            this.readDirectoryMainMenuStripItem.Name = "readDirectoryMainMenuStripItem";
            this.readDirectoryMainMenuStripItem.Size = new System.Drawing.Size(310, 22);
            this.readDirectoryMainMenuStripItem.Text = "Read from Default Installation Directory";
            // 
            // fileToolStripSeparator
            // 
            this.fileToolStripSeparator.Name = "fileToolStripSeparator";
            this.fileToolStripSeparator.Size = new System.Drawing.Size(307, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(310, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // aboutMainWindowMenuStrip
            // 
            this.aboutMainWindowMenuStrip.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.aboutMainWindowMenuStrip.Name = "aboutMainWindowMenuStrip";
            this.aboutMainWindowMenuStrip.Size = new System.Drawing.Size(47, 20);
            this.aboutMainWindowMenuStrip.Text = "About";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.aboutToolStripMenuItem.Text = "About LOLViewer...";
            // 
            // mainWindowStatusStrip
            // 
            this.mainWindowStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mainWindowStatusLabel,
            this.mainWindowProgressBar});
            this.mainWindowStatusStrip.Location = new System.Drawing.Point(10, 491);
            this.mainWindowStatusStrip.Name = "mainWindowStatusStrip";
            this.mainWindowStatusStrip.Size = new System.Drawing.Size(772, 22);
            this.mainWindowStatusStrip.TabIndex = 8;
            // 
            // mainWindowStatusLabel
            // 
            this.mainWindowStatusLabel.Name = "mainWindowStatusLabel";
            this.mainWindowStatusLabel.Size = new System.Drawing.Size(95, 17);
            this.mainWindowStatusLabel.Text = "Initializing...";
            // 
            // mainWindowProgressBar
            // 
            this.mainWindowProgressBar.Name = "mainWindowProgressBar";
            this.mainWindowProgressBar.Size = new System.Drawing.Size(100, 16);
            this.mainWindowProgressBar.Visible = false;
            // 
            // glControlTabControlSplitContainer
            // 
            this.glControlTabControlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlTabControlSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.glControlTabControlSplitContainer.Name = "glControlTabControlSplitContainer";
            // 
            // glControlTabControlSplitContainer.Panel1
            // 
            this.glControlTabControlSplitContainer.Panel1.Controls.Add(this.enableAnimationButton);
            this.glControlTabControlSplitContainer.Panel1.Controls.Add(this.animationOptionsLabel);
            this.glControlTabControlSplitContainer.Panel1.Controls.Add(this.currentAnimationComboBox);
            this.glControlTabControlSplitContainer.Panel1.Controls.Add(this.playAnimationButton);
            this.glControlTabControlSplitContainer.Panel1.Controls.Add(this.timelineTrackBar);
            this.glControlTabControlSplitContainer.Panel1.Controls.Add(this.fullscreenButton);
            this.glControlTabControlSplitContainer.Panel1.Controls.Add(this.backgroundColorButton);
            this.glControlTabControlSplitContainer.Panel1.Controls.Add(this.resetCameraButton);
            this.glControlTabControlSplitContainer.Panel1.Controls.Add(this.renderingOptionsLabel);
            this.glControlTabControlSplitContainer.Panel1MinSize = 120;
            // 
            // glControlTabControlSplitContainer.Panel2
            // 
            this.glControlTabControlSplitContainer.Panel2.Controls.Add(this.glControlMain);
            this.glControlTabControlSplitContainer.Panel2MinSize = 400;
            this.glControlTabControlSplitContainer.Size = new System.Drawing.Size(606, 458);
            this.glControlTabControlSplitContainer.SplitterDistance = 126;
            this.glControlTabControlSplitContainer.TabIndex = 9;
            this.glControlTabControlSplitContainer.TabStop = false;
            // 
            // enableAnimationButton
            // 
            this.enableAnimationButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.enableAnimationButton.Location = new System.Drawing.Point(8, 127);
            this.enableAnimationButton.Name = "enableAnimationButton";
            this.enableAnimationButton.Size = new System.Drawing.Size(109, 21);
            this.enableAnimationButton.TabIndex = 5;
            this.enableAnimationButton.Text = "Enable";
            this.enableAnimationButton.UseVisualStyleBackColor = true;
            // 
            // animationOptionsLabel
            // 
            this.animationOptionsLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.animationOptionsLabel.AutoSize = true;
            this.animationOptionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.animationOptionsLabel.Location = new System.Drawing.Point(9, 110);
            this.animationOptionsLabel.Name = "animationOptionsLabel";
            this.animationOptionsLabel.Size = new System.Drawing.Size(109, 13);
            this.animationOptionsLabel.TabIndex = 18;
            this.animationOptionsLabel.Text = "Animation Options";
            // 
            // currentAnimationComboBox
            // 
            this.currentAnimationComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.currentAnimationComboBox.FormattingEnabled = true;
            this.currentAnimationComboBox.Location = new System.Drawing.Point(3, 203);
            this.currentAnimationComboBox.Name = "currentAnimationComboBox";
            this.currentAnimationComboBox.Size = new System.Drawing.Size(117, 20);
            this.currentAnimationComboBox.TabIndex = 8;
            // 
            // playAnimationButton
            // 
            this.playAnimationButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.playAnimationButton.Location = new System.Drawing.Point(8, 154);
            this.playAnimationButton.Name = "playAnimationButton";
            this.playAnimationButton.Size = new System.Drawing.Size(109, 21);
            this.playAnimationButton.TabIndex = 6;
            this.playAnimationButton.Text = "Play";
            this.playAnimationButton.UseVisualStyleBackColor = true;
            // 
            // timelineTrackBar
            // 
            this.timelineTrackBar.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.timelineTrackBar.LargeChange = 10;
            this.timelineTrackBar.Location = new System.Drawing.Point(3, 181);
            this.timelineTrackBar.Maximum = 100;
            this.timelineTrackBar.Name = "timelineTrackBar";
            this.timelineTrackBar.Size = new System.Drawing.Size(119, 45);
            this.timelineTrackBar.TabIndex = 7;
            this.timelineTrackBar.TabStop = false;
            this.timelineTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // fullscreenButton
            // 
            this.fullscreenButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.fullscreenButton.Location = new System.Drawing.Point(8, 22);
            this.fullscreenButton.Name = "fullscreenButton";
            this.fullscreenButton.Size = new System.Drawing.Size(109, 21);
            this.fullscreenButton.TabIndex = 1;
            this.fullscreenButton.Text = "Fullscreen";
            this.fullscreenButton.UseVisualStyleBackColor = true;
            // 
            // backgroundColorButton
            // 
            this.backgroundColorButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.backgroundColorButton.Location = new System.Drawing.Point(8, 76);
            this.backgroundColorButton.Name = "backgroundColorButton";
            this.backgroundColorButton.Size = new System.Drawing.Size(109, 21);
            this.backgroundColorButton.TabIndex = 3;
            this.backgroundColorButton.Text = "Background Color";
            this.backgroundColorButton.UseVisualStyleBackColor = true;
            // 
            // resetCameraButton
            // 
            this.resetCameraButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.resetCameraButton.Location = new System.Drawing.Point(8, 49);
            this.resetCameraButton.Name = "resetCameraButton";
            this.resetCameraButton.Size = new System.Drawing.Size(109, 21);
            this.resetCameraButton.TabIndex = 2;
            this.resetCameraButton.Text = "Reset";
            this.resetCameraButton.UseVisualStyleBackColor = true;
            // 
            // renderingOptionsLabel
            // 
            this.renderingOptionsLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.renderingOptionsLabel.AutoSize = true;
            this.renderingOptionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderingOptionsLabel.Location = new System.Drawing.Point(6, 6);
            this.renderingOptionsLabel.Name = "renderingOptionsLabel";
            this.renderingOptionsLabel.Size = new System.Drawing.Size(112, 13);
            this.renderingOptionsLabel.TabIndex = 0;
            this.renderingOptionsLabel.Text = "Rendering Options";
            // 
            // glControlMain
            // 
            this.glControlMain.BackColor = System.Drawing.Color.Black;
            this.glControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlMain.Location = new System.Drawing.Point(0, 0);
            this.glControlMain.Name = "glControlMain";
            this.glControlMain.Size = new System.Drawing.Size(476, 458);
            this.glControlMain.TabIndex = 8;
            this.glControlMain.TabStop = false;
            this.glControlMain.VSync = true;
            // 
            // glTabModelListBoxSplitContainer
            // 
            this.glTabModelListBoxSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glTabModelListBoxSplitContainer.Location = new System.Drawing.Point(10, 33);
            this.glTabModelListBoxSplitContainer.Name = "glTabModelListBoxSplitContainer";
            // 
            // glTabModelListBoxSplitContainer.Panel1
            // 
            this.glTabModelListBoxSplitContainer.Panel1.Controls.Add(this.glControlTabControlSplitContainer);
            this.glTabModelListBoxSplitContainer.Panel1MinSize = 600;
            // 
            // glTabModelListBoxSplitContainer.Panel2
            // 
            this.glTabModelListBoxSplitContainer.Panel2.Controls.Add(this.modelListBox);
            this.glTabModelListBoxSplitContainer.Panel2.Controls.Add(this.modelSearchBox);
            this.glTabModelListBoxSplitContainer.Panel2.Controls.Add(this.modelSearchLabel);
            this.glTabModelListBoxSplitContainer.Panel2MinSize = 160;
            this.glTabModelListBoxSplitContainer.Size = new System.Drawing.Size(772, 458);
            this.glTabModelListBoxSplitContainer.SplitterDistance = 606;
            this.glTabModelListBoxSplitContainer.TabIndex = 11;
            this.glTabModelListBoxSplitContainer.TabStop = false;
            // 
            // modelListBox
            // 
            this.modelListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.modelListBox.FormattingEnabled = true;
            this.modelListBox.ItemHeight = 12;
            this.modelListBox.Location = new System.Drawing.Point(0, 48);
            this.modelListBox.Name = "modelListBox";
            this.modelListBox.Size = new System.Drawing.Size(162, 412);
            this.modelListBox.TabIndex = 10;
            // 
            // modelSearchBox
            // 
            this.modelSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.modelSearchBox.Location = new System.Drawing.Point(0, 22);
            this.modelSearchBox.Name = "modelSearchBox";
            this.modelSearchBox.Size = new System.Drawing.Size(162, 21);
            this.modelSearchBox.TabIndex = 9;
            // 
            // modelSearchLabel
            // 
            this.modelSearchLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.modelSearchLabel.AutoSize = true;
            this.modelSearchLabel.Location = new System.Drawing.Point(58, 6);
            this.modelSearchLabel.Name = "modelSearchLabel";
            this.modelSearchLabel.Size = new System.Drawing.Size(41, 12);
            this.modelSearchLabel.TabIndex = 15;
            this.modelSearchLabel.Text = "Search";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 522);
            this.Controls.Add(this.glTabModelListBoxSplitContainer);
            this.Controls.Add(this.mainWindowStatusStrip);
            this.Controls.Add(this.mainWindowMenuStrip);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainWindowMenuStrip;
            this.MinimumSize = new System.Drawing.Size(800, 556);
            this.Name = "MainWindow";
            this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
            this.Text = "LOLViewer 11.16.2012";
            this.mainWindowMenuStrip.ResumeLayout(false);
            this.mainWindowMenuStrip.PerformLayout();
            this.mainWindowStatusStrip.ResumeLayout(false);
            this.mainWindowStatusStrip.PerformLayout();
            this.glControlTabControlSplitContainer.Panel1.ResumeLayout(false);
            this.glControlTabControlSplitContainer.Panel1.PerformLayout();
            this.glControlTabControlSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.glControlTabControlSplitContainer)).EndInit();
            this.glControlTabControlSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.timelineTrackBar)).EndInit();
            this.glTabModelListBoxSplitContainer.Panel1.ResumeLayout(false);
            this.glTabModelListBoxSplitContainer.Panel2.ResumeLayout(false);
            this.glTabModelListBoxSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.glTabModelListBoxSplitContainer)).EndInit();
            this.glTabModelListBoxSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainWindowMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMainWindowMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutMainWindowMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readDirectoryMainMenuStripItem;
        private System.Windows.Forms.StatusStrip mainWindowStatusStrip;
        private System.Windows.Forms.SplitContainer glControlTabControlSplitContainer;
        private System.Windows.Forms.SplitContainer glTabModelListBoxSplitContainer;
        private System.Windows.Forms.ToolStripSeparator fileToolStripSeparator;
        private System.Windows.Forms.ListBox modelListBox;
        private System.Windows.Forms.TextBox modelSearchBox;
        private System.Windows.Forms.Label modelSearchLabel;
        private System.Windows.Forms.ToolStripMenuItem readMainMenuStripItem;
        private OpenTK.GLControl glControlMain;
        private System.Windows.Forms.Label renderingOptionsLabel;
        private System.Windows.Forms.Button fullscreenButton;
        private System.Windows.Forms.Button backgroundColorButton;
        private System.Windows.Forms.Button resetCameraButton;
        private System.Windows.Forms.Button playAnimationButton;
        private System.Windows.Forms.TrackBar timelineTrackBar;
        private System.Windows.Forms.ComboBox currentAnimationComboBox;
        private System.Windows.Forms.Label animationOptionsLabel;
        private System.Windows.Forms.Button enableAnimationButton;
        private System.Windows.Forms.ToolStripStatusLabel mainWindowStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar mainWindowProgressBar;
    }
}

