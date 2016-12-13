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
        public static int g_ScreenHeight = 800;
        
        public static bool p_Physics = false;

        public static float m_defaultRoughness = 0;
        
        public static bool h_DrawLines = true;

        // PostProcessing
        
        public static bool d_defaultMaterial;
        // Screen Space Ambient Occlusion
        public static bool c_UseStringBuilder = true;

        public static void ApplySettings()
        {
            
            d_defaultMaterial = false;
            
        }
        
    }
}
