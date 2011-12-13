
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
// Contains definitions for vertex and fragment shaders.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOLViewer
{
    class GLShaderDefinitions
    {
        public const String PhongVertex
            = @"#version 150

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
	                // The normal graphic's pipeline transform.
    	            gl_Position = u_WorldViewProjection * vec4(in_Position, 1.0);

	                // Required for phong lighting
	                vec3 viewLightDirection = ( u_WorldView * vec4(u_LightDirection, 0.0) ).xyz;
	                v_VertexToLight = normalize( viewLightDirection );
	
	                v_ViewSpacePosition = ( u_WorldView * vec4(in_Position, 1.0) ).xyz;

	                // Only take to view space
	                v_Normal = ( normalize( u_WorldView * vec4(in_Normal, 0.0) ) ).xyz;

	                v_TexCoords = in_TexCoords;
                }";

        public const String PhongRiggedVertex
            = @"#version 150

                //
                // Vertex Shader for Skeletal Animation and Phong Lighting.
                // Supports 4 bones per vertex.
                // - James Lammlein
                //

                uniform mat4  u_WorldView;		// we need vector from vertex to eye
                uniform mat4  u_WorldViewProjection;	// normal transform for vertex
                uniform vec3  u_LightDirection; 
                uniform float u_BoneScale[128];
                uniform mat4  u_BoneTransform[128];

                in  vec3  in_Position;
                in  vec3  in_Normal;
                in  vec2  in_TexCoords;
                in  vec4  in_BoneID;
                in  vec4  in_Weights;

                out vec3 v_ViewSpacePosition;
                out vec3 v_VertexToLight;
                out vec3 v_Normal;
                out vec2 v_TexCoords;

                void main(void) 
                {
                    // Transform the vertex information based on bones.
                    vec3 position = (in_Weights[0] * (u_BoneTransform[ int(in_BoneID[0]) ] * vec4(in_Position, 1.0)).xyz) +
                                    (in_Weights[1] * (u_BoneTransform[ int(in_BoneID[1]) ] * vec4(in_Position, 1.0)).xyz) +
                                    (in_Weights[2] * (u_BoneTransform[ int(in_BoneID[2]) ] * vec4(in_Position, 1.0)).xyz) +
                                    (in_Weights[3] * (u_BoneTransform[ int(in_BoneID[3]) ] * vec4(in_Position, 1.0)).xyz); 

                    vec3 normal =   (in_Weights[0] * (u_BoneTransform[ int(in_BoneID[0]) ] * vec4(in_Normal, 0.0)).xyz) +
                                    (in_Weights[1] * (u_BoneTransform[ int(in_BoneID[1]) ] * vec4(in_Normal, 0.0)).xyz) +
                                    (in_Weights[2] * (u_BoneTransform[ int(in_BoneID[2]) ] * vec4(in_Normal, 0.0)).xyz) +
                                    (in_Weights[3] * (u_BoneTransform[ int(in_BoneID[3]) ] * vec4(in_Normal, 0.0)).xyz);

	                // The normal graphic's pipeline transform.
    	            gl_Position = u_WorldViewProjection * vec4( position, 1.0 );

	                // Required for phong lighting
	                vec3 viewLightDirection = ( u_WorldView * vec4(u_LightDirection, 0.0) ).xyz;
	                v_VertexToLight = normalize( viewLightDirection );
	
	                v_ViewSpacePosition = ( u_WorldView * vec4(in_Position, 1.0) ).xyz;

	                // Only take to view space
	                v_Normal = ( normalize( u_WorldView * vec4(normal, 0.0) ) ).xyz;

	                v_TexCoords = in_TexCoords;
                }";

        public const String TransformTexturedVertex
                 = @"

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
                    }";

        public const String PhongTexOnlyFragment
                 = @"#version 150

                    //
                    // This shader doesn't actually implement phong lighting.
                    // It just samples a texture.  However, it can be paired
                    // with phong.vert in a shader program. - James Lammlein
                    //

                    precision highp float;

                    uniform sampler2D 	u_Texture;

                    in vec3 v_ViewSpaceVertex;
                    in vec3 v_Normal;
                    in vec2 v_TexCoords;
                    out vec4 gl_FragColor;
 
                    void main(void) 
                    {
   	                    gl_FragColor = texture2D( u_Texture, v_TexCoords );
                    }";

        public const String PhongFragment 
             = @"
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
	                // Therefore, camera position - v_ViewSpacePosition
	                // = -v_ViewSpacePosition
   	                vec3 E = normalize(-v_ViewSpacePosition);

   	                vec3 R = normalize(reflect(L,v_Normal));  
 
   	                // Ambient - Sample texture for ambient lighting.
   	                vec4 ambient = texture2D( u_Texture, v_TexCoords );

   	                // Diffuse  
   	                vec4 diffuse = u_LightDiffuse * max(dot(v_Normal,L), 0.0);
   	                diffuse = clamp(diffuse, 0.0, 1.0); 
   
   	                // Specular
   	                vec4 specular = vec4(1.0f, 1.0f, 1.0f, 1.0f) * 
                        pow( max(-dot(R,E), 0.0f ), u_SExponent );
   	                specular = clamp(specular, 0.0, 1.0); 

   	                // Finalize  
	                gl_FragColor = u_KA * ambient + u_KD * diffuse + u_KS * specular;
                }";

        public const String TextureSamplerFragment
             = @"
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
                }";

        public const String TextureSamplerGreyscaleFragment
             = @"
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
                }";
    }
}
