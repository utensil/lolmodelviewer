

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

namespace LOLViewer
{
    public class GLRenderer
    {
        // Renderer Variables
        public OpenTK.Graphics.Color4 clearColor;

        // Shader Variables
        private Dictionary<String, GLShaderProgram> programs;
        private Dictionary<String, GLShader> shaders;

        // Geometry Variables
        private Dictionary<String, GLBillboard> billboards;
        private Dictionary<String, GLStaticModel> sModels;
        private Dictionary<String, GLRiggedModel> rModels;

        // Texture Variables
        private Dictionary<String, GLTexture> textures;

        public Matrix4 world;
        public const float DEFAULT_MODEL_SCALE = 0.11f;
        public const int DEFAULT_MODEL_YOFFSET = -50;

        // Skinning Identities. Used when animation mode is disabled.
        public bool isSkinning;
        private Matrix4[] identityTM = new Matrix4[GLRig.MAX_BONES];

        public GLRenderer()
        {
            clearColor = new OpenTK.Graphics.Color4(0.1f, 0.2f, 0.5f, 0.0f);

            programs = new Dictionary<String, GLShaderProgram>();
            shaders = new Dictionary<String, GLShader>();
            billboards = new Dictionary<String, GLBillboard>();
            sModels = new Dictionary<String, GLStaticModel>();
            rModels = new Dictionary<String, GLRiggedModel>();
            textures = new Dictionary<String, GLTexture>();

            isSkinning = false;
            world = Matrix4.Scale(DEFAULT_MODEL_SCALE);
            world.M42 = DEFAULT_MODEL_YOFFSET;

            for (int i = 0; i < GLRig.MAX_BONES; ++i)
            {
                identityTM[i] = Matrix4.Identity;
            }
        }

        public bool OnLoad()
        {
            bool result = true;

            // Create vertex shaders.
            if (result == true)
            {
                // Unused atm.
                result = CreateShaderFromMemory("transform2D_tex.vert", GLShaderDefinitions.TransformTexturedVertex, ShaderType.VertexShader);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("phong.vert", GLShaderDefinitions.PhongVertex, ShaderType.VertexShader);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("phongRigged.vert", GLShaderDefinitions.PhongRiggedVertex, 
                    ShaderType.VertexShader);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("cellRigged.vert", GLShaderDefinitions.CellShadedRiggedVertex,
                    ShaderType.VertexShader);
            }

            // Create fragment shaders.
            if (result == true)
            {
                // Unused atm.
                result = CreateShaderFromMemory("texSampler.frag", 
                    GLShaderDefinitions.TextureSamplerFragment, ShaderType.FragmentShader);
            }

            if (result == true)
            {
                // Unused atm.
                result = CreateShaderFromMemory("texSamplerGreyscale.frag", 
                    GLShaderDefinitions.TextureSamplerGreyscaleFragment, ShaderType.FragmentShader);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("phong.frag", GLShaderDefinitions.PhongFragment, ShaderType.FragmentShader);
            }

            if (result == true)
            {
                // Unused atm.
                result = CreateShaderFromMemory("phongTexOnly.frag", 
                    GLShaderDefinitions.PhongTexOnlyFragment, ShaderType.FragmentShader);
            }

            if (result == true)
            {
                result = CreateShaderFromMemory("cell.frag", GLShaderDefinitions.CellShadedFragment,
                    ShaderType.FragmentShader);
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
                    attributes, uniforms);
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
                    attributes, uniforms);
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
                    attributes, uniforms);
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
                    attributes, uniforms);
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
                    attributes, uniforms);
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
                    attributes, uniforms);
            }

            // Create Geometry
            // Old Debugging Code.
            // I kept it around incase it would be cool to draw a ground
            // plane or something.
            
            if (result == true)
            {
                List<float> verts = new List<float>();
                // Bottom Left
                verts.Add( -5.0f );
                verts.Add( -5.0f );
                verts.Add( 0.0f );

                // Top Left
                verts.Add( -5.0f );
                verts.Add( 5.0f );
                verts.Add( 0.0f );

                // Top Right
                verts.Add( 5.0f );
                verts.Add( 5.0f );
                verts.Add( 0.0f );

                // Bottom Right
                verts.Add( 5.0f );
                verts.Add( -5.0f );
                verts.Add( 0.0f );

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
                inds.Add( 0 );
                inds.Add( 1 );
                inds.Add( 2 );

                // Tri 2
                inds.Add( 0 );
                inds.Add( 2 );
                inds.Add( 3 );

                result = CreateBillboard("default", verts, texs, inds);
            }         

            // Misc. OpenGL Parameters.
            if (result == true)
            {
                GL.FrontFace(FrontFaceDirection.Cw);
                GL.CullFace(CullFaceMode.Back);
                GL.Enable(EnableCap.CullFace);
                SetClearColor(clearColor);
            }

            return result;
        }

        public void SetClearColor(OpenTK.Graphics.Color4 color)
        {
            clearColor = color;
            GL.ClearColor(clearColor);
        }

        public void SetClearColor(System.Drawing.Color color)
        {
            clearColor = color;
            GL.ClearColor(clearColor);
        }

        public bool LoadModel( LOLModel model)
        {
            bool result = true;

            DestroyCurrentModels();

            //
            // Create model geometry
            //

            // If the model definition does not contain .skl files,
            // then it's a static model.
            if (model.skl == null &&
                result == true)
            {
                result = CreateStaticModel(model);
            }
            // It's a rigged model
            else
            {
                result = CreateRiggedModel(model);
            }

            return result;
        }

        public void OnResize(int x, int y, int width, int height)
        {
            GL.Viewport(x, y, width, height);
        }

        public void OnRender(ref GLCamera camera)
        {
            //
            // TODO: Refactor/clean up this render loop.
            // It supports multiple static and dynamic models at the moment.
            // However, none of that is ever used.  Only one dynamic is ever loaded at the moment.
            // There's just alot of clutter in this function which is really not needed.
            //

            // Clear back buffers.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            
            // Enable a normal depth test without stenciling.
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.StencilTest);
            GL.DepthFunc(DepthFunction.Less);

            GLShaderProgram program = null;
            
            //
            // Load shaders for Phong lit static models.
            //

            if (sModels.Count > 0)
            {
                program = programs["phong"];
                program.Load();

                //
                // Update parameters for phong lighting.
                //

                // Vertex Shader Uniforms
                program.UpdateUniform("u_WorldView",
                    world * camera.view);
                program.UpdateUniform("u_WorldViewProjection",
                    world * camera.view * camera.projection);

                // Fragment Shader Uniforms
                program.UpdateUniform("u_LightDirection", new Vector3(0.0f, 0.0f, 1.0f));
                program.UpdateUniform("u_LightDiffuse",
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                program.UpdateUniform("u_KA", 0.85f);
                program.UpdateUniform("u_KD", 0.1f);
                program.UpdateUniform("u_KS", 0.05f);
                program.UpdateUniform("u_SExponent", 8.0f);
            }

            // Draw Model
            foreach (var s in sModels)
            {
                // Load the model's texture for the shader.
                GL.ActiveTexture(TextureUnit.Texture0);
                program.UpdateUniform("u_Texture", 0);
                if (s.Value.textureName != String.Empty)
                {
                    textures[s.Value.textureName].Bind(); // not checking return value        
                }
       
                s.Value.Draw();
            }

            GL.UseProgram(0);

            //
            // Load shaders for Phong lit rigged models.
            //

            if (rModels.Count > 0)
            {
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
            }

            // Cell Shading Test
            //if (rModels.Count > 0)
            //{
            //    program = programs["cellRigged"];
            //    program.Load();

            //    //
            //    // Update parameters for cell shading.
            //    //

            //    // Fragment Shader Uniforms
            //    program.UpdateUniform("u_LightDirection", new Vector3(0.0f, 0.0f, 1.0f));
            //    program.UpdateUniform("u_QOne", 0.9f);
            //    program.UpdateUniform("u_QTwo", 0.5f);
            //    program.UpdateUniform("u_QThree", 0.3f);
            //    program.UpdateUniform("u_ValueOne", 1.1f);
            //    program.UpdateUniform("u_ValueTwo", 1.0f);
            //    program.UpdateUniform("u_ValueThree", 0.9f);
            //    program.UpdateUniform("u_ValueFour", 0.8f);
            //}

            // Draw Model
            foreach (var r in rModels)
            {
                //
                // Update the uniforms which vary per model.
                //


                // Textures vary.
                GL.ActiveTexture(TextureUnit.Texture0);
                program.UpdateUniform("u_Texture", 0);
                if (r.Value.textureName != String.Empty)
                {
                    textures[r.Value.textureName].Bind(); // not checking return value        
                }

                //
                // Bone Transforms
                //

                
                if (isSkinning == true)
                {
                    Matrix4[] transforms = r.Value.GetBoneTransformations();
                    // Sanity for when the currentAnimation is invalid.
                    if (transforms != null)
                    {
                        //
                        // Normal case when skinning is valid.
                        //

                        program.UpdateUniform("u_BoneTransform", r.Value.GetBoneTransformations());
                    }
                    else
                    {
                        //
                        // Odd case when the currentAnimation does not exist.
                        //

                        // Perserve world space transforms between skinning and non skinning.
                        transforms = new Matrix4[GLRig.MAX_BONES];
                        for ( int i = 0; i < GLRig.MAX_BONES; ++i )
                        {
                            transforms[i] = Matrix4.Scale(1.0f / 
                                rModels.First().Value.rig.bindingJoints[0].scale); //hacky
                        }

                        program.UpdateUniform("u_BoneTransform", transforms);
                    }
                }
                else
                {
                    //
                    // Case when the user does not wish to use animation data and just
                    // wants to render the model.
                    //

                    program.UpdateUniform("u_BoneTransform", identityTM);
                }

                //
                // World Transform.  We need to offset it when not using the skinning
                // pipeline.
                //

                Matrix4 worldView = Matrix4.Identity;
                if (isSkinning == true)
                {
                    worldView = world * camera.view;
                }
                else
                {
                    // Account for the skinning scale if we're not skinning.
                    Matrix4 scale = Matrix4.Scale(rModels.First().Value.rig.bindingJoints[0].scale); //hacky
                    worldView = scale * world * camera.view;
                }

                // Vertex Shader Uniforms
                program.UpdateUniform("u_WorldView",
                    worldView);
                program.UpdateUniform("u_WorldViewProjection",
                    worldView * camera.projection);

                r.Value.Draw();


                // Old code to test billboarding shaders.
                // Possibly role it into a ground model later.
                // I was using this to debug bone and joint transformations.

                //// Load shaders.
                //program = programs["default"];
                //program.Load();

                //// Load the texture for the shader.
                //GL.ActiveTexture(TextureUnit.Texture0);
                //program.UpdateUniform("u_Texture", 0);
                //if (r.Value.textureName != String.Empty)
                //{
                //    textures[r.Value.textureName].Bind(); // not checking return value        
                //}

                //// Draw Geometry
                //Matrix4[] debuggingTransforms = r.Value.GetBoneTransformations();
                //foreach (Matrix4 transform in debuggingTransforms)
                //{
                //    program.UpdateUniform("u_WorldViewProjection",
                //        transform * camera.view * camera.projection);
                //    billboards["default"].Draw();
                //}
            }
          
            // Unload shaders.
            GL.UseProgram( 0 );
        }

        public void IncrementAnimations()
        {
            foreach (var m in rModels)
            {
                m.Value.IncrementCurrentAnimation();
            }
        }

        public void DecrementAnimations()
        {
            foreach (var m in rModels)
            {
                m.Value.DecrementCurrentAnimation();
            }
        }

        // TODO: Doesn't support multiple models.
        public void SetCurrentFrameInCurrentAnimation(int frame, float percentTowardsNextFrame)
        {
            foreach (var m in rModels)
            {
                m.Value.SetCurrentFrame( frame, percentTowardsNextFrame );
                break;
            }
        }

        // TODO: Doesn't support multiple models.
        public float GetCurrentAnimationPercentageAnimated()
        {
            float result = 0.0f;

            foreach (var m in rModels)
            {
                result = m.Value.GetPercentageAnimated();
                break;
            }

            return result;
        }

        // Unlike decrement and increment, this function doesn't directly
        // translate to multiple models.  Need to pass a model ID or something.
        // But for now, who cares.  Only one model should be available at a time anyways.
        public void SetAnimations(String animation)
        {
            foreach (var m in rModels)
            {
                m.Value.SetCurrentAnimation(animation);
            }
        }

        // TODO: Doesn't support multiple models.
        public uint GetNumberOfFramesInCurrentAnimation()
        {
            uint result = 0;

            foreach (var m in rModels)
            {
                result = m.Value.GetNumberOfFramesInCurrentAnimation();
                break;
            }

            return result;
        }

        // TODO: Doesn't support multiple models.
        public List<String> GetAnimationsInCurrentModel()
        {
            List<String> result = new List<String>();

            foreach (var m in rModels)
            {
                foreach (var animation in m.Value.animations)
                {
                    result.Add(animation.Key);
                }
                break;
            }

            return result;
        }

        public void OnUpdate(float elapsedTime)
        {
            foreach (var m in rModels)
            {
                m.Value.Update(elapsedTime);
            }
        }

        public void DestroyCurrentModels()
        {
            // Clear out old model data.
            foreach (var s in sModels)
            {
                s.Value.Destory();
            }
            sModels.Clear();

            foreach (var r in rModels)
            {
                r.Value.Destroy();
            }
            rModels.Clear();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            foreach (var t in textures)
            {
                t.Value.Destroy();
            }
            textures.Clear();
            // End Clearing
        }

        public void ShutDown()
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
            foreach (var b in billboards)
            {
                b.Value.Destory();
            }
            billboards.Clear();

            foreach (var s in sModels)
            {
                s.Value.Destory();
            }
            sModels.Clear();

            foreach (var r in rModels)
            {
                r.Value.Destroy();
            }
            rModels.Clear();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            foreach (var t in textures)
            {
                t.Value.Destroy();
            }
            textures.Clear();
        }

        //
        // Helper Functions
        //

        // TODO: Alot of this code is a mess.
        // It should be refactored into more meaningful sub classes.

        private bool CreateShaderFromFile(String name, String path, ShaderType type)
        {
            bool result = true;

            GLShader shader = new GLShader(type);
            result = shader.LoadFromFile(path + name);

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
                // We need to clean up this shader since it failed.
                shader.Destroy();
            }

            return result;
        }

        private bool CreateShaderFromMemory(String name, String data, ShaderType type)
        {
            bool result = true;

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
                // We need to clean up this shader since it failed.
                shader.Destroy();
            }

            return result;
        }

        private bool CreateProgram(String progName, String vertName, String fragName,
            List<String> attributes, List<String> uniforms) 
        {
            bool result = true;

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
                program.Destroy();
            }

            return result;
        }

        private bool CreateBillboard( String name, List<float> vertexData,
            List<float> texData, List<uint> indexData )
        {
            bool result = true;

            GLBillboard billboard = new GLBillboard();
            result = billboard.Create(vertexData, texData, indexData);

            // Store new billboard.
            if (result == true)
            {
                billboards.Add(name, billboard);
            }
            else
            {
                billboard.Destory();
            }

            return result;
        }

        private bool CreateStaticModel(LOLModel model)
        {
            bool result = true;

            SKNFile file = new SKNFile();
            if (result == true)
            {
                // Model is stored in a RAF.
                result = SKNReader.Read(model.skn,
                    ref file);
                
            }

            GLStaticModel glModel = new GLStaticModel();
            if (result == true)
            {
                result = file.ToGLStaticModel(ref glModel, true);
            }

            // Store it.
            if (result == true)
            {
                String name = model.skn.FileName;
                int pos = name.LastIndexOf("/");
                name = name.Substring(pos + 1);

                sModels.Add(name, glModel);
            }

            //
            // Create Model Texture.
            //

            if (result == true)
            {
                // Texture stored in RAF file.
                result = CreateTexture(model.texture, TextureTarget.Texture2D,
                        GLTexture.SupportedImageEncodings.DDS);

                // Store it in our new model file.
                if (result == true)
                {
                    String name = model.texture.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);

                    glModel.textureName = name;
                }
            }

            return result;
        }

        private bool CreateRiggedModel(LOLModel model)
        {
            bool result = true;

            SKNFile sknFile = new SKNFile();
            if (result == true)
            {
                // Model is stored in a RAF.
                result = SKNReader.Read(model.skn,
                    ref sknFile);
            }

            SKLFile sklFile = new SKLFile();
            if (result == true)
            {
                result = SKLReader.Read(model.skl,
                    ref sklFile);
            }

            GLRiggedModel glModel = new GLRiggedModel();
            if (result == true)
            {
                result = sklFile.ToGLRiggedModel(ref glModel, sknFile, true);
            }

            // Store it.
            if (result == true)
            {
                String name = model.skn.FileName;
                int pos = name.LastIndexOf("/");
                name = name.Substring(pos + 1);

                rModels.Add(name, glModel);
            }

            //
            // Create Model Texture.
            //
            if (result == true)
            {
                // Texture stored in RAF file.
                result = CreateTexture(model.texture, TextureTarget.Texture2D,
                        GLTexture.SupportedImageEncodings.DDS);
                
                // Store it in our new model file.
                if (result == true)
                {
                    String name = model.texture.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);

                    glModel.textureName = name;
                }
            }

            //
            // Load up the model animations
            //
            if (result == true)
            {
                //
                // This code used to be in LOLDirectoryReader.
                // However, it takes awhile to parse all the animation files
                // for all the models.  So, I decided not to preload all of this
                // data and only load it when a model needs to be displayed.
                //

                Dictionary<String, ANMFile> animationFiles =
                    new Dictionary<String, ANMFile>();

                // Read in the animation files.
                foreach (var a in model.animations)
                {
                    ANMFile anmFile = new ANMFile();
                    bool anmResult = ANMReader.Read(a.Value, ref anmFile);
                    if (anmResult == true)
                    {
                        animationFiles.Add(a.Key, anmFile);
                    }
                }

                bool currentSet = false;
                foreach (var a in animationFiles)
                {
                    GLAnimation newAnimation = new GLAnimation();
                    a.Value.ToGLAnimation(ref newAnimation);
                    glModel.AddAnimation(a.Key, newAnimation);

                    // Set a default animation.
                    if (currentSet == false)
                    {
                        glModel.SetCurrentAnimation(a.Key);
                        currentSet = true;
                    }
                }

                glModel.currentFrame = 0;
            }

            return result;
        }
        
        private bool CreateTexture(FileInfo f, TextureTarget target,
            GLTexture.SupportedImageEncodings encoding)
        {
            bool result = true;

            GLTexture texture = new GLTexture();
            result = texture.Create(f, target,
                encoding);

            // Store new texture.
            if (result == true)
            {
                textures.Add(f.Name, texture);
            }
            else
            {
                texture.Destroy();
            }

            return result;
        }

        private bool CreateTexture(RAFlibPlus.RAFFileListEntry f, TextureTarget target,
            GLTexture.SupportedImageEncodings encoding)
        {
            bool result = true;

            GLTexture texture = new GLTexture();
            result = texture.Create(f, target,
                encoding);

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
                texture.Destroy();
            }

            return result;
        }
    }
}
