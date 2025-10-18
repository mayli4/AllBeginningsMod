#define MAX_METABALLS 64

struct metaball {
    float2 position; // world coords
    float radius;    // world coords
    float padding;
};

uniform float4 metaballData[MAX_METABALLS];
uniform int metaballCount;
uniform float smoothness;

uniform float2 screenPos;
uniform float2 worldViewDimensions;

float sdCircle(float2 worldPixelCoords, metaball ball) {
    return length(worldPixelCoords - ball.position) - ball.radius;
}

float smin(float a, float b, float k) {
    float h = max(k - abs(a - b), 0.0) / k;
    return min(a, b) - h * h * h * k * (1.0 / 6.0);
}

float4 PixelShaderFunction(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
    float totalDist = 99999.0;

    float2 worldPixelPosition = screenPos + uv * worldViewDimensions;

    for (int i = 0; i < metaballCount; i++) {
        metaball ball = (metaball)metaballData[i]; 

        float dist = sdCircle(worldPixelPosition, ball);

        totalDist = smin(totalDist, dist, smoothness);
    }

    if (totalDist < 0.0) {
        return float4(1.0, 1.0, 1.0, 1.0);
    }

    return float4(0.0, 0.0, 0.0, 0.0);
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}