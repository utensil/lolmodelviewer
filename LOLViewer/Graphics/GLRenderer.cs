

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
// High Level Abstraction for an OpenGL Renderer
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using LOLViewer.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using LOLFileReader;

using CSharpLogger;

namespace LOLViewer.Graphics
{
    public class GLRenderer
    {
        // Renderer Variables
        public OpenTK.Graphics.Color4 ClearColor
        {
            get
            {
                return clearColor;
            }

            set
            {
                clearColor = value;
                GL.ClearColor(clearColor);
            }
        }
        private OpenTK.Graphics.Color4 clearColor = new OpenTK.Graphics.Color4(0.1f, 0.2f, 0.5f, 0);

        // Skinning Identities. Used when animation mode is disabled.
        public bool IsSkinning 
        {
            get
            {
                return isSkinning;
            }

            set
            {
                isSkinning = value;

                // If we have turned off the skinning pipeline, we need set all the transforms to the identity.
                if (isSkinning == false)
                {
                    for (int i = 0; i < boneTransforms.Length; ++i)
                    {
                        boneTransforms[i] = Matrix4.Identity;
                    }
                }
            }
        }
        private bool isSkinning;

        // Shader Variables
        private Dictionary<String, GLShaderProgram> programs;
        private Dictionary<String, GLShader> shaders;

        // Geometry Variables
        private GLBillboard billboard;
        private GLStaticModel staticModel;

        private GLRiggedModel riggedModel;
        private Matrix4[] boneTransforms;
        private const int MAX_BONE_TRANSFORMS = 128;

        // Texture Variables
        private Dictionary<String, GLTexture> textures;

        private Matrix4 world;
        private Vector3 modelTranslation;
        private Vector3 mouseTranslationOrigin;
        private Vector3 mouseTranslation;

        // Can't actually declare as a const.
        private Vector3 DEFAULT_MODEL_TRANSLATION = new Vector3(0, -50, 0);

        #region Initialization

        public GLRenderer()
        {
            programs = new Dictionary<String, GLShaderProgram>();
            shaders = new Dictionary<String, GLShader>();

            billboard = new GLBillboard();
            staticModel = new GLStaticModel();

            riggedModel = new GLRiggedModel();
            boneTransforms = new Matrix4[MAX_BONE_TRANSFORMS];

            textures = new Dictionary<String, GLTexture>();

            isSkinning = false;

            Reset();
        }

        /// <summary>
        /// Sets up OpenGL.  Calls this before the other functions.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public bool Initialize(Logger logger)
        {
            bool result = true;

            // Create vertex shaders.
            if (result == true)
            {
                // Unused atm.
                result = CreateShaderFromMemory("transform2D_tex.vert", GLShaderDefinitions.TransformTexturedVertex, ShaderType.VertexShader, logger);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("phong.vert", GLShaderDefinitions.PhongVertex, ShaderType.VertexShader, logger);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("phongRigged.vert", GLShaderDefinitions.PhongRiggedVertex,
                    ShaderType.VertexShader, logger);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("cellRigged.vert", GLShaderDefinitions.CellShadedRiggedVertex,
                    ShaderType.VertexShader, logger);
            }

            // Create fragment shaders.
            if (result == true)
            {
                // Unused atm.
                result = CreateShaderFromMemory("texSampler.frag",
                    GLShaderDefinitions.TextureSamplerFragment, ShaderType.FragmentShader, logger);
            }

            if (result == true)
            {
                // Unused atm.
                result = CreateShaderFromMemory("texSamplerGreyscale.frag",
                    GLShaderDefinitions.TextureSamplerGreyscaleFragment, ShaderType.FragmentShader, logger);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("phong.frag", GLShaderDefinitions.PhongFragment, ShaderType.FragmentShader, logger);
            }

            if (result == true)
            {
                // Unused atm.
                result = CreateShaderFromMemory("phongTexOnly.frag",
                    GLShaderDefinitions.PhongTexOnlyFragment, ShaderType.FragmentShader, logger);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("cell.frag", GLShaderDefinitions.CellShadedFragment,
                    ShaderType.FragmentShader, logger);
            }

            //
            // Create shader programs.
            //

            // Billboarding program
            if (result == true)
            {
                List<String> attributes = new List<String>();
                attributes.Add("in_Position");
                attributes.Add("in_TexCoords");

                List<String> uniforms = new List<String>();
                uniforms.Add("u_WorldViewProjection");
                uniforms.Add("u_Texture");

                // Unused atm.
                result = CreateProgram("default", "transform2D_tex.vert", "texSampler.frag",
                    attributes, uniforms, logger);
            }

            // Greyscale billboarding program
            if (result == true)
            {
                List<String> attributes = new List<String>();
                attributes.Add("in_Position");
                attributes.Add("in_TexCoords");

                List<String> uniforms = new List<String>();
                uniforms.Add("u_WorldViewProjection");
                uniforms.Add("u_Texture");

                // Unused atm.
                result = CreateProgram("greyscale", "transform2D_tex.vert", "texSamplerGreyscale.frag",
                    attributes, uniforms, logger);
            }

            // Phong lighting program
            if (result == true)
            {
                List<String> attributes = new List<String>();
                attributes.Add("in_Position");
                attributes.Add("in_Normal");
                attributes.Add("in_TexCoords");

                List<String> uniforms = new List<String>();
                uniforms.Add("u_WorldView");
                uniforms.Add("u_WorldViewProjection");
                uniforms.Add("u_LightDirection");
                uniforms.Add("u_LightDiffuse");
                uniforms.Add("u_KA");
                uniforms.Add("u_KD");
                uniforms.Add("u_KS");
                uniforms.Add("u_SExponent");
                uniforms.Add("u_Texture");

                result = CreateProgram("phong", "phong.vert", "phong.frag",
                    attributes, uniforms, logger);
            }

            // Model w/ Texture Only
            if (result == true)
            {
                List<String> attributes = new List<String>();
                attributes.Add("in_Position");
                attributes.Add("in_TexCoords");

                List<String> uniforms = new List<String>();
                uniforms.Add("u_WorldViewProjection");
                uniforms.Add("u_Texture");

                // Unused atm.
                result = CreateProgram("phongTexOnly", "phong.vert", "phongTexOnly.frag",
                    attributes, uniforms, logger);
            }

            // Phong Lighting with Skeletal Animation
            if (result == true)
            {
                List<String> attributes = new List<String>();
                attributes.Add("in_Position");
                attributes.Add("in_Normal");
                attributes.Add("in_TexCoords");
                attributes.Add("in_BoneID");
                attributes.Add("in_Weights");

                List<String> uniforms = new List<String>();
                uniforms.Add("u_WorldView");
                uniforms.Add("u_WorldViewProjection");
                uniforms.Add("u_LightDirection");
                uniforms.Add("u_BoneTransform");
                uniforms.Add("u_LightDiffuse");
                uniforms.Add("u_KA");
                uniforms.Add("u_KD");
                uniforms.Add("u_KS");
                uniforms.Add("u_SExponent");
                uniforms.Add("u_Texture");

                result = CreateProgram("phongRigged", "phongRigged.vert", "phong.frag",
                    attributes, uniforms, logger);
            }

            // Cell Shading with Skeletal Animation
            if (result == true)
            {
                List<String> attributes = new List<String>();
                attributes.Add("in_Position");
                attributes.Add("in_Normal");
                attributes.Add("in_TexCoords");
                attributes.Add("in_BoneID");
                attributes.Add("in_Weights");

                List<String> uniforms = new List<String>();
                uniforms.Add("u_WorldView");
                uniforms.Add("u_WorldViewProjection");
                uniforms.Add("u_LightDirection");
                uniforms.Add("u_BoneTransform");
                uniforms.Add("u_Texture");
                uniforms.Add("u_QOne");
                uniforms.Add("u_QTwo");
                uniforms.Add("u_QThree");
                uniforms.Add("u_ValueOne");
                uniforms.Add("u_ValueTwo");
                uniforms.Add("u_ValueThree");
                uniforms.Add("u_ValueFour");

                result = CreateProgram("cellRigged", "cellRigged.vert", "cell.frag",
                    attributes, uniforms, logger);
            }

            // Create Geometry
            // Old Debugging Code.
            // I kept it around incase it would be cool to draw a ground
            // plane or something.

            if (result == true)
            {
                List<float> verts = new List<float>();
                // Bottom Left
                verts.Add(-5.0f);
                verts.Add(-5.0f);
                verts.Add(0.0f);

                // Top Left
                verts.Add(-5.0f);
                verts.Add(5.0f);
                verts.Add(0.0f);

                // Top Right
                verts.Add(5.0f);
                verts.Add(5.0f);
                verts.Add(0.0f);

                // Bottom Right
                verts.Add(5.0f);
                verts.Add(-5.0f);
                verts.Add(0.0f);

                List<float> texs = new List<float>();
                // Bottom Left
                texs.Add(0.0f);
                texs.Add(1.0f);

                // Top Left
                texs.Add(0.0f);
                texs.Add(0.0f);

                // Top Right
                texs.Add(1.0f);
                texs.Add(0.0f);

                // Bottom Right
                texs.Add(1.0f);
                texs.Add(1.0f);

                List<uint> inds = new List<uint>();
                // Tri 1
                inds.Add(0);
                inds.Add(1);
                inds.Add(2);

                // Tri 2
                inds.Add(0);
                inds.Add(2);
                inds.Add(3);

                result = CreateBillboard("default", verts, texs, inds, logger);
            }

            // Misc. OpenGL Parameters.
            if (result == true)
            {
                GL.FrontFace(FrontFaceDirection.Cw);
                GL.CullFace(CullFaceMode.Back);
                GL.Enable(EnableCap.CullFace);
                ClearColor = clearColor;
            }

            return result;
        }

        #endregion

        #region Window Input Functions

        public void MouseMove(System.Windows.Forms.MouseEventArgs e, GLCamera camera)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Update the mouse location.
                mouseTranslation = Intersect(e.X, e.Y, camera) - mouseTranslationOrigin;
            }
        }

        public void MouseButtonUp(System.Windows.Forms.MouseEventArgs e, GLCamera camera)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Get the current location of the mouse.
                mouseTranslation = Intersect(e.X, e.Y, camera) - mouseTranslationOrigin;

                // Compute the new model location.
                modelTranslation += mouseTranslation;

                // Clear the mouse origin.
                mouseTranslationOrigin = Vector3.Zero;
                mouseTranslation = Vector3.Zero;
            }
        }

        public void MouseButtonDown(System.Windows.Forms.MouseEventArgs e, GLCamera camera)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // Store the origin of the mouse.
                mouseTranslationOrigin = Intersect(e.X, e.Y, camera);
                mouseTranslation = Vector3.Zero;
            }
        }

        #endregion

        #region Renderer API

        /// <summary>
        /// Draws the scene.
        /// </summary>
        /// <param name="camera">The camera for the scene.</param>
        public void Render(GLCamera camera)
        {
            // Clear back buffers.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // Enable a normal depth test without stenciling.
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.StencilTest);
            GL.DepthFunc(DepthFunction.Less);

            GLShaderProgram program = null;

            // Add the translation in view space.  This allows the model to rotate around
            // itself instead of the origin in world space.
            Matrix4 view = camera.View;
            view.M41 += modelTranslation.X + mouseTranslation.X;
            view.M42 += modelTranslation.Y + mouseTranslation.Y;
            view.M43 += modelTranslation.Z + mouseTranslation.Z;

            //
            // Load shaders for Phong lit static models.
            //

            program = programs["phong"];
            program.Load();

            //
            // Update parameters for phong lighting.
            //                

            // Vertex Shader Uniforms
            program.UpdateUniform("u_WorldView",
                world * view);
            program.UpdateUniform("u_WorldViewProjection",
                world * view * camera.Projection);

            // Fragment Shader Uniforms
            program.UpdateUniform("u_LightDirection", new Vector3(0.0f, 0.0f, 1.0f));
            program.UpdateUniform("u_LightDiffuse",
                new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            program.UpdateUniform("u_KA", 0.85f);
            program.UpdateUniform("u_KD", 0.1f);
            program.UpdateUniform("u_KS", 0.05f);
            program.UpdateUniform("u_SExponent", 8.0f);

            // Draw Static Model

            // Load the model's texture for the shader.
            GL.ActiveTexture(TextureUnit.Texture0);
            program.UpdateUniform("u_Texture", 0);
            if (staticModel.TextureName != String.Empty)
            {
                textures[staticModel.TextureName].Bind(); // not checking return value        
            }

            staticModel.Draw();

            GL.UseProgram(0);

            //
            // Load shaders for Phong lit rigged models.
            //

            program = programs["phongRigged"];
            program.Load();

            //
            // Update parameters for phong lighting.
            //

            // Fragment Shader Uniforms
            program.UpdateUniform("u_LightDirection", new Vector3(0.0f, 0.0f, 1.0f));
            program.UpdateUniform("u_LightDiffuse",
                new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            program.UpdateUniform("u_KA", 0.85f);
            program.UpdateUniform("u_KD", 0.1f);
            program.UpdateUniform("u_KS", 0.05f);
            program.UpdateUniform("u_SExponent", 8.0f);

            // Draw Rigged Model

            //
            // Update the uniforms.
            //

            // Textures vary.
            GL.ActiveTexture(TextureUnit.Texture0);
            program.UpdateUniform("u_Texture", 0);
            if (riggedModel.TextureName != String.Empty)
            {
                textures[riggedModel.TextureName].Bind(); // not checking return value        
            }

            //
            // Bone Transforms
            //

            if (IsSkinning == true)
            {
                // Get the transformations from the rig.
                boneTransforms = riggedModel.GetBoneTransformations(ref boneTransforms);
            }

            program.UpdateUniform("u_BoneTransform", boneTransforms);

            //
            // World Transform.
            //
            Matrix4 worldView = world * view;

            // Vertex Shader Uniforms
            program.UpdateUniform("u_WorldView",
                worldView);
            program.UpdateUniform("u_WorldViewProjection",
                worldView * camera.Projection);

            riggedModel.Draw();

            // Unload shaders.
            GL.UseProgram(0);
        }

        /// <summary>
        /// Updates the animation.
        /// </summary>
        /// <param name="elapsedTime">The time since the last frame.</param>
        public void Update(float elapsedTime)
        {
            riggedModel.Update(elapsedTime);
        }

        /// <summary>
        /// Resizes the viewport.  Call this when the control resizes.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width">The width of the control.</param>
        /// <param name="height">The height of the control.</param>
        public void Resize(int x, int y, int width, int height)
        {
            GL.Viewport(x, y, width, height);
        }

        /// <summary>
        /// Resets the location of the model to its default position.
        /// </summary>
        public void Reset()
        {
            world = Matrix4.Identity;

            world.M41 = DEFAULT_MODEL_TRANSLATION.X;
            world.M42 = DEFAULT_MODEL_TRANSLATION.Y;
            world.M43 = DEFAULT_MODEL_TRANSLATION.Z;

            modelTranslation = Vector3.Zero;
            mouseTranslationOrigin = Vector3.Zero;
            mouseTranslation = Vector3.Zero;
        }

        /// <summary>
        /// Call before finalizing the memory of the renderer.
        /// </summary>
        public void Shutdown()
        {
            // Clean up shaders
            GL.UseProgram(0);

            foreach (var p in programs)
            {
                p.Value.Destroy();
            }
            programs.Clear();

            foreach (var s in shaders)
            {
                s.Value.Destroy();
            }
            shaders.Clear();

            // Clean up renderable objects
            billboard.Destory();
            staticModel.Destory();
            riggedModel.Destroy();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            foreach (var t in textures)
            {
                t.Value.Destroy();
            }
            textures.Clear();
        }

        #endregion

        #region Animation API

        /// <summary>
        /// Sets the animation to be displayed.
        /// </summary>
        /// <param name="animation">The animation name.</param>
        public void SetCurrentAnimation(String animation)
        {
            riggedModel.SetCurrentAnimation(animation);
        }

        /// <summary>
        /// Returns a list of animation names.
        /// </summary>
        /// <returns></returns>
        public List<String> GetAnimations()
        {
            return riggedModel.AnimationNames;
        }

        /// <summary>
        /// Sets the current frame in the animation.
        /// </summary>
        /// <param name="frame">The frame number.</param>
        /// <param name="percentTowardsNextFrame">The percent til the next frame is shown. Expects a value between 0 - 1.</param>
        public void SetCurrentFrameInCurrentAnimation(int frame, float percentTowardsNextFrame)
        {
            riggedModel.SetCurrentFrame(frame, percentTowardsNextFrame);
        }

        /// <summary>
        /// Gets the total number of frames in the animation.
        /// </summary>
        /// <returns></returns>
        public uint GetNumberOfFramesInCurrentAnimation()
        {
            uint result = 0;

            result = riggedModel.GetNumberOfFramesInCurrentAnimation();

            return result;
        }

        /// <summary>
        /// Gets the percentage of the animation completed.
        /// </summary>
        /// <returns>Current Frame / Total Number of Frames</returns>
        public float GetPercentAnimated()
        {
            float result = 0.0f;

            result = riggedModel.GetPercentAnimated();

            return result;
        }

        #endregion

        #region Model API

        /// <summary>
        /// Loads a model into the renderer.
        /// </summary>
        /// <param name="model">The model to load.</param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public bool LoadModel(LOLModel model, Logger logger)
        {
            bool result = true;

            DestroyModel();

            //
            // Create model geometry
            //

            // If the model definition does not contain .skl files,
            // then it's a static model.
            if (model.skl == null &&
                result == true)
            {
                result = CreateStaticModel(model, logger);
            }
            // It's a rigged model
            else
            {
                result = CreateRiggedModel(model, logger);
            }

            return result;
        }

        /// <summary>
        /// Removes the model from the renderer.
        /// </summary>
        public void DestroyModel()
        {
            // Clear out old model data.
            staticModel.Destory();
            riggedModel.Destroy();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            foreach (var t in textures)
            {
                t.Value.Destroy();
            }
            textures.Clear();
            // End Clearing
        }

        /// <summary>
        /// Set the scale of the model.
        /// </summary>
        /// <param name="scale">The scale of the model between 0 and 1.</param>
        public void ScaleModel(float scale)
        {
            // Store old translation.
            Vector3 translation = Vector3.Zero;
            translation.X = world.M41;
            translation.Y = world.M42;
            translation.Z = world.M43;

            // Create the scale.
            world = Matrix4.Scale(scale);

            // Reapply old translation.
            world.M41 = translation.X;
            world.M42 = translation.Y;
            world.M43 = translation.Z;
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Computes the intersection point between the mouse coordinate vector and the plane
        /// formed by the model's location and the camera's eye.
        /// </summary>
        /// <param name="x">The X mouse coordinate.</param>
        /// <param name="y">The Y mouse coordinate.</param>
        /// <param name="camera">The camera.</param>
        private Vector3 Intersect(int x, int y, GLCamera camera)
        {
            // Invert the transformation pipeline.
            Matrix4 inverseProjectionView = camera.View * camera.Projection;
            inverseProjectionView.Invert();

            // Transform mouse coordinates into world space.

            // We are "mouse" space.  Need to convert into screen space.
            Vector4 worldMouse = new Vector4(x, y, 1, 1);

            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            worldMouse.X = (2.0f * (worldMouse.X - viewport[0]) / viewport[2]) - 1.0f;
            worldMouse.Y = -((2.0f * (worldMouse.Y - viewport[1]) / viewport[3]) - 1.0f);

            // We are in screen space.  Need to convert into view space and then world space.
            worldMouse = Vector4.Transform(worldMouse, inverseProjectionView);

            if (worldMouse.W > float.Epsilon || worldMouse.W < float.Epsilon)
            {
                worldMouse.X /= worldMouse.W;
                worldMouse.Y /= worldMouse.W;
                worldMouse.Z /= worldMouse.W;
            }

            // The model translation is applied in VIEW space.  So, we need to project it back
            // into world space for these calculations.  We need to remove the translation components of the view
            // matrix.

            Matrix4 viewRotation = camera.View;
            viewRotation.M41 = 0;
            viewRotation.M42 = 0;
            viewRotation.M43 = 0;

            Matrix4 inverseViewRotation = Matrix4.Invert(viewRotation);

            Vector3 worldModelTranslation = Vector3.Transform(modelTranslation, inverseViewRotation);

            // Get the world location of the model.  This is the point on the plane.
            Vector3 planePoint = worldModelTranslation;

            // Get the camera eye.  This is the line origin.
            Vector3 lineOrigin = camera.Eye;

            // Create the normal of the plane.
            Vector3 planeNormal = lineOrigin - planePoint;
            planeNormal.Normalize();

            // Create the direction of the line intersecting the plane.
            Vector3 lineDirection = new Vector3(worldMouse);
            lineDirection -= lineOrigin;
            lineDirection.Normalize();

            // Computes the distance along the line until it intersects the plane.
            // Note: In pure math, there are three possible solutions.
            //      1.) The line intersects the plane once.  Normal solution.
            //      2.) The line is outside and parallel to the plane. Denom -> 0 -> Undefined solution.
            //      3.) The line is contained inside the plane. Denom & Num -> 0 -> Indeterminate solution.
            // However, in the scope of this problem, only solution 1.) is possible because of the constraints of the camera class.
            // So, we assume nothing crazy can happen with this computation (which is probably a terrible assumption but oh well).
            float distance = Vector3.Dot(planePoint - lineOrigin, planeNormal) / Vector3.Dot(lineDirection, planeNormal);

            // Calculate the new model location.
            Vector3 result = lineOrigin + lineDirection * distance;

            // Convert back into view space.
            result = Vector3.Transform(result, viewRotation);

            return result;
        }

        private bool CreateShaderFromMemory(String name, String data, ShaderType type, Logger logger)
        {
            bool result = true;

            logger.Event("Creating shader: " + name);

            GLShader shader = new GLShader(type);
            result = shader.LoadFromMemory(data);

            if (result == true)
            {
                result = shader.Compile();
            }

            if (result == true)
            {
                shaders.Add(name, shader);
            }
            else
            {
                logger.Error("Failed to create shader: " + name);
                // We need to clean up this shader since it failed.
                shader.Destroy();
            }

            return result;
        }

        private bool CreateProgram(String progName, String vertName, String fragName,
            List<String> attributes, List<String> uniforms, Logger logger)
        {
            bool result = true;

            logger.Event("Creating shader program: " + progName);

            GLShaderProgram program = new GLShaderProgram();
            if (result == true)
            {
                result = program.Create();
            }

            if (result == true)
            {
                result = program.AttachShader(shaders[vertName].shader);
            }
            else
            {
                // We need to clean up this program since it failed.
                program.Destroy();
            }

            if (result == true)
            {
                result = program.AttachShader(shaders[fragName].shader);
            }
            else
            {
                program.Destroy();
            }

            int count = 0;
            foreach (String s in attributes)
            {
                if (result == true)
                {
                    result = program.BindAttribute(count, s);
                }
                else
                {
                    program.Destroy();
                    break;
                }

                count++;
            }

            // Link the program.
            if (result == true)
            {
                result = program.Link();
            }
            else
            {
                program.Destroy();
            }

            // Make sure the attributes were bound correctly.
            if (result == true)
            {
                result = program.VerifyAttributes(attributes);
            }
            else
            {
                program.Destroy();
            }

            foreach (String s in uniforms)
            {
                if (result == true)
                {
                    result = program.SetUniformLocation(s);
                }
                else
                {
                    program.Destroy();
                    break;
                }
            }

            // Store the shader.
            if (result == true)
            {
                programs.Add(progName, program);
            }
            else
            {
                logger.Error("Failed to create shader program: " + progName);
                program.Destroy();
            }

            return result;
        }

        private bool CreateBillboard(String name, List<float> vertexData,
            List<float> texData, List<uint> indexData, Logger logger)
        {
            bool result = true;

            logger.Event("Creating billboard: " + name);

            billboard = new GLBillboard();
            result = billboard.Create(vertexData, texData, indexData);

            if (result == false)
            {
                logger.Error("Failed to create billboard: " + name);
                billboard.Destory();
            }

            return result;
        }

        private bool CreateStaticModel(LOLModel model, Logger logger)
        {
            bool result = true;

            logger.Event("Creating static model.");

            SKNFile file = new SKNFile();
            if (result == true)
            {
                // Model is stored in a RAF.
                result = SKNReader.Read(model.skn, ref file, logger);
            }

            staticModel = new GLStaticModel();
            if (result == true)
            {
                result = staticModel.Create(file, logger);
            }

            //
            // Create Model Texture.
            //

            if (result == true)
            {
                // Texture stored in RAF file.
                result = CreateTexture(model.texture, TextureTarget.Texture2D,
                        GLTexture.SupportedImageEncodings.DDS, logger);

                // Store it in our new model file.
                if (result == true)
                {
                    String name = model.texture.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);

                    staticModel.TextureName = name;
                }
            }

            if (result == false)
            {
                logger.Error("Failed to create static model.");
            }

            return result;
        }

        private bool CreateRiggedModel(LOLModel model, Logger logger)
        {
            bool result = true;

            logger.Event("Creating rigged model.");

            // Open the skn file.
            SKNFile sknFile = new SKNFile();
            if (result == true)
            {
                result = SKNReader.Read(model.skn, ref sknFile, logger);
            }

            // Open the skl file.
            SKLFile sklFile = new SKLFile();
            if (result == true)
            {
                result = SKLReader.Read(model.skl, ref sklFile, logger);
            }

            // Open the anm files.
            Dictionary<String, ANMFile> anmFiles = new Dictionary<String, ANMFile>();
            if (result == true)
            {
                foreach (var a in model.animations)
                {
                    ANMFile anmFile = new ANMFile();
                    bool anmResult = ANMReader.Read(a.Value, ref anmFile, logger);
                    if (anmResult == true)
                    {
                        anmFiles.Add(a.Key, anmFile);
                    }
                }
            }

            // Create the model.
            riggedModel = new GLRiggedModel();
            if (result == true)
            {
                result = riggedModel.Create(sknFile, sklFile, anmFiles, logger);
            }

            // Set up an initial animation.
            if (result == true)
            {
                if (anmFiles.Count > 0)
                {
                    riggedModel.SetCurrentAnimation(anmFiles.First().Key);
                    riggedModel.SetCurrentFrame(0, 0);
                }
            }

            //
            // Create Model Texture.
            //
            if (result == true)
            {
                // Texture stored in RAF file.
                result = CreateTexture(model.texture, TextureTarget.Texture2D,
                        GLTexture.SupportedImageEncodings.DDS, logger);

                // Store it in our new model file.
                if (result == true)
                {
                    String name = model.texture.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);

                    riggedModel.TextureName = name;
                }
            }

            if (result == false)
            {
                logger.Error("Failed to create rigged model.");
            }

            return result;
        }

        private bool CreateTexture(IFileEntry f, TextureTarget target,
            GLTexture.SupportedImageEncodings encoding, Logger logger)
        {
            bool result = true;

            logger.Event("Creating texture: " + f.FileName);

            GLTexture texture = new GLTexture();
            result = texture.Create(f, target, encoding, logger);

            // Store new texture.
            if (result == true)
            {
                String name = f.FileName;
                int pos = name.LastIndexOf("/");
                name = name.Substring(pos + 1);

                textures.Add(name, texture);
            }
            else
            {
                logger.Error("Failed to create texture: " + f.FileName);
                texture.Destroy();
            }

            return result;
        }

        #endregion
    }
}
