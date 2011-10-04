

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
// Represents a model defined from an .skn and an .skl file. 
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
    class GLRiggedModel : GLModel
    {
        public String textureName;
        public int bBuffer, wBuffer;

        public const int MAX_BONES = 128;
        public Matrix4[] boneTransforms;

        public GLRiggedModel()
        {
            textureName = String.Empty;
            bBuffer = wBuffer = 0;

            boneTransforms = new Matrix4[128];
            for( int i = 0; i < MAX_BONES; ++i )
            {
                boneTransforms[i] = Matrix4.Identity;
            }
        }

        public bool Create( List<float> vertexData, List<float> normalData,
            List<float> texData, List<int> boneData,
            List<float> weightData, List<uint> indexData,
            List<Matrix4> btData )
        {
            bool result = true;

            numIndices = indexData.Count;

            // Store the bone transforms.
            for( int i = 0; i < btData.Count; ++i )
            {
                boneTransforms[i] = btData[i];
            }

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
            int[] buffers = new int[6];
            if (result == true)
            {
                GL.GenBuffers(6, buffers);
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
                nBuffer = buffers[1];
                tBuffer = buffers[2];
                iBuffer = buffers[3];
                bBuffer = buffers[4];
                wBuffer = buffers[5];

                GL.BindBuffer(BufferTarget.ArrayBuffer, vBuffer);
            }

            //
            //
            // Set vertex data.
            //
            //
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

            //
            //
            // Bind normal buffer.
            //
            //
            if (result == true)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, nBuffer);
            }

            // Set normal data.
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normalData.Count * sizeof(float)),
                    normalData.ToArray(), BufferUsageHint.StaticDraw);
            }

            // Check for errors.
            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Put normals into attribute slot 1.
            if (result == true)
            {
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float,
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

            //
            //
            // Bind texture cordinates buffer.
            //
            //
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

            // Put texture coords into attribute slot 2.
            if (result == true)
            {
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float,
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
                GL.EnableVertexAttribArray(2);
            }


            //
            //
            // Bind bone index buffer.
            //
            //
            if (result == true)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bBuffer);
            }

            // Set bone index data
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(boneData.Count * sizeof(int)),
                    boneData.ToArray(), BufferUsageHint.StaticDraw);
            }

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Put bone indexes into attribute slot 3.
            if (result == true)
            {
                GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Int,
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
                GL.EnableVertexAttribArray(3);
            }


            //
            //
            // Bind bone weights buffer.
            //
            //
            if (result == true)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, wBuffer);
            }

            // Set bone weight data
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(weightData.Count * sizeof(float)),
                    weightData.ToArray(), BufferUsageHint.StaticDraw);
            }

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Put bone weights into attribute slot 4.
            if (result == true)
            {
                GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float,
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
                GL.EnableVertexAttribArray(4);
            }



            //
            // Bind index buffer.
            //

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

            return result;
        }

        public void SetTexture(String name)
        {
            textureName = name;
        }

        public void Destroy()
        {
            base.Destory();

            if (bBuffer != 0)
            {
                GL.DeleteBuffers(1, ref bBuffer);
                bBuffer = 0;
            }

            if (wBuffer != 0)
            {
                GL.DeleteBuffers(1, ref wBuffer);
                wBuffer = 0;
            }
        }
    }
}
