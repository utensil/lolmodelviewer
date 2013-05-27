
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
// Controls the full screen display.
//


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using LOLViewer.Graphics;

namespace LOLViewer.GUI
{
    public partial class FullScreenWindow : Form
    {
        // Windowing variables
        private bool isGLLoaded;

        // Main OpenGL Control
        private GLControl mainGLControl;

        // Reference variables from main form.
        private GLRenderer renderer;
        private GLCamera camera;

        private float fieldOfView, nearPlane, farPlane;

        // Animation Control Handle
        private AnimationController animationController;

        public FullScreenWindow(ref GLRenderer renderer, ref GLCamera camera,
            ref AnimationController animationController,
            ref GLControl control,
            float fieldOfView, float nearPlane, float farPlane)
        {
            // Store references
            this.renderer = renderer;
            this.camera = camera;
            this.animationController = animationController;
            this.fieldOfView = fieldOfView;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
            this.mainGLControl = control;

            InitializeComponent();

            //
            // Set up callbacks.
            //

            // GLControl Callbacks
            glControlMain.Load += new EventHandler(GLControlMainOnLoad);
            glControlMain.Resize += new EventHandler(GLControlMainOnResize);
            glControlMain.Paint += new PaintEventHandler(GLControlMainOnPaint);

            // Set mouse events
            glControlMain.MouseDown += new MouseEventHandler(GLControlOnMouseDown);
            glControlMain.MouseUp += new MouseEventHandler(GLControlOnMouseUp);
            glControlMain.MouseWheel += new MouseEventHandler(GLControlOnMouseWheel);
            glControlMain.MouseMove += new MouseEventHandler(GLControlOnMouseMove);

            // Set keyboard events
            glControlMain.KeyDown += new KeyEventHandler(GLControlMainOnKeyDown);
            glControlMain.KeyUp += new KeyEventHandler(GLControlMainOnKeyUp);
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
            camera.SetProjectionParameters(fieldOfView, (float)(glControlMain.ClientRectangle.Width - glControlMain.ClientRectangle.X),
                (float)(glControlMain.ClientRectangle.Height - glControlMain.ClientRectangle.Y),
                nearPlane, farPlane);

            renderer.Resize(glControlMain.ClientRectangle.X, glControlMain.ClientRectangle.Y,
                glControlMain.ClientRectangle.Width, glControlMain.ClientRectangle.Height);

            GLControlMainOnUpdateFrame(sender, e);
        }

        public void GLControlMainOnLoad(object sender, EventArgs e)
        {
            // Set up renderer.
            isGLLoaded = true;

            // Go fullscreen
            this.WindowState = FormWindowState.Maximized;

            // Make the original context current and draw to
            // this window.
            mainGLControl.Context.MakeCurrent(glControlMain.WindowInfo);

            // Call an initial resize to get some camera and renderer parameters set up.
            GLControlMainOnResize(sender, e);
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

        private void GLControlMainOnKeyUp(object sender, KeyEventArgs e)
        {
            camera.OnKeyUp(e);
            GLControlMainOnUpdateFrame(sender, e);
        }

        private void GLControlMainOnKeyDown(object sender, KeyEventArgs e)
        {
            camera.OnKeyDown(e);
            GLControlMainOnUpdateFrame(sender, e);

            // Any key, except the camera reset key, closes the full screen view.
            if (e.KeyCode != Keys.R)
            {
                this.Close();
                return;
            }
        }
    }
}
