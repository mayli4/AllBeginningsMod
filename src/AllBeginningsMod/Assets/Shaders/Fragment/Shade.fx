matrix uWorldViewProjection;

float4 baseShadowColor;
float noiseScroll; // Scroll speed of the noise
float noiseStretch; // How far the noise is spread apart
float4 adjustColor; // Used here for its alpha component as cutout intensity

texture noiseTexture;
sampler2D noise = sampler_state{
    texture = <noiseTexture>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    AddressU = wrap;
    AddressV = wrap;
};

struct VertexShaderInput{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input){
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float invlerp(float from, float to, float value){
    return clamp((value - from) / (to - from), 0.0, 1.0);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0{
    float2 uv = input.TextureCoordinates;

    float baseGradientOpacity = pow(1 - uv.y, 1.5);
    baseGradientOpacity *= pow(invlerp(0.5, 0.4, abs(uv.x - 0.5)), 0.6);

    float currentShadowOpacity = baseGradientOpacity * baseShadowColor.a;

    float noiseValue = tex2D(noise, float2(uv.x * noiseStretch, noiseScroll)).r;

    float cutoutStrength = clamp(adjustColor.a, 0.0, 1.0);

    float opacityToRemove = noiseValue * cutoutStrength * currentShadowOpacity;

    float finalOpacity = currentShadowOpacity - opacityToRemove;
    finalOpacity = clamp(finalOpacity, 0.0, 1.0);
    float3 finalColor = baseShadowColor.rgb;

    return float4(finalColor, finalOpacity);
}

technique Technique1
{
    pass TreeShadowCastPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}