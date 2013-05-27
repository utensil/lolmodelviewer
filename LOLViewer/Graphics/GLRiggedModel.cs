

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
// Represents a model defined from an .skn and an .skl file. 
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
    class GLRiggedModel
    {
        public String TextureName { get; set; }
        public List<String> AnimationNames
        {
            get
            {
                List<String> result = new List<String>();

                foreach (var animation in animations)
                {
                    result.Add(animation.Key);
                }

                return result;
            }
        }

        // OpenGL objects.
        private int numIndices;
        private int vao, vertexPositionBuffer, indexBuffer, vertexTextureCoordinateBuffer, vertexNormalBuffer,
            vertexBoneBuffer, vertexBoneWeightBuffer;

        private String currentAnimation;
        private float currentFrameTime;
        private int currentFrame;
        private Dictionary<String, GLAnimation> animations;

        #region Initialization

        public GLRiggedModel()
        {
            TextureName = String.Empty;

            vao = vertexPositionBuffer = indexBuffer = vertexTextureCoordinateBuffer = vertexNormalBuffer = 
                numIndices = vertexBoneBuffer = vertexBoneWeightBuffer = 0;

            currentAnimation = String.Empty;
            currentFrameTime = 0.0f;
            currentFrame = 0;
            animations = new Dictionary<String, GLAnimation>();
        }

        /// <summary>
        /// Loads data from SKN and SKL files into OpenGL.
        /// </summary>
        /// <param name="skn">The .skn data.</param>
        /// <param name="skl">The .skl data.</param>
        /// <returns></returns>
        public bool Create(SKNFile skn, SKLFile skl, Dictionary<String, ANMFile> anms, Logger logger)
        {
            bool result = true;

            // This function converts the handedness of the DirectX style input data
            // into the handedness OpenGL expects.
            // So, vector inputs have their Z value negated and quaternion inputs have their
            // Z and W values negated.

            // Vertex Data
            List<float> vertexPositions = new List<float>();
            List<float> vertexNormals = new List<float>();
            List<float> vertexTextureCoordinates = new List<float>();
            List<float> vertexBoneIndices = new List<float>();
            List<float> vertexBoneWeights = new List<float>();
            List<uint> indices = new List<uint>();

            // Animation data.
            List<OpenTK.Quaternion> boneOrientations = new List<OpenTK.Quaternion>();
            List<OpenTK.Vector3> bonePositions = new List<OpenTK.Vector3>();
            List<float> boneScales = new List<float>();
            List<int> boneParents = new List<int>();
            List<String> boneNames = new List<String>();

            // Bones are not always in order between the ANM and SKL files.
            Dictionary<String, int> boneNameToID = new Dictionary<String, int>();
            Dictionary<int, String> boneIDToName = new Dictionary<int, String>();

            for (int i = 0; i < skn.numVertices; ++i)
            {
                // Position Information
                vertexPositions.Add(skn.vertices[i].position[0]);
                vertexPositions.Add(skn.vertices[i].position[1]);
                vertexPositions.Add(-skn.vertices[i].position[2]);

                // Normal Information
                vertexNormals.Add(skn.vertices[i].normal[0]);
                vertexNormals.Add(skn.vertices[i].normal[1]);
                vertexNormals.Add(-skn.vertices[i].normal[2]);

                // Tex Coords Information
                vertexTextureCoordinates.Add(skn.vertices[i].texCoords[0]);
                vertexTextureCoordinates.Add(skn.vertices[i].texCoords[1]);

                // Bone Index Information
                for (int j = 0; j < SKNVertex.BONE_INDEX_SIZE; ++j)
                {
                    vertexBoneIndices.Add(skn.vertices[i].boneIndex[j]);
                }

                // Bone Weight Information
                vertexBoneWeights.Add(skn.vertices[i].weights[0]);
                vertexBoneWeights.Add(skn.vertices[i].weights[1]);
                vertexBoneWeights.Add(skn.vertices[i].weights[2]);
                vertexBoneWeights.Add(skn.vertices[i].weights[3]);
            }

            // Animation data
            for (int i = 0; i < skl.numBones; ++i)
            {
                Quaternion orientation = Quaternion.Identity;
                if (skl.version == 0)
                {
                    // Version 0 SKLs contain a quaternion.
                    orientation.X = skl.bones[i].orientation[0];
                    orientation.Y = skl.bones[i].orientation[1];
                    orientation.Z = -skl.bones[i].orientation[2];
                    orientation.W = -skl.bones[i].orientation[3];
                }
                else
                {
                    // Other SKLs contain a rotation matrix.

                    // Create a matrix from the orientation values.
                    Matrix4 transform = Matrix4.Identity;

                    transform.M11 = skl.bones[i].orientation[0];
                    transform.M21 = skl.bones[i].orientation[1];
                    transform.M31 = skl.bones[i].orientation[2];

                    transform.M12 = skl.bones[i].orientation[4];
                    transform.M22 = skl.bones[i].orientation[5];
                    transform.M32 = skl.bones[i].orientation[6];

                    transform.M13 = skl.bones[i].orientation[8];
                    transform.M23 = skl.bones[i].orientation[9];
                    transform.M33 = skl.bones[i].orientation[10];

                    // Convert the matrix to a quaternion.
                    orientation = OpenTKExtras.Matrix4.CreateQuatFromMatrix(transform);
                    orientation.Z = -orientation.Z;
                    orientation.W = -orientation.W;
                }

                boneOrientations.Add(orientation);

                // Create a vector from the position values.
                Vector3 position = Vector3.Zero;
                position.X = skl.bones[i].position[0];
                position.Y = skl.bones[i].position[1];
                position.Z = -skl.bones[i].position[2];
                bonePositions.Add(position);

                boneNames.Add(skl.bones[i].name);
                boneNameToID[skl.bones[i].name] = i;
                boneIDToName[i] = skl.bones[i].name;

                boneScales.Add(skl.bones[i].scale);
                boneParents.Add(skl.bones[i].parentID);
            }

            //
            // Version 0 SKL files are similar to the animation files.
            // The bone positions and orientations are relative to their parent.
            // So, we need to compute their absolute location by hand.
            //
            if (skl.version == 0)
            {
                //
                // This algorithm is a little confusing since it's indexing identical data from
                // the SKL file and the local variable List<>s. The indexing scheme works because
                // the List<>s are created in the same order as the data in the SKL files.
                //
                for (int i = 0; i < skl.numBones; ++i)
                {
                    // Only update non root bones.
                    if (skl.bones[i].parentID != -1)
                    {
                        // Determine the parent bone.
                        int parentBoneID = skl.bones[i].parentID;

                        // Update orientation.
                        // Append quaternions for rotation transform B * A.
                        boneOrientations[i] = boneOrientations[parentBoneID] * boneOrientations[i];
                        
                        Vector3 localPosition = Vector3.Zero;
                        localPosition.X = skl.bones[i].position[0];
                        localPosition.Y = skl.bones[i].position[1];
                        localPosition.Z = skl.bones[i].position[2];
                        
                        // Update position.
                        bonePositions[i] = bonePositions[parentBoneID] + Vector3.Transform(localPosition, boneOrientations[parentBoneID]);
                    }
                }
            }

            // Depending on the version of the model, the look ups change.
            if (skl.version == 2 || skl.version == 0)
            {
                for (int i = 0; i < vertexBoneIndices.Count; ++i)
                {
                    // I don't know why things need remapped, but they do.

                    // Sanity
                    if (vertexBoneIndices[i] < skl.boneIDs.Count)
                    {
                        vertexBoneIndices[i] = skl.boneIDs[(int)vertexBoneIndices[i]];
                    }
                }
            }

            // Add the animations.
            foreach (var animation in anms)
            {
                if (animations.ContainsKey(animation.Key) == false)
                {
                    // Create the OpenGL animation wrapper.
                    GLAnimation glAnimation = new GLAnimation();

                    glAnimation.playbackFPS = animation.Value.playbackFPS;
                    glAnimation.numberOfBones = animation.Value.numberOfBones;
                    glAnimation.numberOfFrames = animation.Value.numberOfFrames;

                    // Convert ANMBone to GLBone.
                    foreach (ANMBone bone in animation.Value.bones)
                    {
                        GLBone glBone = new GLBone();

                        if (animation.Value.version == 4 && skl.boneIDMap.Count > 0)
                        {
                            // Version 4 ANM files contain a hash value to represent the bone ID/name.
                            // We need to use the map from the SKL file to match the ANM bone with the correct
                            // SKL bone.

                            if (skl.boneIDMap.ContainsKey(bone.id))
                            {
                                int sklID = (int)skl.boneIDMap[bone.id];
                                glBone.name = boneIDToName[sklID];
                            }
                        }
                        else
                        {
                            glBone.name = bone.name;
                        }

                        // Convert ANMFrame to Matrix4.
                        foreach (ANMFrame frame in bone.frames)
                        {
                            Matrix4 transform = Matrix4.Identity;

                            Quaternion quat = new Quaternion(frame.orientation[0], frame.orientation[1], -frame.orientation[2], -frame.orientation[3]);
                            transform = Matrix4.Rotate(quat);

                            transform.M41 = frame.position[0];
                            transform.M42 = frame.position[1];
                            transform.M43 = -frame.position[2];

                            glBone.frames.Add(transform);
                        }

                        glAnimation.bones.Add(glBone);
                    }

                    glAnimation.timePerFrame = 1.0f / (float)animation.Value.playbackFPS;

                    // Store the animation.
                    animations.Add(animation.Key, glAnimation);
                }
            }

            // Index Information
            for (int i = 0; i < skn.numIndices; ++i)
            {
                indices.Add((uint)skn.indices[i]);
            }
            this.numIndices = indices.Count;

            //
            // Compute the final animation transforms.
            //

            foreach (var animation in animations)
            {
                // This is sort of a mess. 
                // We need to make sure "parent" bones are always updated before their "children".  The SKL file contains
                // bones ordered in this manner.  However, ANM files do not always do this.  So, we sort the bones in the ANM to match the ordering in
                // the SKL file.
                animation.Value.bones.Sort( (a, b) =>
                {
                    if (boneNameToID.ContainsKey(a.name) && boneNameToID.ContainsKey(b.name))
                    {
                        return boneNameToID[a.name].CompareTo(boneNameToID[b.name]);
                    }
                    else if (boneNameToID.ContainsKey(a.name) == false)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                });
            }

            // Create the binding transform.  (The SKL initial transform.)
            GLAnimation bindingBones = new GLAnimation();
            for (int i = 0; i < boneOrientations.Count; ++i)
            {
                GLBone bone = new GLBone();

                bone.name = boneNames[i];
                bone.parent = boneParents[i];

                bone.transform = Matrix4.Rotate(boneOrientations[i]);
                bone.transform.M41 = bonePositions[i].X;
                bone.transform.M42 = bonePositions[i].Y;
                bone.transform.M43 = bonePositions[i].Z;

                bone.transform = Matrix4.Invert(bone.transform);

                bindingBones.bones.Add(bone);
            }
            
            // Convert animations into absolute space.
            foreach (var animation in animations)
            {
                foreach (var bone in animation.Value.bones)
                {
                    // Sanity.
                    if (boneNameToID.ContainsKey(bone.name))
                    {
                        int id = boneNameToID[bone.name];
                        bone.parent = bindingBones.bones[id].parent;

                        // For each frame...
                        for (int i = 0; i < bone.frames.Count; ++i)
                        {
                            Matrix4 parentTransform = Matrix4.Identity;
                            if (bone.parent >= 0)
                            {
                                if (bone.parent < animation.Value.bones.Count)
                                {
                                    GLBone parent = animation.Value.bones[bone.parent];
                                    parentTransform = parent.frames[i];
                                }
                            }
                            bone.frames[i] = bone.frames[i] * parentTransform;
                        }
                    }
                }
            }

            // Multiply the animation transforms by the binding transform.
            foreach (var animation in animations)
            {
                foreach (var bone in animation.Value.bones)
                {
                    // Sanity.
                    if (boneNameToID.ContainsKey(bone.name))
                    {
                        int id = boneNameToID[bone.name];
                        GLBone bindingBone = bindingBones.bones[id];

                        for (int i = 0; i < bone.frames.Count; ++i)
                        {
                            bone.frames[i] = bindingBone.transform * bone.frames[i];
                        }
                    }
                }
            }

            // Create the OpenGL objects.
            result = Create(vertexPositions, vertexNormals, vertexTextureCoordinates,
                vertexBoneIndices, vertexBoneWeights, indices, logger);

            return result;
        }

#endregion

        #region Rendering API

        public void Draw()
        {
            GL.BindVertexArray(vao);

            GL.DrawElements(BeginMode.Triangles, numIndices,
                DrawElementsType.UnsignedInt, 0);
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

        /// <summary>
        /// Computes the bone transformations for the model.
        /// </summary>
        /// <param name="transforms">A preallocated array of transformation matrices.</param>
        /// <returns>The resultant bone transformation.  This value is identical to the reference parameter result.</returns>
        public Matrix4[] GetBoneTransformations(ref Matrix4[] transforms)
        {
            // Sanity.
            if (animations.ContainsKey(currentAnimation) == true)
            {
                //
                // Normal Case
                //
                // Interpolate between the current and next frames to calculate
                // the transform to send to the renderer.
                //

                GLAnimation animation = animations[currentAnimation];
                float blend = currentFrameTime / animation.timePerFrame;

                int nextFrame = (currentFrame + 1) % (int)animation.numberOfFrames;
                for (int i = 0; i < animation.bones.Count; ++i)
                {
                    // Get the current frame's transform.
                    Matrix4 current = animation.bones[i].frames[currentFrame];

                    //
                    // The interpolation code is unstable.  It creates alot of animation anomalies.
                    //
                    // TODO: Fix it.
                    //

                    // Break it down into a vector and quaternion.
                    Vector3 currentPosition = Vector3.Zero;
                    currentPosition.X = current.M41;
                    currentPosition.Y = current.M42;
                    currentPosition.Z = current.M43;
                    
                    Quaternion currentOrientation = OpenTKExtras.Matrix4.CreateQuatFromMatrix(current);
                    
                    // Get the next frame's transform.
                    Matrix4 next = animation.bones[i].frames[nextFrame];
                    
                    // Break it down into a vector and quaternion.
                    Vector3 nextPosition = Vector3.Zero;
                    nextPosition.X = next.M41;
                    nextPosition.Y = next.M42;
                    nextPosition.Z = next.M43;
                    
                    Quaternion nextOrientation = OpenTKExtras.Matrix4.CreateQuatFromMatrix(next);
                    
                    // Interpolate the frame data.
                    Vector3 finalPosition = Vector3.Lerp(currentPosition, nextPosition, blend);
                    Quaternion finalOrientation = Quaternion.Slerp(currentOrientation, nextOrientation, blend);
                    
                    // Rebuild a transform.
                    Matrix4 finalTransform = Matrix4.Rotate(finalOrientation);
                    finalTransform.M41 = finalPosition.X;
                    finalTransform.M42 = finalPosition.Y;
                    finalTransform.M43 = finalPosition.Z;
                    
                    transforms[i] = finalTransform;
                }
            }
            else
            {
                //
                // Case when the animation is not present.
                //

                for (int i = 0; i < transforms.Length; ++i)
                {
                    transforms[i] = Matrix4.Identity;
                }
            }

            return transforms;
        }

        public void Destroy()
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

            if (vertexBoneBuffer != 0)
            {
                GL.DeleteBuffers(1, ref vertexBoneBuffer);
                vertexBoneBuffer = 0;
            }

            if (vertexBoneWeightBuffer != 0)
            {
                GL.DeleteBuffers(1, ref vertexBoneWeightBuffer);
                vertexBoneWeightBuffer = 0;
            }

            numIndices = 0;
        }


#endregion

        #region Animation API

        public void SetCurrentAnimation(String name)
        {
            currentAnimation = name;
            currentFrameTime = 0.0f;
            currentFrame = 0;
        }

        /// <summary>
        /// Sets the current frame.
        /// </summary>
        /// <param name="frame">The index of the frame.</param>
        /// <param name="percentTowardsNextFrame">The percent towards the next frame.  Expects a number between 0 - 1.</param>
        public void SetCurrentFrame(int frame, float percentTowardsNextFrame)
        {
            if (animations.ContainsKey(currentAnimation) == true)
            {
                // Set frame.
                currentFrame = frame % (int)animations[currentAnimation].numberOfFrames;

                // Set elapsed time towards the next frame.
                currentFrameTime = percentTowardsNextFrame * animations[currentAnimation].timePerFrame;
            }
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

        /// <summary>
        /// Returns a decimal representing the percent
        /// of the current animation already animated.
        /// 
        /// I.E. If the currentFrame = 5 with an animation containing 10 frames, this
        /// function will return .5.
        /// </summary>
        /// <returns></returns>
        public float GetPercentAnimated()
        {
            float result = 0.0f;

            if (animations.ContainsKey(currentAnimation) == true)
            {
                float absoluteFrame = (float)currentFrame + (currentFrameTime / animations[currentAnimation].timePerFrame);
                result = absoluteFrame / (float)animations[currentAnimation].numberOfFrames;
            }

            return result;
        }

        #endregion

        #region Helpers

        private bool Create(List<float> vertexPositions, List<float> vertexNormals,
            List<float> vertexTextureCoordinates, List<float> vertexBoneIndices, List<float> vertexBoneWeights,
            List<uint> indices, Logger logger)
        {
            bool result = true;

            logger.Event("Creating GL rigged model.");

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
                vertexPositionBuffer = buffers[0];
                vertexNormalBuffer = buffers[1];
                vertexTextureCoordinateBuffer = buffers[2];
                vertexBoneBuffer = buffers[3];
                vertexBoneWeightBuffer = buffers[4];
                indexBuffer = buffers[5];

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


            //
            //
            // Bind bone index buffer.
            //
            //
            if (result == true)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBoneBuffer);
            }

            // Set bone index data
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexBoneIndices.Count * sizeof(float)),
                    vertexBoneIndices.ToArray(), BufferUsageHint.StaticDraw);
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
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBoneWeightBuffer);
            }

            // Set bone weight data
            if (result == true)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexBoneWeights.Count * sizeof(float)),
                    vertexBoneWeights.ToArray(), BufferUsageHint.StaticDraw);
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

            if (result == false)
            {
                logger.Error("Failed to create GL rigged model.");
            }

            return result;
        }

#endregion
    }
}
