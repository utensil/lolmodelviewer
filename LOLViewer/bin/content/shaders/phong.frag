
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
// Implements the phong lighting model. - James Lammlein
//

precision highp float;

uniform vec4		u_LightDiffuse;
uniform float		u_KA;
uniform float		u_KD;
uniform float		u_KS;
uniform float		u_SExponent;
uniform sampler2D 	u_Texture;

in vec3 v_ViewSpacePosition;
in vec3 v_VertexToLight;
in vec3 v_Normal;
in vec2 v_TexCoords;
out vec4 gl_FragColor;
 
void main(void) 
{
   	vec3 L = v_VertexToLight;  

	// we are in camera space, so camera position is (0,0,0).
	// Therefore, camera position - v_ViewSpaceVertex
	// = -v_ViewSpaceVertex
   	vec3 E = normalize(-v_ViewSpacePosition);

   	vec3 R = normalize(reflect(L,v_Normal));  
 
   	// Ambient - Sample texture for "ambient" lighting.
   	vec4 ambient = texture2D( u_Texture, v_TexCoords );

   	// Diffuse  
   	vec4 diffuse = u_LightDiffuse * max(dot(v_Normal,L), 0.0);
   	diffuse = clamp(diffuse, 0.0, 1.0); 
   
   	// Specular
   	vec4 specular = vec4(1.0f, 1.0f, 1.0f, 1.0f) * 
		max( pow(-dot(R,E), u_SExponent), 0.0 );
   	specular = clamp(specular, 0.0, 1.0); 

   	// Finalize  
	gl_FragColor = u_KA * ambient + u_KD * diffuse + u_KS * specular;
}