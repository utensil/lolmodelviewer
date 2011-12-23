
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
// Represents a billboarded sprite. 
// Inheirits GLModel
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LOLViewer
{
    class GLBillboard
    {
        public int numIndices;
        public int vao, vBuffer, iBuffer, tBuffer, nBuffer;

        public GLBillboard() 
        {
            vao = vBuffer = iBuffer = tBuffer = nBuffer = numIndices = 0;
        }

        public bool Create(List<float> vertexData, List<float> texData,
                List<uint> indexData)
        {
            bool result = true;

            numIndices = indexData.Count;

            // Create Vertex Array Object
            if (result == true)
            {
                GL.GenVertexArrays(1, out vao);
            }

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Bind VAO
            if (result == true)
            {
                GL.BindVertexArray(vao);
            }

            // Create the VBOs
            int[] buffers = new int[3];
            if (result == true)
            {
                GL.GenBuffers(3, buffers);
            }

            // Check for errors
            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Store data and bind vertex buffer.
            if (result == true)
            {
                vBuffer = buffers[0];
                tBuffer = buffers[1];
                iBuffer = buffers[2];

                GL.BindBuffer(BufferTarget.ArrayBuffer, vBuffer);
            }

            // Set vertex data.
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Count * sizeof(float)),
                    vertexData.ToArray(), BufferUsageHint.StaticDraw);
            }

            // Check for errors.
            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Put vertices into attribute slot 0.
            if (result == true)
            {
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
                    false, 0, 0);
            }

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Enable the attribute index.
            if (result == true)
            {
                GL.EnableVertexAttribArray(0);
            }

            // Bind texture cordinates buffer.
            if (result == true)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, tBuffer);
            }

            // Set Texture Coordinate Data
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texData.Count * sizeof(float)),
                    texData.ToArray(), BufferUsageHint.StaticDraw);
            }

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Put texture coords into attribute slot 1.
            if (result == true)
            {
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float,
                    false, 0, 0);
            }

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Enable the attribute index.
            if (result == true)
            {
                GL.EnableVertexAttribArray(1);
            }

            // Bind index buffer.
            if (result == true)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, iBuffer);
            }

            // Set index data.
            if (result == true)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexData.Count * sizeof(uint)),
                    indexData.ToArray(), BufferUsageHint.StaticDraw);
            }

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Unbind VAO from pipeline.
            if (result == true)
            {
                GL.BindVertexArray(0);
            }

            return true;
        }

        public void Draw()
        {
            GL.BindVertexArray(vao);

            GL.DrawElements(BeginMode.Triangles, numIndices,
                DrawElementsType.UnsignedInt, 0);
        }

        public void Destory()
        {
            if (vao != 0)
            {
                GL.DeleteVertexArrays(1, ref vao);
                vao = 0;
            }

            if (vBuffer != 0)
            {
                GL.DeleteBuffers(1, ref vBuffer);
                vBuffer = 0;
            }

            if (tBuffer != 0)
            {
                GL.DeleteBuffers(1, ref tBuffer);
                tBuffer = 0;
            }

            if (nBuffer != 0)
            {
                GL.DeleteBuffers(1, ref nBuffer);
                nBuffer = 0;
            }

            if (iBuffer != 0)
            {
                GL.DeleteBuffers(1, ref iBuffer);
                iBuffer = 0;
            }

            numIndices = 0;
        }
    }
}
