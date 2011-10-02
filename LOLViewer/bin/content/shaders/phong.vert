
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
// Vertex Shader for Phong Lighting. - James Lammlein
//

uniform mat4 u_WorldView;		// we need vector from vertex to eye
uniform mat4 u_WorldViewProjection;	// normal transform for vertex
uniform vec3 u_LightDirection;

in  vec3 in_Position;
in  vec3 in_Normal;
in  vec2 in_TexCoords;

out vec3 v_ViewSpacePosition;
out vec3 v_VertexToLight;
out vec3 v_Normal;
out vec2 v_TexCoords;

void main(void) 
{
	// Normal Transform
    	gl_Position = u_WorldViewProjection * vec4(in_Position, 1.0);

	// Required for phong lighting
	vec3 viewLightDirection = ( u_WorldView * vec4(u_LightDirection, 0.0) ).xyz;
	v_VertexToLight = normalize( viewLightDirection );
	
	v_ViewSpacePosition = ( u_WorldView * vec4(in_Position, 1.0) ).xyz;

	// Only take to view space
	v_Normal = ( normalize( u_WorldView * vec4(in_Normal, 0.0) ) ).xyz;

	v_TexCoords = in_TexCoords;
}