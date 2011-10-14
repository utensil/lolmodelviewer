

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
// Handles input from the animation options panel on the main gui.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Diagnostics;

namespace LOLViewer
{
    class AnimationController
    {
        public bool isFirstTime;
        public bool isEnabled;
        public bool isAnimating;
        public String currentAnimation;
        public Stopwatch timer;

        public AnimationController()
        {
            isFirstTime = true;
            isEnabled = false;
            isAnimating = false;
            currentAnimation = String.Empty;
            timer = new Stopwatch();
            timer.Reset();
            timer.Stop();
        }

        //
        // References to actual forms controls.
        //

        public OpenTK.GLControl glControlMain;
        public CheckBox enableAnimationCheckBox;
        public ComboBox currentAnimationComboBox;
        public Button   previousKeyFrameButton;
        public Button   nextKeyFrameButton;
        public Button   playAnimationButton;

        // More references
        public GLRenderer renderer;

        public void DisableAnimation()
        {
            isEnabled = false;
            StopAnimation();

            currentAnimationComboBox.Enabled = false;
            previousKeyFrameButton.Enabled = false;
            nextKeyFrameButton.Enabled = false;
            playAnimationButton.Enabled = false;

            renderer.isSkinning = false;
        }

        public void EnableAnimation()
        {
            if (isFirstTime == true)
            {
                MessageBox.Show("Animation is still an 'in progress' feature.  Currently, most models should animate properly.  " +
                "However, a few do not.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                isFirstTime = false;
            }

            isEnabled = true;
            StopAnimation();

            currentAnimationComboBox.Enabled = true;
            previousKeyFrameButton.Enabled = true;
            nextKeyFrameButton.Enabled = true;
            playAnimationButton.Enabled = true;

            renderer.isSkinning = true;
        }

        public void StopAnimation()
        {
            isAnimating = false;
            playAnimationButton.Text = "Play";

            // Remove the update loop.
            timer.Stop();
            timer.Reset();
            Application.Idle -= OnApplicationIdle;

            glControlMain.Invalidate();
        }

        public void StartAnimation()
        {
            isAnimating = true;
            playAnimationButton.Text = "Pause";

            // Add an update loop.
            Application.Idle += new EventHandler(OnApplicationIdle);
            timer.Reset();
            timer.Start();

            glControlMain.Invalidate();
        }

        public void OnApplicationIdle(object sender, EventArgs e)
        {
            if (isAnimating == true)
            {
                TimeSpan time = timer.Elapsed;
                float elapsedTime = (float)time.Seconds + ((float)time.Milliseconds / 1000.0f);
                renderer.OnUpdate(elapsedTime);

                timer.Restart();

                glControlMain.Invalidate();
            }
        }

        public void SetAnimation()
        {
            currentAnimation = currentAnimationComboBox.SelectedItem.ToString();

            if (currentAnimation.Length > 0)
            {
                renderer.SetAnimations(currentAnimation);
                glControlMain.Invalidate();
            }
        }

        public void IncrementAnimation()
        {
            if (isEnabled == true)
            {
                StopAnimation();
                renderer.IncrementAnimations();
                glControlMain.Invalidate();
            }
        }

        public void DecrementAnimation()
        {
            if (isEnabled == true)
            {
                StopAnimation();
                renderer.DecrementAnimations();
                glControlMain.Invalidate();
            }
        }

        //
        // Input Handlers
        //

        public void OnCurrentAnimationComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            SetAnimation();
        }

        public void OnPreviousKeyFrameButtonClick(object sender, EventArgs e)
        {
            DecrementAnimation();
        }

        public void OnNextKeyFrameButtonClick(object sender, EventArgs e)
        {
            IncrementAnimation();
        }

        public void OnPlayAnimationButtonClick(object sender, EventArgs e)
        {
            if (isAnimating == true)
            {
                StopAnimation();
            }
            else
            {
                StartAnimation();
            }         
        }

        public void OnEnableCheckBoxClick(object sender, EventArgs e)
        {
            if (isEnabled == true)
            {
                DisableAnimation();
            }
            else
            {
                EnableAnimation();
            }
        }

        //
        // Helper functions
        //
    }
}
