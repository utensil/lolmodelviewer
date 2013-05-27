

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
// Handles input from the animation options panel on the main gui.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Diagnostics;

using LOLViewer.Graphics;

namespace LOLViewer.GUI
{
    public class AnimationController
    {
        public GLRenderer renderer;

        public bool isEnabled;
        public bool isAnimating;
        public String currentAnimation;
        public Stopwatch timer;

        //
        // References to actual forms controls.
        //

        public OpenTK.GLControl glControlMain;
        public Button enableAnimationButton;
        public ComboBox currentAnimationComboBox;
        public Button playAnimationButton;
        public TrackBar timelineTrackBar;
        public ToolStripStatusLabel mainWindowStatusLabel;

        public AnimationController()
        {
            isEnabled = false;
            isAnimating = false;
            currentAnimation = String.Empty;
            timer = new Stopwatch();
            timer.Reset();
            timer.Stop();
        }

        public void DisableAnimation()
        {
            StopAnimation();

            isEnabled = false;

            enableAnimationButton.Text = "Enable";

            currentAnimationComboBox.Enabled = false;
            playAnimationButton.Enabled = false;
            timelineTrackBar.Enabled = false;

            renderer.IsSkinning = false;
        }

        public void EnableAnimation()
        {
            StopAnimation();

            // Sanity. Don't enable animation with no animations available.
            if (currentAnimationComboBox.Items.Count > 0)
            {
                isEnabled = true;

                enableAnimationButton.Text = "Disable";

                currentAnimationComboBox.Enabled = true;
                playAnimationButton.Enabled = true;
                timelineTrackBar.Enabled = true;

                renderer.IsSkinning = true;
            }
            else
            {
                mainWindowStatusLabel.Text = "Can not animate.  No animations are available.";
            }
        }

        //
        // Technically this function should only be called once when the app enters an idle state.
        // However, since I invalidate the GL window, I think that flags the app as non idle 
        // momentarily. Therefore, as long as we're animating and invalidating the GL window, this function
        // should be called repeatedly.  This works on .NET.  However, on Mono, it's only called once.
        //

        public void OnApplicationIdle(object sender, EventArgs e)
        {
            if (isAnimating == true)
            {
                TimeSpan time = timer.Elapsed;
                float elapsedTime = (float)time.Seconds + ((float)time.Milliseconds / 1000.0f);
                renderer.Update(elapsedTime);

                timer.Restart();

                UpdateTimelineTrackBar();

                glControlMain.Invalidate();
            }
        }

        private void SetAnimation()
        {
            // Cache animation name.
            currentAnimation = currentAnimationComboBox.SelectedItem.ToString();

            if (currentAnimation.Length > 0)
            {
                renderer.SetCurrentAnimation(currentAnimation);

                UpdateTimelineTrackBar();

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

        public void OnEnableAnimationButtonClick(object sender, EventArgs e)
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

        public void OnTimelineTrackBar(object sender, EventArgs e)
        {
            //
            // This function updates the renderer relative to the
            // data in the trackbar.
            //
            // Called when a user manipulates the trackbar.
            //

            StopAnimation();

            float percentageOfAnimation = (float)timelineTrackBar.Value / (float)timelineTrackBar.Maximum;
            float frameAbsolute = percentageOfAnimation * renderer.GetNumberOfFramesInCurrentAnimation();

            int selectedFrame = (int)Math.Floor( frameAbsolute );
            float percentTowardsNextFrame = frameAbsolute - selectedFrame;

            renderer.SetCurrentFrameInCurrentAnimation(selectedFrame, percentTowardsNextFrame);

            // Redraw the model.
            glControlMain.Invalidate();
        }

        //
        // Helper functions
        //

        private void UpdateTimelineTrackBar()
        {
            //
            // This function updates the trackbar relative the animation
            // data in the renderer.
            //
            // Called when the animation controller updates the renderer.
            //

            float percentageAnimated = renderer.GetPercentAnimated();
            int timelineValue = (int)Math.Floor(percentageAnimated * 100.0f); // Move the decimal into integer range.

            // Try to cut down on the amount of time we change the track bar value.
            // This way we don't spam update it.
            if (timelineValue != timelineTrackBar.Value)
            {
                timelineTrackBar.Value = timelineValue;
            }
        }

        private void StopAnimation()
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
    }
}
