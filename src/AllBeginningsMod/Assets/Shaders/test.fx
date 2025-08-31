float4 frag(float2 uv : TEXCOORD0) : COLOR0 {
    return float4(uv.x, uv.y, 0.5f, 1.0f);
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_2_0 frag();
    }
};