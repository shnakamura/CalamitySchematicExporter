sampler uImage0 : register(s0);

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    
    return float4(1.0 - color.rgb, color.a);
}

technique Technique1 
{
    pass InvertPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}