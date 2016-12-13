using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Entities
{
    public class SpotLightSource : PointLightSource
    {
        public Vector3 Direction;

        public RenderTarget2D RenderTargetShadowMap;
        public RenderTargetBinding[] RenderTargetShadowMapBinding = new RenderTargetBinding[1];
        public Matrix LightViewProjection;

        public SpotLightSource(Vector3 position, float radius, Color color, float intensity, Vector3 direction, bool drawShadow)
        {
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
            Direction = direction;
            DrawShadow = drawShadow;
        }
        
    }
}
