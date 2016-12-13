﻿using EngineTest.Recources;
using EngineTest.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Entities
{
    public class PointLightSource : TransformableObject
    {
        private Vector3 _position = Vector3.Zero;
        public Matrix WorldMatrix;
        private float _radius;
        private Color _color;
        public Vector3 ColorV3;
        public float Intensity;

        public bool HasChanged = true;

        public readonly int ShadowResolution;
        public readonly bool StaticShadows;

        public RenderTargetCube shadowMapCube;

        public Matrix LightViewProjectionPositiveX;
        public Matrix LightViewProjectionNegativeX;
        public Matrix LightViewProjectionPositiveY;
        public Matrix LightViewProjectionNegativeY;
        public Matrix LightViewProjectionPositiveZ;
        public Matrix LightViewProjectionNegativeZ;

        public Matrix LightViewSpace;
        public Matrix LightWorldViewProj;

        public BoundingSphere BoundingSphere;

        public bool DrawShadow;
        public readonly bool IsVolumetric;
        private readonly float _lightVolumeDensity = 1;


        /// <summary>
        /// A point light is a light that shines in all directions
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <param name="isVolumetric"></param>
        /// <param name="shadowResolution">shadow map resolution per face. Optional</param>
        /// <param name="staticShadow">if set to true the shadows will not update at all. Dynamic shadows in contrast update only when needed.</param>
        /// <param name="drawShadow"></param>
        /// <param name="volumeDensity"></param>
        /// <returns></returns>
        public PointLightSource(Vector3 position, float radius, Color color, float intensity, bool drawShadow, bool isVolumetric, int shadowResolution, bool staticShadow, float volumeDensity = 1)
        {
            BoundingSphere = new BoundingSphere(position, radius);
            Position = position;
            Radius = radius;
            Color = color;
            Intensity = intensity;
            DrawShadow = drawShadow;
            IsVolumetric = isVolumetric;

            ShadowResolution = shadowResolution;
            StaticShadows = staticShadow;
            _lightVolumeDensity = volumeDensity;

            Id = IdGenerator.GetNewId();

        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                ColorV3 = _color.ToVector3();
            }
        }

        public override Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                BoundingSphere.Center = value;
                WorldMatrix = Matrix.CreateScale(Radius * 1.1f) * Matrix.CreateTranslation(Position);
                HasChanged = true;
            }
        }
        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                BoundingSphere.Radius = value;
                WorldMatrix = Matrix.CreateScale(Radius * 1.1f) * Matrix.CreateTranslation(Position);
                HasChanged = true;
            }
        }

        public override int Id { get; set; }

        public override double AngleZ { get; set; }
        public override double AngleX { get; set; }
        public override double AngleY { get; set; }

        public override TransformableObject Clone
        {
            get { return new PointLightSource(Position, Radius, Color, Intensity, DrawShadow, IsVolumetric, ShadowResolution, StaticShadows);}
        }
        
        protected PointLightSource()
        {

        }
        
    }
    
}
