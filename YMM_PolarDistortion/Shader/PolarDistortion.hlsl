Texture2D InputTexture : register(t0);
SamplerState InputSampler : register(s0);

SamplerState CustomSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

cbuffer constants : register(b0)
{
    float transform : packoffset(c0);

    float2 scale : packoffset(c0.z);

    float2 offset : packoffset(c1);

    bool isPolarToRect : packoffset(c1.z);

    bool forPreOrPostProcess : packoffset(c1.w);
};

static float PI0_5 = 1.5707963267948966;

static float PI = 3.141592653589793;

static float PI1_5 = 4.71238898038469;

static float PI2 = 6.283185307179586;

static float PI3 = 9.42477796076938;

static float SQRT0_5 = 0.7071067811865476;

float2 toPolar(float2 pos, float2 scale, float2 offset, float transform)
{
    float2 invertTransform = (1.0 - transform) * pos;
    float2 dir = ((pos + offset) * scale) - float2(0.5, 0.5);
    float radius = length(dir) * 2.0;
    float rad = (atan2(dir.x, dir.y) + PI) / PI2;

    return float2(rad, radius) * transform + invertTransform;
}

float2 toPolarForPostProcess(float2 pos, float transform)
{
    float2 invertTransform = (1.0 - transform) * pos;
    float2 dir = pos - float2(0.5, 0.5);
    float radius = length(dir) / SQRT0_5;
    float rad = (atan2(dir.x, -dir.y) + PI1_5) / PI3;

    return float2(rad, radius) * transform + invertTransform;
}

float2 toRect(float2 pos, float2 scale, float2 offset, float transform)
{
    float2 invertTransform = (1.0 - transform) * pos;
    float t = pos.x * PI2 - PI;
    float c = cos(t);
    float s = sin(t);

    return ((float2(s, c) * SQRT0_5 * pos.y + float2(0.5, 0.5)) / scale - offset) * transform + invertTransform;
}

float2 toRectForPreProcess(float2 pos, float transform)
{
    float2 invertTransform = (1.0 - transform) * pos;
    float dir = pos.x - 0.5;
    float t = dir * PI3 - PI0_5;
    float c = cos(t);
    float s = sin(t);

    float2 result = (float2(c, s) * SQRT0_5 * pos.y + float2(0.5, 0.5)) * transform + invertTransform;
    return clamp(result, float2(0.0, 0.0), float2(1.0, 1.0));
}

float4 main(float4 pos : SV_POSITION, float4 posScene : SCENE_POSITION, float4 uv0 : TEXCOORD0) : SV_Target
{
    float2 uv;
    if (forPreOrPostProcess)
    {
        if (isPolarToRect)
        {
            uv = toRectForPreProcess(uv0.xy, transform);
        }
        else
        {
            uv = toPolarForPostProcess(uv0.xy, transform);
        }
    }
    else
    {
        if (isPolarToRect)
        {
            uv = toRect(uv0.xy, scale, offset, transform);
        }
        else
        {
            uv = toPolar(uv0.xy, scale, offset, transform);
        }
    }

    if (uv.x >= 0.0 && uv.x <= 1.0 && uv.y >= 0.0 && uv.y <= 1.0)
    {
        return InputTexture.Sample(CustomSampler, uv);
    }
    else
    {
        return float4(0.0, 0.0, 0.0, 0.0);
    }
}