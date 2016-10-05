﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class DirectionalLight
    {
        public Color Color;
        public float Intensity;
        private Vector3 _direction;
        private Vector3 _position;
        public bool HasChanged;

        public bool DrawShadows;
        public float ShadowSize;
        public float ShadowDepth;
        public int ShadowResolution;
        public bool StaticShadow;

        public RenderTarget2D shadowMap;
        public Matrix ShadowViewProjection;

        public Matrix LightViewProjection;

        /// <summary>
        /// Create a Directional light, shadows are optional
        /// </summary>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="direction"></param>
        /// <param name="drawShadows"></param>
        /// <param name="shadowSize"></param>
        /// <param name="shadowDepth"></param>
        /// <param name="shadowResolution"></param>
        public DirectionalLight(Color color, float intensity, Vector3 direction,Vector3 position = default(Vector3), bool drawShadows = false, float shadowSize = 100, float shadowDepth = 100, int shadowResolution = 512, bool staticshadows = false)
        {
            Color = color;
            Intensity = intensity;

            Vector3 normalizedDirection = direction;
            normalizedDirection.Normalize();
            Direction = normalizedDirection;

            DrawShadows = drawShadows;

            DrawShadows = drawShadows;
            ShadowSize = shadowSize;
            ShadowDepth = shadowDepth;
            ShadowResolution = shadowResolution;
            StaticShadow = staticshadows;

            Position = position;
        }

        public Vector3 Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                HasChanged = true;
            }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                HasChanged = true;
            }
        }

        public virtual void ApplyShader()
        {
            if (DrawShadows)
            {
                //Shaders.deferredDirectionalLightParameterLightViewProjection.SetValue(LightViewProjection);
                //Shaders.deferredDirectionalLightParameter_ShadowMap.SetValue(shadowMap);

                Shaders.deferredDirectionalLightShadowed.Passes[0].Apply();
            }
            else
            {
                Shaders.deferredDirectionalLightUnshadowed.Passes[0].Apply();  
            }
        }
    }
}