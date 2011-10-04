

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
    class GLRenderer
    {
        // Renderer Variables
        private OpenTK.Graphics.Color4 clearColor;

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
        public const float DEFAULT_MODEL_SCALE = 0.25f;

        public GLRenderer()
        {
            clearColor = new OpenTK.Graphics.Color4(0.1f, 0.2f, 0.5f, 0.0f);

            programs = new Dictionary<String, GLShaderProgram>();
            shaders = new Dictionary<String, GLShader>();
            billboards = new Dictionary<String, GLBillboard>();
            sModels = new Dictionary<String, GLStaticModel>();
            rModels = new Dictionary<String, GLRiggedModel>();
            textures = new Dictionary<String, GLTexture>();

            world = Matrix4.Scale(DEFAULT_MODEL_SCALE);
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

                // Unused atm.
                result = CreateProgram("phong", "phong.vert", "phong.frag",
                    attributes, uniforms);
            }

            // Model w/ Texture Only
            if (result == true)
            {
                List<String> attributes = new List<String>();
                attributes.Add("in_Position");
                attributes.Add("in_Normal");
                attributes.Add("in_TexCoords");

                List<String> uniforms = new List<String>();
                uniforms.Add("u_WorldViewProjection");
                uniforms.Add("u_Texture");

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
                attributes.Add("in_TexBoneID");
                attributes.Add("in_Weights");

                List<String> uniforms = new List<String>();
                uniforms.Add("u_WorldView");
                uniforms.Add("u_WorldViewProjection");
                uniforms.Add("u_LightDirection");
                //uniforms.Add("u_BoneScale");
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

            // Create Geometry
            // Old Debugging Code.
            // I kept it around incase it would be cool to draw a ground
            // plane or something.
            /*
            if (result == true)
            {
                List<float> verts = new List<float>();
                // Bottom Left
                verts.Add( -0.5f );
                verts.Add( -0.5f );
                verts.Add( 0.0f );

                // Top Left
                verts.Add( -0.5f );
                verts.Add( 0.5f );
                verts.Add( 0.0f );

                // Top Right
                verts.Add( 0.5f );
                verts.Add( 0.5f );
                verts.Add( 0.0f );

                // Bottom Right
                verts.Add( 0.5f );
                verts.Add( -0.5f );
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
            */

            // Misc. OpenGL Parameters.
            if (result == true)
            {
                GL.CullFace(CullFaceMode.Back);
                GL.Enable(EnableCap.CullFace);
                GL.ClearColor(clearColor);
            }

            return result;
        }

        public bool LoadModel( LOLModel model)
        {
            bool result = true;

            // Clear out old model data.
            foreach (var s in sModels)
            {
                s.Value.Destory();
            }
            sModels.Clear();

            foreach (var r in rModels)
            {
                r.Value.Destory();
            }
            rModels.Clear();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            foreach (var t in textures)
            {
                t.Value.Destroy();
            }
            textures.Clear();
            // End Clearing

            //
            // Create model geometry
            //

            // If the model definition does not contain .skl files,
            // then it's a static model.
            if (model.fileSkl == null &&
                model.rafSkl == null &&
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

        public void OnRender(GLCamera camera)
        {
            // Clear back buffers.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            
            // Enable a normal depth test without stenciling.
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.StencilTest);
            GL.DepthFunc(DepthFunction.Less);

            // Old code to test billboarding shaders.
            // Possibly role it into a ground model later.
            /*
            // Load shaders.
            GLShaderProgram program = programs["default"];
            program.Load();
            program.UpdateUniform("u_WorldViewProjection", Matrix4.Identity);

            // Load the texture for the shader.
            GL.ActiveTexture(TextureUnit.Texture0);
            program.UpdateUniform("u_Texture", 0);
            textures["missfortune_waterloo_TX_CM.dds"].Bind(); // not checking return value
            
            // Draw Geometry
            foreach (var b in billboards)
            {
                b.Value.Draw();
            }
            */

            GLShaderProgram program = null;
            
            // Load shaders for phong lighting.
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
                program.UpdateUniform("u_KA", 0.8f);
                program.UpdateUniform("u_KD", 0.1f);
                program.UpdateUniform("u_KS", 0.1f);
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

            // Load shaders for rigged phong lighting.
            if (rModels.Count > 0)
            {
                program = programs["phongRigged"];
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
                program.UpdateUniform("u_KA", 0.8f);
                program.UpdateUniform("u_KD", 0.1f);
                program.UpdateUniform("u_KS", 0.1f);
                program.UpdateUniform("u_SExponent", 8.0f);
            }

            // Draw Model
            foreach (var r in rModels)
            {
                // Load the model's texture for the shader.
                GL.ActiveTexture(TextureUnit.Texture0);
                program.UpdateUniform("u_Texture", 0);
                if (r.Value.textureName != String.Empty)
                {
                    textures[r.Value.textureName].Bind(); // not checking return value        
                }

                // Load the bone information for this model.
                program.UpdateUniform("u_BoneTransform", r.Value.boneTransforms);
                
                // Debug Stuff
                //Matrix4[] boneTrans = new Matrix4[GLRiggedModel.MAX_BONES];
                //for (int i = 0; i < GLRiggedModel.MAX_BONES; ++i)
                //{
                //    boneTrans[i] = Matrix4.Identity;
                //}
                //program.UpdateUniform("u_BoneTransform", boneTrans);

                r.Value.Draw();
            }
          
            // Unload shaders.
            GL.UseProgram( 0 );
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
                r.Value.Destory();
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
        // It should be refactored into more meaningful classes.

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
                }

                count++;
            }

            if (result == true)
            {
                result = program.Link();
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
                }
            }

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
                if (model.fileSkn != null)
                {
                    // Model is stored in a raw file on disc.
                    result = SKNReader.Read(model.fileSkn,
                        ref file);
                }
                else
                {
                    // Model is stored in a RAF.
                    result = SKNReader.Read(model.rafSkn,
                        ref file);
                }
            }

            GLStaticModel glModel = new GLStaticModel();
            if (result == true)
            {
                result = file.ToGLStaticModel(ref glModel, true);
            }

            // Store it.
            if (result == true)
            {
                if (model.fileSkn != null)
                {
                    sModels.Add(model.fileSkn.Name, glModel);
                }
                else
                {
                    String name = model.rafSkn.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);

                    sModels.Add(name, glModel);
                }
            }

            //
            // Create Model Texture.
            //

            if (result == true)
            {
                if (model.fileTexture != null)
                {
                    // Texture stored directory on disk.
                    result = CreateTexture(model.fileTexture, TextureTarget.Texture2D,
                            GLTexture.SupportedImageEncodings.DDS);
                }
                else
                {
                    // Texture stored in RAF file.
                    result = CreateTexture(model.rafTexture, TextureTarget.Texture2D,
                            GLTexture.SupportedImageEncodings.DDS);
                }

                // Store it in our new model file.
                if (result == true)
                {
                    if (model.fileTexture != null)
                    {
                        glModel.textureName = model.fileTexture.Name;
                    }
                    else
                    {
                        String name = model.rafTexture.FileName;
                        int pos = name.LastIndexOf("/");
                        name = name.Substring(pos + 1);

                        glModel.textureName = name;
                    }
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
                if (model.fileSkn != null)
                {
                    // Model is stored in a raw file on disc.
                    result = SKNReader.Read(model.fileSkn,
                        ref sknFile);
                }
                else
                {
                    // Model is stored in a RAF.
                    result = SKNReader.Read(model.rafSkn,
                        ref sknFile);
                }
            }

            SKLFile sklFile = new SKLFile();
            if (result == true)
            {
                if (model.fileSkl != null)
                {
                    result = SKLReader.Read(model.fileSkl,
                        ref sklFile);
                }
                else
                {
                    result = SKLReader.Read(model.rafSkl,
                        ref sklFile);
                }
            }

            GLRiggedModel glModel = new GLRiggedModel();
            if (result == true)
            {
                result = sklFile.ToGLRiggedModel(ref glModel, sknFile, true);
            }

            // Store it.
            if (result == true)
            {
                if (model.fileSkn != null)
                {
                    rModels.Add(model.fileSkn.Name, glModel);
                }
                else
                {
                    String name = model.rafSkn.FileName;
                    int pos = name.LastIndexOf("/");
                    name = name.Substring(pos + 1);

                    rModels.Add(name, glModel);
                }
            }

            //
            // Create Model Texture.
            //

            if (result == true)
            {
                if (model.fileTexture != null)
                {
                    // Texture stored directory on disk.
                    result = CreateTexture(model.fileTexture, TextureTarget.Texture2D,
                            GLTexture.SupportedImageEncodings.DDS);
                }
                else
                {
                    // Texture stored in RAF file.
                    result = CreateTexture(model.rafTexture, TextureTarget.Texture2D,
                            GLTexture.SupportedImageEncodings.DDS);
                }

                // Store it in our new model file.
                if (result == true)
                {
                    if (model.fileTexture != null)
                    {
                        glModel.textureName = model.fileTexture.Name;
                    }
                    else
                    {
                        String name = model.rafTexture.FileName;
                        int pos = name.LastIndexOf("/");
                        name = name.Substring(pos + 1);

                        glModel.textureName = name;
                    }
                }
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

        private bool CreateTexture(RAFLib.RAFFileListEntry f, TextureTarget target,
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
