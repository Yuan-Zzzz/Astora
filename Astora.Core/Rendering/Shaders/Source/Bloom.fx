#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

// 参数
float Threshold;   // 亮度阈值 (0.0 - 1.0)
float Intensity;   // 辉光强度
float2 TextureSize; // 纹理尺寸 (用于计算像素偏移)

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

// =========================================================
// 阶段 1: 提取亮部 (Extract)
// =========================================================
float4 ExtractPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);
    
    // 计算亮度 (Luminance)
    float brightness = dot(color.rgb, float3(0.299, 0.587, 0.114));
    
    // 如果亮度大于阈值，保留；否则变黑
    if(brightness > Threshold)
        return color * Intensity;
    else
        return float4(0, 0, 0, 0);
}

// =========================================================
// 阶段 2: 高斯模糊 (Gaussian Blur)
// =========================================================
// 简单的 9-tap 高斯模糊权重
static const float Weights[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };

float4 BlurPS(VertexShaderOutput input, float2 direction) : COLOR
{
    float2 texOffset = 1.0 / TextureSize;
    
    // 中心像素
    float4 result = tex2D(SpriteTextureSampler, input.TextureCoordinates) * Weights[0];
    
    // 两侧采样
    for(int i = 1; i < 5; ++i)
    {
        float2 offset = direction * texOffset * i * 1.5; // 1.5 是扩散系数，让模糊更宽
        result += tex2D(SpriteTextureSampler, input.TextureCoordinates + offset) * Weights[i];
        result += tex2D(SpriteTextureSampler, input.TextureCoordinates - offset) * Weights[i];
    }
    
    return result * input.Color;
}

float4 BlurHorizontalPS(VertexShaderOutput input) : COLOR
{
    return BlurPS(input, float2(1, 0));
}

float4 BlurVerticalPS(VertexShaderOutput input) : COLOR
{
    return BlurPS(input, float2(0, 1));
}

// =========================================================
// Techniques
// =========================================================

technique Extract
{
	pass P0 { PixelShader = compile PS_SHADERMODEL ExtractPS(); }
};

technique BlurH
{
	pass P0 { PixelShader = compile PS_SHADERMODEL BlurHorizontalPS(); }
};

technique BlurV
{
	pass P0 { PixelShader = compile PS_SHADERMODEL BlurVerticalPS(); }
};