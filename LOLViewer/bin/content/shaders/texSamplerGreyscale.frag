

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
// Samples texture for fragment's output and converts to greyscale.
// No lighting. - James Lammlein
//

precision highp float;

uniform sampler2D u_Texture;

in vec2 v_TexCoords;
out vec4 gl_FragColor;
 
void main(void) 
{
	gl_FragColor.rgb = vec3( texture2D( u_Texture, v_TexCoords ) );
	
	// Greyscale filter
	gl_FragColor.r *= 0.299f;
	gl_FragColor.g *= 0.587f;
	gl_FragColor.b *= 0.114f;

	float sum = gl_FragColor.r + gl_FragColor.g + gl_FragColor.b;	

	gl_FragColor.r = sum;
	gl_FragColor.g = sum;
	gl_FragColor.b = sum;

	gl_FragColor.a = 1.0f;
}