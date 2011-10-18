

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
// Encapsulates the process from going from binding joint transformations
// and animation bone transformations to a skinning transformation.
//

// I finally got all this stuff nailed down for most models.  This deserves an asci pokemon.

/*
____________¶¶
___________¶¶¶¶
__________¶¶¶¶¶¶
_________¶¶¥¥¥¶¶¶
________¶¶¥¥¥¥¥¶¶¶__________________________________________¶¶¶¶¶¶¶¶
________¶¶¥¥¥¥¥¥¶¶¶_____________________________________¶¶¶¶¶¥¥¥¥¥¶¶
________¶¶¥¥¥¥¥¥ƒƒ¶¶________________________________¶¶¶¶¥¥¥¥¥¥¥¥¶¶¶¶
________¶¶¥¥¥¥ƒƒƒƒƒ¶¶___________________________¶¶¶¶ƒƒ¥¥¥¥¥¥¥¥¶¶¶¶
________¶¶¶ƒƒƒƒƒƒƒƒ§¶¶________________________¶¶ƒƒƒƒƒƒƒ¥¥¥¥¥¶¶¶¶
_________¶¶¶ƒƒƒƒƒƒ§§¶¶____________________¶¶¶¶ƒƒƒƒƒƒƒƒƒƒ¥¥¶¶¶¶
___________¶¶ƒƒƒƒƒ§§¶¶__________________¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒ¶¶¶¶
____________¶¶ƒƒ§§§§¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§¶¶
_____________¶¶§§§§§§§ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§¶¶
______________¶¶§§§ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§¶¶¶¶___________________¶¶¶¶¶¶
____________¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§¶¶¶____________________¶¶¶ƒƒƒƒƒ¶¶
__________¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ¶¶¶¶¶¶ƒƒƒƒ§§§¶¶¶___________________¶¶ƒƒƒƒƒƒƒƒ¶¶
_________¶¶ƒƒ¶¶¶¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒ¶¶__¶¶¶¶ƒƒƒ§§§§§¶¶________________¶¶ƒƒƒƒƒƒƒƒƒƒ¶¶
________¶¶ƒƒ¶¶__¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒ¶¶¶¶¶¶¶¶ƒƒƒ§§§§§§¶¶___________¶¶¶¶ƒƒƒƒƒƒƒƒ§§§§§§¶¶
_______¶¶ƒƒƒ¶¶¶¶¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ¶¶¶¶¶¶ƒƒƒƒƒ§§§§§§¶¶________¶¶ƒƒƒƒƒƒƒƒ§§§§§§§§§§¶¶
_______¶¶ƒƒƒƒ¶¶¶¶ƒƒƒƒƒ¥¥¥ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ####§§§§§¶¶____¶¶¶¶ƒƒƒƒƒƒƒƒ§§§§§§§§§§§§¶¶
_______¶¶###ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ¥¥ƒƒƒƒƒƒ########§§§§¶¶¶¶¶¶ƒƒƒƒƒƒƒƒ§§§§§§§§§§§§§§§§¶¶
_______¶¶####ƒƒƒƒƒƒƒƒ¥¥¥¥¥¥¥¥¥¥¥ƒƒƒƒƒƒ########§§¶¶¶¶ƒƒ¶¶¶¶ƒƒƒƒ§§§§§§§§§§§§§§§§§§¶¶
____¶¶¶¶¶¶###ƒƒƒƒƒƒƒƒƒ¥¥¥#####¥ƒƒƒƒƒƒƒ########¶¶ƒƒ¶¶ƒƒƒƒƒƒ¶¶§§§§§§§§§§§§§§§§§§§§¶¶
__¶¶¶ƒƒ¶¶¶¶#ƒƒƒƒƒƒƒƒƒƒ¥¥####¥¥ƒƒƒƒƒƒƒƒƒƒ####§§¶¶ƒƒƒƒƒƒƒƒ¶¶¶¶§§§§§§§§§§§§§§§§¶¶¶¶
_¶¶ƒƒ¶ƒƒƒƒ¶¶ƒƒƒƒƒƒƒƒƒƒƒƒ¥¥¥¥ƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§¶¶ƒƒƒƒƒƒƒƒƒƒƒƒ¶¶§§§§§§§§§§§§¶¶¶¶
¶¶ƒƒƒƒƒƒ§§§§¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ¶¶ƒƒƒƒƒƒƒƒ§§§§¶¶§§§§§§§§§§¶¶¶¶
__¶¶ƒƒ§§§§§§¶¶¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§¶¶¶§§§§§§§§¶¶
____¶¶§§§§§§§¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§¶__¶§§§§§§¶¶
______¶¶§§§§§§ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§¶¶____¶¶§§§§§§¶¶
________¶¶¶§ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§¶¶_______¶¶§§§§§§¶¶
_________¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§¶¶¶¶____¶¶¶¶§§§§§§§§§§¶¶
_________¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§¶¶§§¶¶¶¶¶¶ƒƒ§§§§§§§§¶¶¶¶
________¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§§¶¶ƒƒƒƒ§§§§§§¶¶¶¶
________¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§§§¶¶§§§§§§§¶¶¶¶
__¶¶¶¶¶¶¶¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§§§§¶¶§§§§§§¶¶
_¶¶ƒƒ¶¶ƒƒƒ¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§§§§¶¶¶¶§§§§§§¶¶
_¶¶ƒƒƒ¶¶ƒƒƒ¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§§§§§¶¶__¶¶¶###§§§¶¶
__¶¶§§§§§§§§¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§§§§§§§§¶¶¶¶¶#######§§§¶¶
___¶¶§§§§§§§§¶¶ƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒƒ§§§§§§§§§§§§§§§§§########¶¶¶¶¶¶
____¶¶§§§§§§§§¶¶§§§§ƒƒƒƒƒƒƒ§§§§§§§§§§§§§§§§§§§####¶¶¶¶¶¶
_____¶¶§§§§§§§¶¶§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§¶¶¶¶
_______¶¶¶¶¶¶¶§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§¶¶
______________¶¶¶¶¶¶¶¶¶¶§§§§§§§§§§§§§§§§§§§§¶¶
________________________¶¶¶¶¶¶§§§§§§§§§§¶¶¶¶
____________________________¶¶¶¶§§§§¶¶¶¶¶
____________________________¶¶§§§§§§§§¶¶
____________________________¶¶§§¶¶§§§¶¶
_____________________________¶¶§¶¶§§¶¶
______________________________¶¶¶¶¶¶
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace LOLViewer
{
    class GLRig
    {
        public const int MAX_BONES = 128;
        public GLJoint[] bindingJoints;

        public GLBone[] currentFrame;
        public GLBone[] nextFrame;
        
        public GLRig()
        {
            bindingJoints = new GLJoint[MAX_BONES];
            currentFrame = new GLBone[MAX_BONES];
            nextFrame = new GLBone[MAX_BONES];

            for (int i = 0; i < MAX_BONES; ++i)
            {
                bindingJoints[i] = new GLJoint();
                currentFrame[i] = new GLBone();
                nextFrame[i] = new GLBone();
            }
        }

        public void Create(List<Quaternion> bOrientation, List<Vector3> bPosition,
            List<float> bScale, List<int> bParent )
        {

            for (int i = 0; i < bOrientation.Count; ++i)
            {
                GLJoint joint = new GLJoint();

                joint.parent = bParent[i];

                joint.worldPosition = bPosition[i];
                joint.scale = bScale[i];
                joint.worldOrientation = bOrientation[i];

                Matrix4 transform = Matrix4.Rotate(joint.worldOrientation);
                transform *= Matrix4.CreateTranslation(joint.worldPosition);
                joint.worldTransform = transform;

                bindingJoints[i] = joint;
            }
        }

        /// <summary>
        /// Calculate the skinning transforms for bones.  All position and orientation data is assumed to be relative to the parent.
        /// </summary>
        /// <param name="frame">0 - current frame, 1 - next frame</param>
        /// <param name="boneID">The bone ID the data belongs to.</param>
        /// <param name="orientation">Rotation of the bone.</param>
        /// <param name="position">Translation of the bone.</param>
        public void CalculateWorldSpacePose(int frame, int boneID, Quaternion orientation, Vector3 position)
        {
            GLBone poseBone = null;

            // Determine which frame this data applies to.
            if (frame == 0)
            {
                poseBone = currentFrame[boneID];
            }
            else
            {
                poseBone = nextFrame[boneID];
            }

            poseBone.parent = bindingJoints[boneID].parent;
            poseBone.scale = bindingJoints[boneID].scale;

            // Is this a root bone?
            if (poseBone.parent == -1)
            {
                // No parent bone for root bones.
                // So, just calculate directly.

                Matrix4 worldTransform = Matrix4.Rotate(orientation);
                worldTransform.M41 = position.X * ( 1.0f / poseBone.scale );
                worldTransform.M42 = position.Y * ( 1.0f / poseBone.scale );
                worldTransform.M43 = position.Z * ( 1.0f / poseBone.scale );

                poseBone.worldTransform = worldTransform;
            }
            else
            {
                // Determine the parent bone.
                GLBone parentBone = null;
                if (frame == 0)
                {
                    parentBone = currentFrame[poseBone.parent];
                }
                else
                {
                    parentBone = nextFrame[poseBone.parent];
                }

                Matrix4 localTransform = Matrix4.Rotate(orientation);
                localTransform.M41 = position.X * ( 1.0f / poseBone.scale );
                localTransform.M42 = position.Y * ( 1.0f / poseBone.scale );
                localTransform.M43 = position.Z * ( 1.0f / poseBone.scale );

                // Create the final transform based off the parent.
                poseBone.worldTransform = localTransform *
                    parentBone.worldTransform;
            }
        }

        /// <summary>
        /// Interpolates the animation data between the current frame
        /// and the next frame.
        /// </summary>
        /// <param name="blend">(Elasped frame time / max frame time)</param>
        /// <returns></returns>
        public Matrix4[] GetBoneTransformations(float blend)
        {
            Matrix4[] transforms = new Matrix4[MAX_BONES];

            for (int i = 0; i < MAX_BONES; ++i)
            {
                // Get inverse of original pose.
                Matrix4 inv = bindingJoints[i].worldTransform;
                inv.Invert();

                /*  The math isn't quite right with this yet.
                //
                // Interpolate between the current frame
                // and the next frame.
                //

                // Decompose matrix data.
                // Get positions.
                Vector3 currentPosition = Vector3.Zero;
                currentPosition.X = currentFrame[i].worldTransform.M41;
                currentPosition.Y = currentFrame[i].worldTransform.M42;
                currentPosition.Z = currentFrame[i].worldTransform.M43;

                Vector3 nextPosition = Vector3.Zero;
                nextPosition.X = nextFrame[i].worldTransform.M41;
                nextPosition.Y = nextFrame[i].worldTransform.M42;
                nextPosition.Z = nextFrame[i].worldTransform.M43;

                // Get orientations.
                Quaternion currentOrientation = OpenTKExtras.Matrix4.CreateQuatFromMatrix(currentFrame[i].worldTransform);
                Quaternion nextOrientation = OpenTKExtras.Matrix4.CreateQuatFromMatrix(nextFrame[i].worldTransform);

                // Interpolate
                Quaternion finalOrientation = Quaternion.Slerp(currentOrientation, nextOrientation, blend);
                Vector3 finalPosition = Vector3.Lerp(currentPosition, nextPosition, blend);

                // Store
                Matrix4 finalTransform = Matrix4.Rotate(finalOrientation);
                finalTransform.M41 = finalPosition.X;
                finalTransform.M42 = finalPosition.Y;
                finalTransform.M43 = finalPosition.Z;

                // Update to form final transform.
                transforms[i] = inv *                                           // invert of default/binding pose
                    Matrix4.Scale(1.0f / bindingJoints[i].scale) *              // invert the bone scale
                    finalTransform;                                             // transform by the current key frame's pose
                */

                // Update to form final transform.
                transforms[i] = inv *                                           // invert of default/binding pose
                    Matrix4.Scale(1.0f / bindingJoints[i].scale) *              // invert the bone scale
                    currentFrame[i].worldTransform;                             // transform by the current key frame's pose
            }
            
            return transforms;
        }
    }
}

/*
______________________________________________________¶¶¶¶¶¶
__________¶¶¶¶____________________________________¶¶¶¶11333¶¶
________¶¶1111¶¶¶¶__________¶¶¶¶¶¶¶¶¶¶________¶¶¶¶111133¶¶33¶¶
________¶¶¶¶111133¶¶¶¶__¶¶¶¶111111111¶¶¶¶¶__¶¶11111133¶¶¶¶§§¶¶
_________¶¶¶¶¶3333§§§§¶¶1111111133333333¶¶¶¶¶1111133¶¶¶¶¶¶§§¶¶
__________¶¶¶¶¶¶§§§§¶¶1111111133333333333333111133¶¶¶¶¶¶¶¶§§¶¶
___________¶¶¶¶¶¶¶¶¶¶1111111333333333333333333333333¶¶¶¶¶¶§§¶¶
____________¶¶¶¶11¶¶11111133333333333¶¶333333333333333¶¶¶¶§§¶¶
___________¶¶¶1111¶¶1111113333¶¶¶¶333¶¶33333333333333333¶¶§§¶¶
__________¶¶11¶¶¶¶¶¶11111133¶¶3333¶¶¶¶33¶¶¶¶¶¶¶¶33333333333¶¶
________¶¶11¶¶¯¯¯¯¶¶11111133¶¶3333¶¶33¶¶¯¯¯¯¯¯¯¯¶¶3333333333¶¶
_______¶¶1¶¶¯ððððððð¶¶111113333333¶¶¶¶ðððððððððð¯¯¶¶33333333¶¶
______¶¶11¶¶ðð¯¯¯¯ðððð¶¶11111133¶¶¶¶ðð¯¯¯¯ðððððððð¯¯¶¶3333333¶¶
______¶¶11¶¶ðð___ððððððð¶¶¶¶¶¶¶¶11¶¶ððð___ðððððððð¯¯¶¶3333333§¶¶
______¶¶11¶¶ðððððððððððð¶¶11111111¶¶ðððððððððððððð¯¯¶¶333333§§¶¶
______¶¶11¶¶ðððððððððððð¶¶11111111¶¶ðððððððððððððð¯¯¶¶333333§§¶¶
__¶¶¶¶¶¶1111¶¶ðððððððð¶¶111111111111¶¶ðððððððððððð¶¶33333333§§¶¶
¶¶1111¶¶111111¶¶¶¶¶¶¶¶1111111111111111¶¶¶¶¶¶¶¶¶¶¶¶3333333333§§¶¶
¶¶11111¶¶11111111111111111¶¶¶¶¶¶111111113333333333333333333§§§¶¶
_¶¶33333331111111111111111¶¶¶¶¶¶11111111333333333333333333§§§§¶¶
___¶¶333¶¶331111111111111111¶¶¶¶11111133333333¶¶33333333§§§§§¶¶
_____¶¶¶¶¶3333111111111111111111111133333333¶¶333333§§§§§§§§§¶¶
________¶¶¶333333311111111111111333333333333¶¶33§§§§¶¶§§§§§§¶¶
__________¶¶333333333333333333333333333333333¶¶¶¶¶¶¶§§§§§§§¶¶
____________¶¶3333333333333333333333333¶¶¶¶¶¶3§§§§§§§§§§§§¶¶
______________¶¶¶¶33333333333333333333¶¶3333¶¶33§§§§§§§§§¶¶
________________¶¶¶¶§§§33333333333333¶¶33§§§§§¶¶§§§§§§¶¶¶
____________________¶¶§§§§33333333333¶¶3§§§§§§¶¶§§¶¶¶¶
______________________¶¶¶¶¶¶§§§§§§§§§¶¶§§§§§§§¶¶¶¶
____________________¶¶¶§§§§§¶¶¶¶¶¶¶¶¶¶¶§§§§§§§¶¶
__________________¶¶3333§§§§§§§§¶¶_____¶¶¶¶¶¶¶
__________________¶¶333333§§¶¶¶¶
____________________¶¶¶¶¶¶¶¶

*/


