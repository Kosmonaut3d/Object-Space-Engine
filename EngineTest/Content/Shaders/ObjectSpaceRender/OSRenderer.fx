//Lightshader Bounty Road 2016

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  VARIABLES
////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "helper.fx"

//Actually World
float4x4  WorldView;
float4x4  WorldViewProj;

//actually world IT
float3x3  WorldViewIT; //Inverse Transposed

float3 Camera;
float2 Resolution;
float FarClip = 200;

float Roughness = 0.3f;
float Metallic = 0;
int MaterialType = 0;

const float CLIP_VALUE = 0.99;

float4 DiffuseColor = float4(0.8f, 0.8f, 0.8f, 1);

Texture2D<float4> Texture;

Texture2D<float4> NormalMap;

Texture2D<float4> MetallicMap;

Texture2D<float4> RoughnessMap;

Texture2D<float4> DisplacementMap;

Texture2D<float4> Mask;

float lightIntensity = 7;
float3 lightColor = float3(1, 1, 0.9f);

float EnvironmentIntensity = 1.8f;
Texture2D EnvironmentMap;
SamplerState EnvironmentMapSampler
{
	MinFilter = none;
	MagFilter = none;
	MipFilter = none;

	AddressU = Clamp;
	AddressV = Clamp;
};

sampler TextureSampler
{
	Texture = (Texture);
	Filter = Anisotropic;
	MaxAnisotropy = 8;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler PointSampler
{
	Texture = (Texture);
	Filter = Anisotropic;
	MaxAnisotropy = 8;
	AddressU = Clamp;
	AddressV = Clamp;
};

sampler TextureSamplerTrilinear
{
	Texture = (NormalMap);
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct DrawBasicMesh_VS
{
	float4 Position : SV_POSITION0;
	float2 TexCoord : TEXCOORD0;
};


struct DrawBasic_VSIn
{
	float4 Position : SV_POSITION0;
	float3 Normal   : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct DrawBasic_VSOut
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
   // float Depth : TEXCOORD2;
};

struct DrawNormals_VSIn
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0;
    float3 Binormal : BINORMAL0;
    float3 Tangent : TANGENT0;
    float2 TexCoord : TEXCOORD0;
};

struct DrawNormals_VSOut
{
    float4 Position : SV_POSITION0;
    float3x3 WorldToTangentSpace : TEXCOORD3;
    float2 TexCoord : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
    //float Depth : TEXCOORD0;
};

struct Render_IN
{
    float4 Position : SV_POSITION0;
    float4 Color : COLOR0;
    float3 Normal : TEXCOORD0;
    //float2 Depth : DEPTH;
    float Metallic : TEXCOORD1;
    float Roughness : TEXCOORD2;
	float3 WorldPosition : TEXCOORD3;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

DrawBasicMesh_VS DrawBasicMesh_VertexShader(DrawBasicMesh_VS input)
{
	DrawBasicMesh_VS Output;
	Output.Position = mul(input.Position, WorldViewProj);
	Output.TexCoord = input.TexCoord;
	return Output;
}

DrawBasic_VSIn DrawSkybox_VertexShader(DrawBasic_VSIn input)
{
	DrawBasic_VSIn Output;
	//input.Position.z *= input.Position.z+0.5f;
	Output.Position = mul(input.Position, WorldViewProj);
	Output.Normal = mul(input.Normal, WorldViewIT);
	Output.TexCoord = input.TexCoord;
	return Output;
}

 //  DEFAULT LIGHT SHADER FOR MODELS
DrawBasic_VSOut DrawBasic_VertexShader(DrawBasic_VSIn input)
{
    DrawBasic_VSOut Output;
	Output.Position = float4(0, 0, 0, 1);
	Output.Position.x = input.TexCoord.x * 2.0f - 1.0f;
	Output.Position.y = -(input.TexCoord.y * 2.0f - 1.0f);
	float4(input.TexCoord, 0, 1); // mul(input.Position, WorldViewProj);
	Output.Normal = mul(input.Normal, WorldViewIT);//mul(float4(input.Normal, 0), World).xyz;
    Output.TexCoord = input.TexCoord;
	float4 WorldPos = mul(input.Position, WorldView);
	Output.WorldPosition = WorldPos.xyz / WorldPos.w;
	//Linear Depth buffer instead of Z / W
	//Output.Depth = Output.Position.z / Output.Position.w; // mul(input.Position, WorldView).z / -FarClip;//float2(Output.Position.z, Output.Position.w);
    return Output;
}

DrawNormals_VSOut DrawNormals_VertexShader(DrawNormals_VSIn input)
{
    DrawNormals_VSOut Output;
	Output.Position = float4(0,0,0,1);
	Output.Position.x = input.TexCoord.x * 2.0f - 1.0f;
	Output.Position.y = -(input.TexCoord.y * 2.0f - 1.0f);//mul(input.Position, WorldViewProj);
	Output.WorldToTangentSpace[0] = mul(input.Tangent, WorldViewIT);//mul(normalize(float4(input.Tangent, 0)), World).xyz;
    Output.WorldToTangentSpace[1] = mul(input.Binormal, WorldViewIT);//mul(normalize(float4(input.Binormal, 0)), World).xyz;
    Output.WorldToTangentSpace[2] = mul(input.Normal, WorldViewIT);//mul(normalize(float4(input.Normal, 0)), World).xyz;
    Output.TexCoord = input.TexCoord;
	float4 WorldPos = mul(input.Position, WorldView);
	Output.WorldPosition = WorldPos.xyz / WorldPos.w;

	//Linear Depth buffer instead of Z / W
	//Output.Depth = Output.Position.z / Output.Position.w;// mul(input.Position, WorldView).z / -FarClip;//float2(Output.Position.z, Output.Position.w);
    return Output;
}

float3 GetNormalMap(float2 TexCoord)
{
	//This gets normalized anyways, so it doesn't matter that it's technically only half the length
	return NormalMap.Sample(TextureSamplerTrilinear, TexCoord).rgb - float3(0.5f, 0.5f, 0.5f);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  LIGHTING
/////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 DrawBasicMesh_PixelShader(DrawBasicMesh_VS input) : COLOR
{
	return Texture.Sample(TextureSamplerTrilinear, input.TexCoord);
}

float4 DrawSkybox_PixelShader(DrawBasic_VSIn input) : COLOR
{
	float3 normal = normalize(input.Normal);
	float envMapCoord = 1 - saturate((normal.z + 1) / 2);
	float4 ambientSpecular = EnvironmentMap.SampleLevel(TextureSamplerTrilinear, float2(0, envMapCoord * 0.98f + 0.01f),0);
	ambientSpecular = pow(abs(ambientSpecular), 4.4f) * EnvironmentIntensity;

	return float4(pow(abs(ambientSpecular), 1 / 2.2f));
}

float4 Lighting(Render_IN input)
{
	float3 normal = input.Normal;
	float4 color = pow(abs(input.Color), 2.2f);
	float metalness = input.Metallic;
	float roughness = input.Roughness;

	float3 lightVector = normalize(float3(0, -1, 5));
	float3 cameraDirection = normalize(input.WorldPosition - Camera);

	float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

	float NdL = saturate(dot(normal, lightVector));
	float3 diffuseLight = 0;
	[branch]
	if (metalness < 0.99)
	{
		diffuseLight = DiffuseOrenNayar(NdL, normal, lightVector, -cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
	}
	float3 specularLight = SpecularCookTorrance(NdL, normal, lightVector, -cameraDirection, lightIntensity, lightColor, f0, roughness);

	diffuseLight = (diffuseLight * (1 - f0)); //* (1 - f0)) * (f0 + 1) * (f0 + 1);
	specularLight = specularLight;


	float envMapCoord = saturate((-normal.z + 1) / 2);

	float4 ambientDiffuse = float4(EnvironmentMap.Load(int3(127, envMapCoord * 128, 0), int2(0, 0)).rgb, 1); //EnvironmentMap.Sample(EnvironmentMapSampler, float2(1.0f, envMapCoord)); EnvironmentMap.Load(int3(1, envMapCoord * 128, 0), int2(0, 0)); // EnvironmentMap.SampleLevel(EnvironmentMapSampler, float2(-1, envMapCoord), 0)*10;
	float4 ambientSpecular = EnvironmentMap.Load(int3(input.Roughness * 128, envMapCoord * 128, 0), int2(0, 0)); //EnvironmentMap.Load(int3(0, envMapCoord * 128, 0), int2(0, 0));

	ambientDiffuse = pow(abs(ambientDiffuse), 2.2f) * EnvironmentIntensity;
	ambientSpecular = pow(abs(ambientSpecular), 4.4f) * EnvironmentIntensity;

	float strength = lerp(ambientSpecular.a * 2, 1, metalness);

	ambientSpecular = float4(ambientSpecular.rgb *strength, 1);



	float3 plasticFinal = color * (diffuseLight + ambientDiffuse) + specularLight; //ambientSpecular;

	float3 metalFinal = (specularLight+ambientSpecular) * color.rgb;

	float3 finalValue = lerp(plasticFinal, metalFinal, metalness);


	return float4(pow(abs(finalValue), 1/2.2f), 1);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////

[earlydepthstencil]      //experimental
float4 DrawTexture_PixelShader(DrawBasic_VSOut input) : COLOR0
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;
 
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    ////renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.Roughness = Roughness;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
float4 DrawTextureSpecular_PixelShader(DrawBasic_VSOut input) : COLOR0
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;

    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    //renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.Roughness = RoughnessTexture;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
float4 DrawTextureSpecularMetallic_PixelShader(DrawBasic_VSOut input) : COLOR0
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;
    float metallicTexture = MetallicMap.Sample(TextureSampler, input.TexCoord).r;

    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    //renderParams.Depth = input.Depth;
    renderParams.Metallic = metallicTexture;
    renderParams.Roughness = RoughnessTexture;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
float4 DrawTextureSpecularNormal_PixelShader(DrawNormals_VSOut input) : COLOR0
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;

    // NORMAL MAP ////
	float3 normalMap = GetNormalMap(input.TexCoord);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    //renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.Roughness = RoughnessTexture;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}

[earlydepthstencil]      //experimental
float4 DrawTextureSpecularNormalMetallic_PixelShader(DrawNormals_VSOut input) : COLOR0
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;
    float metallicTexture = MetallicMap.Sample(TextureSampler, input.TexCoord).r;
    // NORMAL MAP ////
    float3 normalMap = GetNormalMap(input.TexCoord);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    //renderParams.Depth = input.Depth;
    renderParams.Metallic = metallicTexture;
    renderParams.Roughness = RoughnessTexture;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}



[earlydepthstencil]      //experimental
float4 DrawTextureNormal_PixelShader(DrawNormals_VSOut input) : COLOR0
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    // NORMAL MAP ////
    float3 normalMap = GetNormalMap(input.TexCoord);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    //renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.Roughness = Roughness;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}

      //experimental
float4 DrawTextureMask_PixelShader(DrawBasic_VSOut input) : COLOR0
{
    Render_IN renderParams;

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float mask = Mask.Sample(TextureSampler, input.TexCoord).r;
    if (mask < CLIP_VALUE)
        clip(-1);
 
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    //renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.Roughness = Roughness;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}


float4 DrawTextureSpecularMask_PixelShader(DrawBasic_VSOut input) : COLOR0
{
    Render_IN renderParams;

	float mask = Mask.Sample(TextureSampler, input.TexCoord).r;
	if (mask < CLIP_VALUE)
	clip(-1);

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;

    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    //renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.Roughness = RoughnessTexture; // 1 - (RoughnessTexture.r+RoughnessTexture.b+RoughnessTexture.g) / 3;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}

      //experimental
float4 DrawTextureSpecularNormalMask_PixelShader(DrawNormals_VSOut input) : COLOR0
{
    Render_IN renderParams;

	float mask = Mask.Sample(TextureSampler, input.TexCoord).r;
	if (mask < CLIP_VALUE)
	clip(-1);

    float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
    float4 outputColor = textureColor; //* input.Color;

    float3x3 worldSpace = input.WorldToTangentSpace;

    float RoughnessTexture = RoughnessMap.Sample(TextureSampler, input.TexCoord).r;


    // NORMAL MAP ////
    float3 normalMap = GetNormalMap(input.TexCoord);
    normalMap = normalize(mul(normalMap, worldSpace));
  
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalMap;
    //renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.Roughness = RoughnessTexture;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}

    //experimental
float4 DrawTextureNormalMask_PixelShader(DrawNormals_VSOut input) : COLOR0
{
    Render_IN renderParams;

	float mask = Mask.Sample(TextureSampler, input.TexCoord).r;

	//Branching has shown to make no difference here
	if (mask < CLIP_VALUE)
	{
		clip(-1);
	}

	float4 textureColor = Texture.Sample(TextureSampler, input.TexCoord);
	float4 outputColor = textureColor; //* input.Color;

	float3x3 worldSpace = input.WorldToTangentSpace;

	// NORMAL MAP ////
	float3 normalMap = GetNormalMap(input.TexCoord);
	normalMap = normalize(mul(normalMap, worldSpace));

	renderParams.Position = input.Position;
	renderParams.Color = outputColor;
	renderParams.Normal = normalMap;
	//renderParams.Depth = input.Depth;
	renderParams.Metallic = Metallic;
	renderParams.Roughness = Roughness;
	renderParams.WorldPosition = input.WorldPosition;

	return Lighting(renderParams);
}

float4 DrawBasic_PixelShader(DrawBasic_VSOut input) : COLOR0
{
    Render_IN renderParams;

    float4 outputColor = DiffuseColor; //* input.Color;

         
    renderParams.Position = input.Position;
    renderParams.Color = outputColor;
    renderParams.Normal = normalize(input.Normal);
    //renderParams.Depth = input.Depth;
    renderParams.Metallic = Metallic;
    renderParams.Roughness = Roughness;
	renderParams.WorldPosition = input.WorldPosition;

    return Lighting(renderParams);
}

technique DrawBasicMesh
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 DrawBasicMesh_VertexShader();
		PixelShader = compile ps_5_0 DrawBasicMesh_PixelShader();
	}
}

technique DrawSkybox
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 DrawSkybox_VertexShader();
		PixelShader = compile ps_5_0 DrawSkybox_PixelShader();
	}
}

technique DrawBasic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawBasic_PixelShader();
    }
}

technique DrawTexture
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawTexture_PixelShader();
    }
}

technique DrawTextureSpecular
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecular_PixelShader();
    }
}

technique DrawTextureSpecularMetallic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecularMetallic_PixelShader();
    }
}

technique DrawTextureSpecularNormal
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecularNormal_PixelShader();
    }
}

technique DrawTextureSpecularNormalMetallic
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureSpecularNormalMetallic_PixelShader();
    }
}


technique DrawTextureNormal
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_5_0 DrawTextureNormal_PixelShader();
    }
}

technique DrawTextureMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_4_0 DrawTextureMask_PixelShader();
    }
}

technique DrawTextureSpecularMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawBasic_VertexShader();
        PixelShader = compile ps_4_0 DrawTextureSpecularMask_PixelShader();
    }
}

technique DrawTextureSpecularNormalMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_4_0 DrawTextureSpecularNormalMask_PixelShader();
    }
}

technique DrawTextureNormalMask
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 DrawNormals_VertexShader();
        PixelShader = compile ps_4_0 DrawTextureNormalMask_PixelShader();
    }
}
