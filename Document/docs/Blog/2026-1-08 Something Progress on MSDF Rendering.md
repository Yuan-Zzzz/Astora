# Something Progress on MSDF Rendering
This is mainly because when I was developing with MonoGame, there was a lack of libraries that offer good support for SDF text rendering. Therefore, I tried to build one myself.
Currently, some progress has been made: I can successfully render MSDF (Multi-channel Signed Distance Field) maps, and they can also be scaled up or down without loss of quality. 
This will serve as an underlying function of the engine, facilitating support for text rendering or other more exciting features in the future.

Here is the simple shader code I used for rendering MSDF maps:
``` c++
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_3
#define PS_SHADERMODEL ps_4_0_level_9_3
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float pxRange;
float2 textureSize;
float4 bgColor;
float4 fgColor;

float median(float r, float g, float b)
{
    return max(min(r, g), min(max(r, g), b));
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 coord = input.TextureCoordinates;
    float2 msdfUnit = pxRange / textureSize;
    float3 samp = (tex2D(SpriteTextureSampler, coord) * input.Color).rgb;
    float sigDist = median(samp.r, samp.g, samp.b) - 0.5;
    sigDist *= dot(msdfUnit, 0.5 / fwidth(coord));
    float opacity = clamp(sigDist + 0.5, 0, 1);
    return lerp(bgColor, fgColor, opacity);
}

technique SpriteDrawing{
    pass P0{
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
;
```