sampler Panel : register(s0);
sampler Target : register(s1);

float4 Source;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : SV_POSITION, float2 textureCoords : TEXCOORD0) : COLOR0{
    float2 resolution = Source.xy;
    float2 position = Source.zw;

    coords = coords - position;
    coords = coords / resolution;
    
    float4 color = tex2D(Target, coords);
    
    float4 panel = tex2D(Panel, textureCoords);
    
    color *= panel.a;
    
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}