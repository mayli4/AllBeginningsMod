sampler uImage0 : register(s0);

float2 uSize;
float4 uColor;
float uThreshold;

inline bool opaque(float2 coords)
{
  return tex2D(uImage0, coords).a > uThreshold;
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0 {

    float2 fragCoord = coords * uSize;

    const float2 right = float2(1, 0);
    const float2 down = float2(0, 1);
    
    if (!opaque(coords) && (
        opaque((fragCoord + right) / uSize.xy) ||
        opaque((fragCoord + down) / uSize.xy) ||
        opaque((fragCoord - right) / uSize.xy) ||
        opaque((fragCoord - down) / uSize.xy)
    )) return uColor;
    return tex2D(uImage0, coords);
}

technique Technique1 {
    pass AwesomePass {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};