

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
// Samples texture for fragment's output.
// No lighting. - James Lammlein
//

precision highp float;

uniform sampler2D u_Texture;

in vec2 v_TexCoords;
out vec4 gl_FragColor;
 
void main(void) 
{
	gl_FragColor.rgb = vec3( texture2D( u_Texture, v_TexCoords ) );
	gl_FragColor.a = 1.0f;
}