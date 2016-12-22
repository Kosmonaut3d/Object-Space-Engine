using System;

namespace EngineTest.Recources
{
    public static class GameSettings
    {
        public static float g_FarPlane = 500;
        public static float g_supersampling = 1;
        public static int ShowDisplayInfo = 0;

        public static Renderer.Renderer.RenderModes g_RenderMode = Renderer.Renderer.RenderModes.Final;
        public static bool g_CPU_Culling = true;

        public static bool g_BatchByMaterial = false; //Note this must be activated before the application is started.

        public static bool d_profiler = false;

        public static bool g_CPU_Sort = true;
        public static int g_ScreenWidth = 1280;
        public static int g_ScreenHeight = 720;
        
        public static bool p_Physics = false;

        public static float m_defaultRoughness = 0;
        
        public static bool h_DrawLines = true;

        // PostProcessing
        
        public static bool d_defaultMaterial;
        // Screen Space Ambient Occlusion
        public static bool c_UseStringBuilder = true;
        public static bool g_UpdateShading = true;
        public static bool u_updateUI = true;
        public static int g_texResolution = 512;

        public static bool g_FixSeams = true;

        public static bool s_rotateModel = true;

        private static float _g_EnvironmentIntensity = 1.8f;

        public static float g_EnvironmentIntensity
        {
            get
            {
                return _g_EnvironmentIntensity;
            }

            set
            {
                if (Math.Abs(_g_EnvironmentIntensity - value) > 0.0001f)
                {
                    _g_EnvironmentIntensity = value;
                    Shaders.GBufferEffectParameter_EnvironmentIntensity.SetValue(value);
                }
            }
        }

        private static int _g_SeamSearchSteps;
        public static int g_SeamSearchSteps
        {
            get
            {
                return _g_SeamSearchSteps;
            }

            set
            {
                if (_g_SeamSearchSteps!=value)
                {
                    _g_SeamSearchSteps = value;
                    Shaders.SeamFixSteps.SetValue((float)_g_SeamSearchSteps);
                }
            }
        }

        public static void ApplySettings()
        {
            
            d_defaultMaterial = false;
            
        }
        
    }
}
