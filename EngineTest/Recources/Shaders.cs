using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public static class Shaders
    {
        //Test

        public static Effect NormalMappingEffect;

        //Id Generator
        public static Effect IdRenderEffect;
        public static EffectParameter IdRenderEffectParameterWorld;
        public static EffectParameter IdRenderEffectParameterWorldViewProj;
        public static EffectParameter IdRenderEffectParameterColorId;

        public static EffectPass IdRenderEffectDrawId;
        public static EffectPass IdRenderEffectDrawOutline;

        //Billboard Renderer

        public static Effect BillboardEffect;
        public static EffectParameter BillboardEffectParameter_WorldViewProj;
        public static EffectParameter BillboardEffectParameter_WorldView;
        public static EffectParameter BillboardEffectParameter_AspectRatio;
        public static EffectParameter BillboardEffectParameter_FarClip;
        public static EffectParameter BillboardEffectParameter_Texture;
        public static EffectParameter BillboardEffectParameter_DepthMap;
        public static EffectParameter BillboardEffectParameter_IdColor;

        public static EffectTechnique BillboardEffectTechnique_Billboard;
        public static EffectTechnique BillboardEffectTechnique_Id;

        //Lines
        public static Effect LineEffect;
        public static EffectParameter LineEffectParameter_WorldViewProj;

        //Temporal AntiAliasing

        public static Effect TemporalAntiAliasingEffect;

        public static EffectParameter TemporalAntiAliasingEffect_DepthMap;
        public static EffectParameter TemporalAntiAliasingEffect_AccumulationMap;
        public static EffectParameter TemporalAntiAliasingEffect_UpdateMap;
        public static EffectParameter TemporalAntiAliasingEffect_CurrentToPrevious;
        public static EffectParameter TemporalAntiAliasingEffect_Resolution;
        public static EffectParameter TemporalAntiAliasingEffect_FrustumCorners;
        public static EffectParameter TemporalAntiAliasingEffect_Threshold;

        //Vignette and CA

        public static Effect PostProcessing;

        public static EffectParameter PostProcessingParameter_ScreenTexture;
        public static EffectParameter PostProcessingParameter_ChromaticAbberationStrength;
        public static EffectParameter PostProcessingParameter_SCurveStrength;

        public static EffectTechnique PostProcessingTechnique_Vignette;
        public static EffectTechnique PostProcessingTechnique_VignetteChroma;
        
        //Gaussian Blur
        public static Effect GaussianBlurEffect;
        public static EffectParameter GaussianBlurEffectParameter_InverseResolution;
        public static EffectParameter GaussianBlurEffectParameter_TargetMap;

        //ClearGBuffer
        public static Effect ClearGBufferEffect;

        //GBuffer
        public static Effect GBufferEffect;

        public static EffectParameter GBufferEffectParameter_World;
        public static EffectParameter GBufferEffectParameter_WorldViewProj;
        public static EffectParameter GBufferEffectParameter_WorldIT;
        public static EffectParameter GBufferEffectParameter_Camera;
        public static EffectParameter GBufferEffectParameter_FarClip;

        public static EffectParameter GBufferEffectParameter_Material_Metallic;
        public static EffectParameter GBufferEffectParameter_Material_MetallicMap;
        public static EffectParameter GBufferEffectParameter_Material_DiffuseColor;
        public static EffectParameter GBufferEffectParameter_Material_Roughness;
        public static EffectParameter GBufferEffectParameter_Material_MaskMap;
        public static EffectParameter GBufferEffectParameter_Material_Texture;
        public static EffectParameter GBufferEffectParameter_Material_NormalMap;
        public static EffectParameter GBufferEffectParameter_Material_DisplacementMap;
        public static EffectParameter GBufferEffectParameter_Material_RoughnessMap;
        public static EffectParameter GBufferEffectParameter_Material_MaterialType;
        public static EffectParameter GBufferEffectParameter_Material_EnvironmentMap;
        public static EffectParameter GBufferEffectParameter_EnvironmentIntensity;

        public static EffectTechnique GBufferEffectTechniques_DrawTextureDisplacement;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularNormalMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureNormalMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureMask;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularNormalMetallic;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularNormal;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureNormal;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecular;
        public static EffectTechnique GBufferEffectTechniques_DrawTextureSpecularMetallic;
        public static EffectTechnique GBufferEffectTechniques_DrawTexture;
        public static EffectTechnique GBufferEffectTechniques_DrawBasic;
        public static EffectTechnique GBufferEffectTechniques_DrawBasicMesh;

        //SEAMFIX

        public static Effect SeamFixEffect;
        public static EffectParameter SeamFixBaseTexture;
        public static EffectParameter SeamFixInverseResolution;
        public static EffectParameter SeamFixSteps;
        //COMPOSE

        //SHADOW MAPPING

        public static Effect virtualShadowMappingEffect;
        public static EffectParameter virtualShadowMappingEffectParameter_WorldViewProj;
        public static EffectTechnique virtualShadowMappingEffect_Technique_Depth;
        public static EffectTechnique virtualShadowMappingEffect_Technique_VSM;

        public static void Load(ContentManager content)
        {
            NormalMappingEffect = content.Load<Effect>("Shaders/ObjectSpaceRender/NormalMapping");

            //SeamFix
            SeamFixEffect = content.Load<Effect>("Shaders/ObjectSpaceRender/SeamFix");
            SeamFixBaseTexture = SeamFixEffect.Parameters["BaseTexture"];
            SeamFixInverseResolution = SeamFixEffect.Parameters["InverseResolution"];
            SeamFixSteps = SeamFixEffect.Parameters["Steps"];

            //Gbuffer
            GBufferEffect = content.Load<Effect>("Shaders/ObjectSpaceRender/OSRenderer");

            GBufferEffectParameter_World = GBufferEffect.Parameters["WorldView"];
            GBufferEffectParameter_WorldViewProj = GBufferEffect.Parameters["WorldViewProj"];
            GBufferEffectParameter_WorldIT = GBufferEffect.Parameters["WorldViewIT"];
            GBufferEffectParameter_Camera = GBufferEffect.Parameters["Camera"];
            GBufferEffectParameter_FarClip = GBufferEffect.Parameters["FarClip"];

            GBufferEffectParameter_Material_Metallic = GBufferEffect.Parameters["Metallic"];
            GBufferEffectParameter_Material_MetallicMap = GBufferEffect.Parameters["MetallicMap"];
            GBufferEffectParameter_Material_DiffuseColor = GBufferEffect.Parameters["DiffuseColor"];
            GBufferEffectParameter_Material_Roughness = GBufferEffect.Parameters["Roughness"];

            GBufferEffectParameter_Material_MaskMap = GBufferEffect.Parameters["Mask"];
            GBufferEffectParameter_Material_Texture = GBufferEffect.Parameters["Texture"];
            GBufferEffectParameter_Material_NormalMap = GBufferEffect.Parameters["NormalMap"];
            GBufferEffectParameter_Material_RoughnessMap = GBufferEffect.Parameters["RoughnessMap"];
            GBufferEffectParameter_Material_DisplacementMap = GBufferEffect.Parameters["DisplacementMap"];
            GBufferEffectParameter_Material_EnvironmentMap = GBufferEffect.Parameters["EnvironmentMap"];

            GBufferEffectParameter_Material_MaterialType = GBufferEffect.Parameters["MaterialType"];

            GBufferEffectParameter_EnvironmentIntensity = GBufferEffect.Parameters["EnvironmentIntensity"];

            ClearGBufferEffect = content.Load<Effect>("Shaders/GbufferSetup/ClearGBuffer");

            //Techniques

            GBufferEffectTechniques_DrawTextureDisplacement = GBufferEffect.Techniques["DrawTextureDisplacement"];
            GBufferEffectTechniques_DrawTextureSpecularNormalMask = GBufferEffect.Techniques["DrawTextureSpecularNormalMask"];
            GBufferEffectTechniques_DrawTextureNormalMask = GBufferEffect.Techniques["DrawTextureNormalMask"];
            GBufferEffectTechniques_DrawTextureSpecularMask = GBufferEffect.Techniques["DrawTextureSpecularMask"];
            GBufferEffectTechniques_DrawTextureMask = GBufferEffect.Techniques["DrawTextureMask"];
            GBufferEffectTechniques_DrawTextureSpecularNormalMetallic = GBufferEffect.Techniques["DrawTextureSpecularNormalMetallic"];
            GBufferEffectTechniques_DrawTextureSpecularNormal = GBufferEffect.Techniques["DrawTextureSpecularNormal"];
            GBufferEffectTechniques_DrawTextureNormal = GBufferEffect.Techniques["DrawTextureNormal"];
            GBufferEffectTechniques_DrawTextureSpecular = GBufferEffect.Techniques["DrawTextureSpecular"];
            GBufferEffectTechniques_DrawTextureSpecularMetallic = GBufferEffect.Techniques["DrawTextureSpecularMetallic"];
            GBufferEffectTechniques_DrawTexture = GBufferEffect.Techniques["DrawTexture"];
            GBufferEffectTechniques_DrawBasic = GBufferEffect.Techniques["DrawBasic"];
            GBufferEffectTechniques_DrawBasicMesh = GBufferEffect.Techniques["DrawBasicMesh"];

            //VSM

            //virtualShadowMappingEffect = content.Load<Effect>("Shaders/Shadow/VirtualShadowMapsGenerate");
            //virtualShadowMappingEffectParameter_WorldViewProj = virtualShadowMappingEffect.Parameters["WorldViewProj"];

            //virtualShadowMappingEffect_Technique_VSM = virtualShadowMappingEffect.Techniques["DrawVSM"];
            //virtualShadowMappingEffect_Technique_Depth = virtualShadowMappingEffect.Techniques["DrawDepth"];

            //Editor

            //IdRenderEffect = content.Load<Effect>("Shaders/Editor/IdRender");
            //IdRenderEffectParameterWorldViewProj = IdRenderEffect.Parameters["WorldViewProj"];
            //IdRenderEffectParameterColorId = IdRenderEffect.Parameters["ColorId"];
            //IdRenderEffectParameterWorld = IdRenderEffect.Parameters["World"];

            //IdRenderEffectDrawId = IdRenderEffect.Techniques["DrawId"].Passes[0];
            //IdRenderEffectDrawOutline = IdRenderEffect.Techniques["DrawOutline"].Passes[0];

            //BillboardEffect = content.Load<Effect>("Shaders/Editor/BillboardEffect");
            //BillboardEffectParameter_WorldViewProj = BillboardEffect.Parameters["WorldViewProj"];
            //BillboardEffectParameter_WorldView = BillboardEffect.Parameters["WorldView"];
            //BillboardEffectParameter_AspectRatio = BillboardEffect.Parameters["AspectRatio"];
            //BillboardEffectParameter_FarClip = BillboardEffect.Parameters["FarClip"];
            //BillboardEffectParameter_Texture = BillboardEffect.Parameters["Texture"];
            //BillboardEffectParameter_DepthMap = BillboardEffect.Parameters["DepthMap"];
            //BillboardEffectParameter_IdColor = BillboardEffect.Parameters["IdColor"];

            //BillboardEffectTechnique_Billboard = BillboardEffect.Techniques["Billboard"];
            //BillboardEffectTechnique_Id = BillboardEffect.Techniques["Id"];

            //LineEffect = content.Load<Effect>("Shaders/Editor/LineEffect");
            //LineEffectParameter_WorldViewProj = LineEffect.Parameters["WorldViewProj"];

            ////TAA

            //TemporalAntiAliasingEffect = content.Load<Effect>("Shaders/TemporalAntiAliasing/TemporalAntiAliasing");

            //TemporalAntiAliasingEffect_AccumulationMap = TemporalAntiAliasingEffect.Parameters["AccumulationMap"];
            //TemporalAntiAliasingEffect_UpdateMap = TemporalAntiAliasingEffect.Parameters["UpdateMap"];
            //TemporalAntiAliasingEffect_DepthMap = TemporalAntiAliasingEffect.Parameters["DepthMap"];
            //TemporalAntiAliasingEffect_CurrentToPrevious = TemporalAntiAliasingEffect.Parameters["CurrentToPrevious"];
            //TemporalAntiAliasingEffect_Resolution = TemporalAntiAliasingEffect.Parameters["Resolution"];
            //TemporalAntiAliasingEffect_FrustumCorners = TemporalAntiAliasingEffect.Parameters["FrustumCorners"];
            //TemporalAntiAliasingEffect_Threshold = TemporalAntiAliasingEffect.Parameters["Threshold"];

            ////Post

            //PostProcessing = content.Load<Effect>("Shaders/PostProcessing/PostProcessing");

            //PostProcessingParameter_ChromaticAbberationStrength =
            //    PostProcessing.Parameters["ChromaticAbberationStrength"];
            //PostProcessingParameter_SCurveStrength = PostProcessing.Parameters["SCurveStrength"];
            //PostProcessingParameter_ScreenTexture = PostProcessing.Parameters["ScreenTexture"];
            //PostProcessingTechnique_Vignette = PostProcessing.Techniques["Vignette"];
            //PostProcessingTechnique_VignetteChroma = PostProcessing.Techniques["VignetteChroma"];

            ////Blur
            //GaussianBlurEffect = content.Load<Effect>("Shaders/ScreenSpace/GaussianBlur");
            //GaussianBlurEffectParameter_InverseResolution = GaussianBlurEffect.Parameters["InverseResolution"];
            //GaussianBlurEffectParameter_TargetMap = GaussianBlurEffect.Parameters["TargetMap"];

        }
    }
}
