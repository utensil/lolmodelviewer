

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

        public GLBone[] currentPose;
        
        public GLRig()
        {
            bindingJoints = new GLJoint[MAX_BONES];
            currentPose = new GLBone[MAX_BONES];

            for (int i = 0; i < MAX_BONES; ++i)
            {
                bindingJoints[i] = new GLJoint();
                currentPose[i] = new GLBone();
            }
        }

        public void Create(List<Quaternion> bOrientation, List<Vector3> bPosition,
            List<float> bScale, List<int> bParent)
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

        public void CalculateWorldSpacePose(int id, Quaternion orientation, Vector3 position)
        {
            GLBone poseBone = currentPose[id];

            poseBone.parent = bindingJoints[id].parent;
            poseBone.scale = bindingJoints[id].scale;


            if (poseBone.parent == -1)
            {
                Matrix4 worldTransform = Matrix4.Rotate(orientation);
                worldTransform.M41 = position.X * ( 1.0f / poseBone.scale );
                worldTransform.M42 = position.Y * ( 1.0f / poseBone.scale );
                worldTransform.M43 = position.Z * ( 1.0f / poseBone.scale );

                poseBone.worldTransform = worldTransform;
            }
            else
            {
                Matrix4 localTransform = Matrix4.Rotate(orientation);
                localTransform.M41 = position.X * ( 1.0f / poseBone.scale );
                localTransform.M42 = position.Y * ( 1.0f / poseBone.scale );
                localTransform.M43 = position.Z * ( 1.0f / poseBone.scale );

                poseBone.worldTransform = localTransform *
                    currentPose[poseBone.parent].worldTransform;
            }
        }

        public Matrix4[] GetBoneTransformations()
        {
            Matrix4[] transforms = new Matrix4[MAX_BONES];

            for (int i = 0; i < MAX_BONES; ++i)
            {
                Matrix4 inv = bindingJoints[i].worldTransform;
                inv.Invert();

                transforms[i] = inv *                                     // invert of default/binding pose
                    Matrix4.Scale(1.0f / bindingJoints[i].scale) *        // invert the bone scale
                    currentPose[i].worldTransform;                        // transform by the current key frame's pose
            }

            return transforms;
        }
    }
}
