
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
//  Represents an OpenGL Shader
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace LOLViewer.Graphics
{
    class GLShader
    {
        public int shader;
        private ShaderType type;
        private String data;

        public GLShader(ShaderType type)
        {
            this.type = type;
            shader = 0;
            data = String.Empty;
        }

        /// <summary>
        /// Loads the shader from a string
        /// </summary>
        /// <param name="data">
        /// The definition of the shader.
        /// </param>
        /// <returns></returns>
        public bool LoadFromMemory(String data)
        {
            this.data = data;
            return true;
        }

        public bool Compile()
        {
            bool result = true;

            // Create the shader.
            shader = GL.CreateShader(type);
            ErrorCode error = GL.GetError();
            if (shader < 1)
                result = false;

            // Send the source to the shader.
            if (result == true)
            {
                GL.ShaderSource(shader, data);
            }

            // Check for errors.
            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Compile the shader
            if (result == true)
            {
                GL.CompileShader(shader);
            }

            // Check for errors.
            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                result = false;
            }

            // Check for shader syntax errors.
            if( result == true )
            {
                int isCompiled;
                GL.GetShader(shader, ShaderParameter.CompileStatus, out isCompiled);

                if (isCompiled == 0) // 0 = fail to compile
                {
                    // Output compilation errors.
                    String compilingErrors = String.Empty;
                    GL.GetShaderInfoLog(shader, out compilingErrors);
                    Debug.WriteLine(compilingErrors);

                    result = false;
                }
            }

            return result;
        }

        public void Destroy()
        {
            GL.DeleteShader(shader);
        }
    }
}
