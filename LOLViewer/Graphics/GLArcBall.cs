
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
// Allows mouse input to specify orientation of
// an object. 
//
// Based off the DXUT implementation.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace LOLViewer.Graphics
{
    class GLArcBall
    {
        private bool isBeingDragged;

        private Vector3 downPoint;
        private Vector3 currentPoint;

        private Quaternion down;
        private Quaternion now;

        private float width, height;

        public GLArcBall()
        {
            Reset();

            width = height = 0.0f;
        }

        public void Reset()
        {
            isBeingDragged = false;

            downPoint = Vector3.Zero;
            currentPoint = Vector3.Zero;

            down = Quaternion.Identity;
            now = Quaternion.Identity;
        }

        public void SetWindow(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public void SetNowQuat(Quaternion q)
        {
            now = q;
        }

        /// <summary>
        /// Call when the user begins to change orientation.
        /// </summary>
        /// <param name="x">Actual position. Not delta.</param>
        /// <param name="y">Acutal position. Not delta.</param>
        public void OnBeginDrag(float x, float y)
        {
            isBeingDragged = true;
            down = now;
            downPoint = ScreenToVector(x, y);
        }

        /// <summary>
        /// Call when mouse is moved.
        /// </summary>
        /// <param name="x">Actual position. Not delta.</param>
        /// <param name="y">Actual position. Not delta.</param>
        public void OnMove(float x, float y)
        {
            if (isBeingDragged == true)
            {
                currentPoint = ScreenToVector(x, y);
                now = down * QuatFromBallPoints(downPoint, currentPoint);
            }
        }

        public void OnStopDrag()
        {
            isBeingDragged = false;
        }

        public Matrix4 GetRotationMatrix()
        {   
            return Matrix4.Rotate(now);
        }

        //
        // Helper Functions
        //

        private Vector3 ScreenToVector(float screentPointX, float screenPointY)
        {
            float x =  (screentPointX - width * 0.5f) / (width * 0.5f );
            float y =  -(screenPointY - height * 0.5f) / (height * 0.5f);
            float z = 0.0f;

            float mag = x * x + y * y;

            // Selection is "outside" the arc ball.
            if (mag > 1.0f)
            {
                float scale = 1.0f / (float)Math.Sqrt(mag);

                x *= scale;
                y *= scale;
            }
            // Normal arc ball case.
            else
            {
                z = (float)Math.Sqrt(1.0f - mag);
            }

            return new Vector3(-x, y, -z);
        }

        private Quaternion QuatFromBallPoints(Vector3 from, Vector3 to)
        {
            float dot = Vector3.Dot(from, to);
            Vector3 part = Vector3.Cross(from, to);

            return new Quaternion(part.X, part.Y, part.Z, dot);
        }
    }
}
