
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

//
// Main GUI for the program.
//


using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

using OpenTK;

using LOLFileReader;

using LOLViewer.IO;
using LOLViewer.Graphics;

using CSharpLogger;

namespace LOLViewer.GUI
{
    public partial class MainWindow : Form
    {
        private const String DEFAULT_SAMPLE_MODELS_DIRECTORY = "./SampleModels/filearchives/";
        private const String DEFAULT_DATA_FILE = "lolviewer.dat";
        private const String DEFAULT_LOG_FILE = "lolviewer.log";

        // Windowing variables
        private bool isGLLoaded;
        private Stopwatch timer;

        // Graphics abstraction
        private GLRenderer renderer;

        // Default Camera
        private const float FIELD_OF_VIEW = OpenTK.MathHelper.PiOver4;
        private const float NEAR_PLANE = 1.0f;
        private const float FAR_PLANE = 2000.0f;
        private GLCamera camera;

        // IO Variables
        private LOLDirectoryReader reader;
        private Logger logger;

        // Model Name Search Variables
        private String lastSearch;
        private List<String> currentSearchSubset;

        // Animation Control Handle
        private AnimationController animationController;

        public MainWindow()
        {
            logger = new Logger(DEFAULT_LOG_FILE); // Not checking result.
            logger.Event("Program Start.");

            isGLLoaded = false;
            timer = new Stopwatch();

            camera = new GLCamera();
            camera.SetViewParameters(new Vector3(0.0f, 0.0f, 2.0f), Vector3.Zero);
            renderer = new GLRenderer();

            // Set up the reader and initialize its root to the value in 'lolviewer.dat' if
            // the file exists.
            {
                reader = new LOLDirectoryReader();

                bool isFileOpen = false;
                FileStream file = null;
                try
                {
                    FileInfo fileInfo = new FileInfo(DEFAULT_DATA_FILE);
                    if (fileInfo.Exists == true)
                    {
                        file = new FileStream(fileInfo.FullName, FileMode.Open);
                        isFileOpen = true;
                    }
                    else
                    {
                        logger.Warning("Failed to locate " + DEFAULT_DATA_FILE + ".");
                    }
                }
                catch
                {
                    logger.Warning("Failed to open " + DEFAULT_DATA_FILE + ".");
                }

                if (isFileOpen == true)
                {
                    BinaryReader fileReader = null;
                    if (file != null)
                    {
                        try
                        {
                            logger.Event("Reading " + DEFAULT_DATA_FILE + ".");

                            fileReader = new BinaryReader(file);
                            reader.Root = fileReader.ReadString();
                            fileReader.Close();
                        }
                        catch
                        {
                            logger.Warning("Failed to read " + DEFAULT_DATA_FILE + ".");
                            file.Close();
                        }
                    }
                }
            }

            InitializeComponent();

            mainWindowProgressBar.Style = ProgressBarStyle.Marquee;
            mainWindowProgressBar.Value = 100;

            lastSearch = String.Empty;
            currentSearchSubset = new List<String>();

            // Main window Callbacks
            this.Shown += new EventHandler(OnMainWindowShown);

            // GLControl Callbacks
            glControlMain.Load += new EventHandler(GLControlMainOnLoad);
            glControlMain.Resize += new EventHandler(GLControlMainOnResize);
            glControlMain.Paint += new PaintEventHandler(GLControlMainOnPaint);
            glControlMain.Disposed += new EventHandler(GLControlMainOnDispose);

            // Set mouse events
            glControlMain.MouseDown += new MouseEventHandler(GLControlOnMouseDown);
            glControlMain.MouseUp += new MouseEventHandler(GLControlOnMouseUp);
            glControlMain.MouseWheel += new MouseEventHandler(GLControlOnMouseWheel);
            glControlMain.MouseMove += new MouseEventHandler(GLControlOnMouseMove);

            // Set keyboard events
            glControlMain.KeyDown += new KeyEventHandler(GLControlMainOnKeyDown);
            glControlMain.KeyUp += new KeyEventHandler(GLControlMainOnKeyUp);

            // Menu Callbacks
            exitToolStripMenuItem.Click += new EventHandler(OnExit);
            aboutToolStripMenuItem.Click += new EventHandler(OnAbout);
            readDirectoryMainMenuStripItem.Click += new EventHandler(OnReadModels);
            readMainMenuStripItem.Click += new EventHandler(OnSetDirectory);

            // Model View Callbacks
            modelListBox.DoubleClick += new EventHandler(OnModelListDoubleClick);
            modelListBox.KeyPress += new KeyPressEventHandler(OnModelListKeyPress);

            // Buttons
            resetCameraButton.Click += new EventHandler(OnResetCameraButtonClick);
            backgroundColorButton.Click += new EventHandler(OnBackgroundColorButtonClick);
            fullscreenButton.Click += new EventHandler(OnFullscreenButtonClick);

            //
            // Animation Controller
            //

            // TODO: Pass the references and callbacks into constructor instead of doing them out here.
            // Kind of ugly code. :(
            animationController = new AnimationController();

            // Set references
            animationController.enableAnimationButton = enableAnimationButton;
            animationController.currentAnimationComboBox = currentAnimationComboBox;
            animationController.playAnimationButton = playAnimationButton;
            animationController.glControlMain = glControlMain;
            animationController.timelineTrackBar = timelineTrackBar;
            animationController.mainWindowStatusLabel = mainWindowStatusLabel;

            animationController.renderer = renderer;

            // Set callbacks.
            enableAnimationButton.Click += new EventHandler(animationController.OnEnableAnimationButtonClick);
            playAnimationButton.Click += new EventHandler(animationController.OnPlayAnimationButtonClick);
            currentAnimationComboBox.SelectedIndexChanged += new EventHandler(animationController.OnCurrentAnimationComboBoxSelectedIndexChanged);
            timelineTrackBar.Scroll += new EventHandler(animationController.OnTimelineTrackBar);

            animationController.DisableAnimation();

            //
            // End Animation Controller
            //

            // Search Box
            modelSearchBox.TextChanged += new EventHandler(OnModelSearchBoxTextChanged);
            modelSearchBox.KeyPress += new KeyPressEventHandler(OnModelSearchBoxKeyPress);
            modelSearchBox.KeyDown += new KeyEventHandler(OnModelSearchBoxKeyDown);
        }

        //
        // Main Window Handlers
        //

        private void OnMainWindowShown(object sender, EventArgs e)
        {
            if (isGLLoaded == true)
            {
                // Read model files.
                OnReadModels(sender, e);
            }
            else
            {
                this.Close();
            }
        }

        //
        // GLControl Handlers
        //

        public void GLControlMainOnPaint(object sender, PaintEventArgs e)
        {
            if (isGLLoaded == false)
                return;

            renderer.Render(camera);

            glControlMain.SwapBuffers();
        }

        public void GLControlMainOnResize(object sender, EventArgs e)
        {
            if (isGLLoaded == false)
                return;

            // Set up camera projection parameters based on window's size.
            camera.SetProjectionParameters(FIELD_OF_VIEW, (float)(glControlMain.ClientRectangle.Width - glControlMain.ClientRectangle.X),
                (float)(glControlMain.ClientRectangle.Height - glControlMain.ClientRectangle.Y),
                NEAR_PLANE, FAR_PLANE);

            renderer.Resize(glControlMain.ClientRectangle.X, glControlMain.ClientRectangle.Y,
                glControlMain.ClientRectangle.Width, glControlMain.ClientRectangle.Height);

            GLControlMainOnUpdateFrame(sender, e);
        }

        public void GLControlMainOnLoad(object sender, EventArgs e)
        {
            // Set up renderer.
            bool result = renderer.Initialize(logger);
            if (result == true)
            {
                isGLLoaded = true;

                // Call an initial resize to get some camera and renderer parameters set up.
                GLControlMainOnResize(sender, e);
                timer.Start();
            }
            else
            {
                isGLLoaded = false;

                MessageBox.Show(this, "OpenGL failed to load." +
                    "  Please install the latest display drivers from your GPU manufacturer.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }            
        }

        public void GLControlMainOnUpdateFrame(object sender, EventArgs e)
        {
            // Update camera and animation controller.
            camera.OnUpdate();
            animationController.OnApplicationIdle(sender, e);

            // Hacky, prevents double invalidation.
            if (animationController.isAnimating == false)
            {
                glControlMain.Invalidate();
            }
        }

        private void GLControlMainOnDispose(object sender, EventArgs e)
        {
            if (isGLLoaded == true)
            {
                renderer.Shutdown();
            }

            // Close logger at this point.
            logger.Event("Program shutdown.");
            logger.Close();
        }

        //
        // Mouse Handlers
        //

        private void GLControlOnMouseMove(object sender, MouseEventArgs e)
        {
            camera.OnMouseMove(e);
            renderer.MouseMove(e, camera);

            GLControlMainOnUpdateFrame(sender, e);
        }

        private void GLControlOnMouseWheel(object sender, MouseEventArgs e)
        {
            camera.OnMouseWheel(e);
            GLControlMainOnUpdateFrame(sender, e);
        }

        private void GLControlOnMouseUp(object sender, MouseEventArgs e)
        {
            camera.OnMouseButtonUp(e);
            renderer.MouseButtonUp(e, camera);

            GLControlMainOnUpdateFrame(sender, e);
        }

        private void GLControlOnMouseDown(object sender, MouseEventArgs e)
        {
            camera.OnMouseButtonDown(e);
            renderer.MouseButtonDown(e, camera);

            GLControlMainOnUpdateFrame(sender, e);
        }

        //
        // Keyboard Handlers
        //

        private void GLControlMainOnKeyUp(object sender, KeyEventArgs e)
        {
            camera.OnKeyUp(e);
            GLControlMainOnUpdateFrame(sender, e);
        }

        private void GLControlMainOnKeyDown(object sender, KeyEventArgs e)
        {
            camera.OnKeyDown(e);
            GLControlMainOnUpdateFrame(sender, e);

            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                return;
            }
        }

        //
        // Menu Strip Handlers
        //

        private void OnAbout(object sender, EventArgs e)
        {
            AboutWindow aboutDlg = new AboutWindow();
            aboutDlg.StartPosition = FormStartPosition.CenterParent;
            aboutDlg.ShowDialog();
        }

        private void OnExit(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnSetDirectory(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Select the League of Legends' root installation folder.";
            dlg.ShowNewFolderButton = false;

            DialogResult result = dlg.ShowDialog();

            String selectedDir = String.Empty;
            if (result == DialogResult.OK)
            {
                // Lets not check and let the directory reader sort it out.
                reader.Root = dlg.SelectedPath;

                // Reread the models.
                OnReadModels(sender, e);
            }
        }

        private void OnReadModels(object sender, EventArgs e)
        {
            // Update GUI.
            mainWindowStatusLabel.Text = "Reading League of Legends' directory...";
            mainWindowProgressBar.Visible = true;

            // Clear old data.
            modelListBox.Items.Clear();
            renderer.DestroyModel();
            glControlMain.Invalidate();

            // Create a background worker to read in the models.
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(ReadModels);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ReadModelsCallback);
            worker.RunWorkerAsync();
        }

        #region Helper functions for the read models background worker

        private void ReadModels(object sender, DoWorkEventArgs e)
        {
            logger.HoldFlushes();

            // Read League of Legends' information.
            bool result = reader.Read(logger);
            if (result == true)
            {
                logger.Event("Sorting models.");
                reader.SortModelNames();
            }
            else
            {
                // Bail if reading fails.
                logger.Error("Failed to read models.");

                // UI calls need to be executed on the main thread.
                this.BeginInvoke((Action)(() =>
                {
                    MessageBox.Show(this,
                        "Unable to read the League of Legends' installation directory. " +
                        "If you installed League of Legends " +
                        "in a non-default location, use 'File -> Read...' to manually " +
                        "select the League of Legends' installation directory." +
                        "\n\n" +
                        "Loading some sample models.",
                        "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }));

                // Load some sample models.
                logger.Event("Reading sample models.");
                reader.Root = DEFAULT_SAMPLE_MODELS_DIRECTORY;
                
                result = reader.Read(logger);
                if (result == true)
                {
                    logger.Event("Sorting sample models.");
                    reader.SortModelNames();
                }

                return;
            }

            // On successful read, write the root directory to file.
            logger.Event("Storing League of Legends installation directory path.");
            FileStream file = null;
            try
            {
                logger.Event("Opening " + DEFAULT_DATA_FILE + ".");
                file = new FileStream(DEFAULT_DATA_FILE, FileMode.OpenOrCreate);
            }
            catch
            {
                logger.Warning("Failed to open " + DEFAULT_DATA_FILE + ".");
            }

            BinaryWriter writer = null;
            if (file != null)
            {
                try
                {
                    logger.Event("Writing League of Legends directory path.");
                    writer = new BinaryWriter(file);
                    writer.Write(reader.Root);
                    writer.Close();
                }
                catch
                {
                    logger.Warning("Failed to write League of Legends directory path.");
                    file.Close();
                }
            }
        }

        private void ReadModelsCallback(object sender, RunWorkerCompletedEventArgs e)
        {
            // Populate the model list box.
            modelListBox.BeginUpdate();

            List<String> modelNames = reader.GetModelNames();
            foreach (String name in modelNames)
            {
                modelListBox.Items.Add(name);
            }

            modelListBox.EndUpdate();

            // Set Status text and progress bar
            mainWindowStatusLabel.Text = "Ready.";
            mainWindowProgressBar.Visible = false;

            // Re-enable auto log flushing
            logger.RestartFlushes();
        }

        #endregion

        //
        // Model List Box Handlers
        //

        private void OnModelListDoubleClick(object sender, EventArgs e)
        {
            String modelName = (String)modelListBox.SelectedItem;

            // TODO: Not really sure how to handle errors
            // if either of these functions fail.
            LOLModel model = reader.GetModel(modelName);
            if (model != null)
            {
                bool result = renderer.LoadModel(model, logger);

                currentAnimationComboBox.Items.Clear();
                foreach (String name in renderer.GetAnimations())
                {
                    currentAnimationComboBox.Items.Add(name);
                }

                currentAnimationComboBox.Text = "";

                if (currentAnimationComboBox.Items.Count > 0)
                {
                    currentAnimationComboBox.SelectedIndex = 0;
                }

                animationController.DisableAnimation();
                animationController.EnableAnimation();
                animationController.StartAnimation();

                // Update status bar text.
                mainWindowStatusLabel.Text = "Viewing " + modelName + ". Left mouse rotates, right mouse pans, and mouse wheel zooms.";
            }

            OnResetCameraButtonClick(sender, e);
            GLControlMainOnUpdateFrame(sender, e);
        }

        private void OnModelListKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            // When enter is pressed
            if (e.KeyChar == '\r')
            {
                // Update model.
                OnModelListDoubleClick(sender, e);
                e.Handled = true; // fixes unwanted 'ding' sound
            }
        }

        // Button Handlers
        private void OnResetCameraButtonClick(object sender, EventArgs e)
        {
            camera.Reset();
            renderer.Reset();

            // Redraw.
            glControlMain.Invalidate();
        }

        private void OnBackgroundColorButtonClick(object sender, EventArgs e)
        {
            ColorDialog colorDlg = new ColorDialog();

            Color iniColor = Color.FromArgb((int)(renderer.ClearColor.A * 255),
                (int)(renderer.ClearColor.R * 255), (int)(renderer.ClearColor.G * 255),
                (int)(renderer.ClearColor.B * 255));

            colorDlg.Color = iniColor;

            if (colorDlg.ShowDialog() == DialogResult.OK)
            {
                renderer.ClearColor = colorDlg.Color;

                glControlMain.Invalidate();
            }
        }

        private void OnFullscreenButtonClick(object sender, EventArgs e)
        {
            // Create a full screen window.
            FullScreenWindow fullScreenWindow = new
                FullScreenWindow(ref renderer, ref camera,
                ref animationController,
                ref glControlMain,
                FIELD_OF_VIEW, NEAR_PLANE, FAR_PLANE);

            // Display it.
            fullScreenWindow.ShowDialog(this);

            // The full screen context makes itself the current context
            // for OpenGL.  So, when it's done being shown, we need to make
            // the original form the current context and redraw it.
            glControlMain.Context.MakeCurrent(glControlMain.WindowInfo);

            // Send a resize message to update the camera and renderer.
            GLControlMainOnResize(null, null);

            // Redraw
            glControlMain.Invalidate();
        }

        //
        // Search Box Handlers
        //

        private void OnModelSearchBoxTextChanged(object sender, EventArgs e)
        {
            String search = modelSearchBox.Text;
            search = search.ToLower();

            // Santiy
            if (search == lastSearch)
                return;

            // We need to start from scratch.
            if (search.Contains(lastSearch) == false ||
                lastSearch == "")
            {
                currentSearchSubset = reader.GetModelNames();
            }
            //else
            // We can search off of the last subset of strings.

            modelListBox.BeginUpdate();

            lastSearch = search;
            modelListBox.Items.Clear();

            if (search != "")
            {
                List<String> result = new List<String>();
                foreach (String s in currentSearchSubset)
                {
                    String compare = s.ToLower();
                    if (compare.Contains(search) == true)
                    {
                        result.Add(s);
                        modelListBox.Items.Add(s); // update gui as we go.
                    }
                }

                currentSearchSubset = result;
            }
            else
            {
                // Special base where we just repopulate the list.
                foreach (String s in reader.GetModelNames())
                {
                    modelListBox.Items.Add(s);
                }
            }

            if (modelListBox.Items.Count > 0)
                modelListBox.SelectedIndex = 0;

            modelListBox.EndUpdate();
        }

        private void OnModelSearchBoxKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            // Note: Arrow keys don't get passed through this handler.

            // Pass it through to the list box handler.
            OnModelListKeyPress(sender, e);
        }

        private void OnModelSearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            //
            // The default windows forms behavior will intercept the arrow key messages
            // and handle them in the background.  This is normally ideal.  However, here, it would
            // be nice if the search box could change the the selection of the list box.  That way, users
            // can manipulate the list box while they're typing in a search.
            //

            // Handle the arrow keys at this point.
            if (e.KeyCode == Keys.Down)
            {
                // Not doing a wrap around on this.  Just increment it if we can.
                if (modelListBox.Items.Count > 0 && modelListBox.SelectedIndex + 1 < modelListBox.Items.Count)
                {
                    modelListBox.SelectedIndex++;
                }

                // Flag the key as handled so the text box doesn't move the cursor.
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                // Not doing a wrap around on this.  Just decrement it if we can.
                if (modelListBox.Items.Count > 0 && modelListBox.SelectedIndex - 1 >= 0)
                {
                    modelListBox.SelectedIndex--;
                }

                // Flag the key as handled so the text box doesn't move the cursor.
                e.Handled = true;
            }
        }
    }
}
