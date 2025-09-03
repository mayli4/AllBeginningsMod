sampler uImage0 : register(s0);
float4 color;

float4 frag(float2 uv : TEXCOORD0) : COLOR0 {
    float4 s = tex2D(uImage0, uv);
    return float4(lerp(s.rgb, color.rgb, color.a), 1) * s.a;
}

technique Technique1 {
    pass AwesomePass {
        PixelShader = compile ps_2_0 frag();
    }
};