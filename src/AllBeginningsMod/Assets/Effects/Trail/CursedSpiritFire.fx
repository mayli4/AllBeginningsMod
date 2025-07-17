matrix mat;
float time;
float stepY;
float scale = 1;

texture texture1;
sampler2D sampler1 = sampler_state
{
    texture = <texture1>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture texture2;
sampler2D sampler2 = sampler_state
{
    texture = <texture2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

struct VSInput
{
    float4 position : POSITION;
    float2 coords : TEXCOORD;
    float4 color : COLOR;
};

struct VSOutput
{
    float4 position : POSITION;
    float2 coords : TEXCOORD;
    float4 color : COLOR;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output;
    output.color = input.color;
    output.position = mul(input.position, mat);
    output.coords = input.coords;
    
    return output;
}

const float PI = 3.14;

float4 PixelShaderFunction(VSOutput output) : COLOR0
{
    float s1 = tex2D(sampler1, output.coords * scale - float2(time, 0)).r;
    float s2 = tex2D(sampler2, output.coords * scale + float2(time, 0)).r;

    float s2Alpha = lerp(1, s2, output.coords.x);
    float alpha = step(
        stepY, 
        s1 * (1 - output.coords.x) * sin(output.coords.y * PI)
    ) * s2Alpha;
    return output.color * alpha;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}