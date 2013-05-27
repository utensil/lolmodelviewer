
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
//  Abstracts the requirements for an OpenGL vertex-fragment
//  shader pair.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace LOLViewer.Graphics
{
    class GLShaderProgram
    {
        private int program;
        private List<int> shaders;
        private Dictionary<String, int> uniforms;

        public GLShaderProgram()
        {
            program = 0;
            shaders = new List<int>();
            uniforms = new Dictionary<string, int>();
        }

        public bool Create()
        {
            bool result = true;

            program = GL.CreateProgram();
            if (program < 1)
            {
                result = false;
            }

            return result;
        }

        public bool AttachShader(int shader)
        {
            bool result = true;

            GL.AttachShader(program, shader);

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }
            else
            {
                shaders.Add(shader);
            }

            return result;
        }

        public bool BindAttribute(int location, String value)
        {
            bool result = true;

            GL.BindAttribLocation(program, location, value);

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            return result;
        }

        public bool SetUniformLocation(String name)
        {
            bool result = true;

            int location = GL.GetUniformLocation(program, name);
            if (location == -1) // -1 = fail
            {
                result = false;
            }

            if (result == true)
            {
                uniforms.Add(name, location);
            }

            return result;
        }

        public bool Link()
        {
            bool result = true;

            GL.LinkProgram(program);

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Check for linkage errors.
            if (result == true)
            {
                int isLinked;
                GL.GetProgram( program, ProgramParameter.LinkStatus, out isLinked );

                if (isLinked == 0) // 0 = fail to link
                {
                    // Output compilation errors.
                    String linkingErrors = String.Empty;
                    GL.GetProgramInfoLog(program, out linkingErrors);
                    Debug.WriteLine(linkingErrors);

                    result = false;
                }
            }

            return result;
        }

        public bool Load()
        {
            bool result = true;

            GL.UseProgram(program);

            ErrorCode error = GL.GetError();
            if(error != ErrorCode.NoError)
            {
                result = false;
            }

            return result;
        }

        public void Destroy()
        {
            foreach (int s in shaders)
            {
                GL.DetachShader(program, s);
            }
            shaders.Clear();

            if (program != 0)
            {
                GL.DeleteProgram(program);
            }
        }

        public bool VerifyAttributes(List<String> attributes)
        {
            bool result = true;

            int location = 0;
            foreach (String s in attributes)
            {
                int position = GL.GetAttribLocation(program, s);
                if (position != location)
                {
                    result = false;
                    break;
                }

                location++;
            }

            return result;
        }

        //
        //
        // Overloaded Funtions to update uniforms.
        // Added as needed.
        //
        //

        public bool UpdateUniform(String name, OpenTK.Vector3 vec)
        {
            bool result = true;

            GL.Uniform3(uniforms[name], ref vec);

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            return result;
        }

        public bool UpdateUniform(String name, OpenTK.Vector4 vec)
        {
            bool result = true;

            GL.Uniform4(uniforms[name], ref vec);

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            return result;
        }

        public bool UpdateUniform(String name, OpenTK.Matrix4 mat)
        {
            bool result = true;

            GL.UniformMatrix4(uniforms[name], false, ref mat);
            
            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            return result;
        }

        public bool UpdateUniform(String name, OpenTK.Matrix4[] matArray)
        {
            bool result = true;
            
            List<float> data = new List<float>();
            foreach( OpenTK.Matrix4 mat in matArray )
            {
                // Row 1
                data.Add( mat.M11 );
                data.Add( mat.M12 );
                data.Add( mat.M13 );
                data.Add( mat.M14 );

                // Row 2
                data.Add( mat.M21 );
                data.Add( mat.M22 );
                data.Add( mat.M23 );
                data.Add( mat.M24 );

                // Row 3
                data.Add( mat.M31 );
                data.Add( mat.M32 );
                data.Add( mat.M33 );
                data.Add( mat.M34 );

                // Row 4
                data.Add( mat.M41 );
                data.Add( mat.M42 );
                data.Add( mat.M43 );
                data.Add( mat.M44 );
            }

            GL.UniformMatrix4(uniforms[name], matArray.Count(), false,
                data.ToArray());

            ErrorCode error = GL.GetError();
            if( error != ErrorCode.NoError )
            {
                result = false;
            }

            return result;
        }

        public bool UpdateUniform(String name, int i)
        {
            bool result = true;

            GL.Uniform1(uniforms[name], i);

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            return result;
        }

        public bool UpdateUniform(String name, float f)
        {
            bool result = true;

            GL.Uniform1(uniforms[name], f);

            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            return result;
        }
    }
}
