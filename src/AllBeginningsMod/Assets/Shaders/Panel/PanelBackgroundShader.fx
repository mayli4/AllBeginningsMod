#include "../pixelation.h"

sampler uImage0 : register(s0);

texture uTexture;
sampler tex0 = sampler_state
{
    texture = <uTexture>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

float4 uSource;
float uHoverIntensity;
float uPixel;
float uColorResolution;
float uGrayness;
float4 uColor;
float4 uSecondaryColor;
float uSpeed;
bool uSmallPanel;

float4 main(float2 coords : SV_POSITION, float2 tex_coords : TEXCOORD0, float4 baseColor : COLOR0) : COLOR0
{
    float2 resolution = uSource.xy;
    float2 position = uSource.zw;
    coords -= position;
    float2 uv = normalize_with_pixelation(coords, uPixel, float2(resolution.x, resolution.y));
    
    
    return float4(1, 1, 1, 1);
}

technique Technique1 {
    pass PanelShader {
        PixelShader = compile ps_3_0 main();
    }
}