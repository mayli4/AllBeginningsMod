float2 normalize_with_pixelation(float2 coords, float pixel_size, float2 resolution)
{
    return floor(coords / pixel_size) / (resolution / pixel_size);
}

/**
 * Quantizes the input color to a given resolution.
 *
 * @param color The input color.
 * @param color_resolution The resolution of quantization.
 * @return The quantized color.
 */
float3 quantize_color(float3 color, float3 color_resolution)
{
    return floor(color * color_resolution) / (color_resolution - 1.0f);
}

float4 quantize_color_with_alpha(float4 color, float4 color_resolution)
{
    return floor(color * color_resolution) / (color_resolution - 1.0f);
}