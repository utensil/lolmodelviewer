

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
    class GLRiggedModel
    {
        public int version;

        public int numIndices;
        public int vao, vBuffer, iBuffer, tBuffer, nBuffer;

        public String textureName;
        public int bBuffer, wBuffer;

        public GLRig rig;

        // Not 100% if we need this.
        // Bones might map in order from the .anm files.
        public Dictionary<String, int> boneNameToIndex;

        public String   currentAnimation;
        public float    currentFrameTime;
        public int      currentFrame;
        public Dictionary<String, GLAnimation> animations;

        public GLRiggedModel()
        {
            version = 0;

            vao = vBuffer = iBuffer = tBuffer = nBuffer = numIndices = 0;

            textureName = String.Empty;
            bBuffer = wBuffer = 0;

            rig = new GLRig();

            boneNameToIndex = new Dictionary<String, int>();

            currentAnimation = String.Empty;
            currentFrameTime = 0.0f;
            currentFrame = 0;
            animations = new Dictionary<String, GLAnimation>();
        }

        // Version 2 Create
        public bool Create(int version, List<float> vertexData, List<float> normalData,
            List<float> texData, List<float> boneData, List<float> weightData, List<uint> indexData,
            List<Quaternion> bOrientation, List<Vector3> bPosition, List<float> bScale,
            List<String> bName, List<int> bParent , List<uint> boneIDs)
        {
            // Depending on the version of the model, the look ups change.
            if (version == 2)
            {
                for (int i = 0; i < boneData.Count; ++i)
                {
                    // I don't know why things need remapped, but they do.

                    // Sanity
                    if (boneData[i] < boneIDs.Count)
                    {
                        boneData[i] = boneIDs[(int)boneData[i]];
                    }
                    else
                    {
                        boneData[i] = 0;
                    }
                }
            }

            // Call the version 1 create with the remapped bone data.
            return Create(version, vertexData, normalData, texData, boneData, weightData, indexData,
                bOrientation, bPosition, bScale, bName, bParent);
        }

        // Version 1 Create
        public bool Create(int version, List<float> vertexData, List<float> normalData,
            List<float> texData, List<float> boneData, List<float> weightData, 
            List<uint> indexData, List<Quaternion> bOrientation, List<Vector3> bPosition,
            List<float> bScale, List<String> bName, List<int> bParent)
        {
            bool result = true;

            this.version = version;
            this.numIndices = indexData.Count;

            // Create the initial binding joints.
            rig.Create(bOrientation, bPosition, bScale, bParent);

            // Store the bone transforms.
            for (int i = 0; i < bOrientation.Count; ++i)
            {
                // Sanity
                if( boneNameToIndex.ContainsKey( bName[i] ) == false )
                {
                    boneNameToIndex.Add(bName[i], i);
                }
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
                bBuffer = buffers[3];
                wBuffer = buffers[4];
                iBuffer = buffers[5];

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
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(boneData.Count * sizeof(float)),
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
                GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float,
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

        public void Draw()
        {
            GL.BindVertexArray(vao);

            GL.DrawElements(BeginMode.Triangles, numIndices,
                DrawElementsType.UnsignedInt, 0);
        }
        
        public void SetTexture(String name)
        {
            textureName = name;
        }

        public void AddAnimation(String name, GLAnimation animation)
        {
            if (animations.ContainsKey(name) == false)
            {
                animations.Add(name, animation);
            }
        }

        public void SetCurrentAnimation(String name)
        {
            currentAnimation = name;
            currentFrameTime = 0.0f;
            currentFrame = 0;
        }

        public void Update(float elapsedTime)
        {
            // Sanity
            if (animations.ContainsKey(currentAnimation) == false)
                return;

            currentFrameTime += elapsedTime;

            // See if we need to switch to next frame.
            while (currentFrameTime >= animations[currentAnimation].timePerFrame)
            {
                currentFrame = (currentFrame + 1) % (int)animations[currentAnimation].numberOfFrames;
                currentFrameTime -= animations[currentAnimation].timePerFrame;
            }
        }

        public Matrix4[] GetBoneTransformations()
        {
            // Sanity.
            if (animations.ContainsKey(currentAnimation) == false)
            {
                return null;
            }

            foreach (ANMBone bone in animations[currentAnimation].bones)
            {
                if (boneNameToIndex.ContainsKey(bone.name))
                {
                    int index = boneNameToIndex[bone.name];

                    // For current frame.
                    ANMFrame frame = bone.frames[currentFrame];
                    rig.CalculateWorldSpacePose(0, index, frame.orientation,
                        frame.position);

                    // For next frame.
                    frame = bone.frames[(currentFrame + 1) % bone.frames.Count];
                    rig.CalculateWorldSpacePose(1, index, frame.orientation,
                        frame.position);
                }
                //else

                // Not sure what to do if it doesn't contain the bone.
                // Last time I checked, this does happen more frequently that I'd like to ignore.
                // It's probably why certain models don't animate properly.
            }

            return rig.GetBoneTransformations( currentFrameTime / animations[currentAnimation].timePerFrame );
        }

        public uint GetNumberOfFramesInCurrentAnimation()
        {
            uint result = 0;

            if (animations.ContainsKey(currentAnimation) == true)
            {
                result = animations[currentAnimation].numberOfFrames;
            }

            return result;
        }

        public void IncrementCurrentAnimation()
        {
            if (animations.ContainsKey(currentAnimation) == true)
            {
                currentFrame = (currentFrame + 1) % (int)animations[currentAnimation].numberOfFrames;
                currentFrameTime = 0;
            }
        }

        public void DecrementCurrentAnimation()
        {
            if (animations.ContainsKey(currentAnimation) == true)
            {
                currentFrame--;
                if (currentFrame < 0)
                {
                    currentFrame = (int)animations[currentAnimation].numberOfFrames - 1;
                }
                currentFrameTime = 0;
            }
        }

        public void SetCurrentFrame( int frame, float percentTowardsNextFrame )
        {  
            if (animations.ContainsKey(currentAnimation) == true)
            {
                // Set frame.
                currentFrame = frame % (int)animations[currentAnimation].numberOfFrames;

                // Set elapsed time towards the next frame.
                currentFrameTime = percentTowardsNextFrame * animations[currentAnimation].timePerFrame;
            }
        }

        /// <summary>
        /// Returns a decimal representing the percent
        /// of the current animation already animated.
        /// 
        /// I.E. If the currentFrame = 5 with an animation containing 10 frames, this
        /// function will return .5.
        /// </summary>
        /// <returns></returns>
        public float GetPercentageAnimated()
        {
            float result = 0.0f;

            if (animations.ContainsKey(currentAnimation) == true)
            {
                float absoluteFrame = (float)currentFrame + (currentFrameTime / animations[currentAnimation].timePerFrame);
                result = absoluteFrame / (float)animations[currentAnimation].numberOfFrames;
            }

            return result;
        }

        public void Destroy()
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

            numIndices = 0;
        }
    }
}
