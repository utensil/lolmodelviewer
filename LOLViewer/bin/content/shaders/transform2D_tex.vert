

/*

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



#version 150

//
// Basic vertex shader to transform
// 3D coordinates to screen space. - James Lammlein
//

uniform mat4 u_WorldViewProjection;

in  vec3 in_Position;
in  vec2 in_TexCoords;

out vec2 v_TexCoords;

void main(void) 
{
    	gl_Position = u_WorldViewProjection * vec4(in_Position, 1.0);

	v_TexCoords = in_TexCoords;
}