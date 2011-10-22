
/*
LOLViewer
Copyright 2011 James Lammlein 

 

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

using OpenTK;
using LOLViewer.IO;

namespace LOLViewer
{
    public partial class MainWindow : Form
    {
        // Windowing variables
        private bool isGLLoaded;
        private Stopwatch timer;

        // Graphics abstraction
        private GLRenderer renderer;

        // Default Camera
        private const float FIELD_OF_VIEW = OpenTK.MathHelper.PiOver4;
        private const float NEAR_PLANE = 0.1f;
        private const float FAR_PLANE = 1000.0f;
        private GLCamera camera;

        // IO Variables
        public LOLDirectoryReader reader;

        // Model Name Search Variables
        public String lastSearch;
        public List<String> currentSearchSubset;

        // GUI Variables
        // converts from World Transform scale to trackbar units.
        private const float DEFAULT_SCALE_TRACKBAR = 1000.0f;

        // Animation Control Handle
        private AnimationController animationController;
       
        public MainWindow()
        {
            isGLLoaded = false;
            timer = new Stopwatch();

            camera = new GLCamera();
            camera.SetViewParameters(new Vector3(0.0f, 0.0f, 300.0f), Vector3.Zero);
            renderer = new GLRenderer();

            reader = new LOLDirectoryReader();

            InitializeComponent();
            modelScaleTrackbar.Value = (int) (GLRenderer.DEFAULT_MODEL_SCALE * DEFAULT_SCALE_TRACKBAR);
            yOffsetTrackbar.Value = -GLRenderer.DEFAULT_MODEL_YOFFSET;

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
            closeToolStripMenuItem.Click += new EventHandler(OnClose);
            setDirectoryToolStripMenuItem.Click += new EventHandler(OnSetDirectory);
            aboutToolStripMenuItem.Click += new EventHandler(OnAbout);
            readDefaultDirectoryToolStrip.Click += new EventHandler(OnReadModels);

            // Model View Callbacks
            modelListBox.DoubleClick += new EventHandler(OnModelListDoubleClick);
            modelListBox.KeyPress += new KeyPressEventHandler(OnModelListKeyPress);

            // Trackbars
            yOffsetTrackbar.Scroll += new EventHandler(YOffsetTrackbarOnScroll);
            modelScaleTrackbar.Scroll += new EventHandler(ModelScaleTrackbarOnScroll);

            // Buttons
            resetCameraButton.Click += new EventHandler(OnResetCameraButtonClick);
            backgroundColorButton.Click += new EventHandler(OnBackgroundColorButtonClick);

            // Animation Controller
            animationController = new AnimationController();

            // Set references
            animationController.enableAnimationCheckBox = enableAnimationCheckBox;
            animationController.currentAnimationComboBox = currentAnimationComboBox;
            animationController.nextKeyFrameButton = nextKeyFrameButton;
            animationController.playAnimationButton = playAnimationButton;
            animationController.previousKeyFrameButton = previousKeyFrameButton;
            animationController.glControlMain = glControlMain;

            animationController.renderer = renderer;

            // Set callbacks.
            enableAnimationCheckBox.Click += new EventHandler(animationController.OnEnableCheckBoxClick);
            previousKeyFrameButton.Click += new EventHandler(animationController.OnPreviousKeyFrameButtonClick);
            nextKeyFrameButton.Click += new EventHandler(animationController.OnNextKeyFrameButtonClick);
            playAnimationButton.Click += new EventHandler(animationController.OnPlayAnimationButtonClick);
            currentAnimationComboBox.SelectedIndexChanged += new EventHandler(animationController.OnCurrentAnimationComboBoxSelectedIndexChanged);

            animationController.DisableAnimation();

            // Search Box
            modelSearchBox.TextChanged += new EventHandler(OnModelSearchBoxTextChanged);
            modelSearchBox.KeyPress += new KeyPressEventHandler(OnModelSearchBoxKeyPress);
            modelSearchBox.PreviewKeyDown += new PreviewKeyDownEventHandler(OnModelSearchBoxPreviewKeyDown);
        }

        //
        // Main Window Handlers
        //

        void OnMainWindowShown(object sender, EventArgs e)
        {
            // Read model files.
            OnReadModels(sender, e);
        }

        //
        // GLControl Handlers
        //

        public void GLControlMainOnPaint(object sender, PaintEventArgs e)
        {
            if (isGLLoaded == false)
                return;

            renderer.OnRender(camera);

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

            renderer.OnResize(glControlMain.ClientRectangle.X, glControlMain.ClientRectangle.Y,
                glControlMain.ClientRectangle.Width, glControlMain.ClientRectangle.Height);

            GLControlMainOnUpdateFrame(sender, e);
        }

        public void GLControlMainOnLoad(object sender, EventArgs e)
        {
            // Set up renderer.
            bool result = renderer.OnLoad();
            if (result == false)
            {
                MessageBox.Show("OpenGL failed to load." +
                    "  Please install the latest drivers from your GPU manufacturer.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                this.Close();
                return;
            }

            isGLLoaded = true;

            // Call an initial resize to get some camera and renderer parameters set up.
            GLControlMainOnResize(sender, e);
            timer.Start();
        }

        public void GLControlMainOnUpdateFrame(object sender, EventArgs e)
        {
            double elapsedTime = ComputeElapsedTime();

            // Update camera and animation controller.
            camera.OnUpdate((float) elapsedTime);
            animationController.OnApplicationIdle(sender, e);

            // Hacky, prevents double invalidation.
            if (animationController.isAnimating == false)
            {
                glControlMain.Invalidate();
            }
        }

        void GLControlMainOnDispose(object sender, EventArgs e)
        {
            renderer.ShutDown();
        }

        //
        // Mouse Handlers
        //

        private void GLControlOnMouseMove(object sender, MouseEventArgs e)
        {
            camera.OnMouseMove(e);
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
            GLControlMainOnUpdateFrame(sender, e);
        }

        private void GLControlOnMouseDown(object sender, MouseEventArgs e)
        {
            camera.OnMouseButtonDown(e);
            GLControlMainOnUpdateFrame(sender, e);
        }

        //
        // Keyboard Handlers
        //

        void GLControlMainOnKeyUp(object sender, KeyEventArgs e)
        {
            camera.OnKeyUp(e);
            GLControlMainOnUpdateFrame(sender, e);
        }

        void GLControlMainOnKeyDown(object sender, KeyEventArgs e)
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

        void OnAbout(object sender, EventArgs e)
        {
            AboutWindow aboutDlg = new AboutWindow();
            aboutDlg.ShowDialog();
        }

        void OnSetDirectory(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Select the 'League of Legends' folder.";
            dlg.ShowNewFolderButton = false;

            DialogResult result = dlg.ShowDialog();
            
            String selectedDir = String.Empty;
            if (result == DialogResult.OK)
            {
                // Lets not check and let the directory reader sort it out.
                reader.SetRoot(dlg.SelectedPath);
                    
                // Reread the models.
                OnReadModels(sender, e);
            }
        }

        void OnClose(object sender, EventArgs e)
        {
            this.Close();
        }

        void OnReadModels(object sender, EventArgs e)
        {
            // Clear old data.
            modelListBox.Items.Clear();
            renderer.DestroyCurrentModels();
            glControlMain.Invalidate();

            LoadingModelsWindow loader = new LoadingModelsWindow();
            loader.reader = reader;
            loader.ShowDialog();

            DialogResult result = loader.result;
            if (result == DialogResult.Abort)
            {
                MessageBox.Show("Unable to read models. If you installed League of legends" +
                                 " in a non-default location, change the default directory" +
                                 " to the 'League of Legends' folder by using the command in the 'Options' menu.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }

            // Populate the model list box.
            List<String> modelNames = reader.GetModelNames();
            foreach (String name in modelNames)
            {
                modelListBox.Items.Add(name);
            }
        }

        //
        // Model List Box Handlers
        //

        void OnModelListDoubleClick(object sender, EventArgs e)
        {
            String modelName = (String) modelListBox.SelectedItem;

            // TODO: Not really sure how to handle errors
            // if either of these functions fail.
            LOLModel model = reader.GetModel(modelName);
            if (model != null)
            {
                bool result = renderer.LoadModel(model);

                currentAnimationComboBox.Items.Clear();
                foreach (var a in model.animations)
                {
                    currentAnimationComboBox.Items.Add(a.Key);
                }

                currentAnimationComboBox.Text = "";

                if (currentAnimationComboBox.Items.Count > 0)
                {
                    currentAnimationComboBox.SelectedIndex = 0;
                }
            }

            GLControlMainOnUpdateFrame(sender, e);
        }

        void OnModelListKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            // When enter is pressed
            if (e.KeyChar == '\r')
            {
                // Update model.
                OnModelListDoubleClick(sender, e);
            }
        }

        //
        // Trackbar Handlers
        //
        void YOffsetTrackbarOnScroll(object sender, EventArgs e)
        {
            Matrix4 world = Matrix4.Scale(modelScaleTrackbar.Value / DEFAULT_SCALE_TRACKBAR);
            world.M42 = (float)-yOffsetTrackbar.Value;
            renderer.world = world;

            // Redraw.
            GLControlMainOnPaint(sender, null);
        }

        void ModelScaleTrackbarOnScroll(object sender, EventArgs e)
        {
            Matrix4 world = Matrix4.Scale(modelScaleTrackbar.Value / DEFAULT_SCALE_TRACKBAR);
            world.M42 = (float)-yOffsetTrackbar.Value;
            renderer.world = world;

            // Redraw.
            GLControlMainOnPaint(sender, null);
        }

        // Button Handlers
        void OnResetCameraButtonClick(object sender, EventArgs e)
        {
            camera.Reset();

            // Redraw.
            glControlMain.Invalidate();
        }

        void OnBackgroundColorButtonClick(object sender, EventArgs e)
        {
            ColorDialog colorDlg = new ColorDialog();

            Color iniColor = Color.FromArgb( (int) (renderer.clearColor.A * 255),
                (int) (renderer.clearColor.R * 255), (int) (renderer.clearColor.G * 255),
                (int) (renderer.clearColor.B * 255) );

            colorDlg.Color = iniColor;

            if (colorDlg.ShowDialog() == DialogResult.OK)
            {
                renderer.SetClearColor(colorDlg.Color);

                glControlMain.Invalidate();
            }
        }

        //
        // Search Box Handlers
        //

        void OnModelSearchBoxTextChanged(object sender, EventArgs e)
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

            // Redraw the list box.
            modelListBox.Invalidate();
        }

        void OnModelSearchBoxKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            // Pass it through to the list box handler.
            OnModelListKeyPress(sender, e);
        }

        void OnModelSearchBoxPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //
            // The default windows forms behavior will intercept the arrow key messages
            // and handle them in the background.  This is normally ideal.  However, here, it would
            // be nice if the search box could change the the selection of the list box.  That way, users
            // can manipulate the list box while their typing in a search.
            //

            // Handle the arrow keys at this point.
            if (e.KeyCode == Keys.Down )
            {
                // Not doing a wrap around on this.  Just increment it if we can.
                if (modelListBox.Items.Count > 0 && modelListBox.SelectedIndex + 1 < modelListBox.Items.Count)
                {
                    modelListBox.SelectedIndex++;
                }

                e.IsInputKey = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                // Not doing a wrap around on this.  Just decrement it if we can.
                if (modelListBox.Items.Count > 0 && modelListBox.SelectedIndex - 1 >= 0)
                {
                    modelListBox.SelectedIndex--;
                }

                e.IsInputKey = true;
            }
        }
        
        //
        // Helper Functions
        //

        public double ComputeElapsedTime()
        {
            timer.Stop();
            double elapsedTime = timer.Elapsed.TotalSeconds;
            timer.Reset();
            timer.Start();
            return elapsedTime;
        }
    }
}
