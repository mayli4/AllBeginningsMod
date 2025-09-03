float4 outlineColor = float4(0.0, 0.0, 0.0, 1.0);
float outlineWidth = 0.1;
float outlineSmooth = 0.0;
bool blackAsAlpha = false;

matrix transformationMatrix;
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
    float2 textureCoordinates : TEXCOORD;
    float4 color : COLOR;
};

struct VSOutput
{
    float4 position : POSITION;
    float2 textureCoordinates : TEXCOORD;
    float4 color : COLOR;
};

VSOutput vertex_shader_function(VSInput input)
{
    VSOutput output;
    output.color = input.color;
    output.position = mul(input.position, transformationMatrix);
    output.textureCoordinates = input.textureCoordinates;
    
    return output;
}

float4 pixel_shader_function(VSOutput input) : COLOR0
{
    float max = 1.0 - outlineWidth;
    float intensity = smoothstep(max - outlineSmooth, max, abs((input.textureCoordinates.y - 0.5) * 2.0));
    
    float4 sampleColor = tex2D(samplerTexture, input.textureCoordinates);
    float4 middleColor = input.color * (blackAsAlpha ? sampleColor.r : sampleColor);
    
    return lerp(middleColor, outlineColor, intensity);
}

technique technique1
{
    pass pass1
    {
        VertexShader = compile vs_2_0 vertex_shader_function();
        PixelShader = compile ps_2_0 pixel_shader_function();
    }
}