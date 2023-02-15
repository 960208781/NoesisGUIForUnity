struct In
{
    float4 pos: SV_POSITION;
    half4 color : COLOR;
    float2 uv : TEXCOORD;
    float4 rect : RECT;
    float4 imagePos: IMAGE_POSITION;
};

Texture2D inputTexture: register(t0);
SamplerState inputSampler: register(s0);

static float4 _i_color_;
static float2 _i_uv_;
static float4 _i_rect_;
static float4 _i_imagePos_;

// Returns the current input coordinate. As the effect may be generated inside an texture atlas,
// shaders shouldn't take any dependencies on how this value is calculated. It should use it only
// to the pixel shader's input. For rest of cases GetNormalizedInputCoordinate is recommended
float2 GetInputCoordinate() { return _i_uv_; }

/// Returns the current normalized input coordinates in the range 0 to 1
float2 GetNormalizedInputCoordinate() { return float2((_i_uv_.x - _i_rect_.x) / (_i_rect_.z - _i_rect_.x), (_i_uv_.y - _i_rect_.y) / (_i_rect_.w - _i_rect_.y)); }

// Returns the current image position in pixels
float2 GetImagePosition() { return _i_imagePos_.xy; }

// Returns the color at the current input coordinates
float4 GetInput() { return inputTexture.Sample(inputSampler, _i_uv_); }

// Samples input at position uv
float4 SampleInput(float2 uv) { return inputTexture.Sample(inputSampler, uv); }

// Samples input at an offset in pixels from the input coordinate
float4 SampleInputAtOffset(float2 offset) { return SampleInput(clamp(_i_uv_ + offset * _i_imagePos_.zw, _i_rect_.xy, _i_rect_.zw)); }

// Samples input at an absolute scene position in pixels
float4 SampleInputAtPosition(float2 pos) { return SampleInputAtOffset(pos - _i_imagePos_.xy); }

float4 GetCustomEffect();

float4 main(in In i): SV_TARGET
{
    _i_color_ = i.color;
    _i_uv_ = i.uv;
    _i_rect_ = i.rect;
    _i_imagePos_ = i.imagePos;

    return GetCustomEffect() * i.color.a;
}
