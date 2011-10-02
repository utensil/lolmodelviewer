
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

namespace LOLViewer
{
    class GLArcBall
    {
        private bool isBeingDragged;

        private Vector2 offset;

        private Vector3 downPoint;
        private Vector3 currentPoint;

        private Quaternion down;
        private Quaternion now;

        private Matrix4 rotation;
        private Matrix4 translation;
        private Matrix4 translationDelta;

        private const float DEFAULT_RADIUS = 2.0f;
        private float radius;

        private float width, height;
        private Vector2 center;

        public GLArcBall()
        {
            Reset();

            offset = Vector2.Zero;

            width = height = 0.0f;
            center = Vector2.Zero;
        }

        public void Reset()
        {
            isBeingDragged = false;

            downPoint = Vector3.Zero;
            currentPoint = Vector3.Zero;

            down = Quaternion.Identity;
            now = Quaternion.Identity;

            rotation = Matrix4.Identity;
            translation = Matrix4.Identity;
            translationDelta = Matrix4.Identity;

            radius = DEFAULT_RADIUS;
        }

        public void SetWindow(float width, float height)
        {
            this.width = width;
            this.height = height;
            this.radius = DEFAULT_RADIUS;

            center = new Vector2(width / 2.0f, height / 2.0f);
        }

        public void SetOffset(float x, float y)
        {
            offset.X = x;
            offset.Y = y;
        }

        public void SetNowQuat(Quaternion q)
        {
            now = q;
        }

        public void SetRadius(float r)
        {
            radius = r;
        }

        /// <summary>
        /// Call when the user begins to change orientation.
        /// </summary>
        /// <param name="x">Actual position. Not delta.</param>
        /// <param name="y">Acutal position. Not delta.</param>
        public void OnBeginDrag(float x, float y)
        {
            if( x >= offset.X &&
                x < offset.X + width &&
                y >= offset.Y &&
                y < offset.Y + height )
            {
                isBeingDragged = true;
                down = now;
                downPoint = ScreenToVector(x, y);
            }
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
            float x =  (screentPointX - offset.X - width / 2.0f) / (radius * width / 2.0f );
            float y =  -(screenPointY - offset.Y - height / 2.0f) / (radius * height / 2.0f);
            float z = 0.0f;

            float mag = x * x + y * y;

            if (mag > 1.0f)
            {
                float scale = 1.0f / (float)Math.Sqrt(mag);

                x *= scale;
                y *= scale;
            }
            else
            {
                z = (float)Math.Sqrt(1.0f - mag);
            }

            return new Vector3(x, y, z);
        }

        private Quaternion QuatFromBallPoints(Vector3 from, Vector3 to)
        {
            float dot = Vector3.Dot(from, to);
            Vector3 part = Vector3.Cross(from, to);

            return new Quaternion(part.X, part.Y, part.Z, dot);
        }
    }
}
