

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

namespace LOLViewer
{
    class GLCamera
    {
        // Keybinding
        public enum CameraKeyValues
        {
            Left = 0,
            Right = 1,
            Forward = 2,
            Backward = 3,
            Reset = 4,
            Size = 5
        };
        public Dictionary<Keys, CameraKeyValues> bindings;

        // View / Projection Parameters
        public float fov, aspect, near, far;
        public Vector3 eye, target, defaultEye, defaultTarget;
        public Matrix4 view, projection;

        // We need to account for the fact that OpenGL does not have
        // the same handedness as DirectX.  So, we compute everything 
        // left handed as a DirectX pipeline would.  Then, at the very last step,
        // we multiply in the conversion into the view matrix.  This will convert
        // the data to a right handed system.
        public Matrix4 handConverter;

        public Dictionary<CameraKeyValues, bool> keyState;
        public const float MOVEMENT_SCALE = 100.0f;
        public Vector3 velocity;

        public Dictionary<MouseButtons, bool> mouseState;
        public int wheelDelta;
        private const float WHEEL_SCALE = 0.1f;

        private bool wasDraggedSinceLastUpdate;
        private Vector3 center;
        private const float RADIUS_SCALE = 0.0125f;
        private float radius, defaultRadius, minRadius, maxRadius;
        private GLArcBall viewArcBall;
        private Matrix4 lastRotation;
        private bool updateLastRotation;

        public GLCamera()
        {
            fov = aspect = near = far = 0.0f;

            eye = defaultEye = Vector3.Zero;
            target = defaultTarget = Vector3.Zero;

            view        = Matrix4.Identity;
            projection  = Matrix4.Identity;
            handConverter = Matrix4.Identity;
            handConverter.M33 = -handConverter.M33;

            // Default Keybindings
            bindings = new Dictionary<Keys, CameraKeyValues>();
            bindings.Add(Keys.A, CameraKeyValues.Left      );
            bindings.Add(Keys.D, CameraKeyValues.Right     );
            bindings.Add(Keys.W, CameraKeyValues.Forward   );
            bindings.Add(Keys.S, CameraKeyValues.Backward  );
            bindings.Add(Keys.R, CameraKeyValues.Reset     );
            
            velocity = Vector3.Zero;
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
            center = Vector3.Zero;
            radius = defaultRadius = 300.0f;
            minRadius = 20.0f;
            maxRadius = 1000.0f;

            viewArcBall.SetRadius(radius * RADIUS_SCALE);

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

        public void OnUpdate(float elapsedTime)
        {
            if (keyState[CameraKeyValues.Reset] == true)
            {
                Reset();
            }

            if (wasDraggedSinceLastUpdate == false)
                return;
            wasDraggedSinceLastUpdate = false;

            // Not doing translation at the moment.
            /*
            if (keyState[CameraKeyValues.Left] == true)
            {
                keyboardDirection.X += -1.0f;
            }

            if (keyState[CameraKeyValues.Right] == true)
            {
                keyboardDirection.X += 1.0f;
            }

            if (keyState[CameraKeyValues.Backward] == true)
            {
                keyboardDirection.X += 1.0f;
            }

            if (keyState[CameraKeyValues.Forward] == true)
            {
                keyboardDirection.X += -1.0f;
            }
            */

            //
            // Calculate rotation and translation vectors.
            //

            if (velocity.Length > 0.0f)
            {
                velocity.Normalize();
            }
            velocity *= MOVEMENT_SCALE;

            // Integrate
            Vector3 positionDelta = velocity * elapsedTime;

            // Update radius from mouse wheel.
            if (wheelDelta != 0)
            {
                radius -= wheelDelta * WHEEL_SCALE;
            }
            radius = Math.Min(radius, maxRadius);
            radius = Math.Max(radius, minRadius);
            viewArcBall.SetRadius(radius * RADIUS_SCALE);
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

            // Transform delta position
            Vector3 posDeltaWorld = Vector3.Transform(positionDelta, invCamRotation);
            
            // Move position
            target += posDeltaWorld;

            // Update eye
            eye = target - worldAhead * radius;

            // Update view
            view = Matrix4.LookAt(eye, target, worldUp);
            view = handConverter * view;

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
        /// <param name="eye">We the eye of the camera is located.</param>
        /// <param name="target">We the eye of the camera is looking.</param>
        public void SetViewParameters(Vector3 eye, Vector3 target)
        {
            this.eye = this.defaultEye = eye;
            this.target = this.defaultTarget = target;

            view = Matrix4.LookAt(eye, target, new Vector3(0.0f, 1.0f, 0.0f));
            view = handConverter * view;

            // Update arc ball
            Matrix4 rotation = Matrix4.LookAt(eye, target, new Vector3(0.0f, 1.0f, 0.0f));
            Quaternion quat = OpenTKExtras.Matrix4.CreateQuatFromMatrix( rotation );
            viewArcBall.SetNowQuat(quat);

            // Update radius
            Vector3 eyeToPoint = target - eye;
            radius = eyeToPoint.Length;
            viewArcBall.SetRadius(radius * RADIUS_SCALE);

            wasDraggedSinceLastUpdate = true;
        }

        /// <summary>
        /// Set up the projection matrix for the camera.
        /// </summary>
        /// <param name="fov">In radians.</param>
        /// <param name="width">Width of window.</param>
        /// <param name="height">Height of window.</param>
        /// <param name="near">Near clipping plane.</param>
        /// <param name="far">Far clipping plane.</param>
        public void SetProjectionParameters(float fov, float width, float height, 
            float near, float far)
        {
            this.fov = fov;
            this.aspect = width / height;
            this.near = near;
            this.far = far;

            projection = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, near, far);

            viewArcBall.SetWindow(width, height);
        }

        public void Reset()
        {
            eye = defaultEye;
            target = defaultTarget;

            radius = defaultRadius;
            viewArcBall.Reset();
            viewArcBall.SetRadius(radius * RADIUS_SCALE);

            wasDraggedSinceLastUpdate = true;

            lastRotation = Matrix4.Identity;
            updateLastRotation = false;

            keyState[CameraKeyValues.Reset] = false;

            // Force camera to update it's view params.
            OnUpdate(0.1f);
        }
    }
}
