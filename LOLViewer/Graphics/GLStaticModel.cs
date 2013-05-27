
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
// Represents a model defined from an .skn file. 
// Inheirits GLModel
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using LOLFileReader;

using LOLViewer.IO;

using CSharpLogger;

namespace LOLViewer.Graphics
{
    class GLStaticModel
    {
        public String TextureName { get; set; }

        private int numIndices;

        // OpenGL objects.
        private int vao, vertexPositionBuffer, indexBuffer, vertexTextureCoordinateBuffer, vertexNormalBuffer;

        public GLStaticModel() 
        {
            vao = vertexPositionBuffer = indexBuffer = vertexTextureCoordinateBuffer = vertexNormalBuffer = numIndices = 0;
            TextureName = String.Empty;
        }

        public bool Create(SKNFile file, Logger logger)
        {
            // This function converts the handedness of the DirectX style input data
            // into the handedness OpenGL expects.
            // So, vector inputs have their Z value negated and quaternion inputs have their
            // Z and W values negated.

            List<float> vertexPositions = new List<float>();
            List<float> vertexNormals = new List<float>();
            List<float> vertexTextureCoordinates = new List<float>();

            for (int i = 0; i < file.numVertices; ++i)
            {
                vertexPositions.Add(file.vertices[i].position[0]);
                vertexPositions.Add(file.vertices[i].position[1]);
                vertexPositions.Add(-file.vertices[i].position[2]);

                vertexNormals.Add(file.vertices[i].normal[0]);
                vertexNormals.Add(file.vertices[i].normal[1]);
                vertexNormals.Add(-file.vertices[i].normal[2]);

                vertexTextureCoordinates.Add(file.vertices[i].texCoords[0]);
                vertexTextureCoordinates.Add(file.vertices[i].texCoords[1]);
            }

            List<uint> iData = new List<uint>();
            for (int i = 0; i < numIndices; ++i)
            {
                iData.Add((uint)file.indices[i]);
            }

            return Create(vertexPositions, vertexNormals, vertexTextureCoordinates, iData, logger);
        }

        //
        // Helper creation function.
        //

        private bool Create(List<float> vertexPositions, List<float> vertexNormals,
            List<float> vertexTextureCoordinates, List<uint> indices, Logger logger)
        {
            bool result = true;

            logger.Event("Creating OpenGL static model.");

            numIndices = indices.Count;

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
            int[] buffers = new int[4];
            if (result == true)
            {
                GL.GenBuffers(4, buffers);
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
                vertexPositionBuffer = buffers[0];
                vertexNormalBuffer = buffers[1];
                vertexTextureCoordinateBuffer = buffers[2];
                indexBuffer = buffers[3];

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexPositionBuffer);
            }

            //
            //
            // Set vertex data.
            //
            //
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexPositions.Count * sizeof(float)),
                    vertexPositions.ToArray(), BufferUsageHint.StaticDraw);
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
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexNormalBuffer);
            }

            // Set normal data.
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexNormals.Count * sizeof(float)),
                    vertexNormals.ToArray(), BufferUsageHint.StaticDraw);
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
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexTextureCoordinateBuffer);
            }

            // Set Texture Coordinate Data
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexTextureCoordinates.Count * sizeof(float)),
                    vertexTextureCoordinates.ToArray(), BufferUsageHint.StaticDraw);
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

            // Bind index buffer.
            if (result == true)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            }

            // Set index data.
            if (result == true)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Count * sizeof(uint)),
                    indices.ToArray(), BufferUsageHint.StaticDraw);
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
            else
            {
                logger.Error("Failed to create OpenGL static model.");
            }

            return result;
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

            if (vertexPositionBuffer != 0)
            {
                GL.DeleteBuffers(1, ref vertexPositionBuffer);
                vertexPositionBuffer = 0;
            }

            if (vertexTextureCoordinateBuffer != 0)
            {
                GL.DeleteBuffers(1, ref vertexTextureCoordinateBuffer);
                vertexTextureCoordinateBuffer = 0;
            }

            if (vertexNormalBuffer != 0)
            {
                GL.DeleteBuffers(1, ref vertexNormalBuffer);
                vertexNormalBuffer = 0;
            }

            if (indexBuffer != 0)
            {
                GL.DeleteBuffers(1, ref indexBuffer);
                indexBuffer = 0;
            }

            numIndices = 0;
        }
    }
}
