matrix transformationMatrix;
float4 color = float4(1, 1, 1, 1);
bool blackAsAlpha = false;
int spriteRotation = 0;

texture sampleTexture;
sampler2D samplerTexture = sampler_state
{
    texture = <sampleTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

struct VSInput
{
    float4 position : POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};

struct VSOutput
{
    float4 position : POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};

const float2 rotationTable[8] = {
    float2(1, 0),
    float2(0, 1),

    float2(0, -1),
    float2(1, 0),

    float2(-1, 0),
    float2(0, -1),

    float2(0, 1),
    float2(-1, 0)
};

VSOutput vert(VSInput input)
{
    VSOutput output;
    output.color = input.color;
    output.position = mul(input.position, transformationMatrix);
    output.uv = input.uv;

    return output;
}

float4 frag(VSOutput input) : COLOR0
{
    int index = spriteRotation % 4;
    float2 rotX = rotationTable[index * 2];
    float2 rotY = rotationTable[index * 2 + 1];

    float2 uv = float2(
        rotX.x * input.uv.x + rotX.y * input.uv.y, 
        rotY.x * input.uv.x + rotY.y * input.uv.y
    );

    if (blackAsAlpha)
    {
        return input.color * tex2D(samplerTexture, uv).r * color;
    }
    
    return input.color * tex2D(samplerTexture, uv) * color;
}

technique technique1
{
    pass pass1
    {
        VertexShader = compile vs_2_0 vert();
        PixelShader = compile ps_2_0 frag();
    }
}