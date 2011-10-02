
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
// High Level Abstraction for an OpenGL model. 
// This class is designed to be inheirited for
// more specific functionality.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LOLViewer
{
    class GLModel
    {
        protected int numIndices;
        protected int vao, vBuffer, iBuffer, tBuffer, nBuffer;

        public GLModel()
        {
            vao = vBuffer = iBuffer = tBuffer = nBuffer = numIndices = 0;
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
                // GL.BindVertexArray(vao);
                // GL.DisableVertexAttribArray(vBuffer);
                // GL.DisableVertexAttribArray(tBuffer);
                // GL.DisableVertexAttribArray(nBuffer);
                // GL.BindVertexArray(0);

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

            numIndices = 0;
        }
    }
}
