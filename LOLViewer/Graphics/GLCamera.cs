

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
// Simple camera abstraction to handle
// world, view, and projection matrices. 
//
// Based off the DXUT implementation.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;

using OpenTK;

namespace LOLViewer.Graphics
{
    public class GLCamera
    {
        // Keybinding
        private enum CameraKeyValues
        {
            Left = 0,
            Right = 1,
            Forward = 2,
            Backward = 3,
            Reset = 4,
            Size = 5
        };
        private Dictionary<Keys, CameraKeyValues> bindings;

        // View / Projection Parameters
        private float fov, aspect, near, far;
        private Vector3 defaultEye, defaultTarget;

        public Vector3 Eye { get; set; }
        public Vector3 Target { get; set; }

        public Matrix4 View { get; set; }
        public Matrix4 Projection { get; set; }

        private Dictionary<CameraKeyValues, bool> keyState;

        private Dictionary<MouseButtons, bool> mouseState;
        private int wheelDelta;
        private const float WHEEL_SCALE = 0.0015f;

        private const float MINIMUM_RADIUS = 35.0f;
        private const float MAXIMUM_RADIUS = 800.0f;
        private const float STARTING_DEFAULT_RADIUS = 300.0f;
        private float radius, defaultRadius;

        private bool wasDraggedSinceLastUpdate;
        private GLArcBall viewArcBall;
        private Matrix4 lastRotation;
        private bool updateLastRotation;

        public GLCamera()
        {
            fov = aspect = near = far = 0.0f;

            Eye = defaultEye = Vector3.Zero;
            Target = defaultTarget = Vector3.Zero;

            View        = Matrix4.Identity;
            Projection  = Matrix4.Identity;

            // Default Keybindings
            bindings = new Dictionary<Keys, CameraKeyValues>();
            bindings.Add(Keys.A, CameraKeyValues.Left      );
            bindings.Add(Keys.D, CameraKeyValues.Right     );
            bindings.Add(Keys.W, CameraKeyValues.Forward   );
            bindings.Add(Keys.S, CameraKeyValues.Backward  );
            bindings.Add(Keys.R, CameraKeyValues.Reset     );
            
            keyState = new Dictionary<CameraKeyValues, bool>();
            for (CameraKeyValues i = 0; i < CameraKeyValues.Size; ++i)
            {
                keyState.Add(i, false);
            }

            wheelDelta = 0;
            mouseState = new Dictionary<MouseButtons, bool>();
            mouseState.Add(MouseButtons.Left, false);
            mouseState.Add(MouseButtons.Right, false);
            mouseState.Add(MouseButtons.Middle, false);

            viewArcBall = new GLArcBall();

            wasDraggedSinceLastUpdate = false;
            radius = defaultRadius = STARTING_DEFAULT_RADIUS;

            Reset();
        }

        //
        // Mouse Handlers
        //

        public void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            // Only rotate when left mouse is down.
            if (mouseState[MouseButtons.Left] == true)
            {
                wasDraggedSinceLastUpdate = true;
            }

            viewArcBall.OnMove((float)e.X, (float)e.Y);
        }

        public void OnMouseButtonUp(MouseEventArgs e)
        {
            mouseState[e.Button] = false;

            if (e.Button == MouseButtons.Left)
            {
                wasDraggedSinceLastUpdate = true;
                updateLastRotation = true;
                viewArcBall.OnStopDrag();
            }
        }

        public void OnMouseButtonDown(MouseEventArgs e)
        {
            mouseState[e.Button] = true;

            if (e.Button == MouseButtons.Left)
            {
                wasDraggedSinceLastUpdate = true;
                viewArcBall.OnBeginDrag((float)e.X, (float)e.Y);
            }
        }

        public void OnMouseWheel(MouseEventArgs e)
        {
            wheelDelta += e.Delta;
            wasDraggedSinceLastUpdate = true;
        }

        //
        // Keyboard handlers
        //

        public void OnKeyUp(KeyEventArgs e)
        {
            if( bindings.ContainsKey(e.KeyCode) == true )
            {
                keyState[bindings[e.KeyCode]] = false;
            }
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if( bindings.ContainsKey(e.KeyCode) == true )
            {
                keyState[bindings[e.KeyCode]] = true;
            }
        }

        public void OnUpdate()
        {
            if (keyState[CameraKeyValues.Reset] == true)
            {
                Reset();
            }

            //
            // If the camera hasn't been dragged, we don't need to update its values.
            //
            if (wasDraggedSinceLastUpdate == false)
            {
                return;
            }

            // We're updating it now.  So, toggle the flag.
            wasDraggedSinceLastUpdate = false;

            //
            // Calculate rotation.
            //

            // Update radius from mouse wheel.
            if (wheelDelta != 0)
            {
                radius -= wheelDelta * radius * WHEEL_SCALE;
            }
            radius = Math.Min(radius, MAXIMUM_RADIUS);
            radius = Math.Max(radius, MINIMUM_RADIUS);
            wheelDelta = 0;

            // Get inverse of arc ball rotation matrix.
            Matrix4 invCamRotation = viewArcBall.GetRotationMatrix();
            invCamRotation = lastRotation * invCamRotation;
            invCamRotation.Invert();

            // Transform vectors based on rotation matrix
            Vector3 worldUp = Vector3.Transform( new Vector3( 0.0f, 1.0f, 0.0f ),
                        invCamRotation);
            Vector3 worldAhead = Vector3.Transform( new Vector3(0.0f, 0.0f, 1.0f),
                        invCamRotation);

            // Update the camera's eye.
            Eye = Target - (worldAhead * radius);

            // Update view.
            View = Matrix4.LookAt(Eye, Target, worldUp);

            // After a drag has finished
            if (updateLastRotation == true)
            {
                // Update the last rotation from the arc ball and reset it.
                lastRotation = lastRotation * viewArcBall.GetRotationMatrix();
                viewArcBall.SetNowQuat(Quaternion.Identity);
                updateLastRotation = false;
            }
        }

        /// <summary>
        /// Set up the view parameters for the camera.
        /// </summary>
        /// <param name="eye">The location of the eye of the camera.</param>
        /// <param name="target">The direction of the camera.</param>
        public void SetViewParameters(Vector3 eye, Vector3 target)
        {
            // Store parameters
            this.Eye = this.defaultEye = eye;
            this.Target = this.defaultTarget = target;

            // Update view matrix.
            Matrix4 rotation = Matrix4.LookAt(eye, target, new Vector3(0.0f, 1.0f, 0.0f));
            View = rotation;

            // Update arc ball.
            Quaternion quat = OpenTKExtras.Matrix4.CreateQuatFromMatrix(rotation);
            viewArcBall.SetNowQuat(quat);

            // Force camera to update.
            wasDraggedSinceLastUpdate = true;
            OnUpdate();
        }

        /// <summary>
        /// Set up the projection matrix for the camera.
        /// </summary>
        /// <param name="fov">The field of view. (in radians)</param>
        /// <param name="width">The width of window.</param>
        /// <param name="height">The height of window.</param>
        /// <param name="near">The near clipping plane.</param>
        /// <param name="far">The far clipping plane.</param>
        public void SetProjectionParameters(float fov, float width, float height, 
            float near, float far)
        {
            // Store parameters.
            this.fov = fov;
            this.aspect = width / height;
            this.near = near;
            this.far = far;

            // Update projection matrix.
            Projection = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, near, far);

            // Update arc ball.
            viewArcBall.SetWindow(width, height);

            // Force camera to update.
            wasDraggedSinceLastUpdate = true;
            OnUpdate();
        }

        public void Reset()
        {
            Eye = defaultEye;
            Target = defaultTarget;

            radius = defaultRadius;
            viewArcBall.Reset();

            lastRotation = Matrix4.Identity;
            updateLastRotation = false;

            keyState[CameraKeyValues.Reset] = false;

            // Force camera to update.
            wasDraggedSinceLastUpdate = true;
            OnUpdate();
        }
    }
}
