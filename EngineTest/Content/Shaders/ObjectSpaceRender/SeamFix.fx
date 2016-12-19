Texture2D BaseTexture;
SamplerState texSampler
{
    Texture = <ScreenTexture>; 

	MagFilter = POINT;
	MinFilter = POINT;
	Mipfilter = POINT;

	AddressU = CLAMP;
	AddressV = CLAMP;
};

float InverseResolution;
float Steps = 1;

struct VertexShaderStruct
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
}; 

VertexShaderStruct VertexShaderFunction(VertexShaderStruct input)
{
	VertexShaderStruct output;
	output.Position = input.Position;
	output.TexCoord = input.TexCoord;
	return output;
}

float4 FlatPS(VertexShaderStruct input) : SV_TARGET0
{
	//loop through the mips until we find one

	float4 color = BaseTexture.SampleLevel(texSampler, input.TexCoord, 0);

	const float2 offsets[] = {
		float2(0,1),
		float2(1,0),
		float2(0,-1),
		float2(-1,0)
	};

	if (color.a > 0) return color;

	[loop]
	for(float i = 0; i<Steps * 4; i++)
	{
		float j = i % 4;
		float multiplier = floor(i / 4) + 1;
		//float multiplier2 = multiplier > 2 ? 2 : 1;
		float2 offset = offsets[j] * InverseResolution * multiplier; //* multiplier2;
		color = BaseTexture.SampleLevel(texSampler, input.TexCoord + offset, 0);

		if (color.a > 0) break;
	}

	return color;
}


technique Flat
{
	pass Flat
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 FlatPS();
	}
}