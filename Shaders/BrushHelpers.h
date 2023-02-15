#if PAINT_LINEAR || PAINT_RADIAL || PAINT_PATTERN
    #define HAS_UV0 1
#endif

#if CLAMP_PATTERN
    #define HAS_RECT 1
#endif

#if REPEAT_PATTERN || MIRRORU_PATTERN || MIRRORV_PATTERN || MIRROR_PATTERN
    #define HAS_RECT 1
    #define HAS_TILE 1
#endif

#if EFFECT_SDF || EFFECT_SDF_LCD
    #define HAS_UV1 1
    #define HAS_ST1 1
    #define SDF_SCALE 7.96875
    #define SDF_BIAS 0.50196078431
    #define SDF_AA_FACTOR 0.65
    #define SDF_BASE_MIN 0.125
    #define SDF_BASE_MAX 0.25
    #define SDF_BASE_DEV -0.65
#endif

#if EFFECT_OPACITY
    #define HAS_UV1 1
#endif

#if EFFECT_SHADOW
    #define HAS_UV1 1
    #define HAS_RECT 1
#endif

#if EFFECT_BLUR
    #define HAS_UV1 1
#endif

#if EFFECT_DOWNSAMPLE
    #define HAS_UV0 1
    #define HAS_UV1 1
    #define HAS_UV2 1
    #define HAS_UV3 1
#endif

#if EFFECT_UPSAMPLE
    #define PAINT_SOLID 1
    #define HAS_UV0 1
    #define HAS_UV1 1
#endif

Texture2D pattern: register(t0);
SamplerState patternSampler: register(s0);

Texture2D ramps: register(t1);
SamplerState rampsSampler: register(s1);

Texture2D image: register(t2);
SamplerState imageSampler: register(s2);

Texture2D glyphs: register(t3);
SamplerState glyphsSampler: register(s3);

Texture2D shadow: register(t4);
SamplerState shadowSampler: register(s4);

cbuffer Buffer0: register(b0)
{
#if EFFECT_RGBA
    float4 rgba;
#endif

#if PAINT_LINEAR || PAINT_PATTERN
    float opacity;
#endif

#if PAINT_RADIAL
    float4 radialGrad[2];
#endif
}

#if EFFECT_BLUR
cbuffer Buffer2: register(b1)
{
    float blend;
}
#endif

#if EFFECT_SHADOW
cbuffer Buffer2: register(b1)
{
    float4 shadowColor;
    float2 shadowOffset;
    float blend;
}
#endif

struct In
{
    float4 position: SV_POSITION;

#if PAINT_SOLID
    nointerpolation half4 color: COLOR;
#endif

#if HAS_UV0
    float2 uv0: TEXCOORD0;
#endif

#if HAS_UV1
    float2 uv1: TEXCOORD1;
#endif

#if HAS_UV2
    float2 uv2: TEXCOORD2;
#endif

#if HAS_UV3
    float2 uv3: TEXCOORD3;
#endif

#if HAS_ST1
    float4 st1: TEXCOORD2;
#endif

#if EFFECT_PATH_AA
    half coverage: COVERAGE;
#endif

#if HAS_RECT
    nointerpolation float4 rect: RECT;
#endif

#if HAS_TILE
    nointerpolation float4 tile: TILE;
#endif
};

struct Out
{
    half4 color: SV_TARGET0;

#if EFFECT_SDF_LCD
    half4 alpha: SV_TARGET1;
#endif
};

half4 GetCustomPattern(in In i);

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Out main(in In i)
{
    /////////////////////////////////////////////////////
    // Fetch paint color and opacity
    /////////////////////////////////////////////////////
    #if PAINT_SOLID
        half4 paint = i.color;
        half opacity_ = 1.0;

    #elif PAINT_LINEAR
        half4 paint = ramps.Sample(rampsSampler, i.uv0);
        half opacity_ = opacity;

    #elif PAINT_RADIAL
        half dd = radialGrad[1].x * i.uv0.x - radialGrad[1].y * i.uv0.y;
        half u = radialGrad[0].x * i.uv0.x + radialGrad[0].y * i.uv0.y + radialGrad[0].z *
            sqrt(i.uv0.x * i.uv0.x + i.uv0.y * i.uv0.y - dd * dd);
        half4 paint = ramps.Sample(rampsSampler, half2(u, radialGrad[1].z));
        half opacity_ = radialGrad[0].w;

    #elif PAINT_PATTERN
        #if CUSTOM_PATTERN
            half4 paint = GetCustomPattern(i);
        #elif CLAMP_PATTERN
            float inside = all(i.uv0 == clamp(i.uv0, i.rect.xy, i.rect.zw));
            half4 paint = inside * pattern.Sample(patternSampler, i.uv0);
        #elif REPEAT_PATTERN || MIRRORU_PATTERN || MIRRORV_PATTERN || MIRROR_PATTERN
            float2 uv = (i.uv0 - i.tile.xy) / i.tile.zw;
            #if REPEAT_PATTERN
                uv = frac(uv);
            #elif MIRRORU_PATTERN
                uv.x = abs(uv.x - 2.0 * floor((uv.x - 1.0) / 2.0) - 2.0);
                uv.y = frac(uv.y);
            #elif MIRRORV_PATTERN
                uv.x = frac(uv.x);
                uv.y = abs(uv.y - 2.0 * floor((uv.y - 1.0) / 2.0) - 2.0);
            #else 
                uv = abs(uv - 2.0 * floor((uv - 1.0) / 2.0) - 2.0);
            #endif
            uv = uv * i.tile.zw + i.tile.xy;
            float inside = all(uv == clamp(uv, i.rect.xy, i.rect.zw));
            half4 paint = inside * pattern.SampleGrad(patternSampler, uv, ddx(i.uv0), ddy(i.uv0));
        #else
            half4 paint = pattern.Sample(patternSampler, i.uv0);
        #endif
        half opacity_ = opacity;
    #endif

    Out o;

    /////////////////////////////////////////////////////
    // Apply selected effect
    /////////////////////////////////////////////////////
    #if EFFECT_RGBA
        o.color = rgba;

    #elif EFFECT_MASK
        o.color = half4(1, 1, 1, 1);

    #elif EFFECT_CLEAR
        o.color = half4(0, 0, 0, 0);

    #elif EFFECT_PATH
        o.color = opacity_ * paint;

    #elif EFFECT_PATH_AA
        o.color = (opacity_ * i.coverage) * paint;

    #elif EFFECT_OPACITY
        o.color = image.Sample(imageSampler, i.uv1) * (opacity_ * paint.a);

    #elif EFFECT_SHADOW
        half2 uv = clamp(i.uv1 - shadowOffset, i.rect.xy, i.rect.zw);
        half alpha = lerp(image.Sample(imageSampler, uv).a, shadow.Sample(shadowSampler, uv).a, blend);
        half4 img = image.Sample(imageSampler, clamp(i.uv1, i.rect.xy, i.rect.zw));
        o.color = (img + (1.0 - img.a) * (shadowColor * alpha)) * paint.a;

    #elif EFFECT_BLUR
        o.color = lerp(image.Sample(imageSampler, i.uv1), shadow.Sample(shadowSampler, i.uv1), blend) * paint.a;

    #elif EFFECT_SDF
        half4 color = glyphs.Sample(glyphsSampler, i.uv1);
        half distance = SDF_SCALE * (color.r - SDF_BIAS);

        #if 1
            half2 grad = ddx(i.st1.xy);
        #else
            // For non-uniform scale or perspective this is the correct code. It is much more complex than the isotropic
            // case and probably not worth it
            // http://www.essentialmath.com/GDC2015/VanVerth_Jim_DrawingAntialiasedEllipse.pdf
            // https://www.essentialmath.com/blog/?p=151
            half2 Jdx = ddx(i.st1);
            half2 Jdy = ddy(i.st1);
            half2 distGrad = half2(ddx(distance), ddy(distance));
            half distGradLen2 = dot(distGrad, distGrad);
            distGrad = distGradLen2 < 0.0001 ? half2(0.7071, 0.7071) : distGrad * half(rsqrt(distGradLen2));
            half2 grad = half2(distGrad.x * Jdx.x + distGrad.y * Jdy.x, distGrad.x * Jdx.y + distGrad.y * Jdy.y);
        #endif

        half gradLen = (half)length(grad);
        half scale = 1.0 / gradLen;
        half base = SDF_BASE_DEV * (1.0 - (clamp(scale, SDF_BASE_MIN, SDF_BASE_MAX) - SDF_BASE_MIN) / (SDF_BASE_MAX - SDF_BASE_MIN));
        half range = SDF_AA_FACTOR * gradLen;
        half alpha = smoothstep(base - range, base + range, distance);

        o.color = (alpha * opacity_) * paint;

    #elif EFFECT_SDF_LCD
        half2 grad = ddx(i.st1.xy);
        half2 offset = grad * i.st1.zw;

        half4 red = glyphs.Sample(glyphsSampler, i.uv1 - offset);
        half4 green = glyphs.Sample(glyphsSampler, i.uv1);
        half4 blue = glyphs.Sample(glyphsSampler, i.uv1 + offset);
        half3 distance = SDF_SCALE * (half3(red.r, green.r, blue.r) - SDF_BIAS);

        half gradLen = (half)length(grad);
        half scale = 1.0 / gradLen;
        half base = SDF_BASE_DEV * (1.0 - (clamp(scale, SDF_BASE_MIN, SDF_BASE_MAX) - SDF_BASE_MIN) / (SDF_BASE_MAX - SDF_BASE_MIN));
        half range = SDF_AA_FACTOR * gradLen;
        half3 alpha = smoothstep(base - range, base + range, distance);

        o.color = half4(opacity_ * paint.rgb * alpha.rgb, alpha.g);
        o.alpha = half4((opacity_ * paint.a) * alpha.rgb, alpha.g);

    #elif EFFECT_DOWNSAMPLE
        o.color = 
        (
            pattern.Sample(patternSampler, i.uv0) +
            pattern.Sample(patternSampler, i.uv1) +
            pattern.Sample(patternSampler, i.uv2) +
            pattern.Sample(patternSampler, i.uv3)
        ) * 0.25;

    #elif EFFECT_UPSAMPLE
        o.color = lerp(image.Sample(imageSampler, i.uv1), pattern.Sample(patternSampler, i.uv0), paint.a);

    #else
        #error EFFECT not defined
    #endif

    return o;
}
