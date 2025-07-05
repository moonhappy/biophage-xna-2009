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

// Ellipse  effect - renders an ellipse primitive to a
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

VertexToPixel Ellipse_vs( 
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

PixelToFrame Ellipse_ps( VertexToPixel psIn )
{
     PixelToFrame outPS = (PixelToFrame) 0;
     
     float  distFromCenter;
     float2 circleCenter = float2( 0.5f, 0.5f );

     distFromCenter = distance( circleCenter, psIn.TexCoord.xy );

     //if distance fragment/pixel is from circle
     // centre is less than radius, colour it
     if ( 0.5f > distFromCenter )
     {
          outPS.Color = g_rgbaColor;
     } 
     else
     {
          outPS.Color = float4(0.0f, 0.0f, 0.0f, 0.0f);
     }
     
     return outPS;
}


//===============================================//


//-----------------------------------------------//
//---------------Techniques----------------------//
//-----------------------------------------------//

technique EllipseEffect
{
     pass P0
     {
          VertexShader = compile vs_2_0 Ellipse_vs();
          PixelShader  = compile ps_2_0 Ellipse_ps();
     }
}

