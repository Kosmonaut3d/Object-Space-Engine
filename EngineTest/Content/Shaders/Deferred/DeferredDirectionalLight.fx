﻿float4x4 ViewProjection;
//color of the light 
float3 lightColor;
//position of the camera, for specular light
float3 cameraPosition = float3(0,0,0);
//this is used to compute the world-position
float4x4 InvertViewProjection;
float4x4 LightViewProjection;

float3 LightVector;
//control the brightness of the light
float lightIntensity = 1.0f;

// diffuse color, and specularIntensity in the alpha channel
Texture2D AlbedoMap;
// normals, and specularPower in the alpha channel
Texture2D NormalMap;
      
//depth
Texture2D DepthMap;

Texture2D ShadowMap;
Texture2D SSShadowMap;

int ShadowFiltering = 0; //PCF, Poisson, VSM

float ShadowMapSize = 2048;
float DepthBias = 0.02;

#include "helper.fx"
       

SamplerState pointSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP; 
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
  
SamplerState linearSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

SamplerState ShadowSampler
{
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  STRUCT DEFINITIONS

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 viewDir : TEXCOORD1;
};

struct VertexShaderBasicOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelShaderOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
};


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  FUNCTION DEFINITIONS

 //  DEFAULT LIGHT SHADER FOR MODELS
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    //align texture coordinates
    output.TexCoord = input.TexCoord;
    output.viewDir = normalize(mul(output.Position, InvertViewProjection).xyz);
    return output;

}

VertexShaderBasicOutput VertexShaderBasicFunction(VertexShaderInput input)
{
    VertexShaderBasicOutput output;
    output.Position = float4(input.Position, 1);
    //align texture coordinates
    output.TexCoord = input.TexCoord;
    return output;
}

PixelShaderOutput PixelShaderUnshadowedFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(pointSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    [branch]
    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        output.Diffuse = float4(0, 0, 0, 0);
        output.Specular = float4(0, 0, 0, 0);
        return output;
    }
    else
    {
    //get metalness
        float roughness = normalData.a;
    //get specular intensity from the AlbedoMap
        float4 color = AlbedoMap.Sample(pointSampler, texCoord);

        float metalness = decodeMetalness(color.a);
    
        float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

        float3 cameraDirection = -normalize(input.viewDir);

        float NdL = saturate(dot(normal, -LightVector));

        float3 diffuse = DiffuseOrenNayar(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    
        float3 specular = SpecularCookTorrance(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);

        output.Diffuse = float4(diffuse, 0) * (1 - f0) * 0.01f;
        output.Specular = float4(specular, 0) * 0.01f;

        return output;
    }
}

float CalcShadowTermPCF(float light_space_depth, float ndotl, float2 shadow_coord)
{
    float shadow_term = 0;

    //float2 v_lerps = frac(ShadowMapSize * shadow_coord);

    float variableBias = clamp(0.001 * tan(acos(ndotl)), 0, DepthBias);

    //safe to assume it's a square
    float size = 1 / ShadowMapSize;
    	
    float samples[5];
    samples[0] = (light_space_depth - variableBias < 1-ShadowMap.SampleLevel(pointSampler, shadow_coord, 0).r);
    samples[1] = (light_space_depth - variableBias < 1 - ShadowMap.SampleLevel(pointSampler, shadow_coord + float2(size, 0), 0).r);
    samples[2] = (light_space_depth - variableBias < 1 - ShadowMap.SampleLevel(pointSampler, shadow_coord + float2(0, size), 0).r);
    samples[3] = (light_space_depth - variableBias < 1 - ShadowMap.SampleLevel(pointSampler, shadow_coord - float2(size, 0), 0).r);
    samples[4] = (light_space_depth - variableBias < 1 - ShadowMap.SampleLevel(pointSampler, shadow_coord - float2(0, size), 0).r);

    shadow_term = (samples[0] + samples[1] + samples[2] + samples[3] + samples[4]) / 5.0;
    //shadow_term = lerp(lerp(samples[0],samples[1],v_lerps.x),lerp(samples[2],samples[3],v_lerps.x),v_lerps.y);

    return shadow_term;
}

float random(float4 seed4)
{
    float dot_product = dot(seed4, float4(12.9898, 78.233, 45.164, 94.673));
    return frac(sin(dot_product) * 43758.5453);
}

float CalcShadowPoisson(float light_space_depth, float ndotl, float2 shadow_coord, float2 texCoord)
{
    float shadow_term = 0;

    const float2 poissonDisk[] =
    {
    float2(0.1908291f, 0.1823764f),
    float2(0.4236465f, 0.76107f),
    float2(-0.3056469f, 0.5557697f),
    float2(-0.4979181f, 0.1770361f),
    float2(0.4962559f, -0.2154941f),
    float2(0.6897131f, 0.4324413f),
    float2(-0.3782056f, -0.3405231f),
    float2(0.04382932f, -0.2403435f),
    float2(0.886423f, -0.05176726f),
    float2(0.4599024f, -0.6679791f),                       
    float2(-0.8389286f, -0.4176486f),
    float2(-0.9797052f, -0.0152119f),
    float2(-0.2747172f, -0.7914276f),
    float2(-0.7316247f, 0.6114004f),
    float2(-0.220655f, 0.9378002f),
    float2(0.1389218f, -0.8920172f)};

       //float2 v_lerps = frac(ShadowMapSize * shadow_coord);

    float variableBias = clamp(0.001 * tan(acos(ndotl)), 0, DepthBias);

    float sampleDepth = light_space_depth - variableBias;
    	//safe to assume it's a square
    float size = 1 / ShadowMapSize;

    const uint j = 16;
    [unroll]
    for (uint i = 0; i < 4; i++)
    {
        int index = int(16.0 * random(float4(texCoord.xyy, i))) % j;

        shadow_term += (light_space_depth - variableBias < 1 - ShadowMap.SampleLevel(pointSampler, shadow_coord + poissonDisk[index] * size, 0).r);
    }

    shadow_term /= 4.0f;

    return shadow_term;
}

//ChebyshevUpperBound
float CalcShadowVSM(float distance, float2 texCoord)
{
		// We retrive the two moments previously stored (depth and depth*depth)
    float2 moments = 1 - ShadowMap.SampleLevel(linearSampler, texCoord, 0).rg;
		
		// Surface is fully lit. as the current fragment is before the light occluder
    //if (distance <= moments.x)
    //    return 1.0;

		// The fragment is either in shadow or penumbra. We now use chebyshev's upperBound to check
		// How likely this pixel is to be lit (p_max)
    float variance = moments.y - (moments.x * moments.x);
    variance = max(variance, 0.0002);
	
    float d = distance - moments.x;
    float p_max = variance / (variance + d * d);
	
    return p_max;
}


float4 PixelShaderScreenSpaceShadowFunction(VertexShaderOutput input) : SV_Target
{
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(pointSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        return float4(0, 1, 0, 0);
    }
    else
    {
        float NdL = saturate(dot(normal, -LightVector));

        float depthVal = 1 - DepthMap.Sample(pointSampler, texCoord).r;

        //float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
        float2 ScreenPosition = texCoord * 2 - float2(1, 1);
        ScreenPosition.y = -ScreenPosition.y;

        float4 position;
        position.xy = ScreenPosition;
        position.z = depthVal;
        position.w = 1.0f;
    //transform to world space
        position = mul(position, InvertViewProjection);
        position /= position.w;

        float4 positionInLS = mul(position, LightViewProjection);
        float depthInLS = (positionInLS.z / positionInLS.w);

        float2 ShadowTexCoord = mad(positionInLS.xy / positionInLS.w, 0.5f, float2(0.5f, 0.5f));
        ShadowTexCoord.y = 1 - ShadowTexCoord.y;
        //float depthInSM = 1-ShadowMap.Sample(pointSampler, ShadowTexCoord);
        float shadowContribution;
     
        [branch]
        if(NdL>0)
        {
            [branch]
            if(ShadowFiltering == 0)
            {
                shadowContribution = CalcShadowTermPCF(depthInLS, NdL, ShadowTexCoord);
            }
            else if(ShadowFiltering == 1)
            {
                shadowContribution = CalcShadowPoisson(depthInLS, NdL, ShadowTexCoord, texCoord);
            }
            else
            {
                float3 lightVector = LightVector;
                lightVector.z = -LightVector.z;
                shadowContribution = CalcShadowVSM(depthInLS, ShadowTexCoord);
            }
        }
        else
        {
            shadowContribution = 0;
        }

        return float4(0, shadowContribution, 0, 0);
    }
}

//No screen space shadows - we need to calculate them together with the lighting
PixelShaderOutput PixelShaderShadowedFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(pointSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        output.Diffuse = float4(0, 0, 0, 0);
        output.Specular = float4(0, 0, 0, 0);
        return output;
    }
    else
    {
        float NdL = saturate(dot(normal, -LightVector));

        float depthVal = 1 - DepthMap.Sample(pointSampler, texCoord).r;

        //float2 texCoord = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1);
        float2 ScreenPosition = texCoord*2 - float2(1,1);
        ScreenPosition.y = -ScreenPosition.y;

        float4 position;
        position.xy = ScreenPosition;
        position.z = depthVal;
        position.w = 1.0f;
    //transform to world space
        position = mul(position, InvertViewProjection);
        position /= position.w;

        float4 positionInLS = mul(position, LightViewProjection);
        float depthInLS = (positionInLS.z / positionInLS.w);

        float2 ShadowTexCoord = mad(positionInLS.xy / positionInLS.w, 0.5f, float2(0.5f, 0.5f));
        ShadowTexCoord.y = 1 - ShadowTexCoord.y;
        //float depthInSM = 1-ShadowMap.Sample(pointSampler, ShadowTexCoord);
        float shadowContribution;

        [branch]
        if (NdL > 0)
        {
            [branch]
            if (ShadowFiltering == 0)
            {
                shadowContribution = CalcShadowTermPCF(depthInLS, NdL, ShadowTexCoord);
            }
            else if (ShadowFiltering == 1)
            {
                shadowContribution = CalcShadowPoisson(depthInLS, NdL, ShadowTexCoord, texCoord);
            }
            else
            {
                float3 lightVector = LightVector;
                lightVector.z = -LightVector.z;
                shadowContribution = CalcShadowVSM(depthInLS, ShadowTexCoord);
            }
        }
        else
        {
            shadowContribution = 0;
        }

    //get metalness
        float roughness = normalData.a;
    //get specular intensity from the AlbedoMap
        float4 color = AlbedoMap.Sample(pointSampler, texCoord);

        float metalness = decodeMetalness(color.a);
    
        float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

        float3 cameraDirection = -normalize(input.viewDir);

        float3 diffuse = float3(0,0,0);
        float3 specular = float3(0, 0, 0);

        [branch]
        if(shadowContribution > 0)
        {
            diffuse = DiffuseOrenNayar(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    
            specular = SpecularCookTorrance(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
        }

        output.Diffuse = float4(diffuse, 0) * (1 - f0) * 0.01f * shadowContribution;
        output.Specular = float4(specular, 0) * 0.01f * shadowContribution;

        return output;
    }
}
            
//This one is used when we have screen space shadows already
PixelShaderOutput PixelShaderSSShadowedFunction(VertexShaderOutput input)
{
    PixelShaderOutput output;
    float2 texCoord = float2(input.TexCoord);
    
    //get normal data from the NormalMap
    float4 normalData = NormalMap.Sample(pointSampler, texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = decode(normalData.xyz); //2.0f * normalData.xyz - 1.0f;    //could do mad

    if (normalData.x + normalData.y <= 0.001f) //Out of range
    {
        output.Diffuse = float4(0, 0, 0, 0);
        output.Specular = float4(0, 0, 0, 0);
        return output;
    }
    else
    {     
        float NdL = saturate(dot(normal, -LightVector));

        float shadowContribution = SSShadowMap.Sample(ShadowSampler, texCoord).g;

        //get metalness
        float roughness = normalData.a;
        //get specular intensity from the AlbedoMap
        float4 color = AlbedoMap.Sample(pointSampler, texCoord);

        float metalness = decodeMetalness(color.a);
    
        float f0 = lerp(0.04f, color.g * 0.25 + 0.75, metalness);

        float3 cameraDirection = -normalize(input.viewDir);


        float3 diffuse = float3(0, 0, 0);
        float3 specular = float3(0, 0, 0);

        [branch]
        if (shadowContribution > 0)
        {
            diffuse = DiffuseOrenNayar(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, roughness); //NdL * lightColor.rgb;
    
            specular = SpecularCookTorrance(NdL, normal, -LightVector, cameraDirection, lightIntensity, lightColor, f0, roughness);
        }

        output.Diffuse = float4(diffuse, 0) * (1 - f0) * 0.01f * shadowContribution;
        output.Specular = float4(specular, 0) * 0.01f * shadowContribution;

        return output;
    }
}



technique ShadowOnly
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderBasicFunction();
        PixelShader = compile ps_5_0 PixelShaderScreenSpaceShadowFunction();
    }
}

technique Unshadowed
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderUnshadowedFunction();
    }
}

technique Shadowed
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderShadowedFunction();
    }
}

technique SSShadowed
{
    pass Pass1
    {
        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderSSShadowedFunction();
    }
}