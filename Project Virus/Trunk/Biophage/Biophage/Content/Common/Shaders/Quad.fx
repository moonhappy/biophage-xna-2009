//Copyright 2009 Phillip Cooper
//
//   This file is part of LNA.
//
//   LNA is free software: you can redistribute it and/or modify
//   it under the terms of the GNU Lesser General Public License as
//   published by the Free Software Foundation, either version 3 of
//   the License, or (at your option) any later version.
//
//   LNA is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU Lesser General Public License for more details.
//
//   You should have received a copy of the GNU Lesser General Public
//   License along with LNA.
//   If not, see <http://www.gnu.org/licenses/>.

// Quad  effect - renders a quad primitive to a
// surface.

//-----------------------------------------------//
//----------------Connectors---------------------//
//-----------------------------------------------//

float4x4 g_mWorldViewProj;
float4   g_rgbaColor;

//===============================================//



//-----------------------------------------------//
//--------------IN/OUT Structs-------------------//
//-----------------------------------------------//

struct VertexToPixel
{
     float4 Position   : POSITION;
     float2 TexCoord   : TEXCOORD0;
};

struct PixelToFrame
{
     float4 Color      : COLOR0;
};

//===============================================//


//-----------------------------------------------//
//---------------Vertex Shader-------------------//
//-----------------------------------------------//

VertexToPixel Quad_vs( 
     float4 inPosition : POSITION,
     float2 inTexCoord : TEXCOORD0 )
{
     VertexToPixel Out = (VertexToPixel) 0;

     //transform the input position to the output
     Out.Position = mul( inPosition, g_mWorldViewProj);
     
     //pass through the tex coord
     Out.TexCoord = inTexCoord;

     //return the output position
     return Out;
}

//===============================================//


//-----------------------------------------------//
//---------------Pixel Shader--------------------//
//-----------------------------------------------//

PixelToFrame Quad_ps( VertexToPixel psIn )
{
     PixelToFrame outPS = (PixelToFrame) 0;
     
     outPS.Color = g_rgbaColor;
     
     return outPS;
}


//===============================================//


//-----------------------------------------------//
//---------------Techniques----------------------//
//-----------------------------------------------//

technique QuadEffect
{
     pass P0
     {
          VertexShader = compile vs_2_0 Quad_vs();
          PixelShader  = compile ps_2_0 Quad_ps();
     }
}

