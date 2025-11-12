#include "../pixelation.h"

sampler uImage0 : register(s0);

float4 uSource;
float uHoverIntensity;
float uPixel;


float4 main(float2 coords : SV_POSITION, float2 tex_coords : TEXCOORD0, float4 baseColor : COLOR0) : COLOR0
{
    float2 resolution = uSource.xy;
    float2 panelCoords = coords; 
    
    float2 uv = panelCoords / resolution.xy;
    uv = normalize_with_pixelation(panelCoords, uPixel, uSource.xy);

    return float4(0, 0, 1, 0.5f);
}

technique Technique1 {
    pass PanelShader {
        PixelShader = compile ps_3_0 main();
    }
}