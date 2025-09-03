sampler uImage0 : register(s0);

float4 uColor;
float4 uSource;
matrix uMatrix;

struct VSInput
{
    float id : TEXCOORD0;
};

struct VSOutput
{
    float4 position : POSITION;
    float2 uv : TEXCOORD;
};

const float2 positions[4] = {
    float2(1, 0),
    float2(1, 1),
    float2(0, 0),
    float2(0, 1)
};

const float4 sources[8] = {
    float4(1, 0, 1, 0), float4(0, 1, 0, 1),
    float4(1, 0, 1, 0), float4(0, 1, 0, 0),
    float4(1, 0, 0, 0), float4(0, 1, 0, 1),
    float4(1, 0, 0, 0), float4(0, 1, 0, 0)
};

VSOutput vert(VSInput input)
{
    VSOutput output;
    output.position = mul(float4(positions[input.id], 0, 1.0), uMatrix);

    float4 uvx = uSource * sources[input.id * 2];
    float4 uvy = uSource * sources[input.id * 2 + 1];
    output.uv = float2(
        uvx.x + uvx.y + uvx.z + uvx.w,
        uvy.x + uvy.y + uvy.z + uvy.w
    );

    return output;
}

float4 frag(VSOutput input) : COLOR0
{
    return tex2D(uImage0, input.uv) * uColor;
}

technique technique1
{
    pass pass1
    {
        VertexShader = compile vs_2_0 vert();
        PixelShader = compile ps_2_0 frag();
    }
}