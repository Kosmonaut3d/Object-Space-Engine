using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Recources
{
    public class Assets
    {
        public static Texture2D BaseTex;

        public Model Truck;

        public MaterialEffect TruckMaterial;
        public MaterialEffect TruckMaterial2;
        public MaterialEffect GoldMaterial;
        public Texture2D EnvironmentMap;
        public bool IconLight { get; set; }


        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            BaseTex = new Texture2D(graphicsDevice, 1, 1);
            BaseTex.SetData(new Color[] { Color.White });

            Truck = content.Load<Model>("Art/truck_skeleton");
            EnvironmentMap = content.Load<Texture2D>("Art/1900");
            TruckMaterial = CreateMaterial(Color.Wheat, 0.1f, 0.1f,
                albedoMap: content.Load<Texture2D>("Art/truck_skeleton_albedo"),
                normalMap: content.Load<Texture2D>("Art/truck_skeleton_normal"),
                //albedoMap: content.Load<Texture2D>("Art/squarebricks-Diffuse"),
                //normalMap: content.Load<Texture2D>("Art/squarebricks-normal"),
                roughnessMap: content.Load<Texture2D>("Art/truck_skeleton_roughness"),
                metallicMap: content.Load<Texture2D>("Art/truck_skeleton_metallic")
                );
            TruckMaterial2 = CreateMaterial(Color.Wheat, 0.1f, 0.1f,
                albedoMap: content.Load<Texture2D>("Art/truck_skeleton_albedo"),
                normalMap: content.Load<Texture2D>("Art/truck_skeleton_normal"),
                roughnessMap: content.Load<Texture2D>("Art/truck_skeleton_roughness"),
                metallicMap: content.Load<Texture2D>("Art/truck_skeleton_metallic")
                );
            TruckMaterial2.RenderCClockwise = true;

            GoldMaterial = CreateMaterial(Color.Gold, 0.2f, 1);
        }

        /// <summary>
        /// Create custom materials, you can add certain maps like Albedo, normal, etc. if you like.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="roughness"></param>
        /// <param name="metallic"></param>
        /// <param name="albedoMap"></param>
        /// <param name="normalMap"></param>
        /// <param name="roughnessMap"></param>
        /// <param name="metallicMap"></param>
        /// <param name="mask"></param>
        /// <param name="type">2: hologram, 3:emissive</param>
        /// <param name="emissiveStrength"></param>
        /// <returns></returns>
        private MaterialEffect CreateMaterial(Color color, float roughness, float metallic, Texture2D albedoMap = null, Texture2D normalMap = null, Texture2D roughnessMap = null, Texture2D metallicMap = null, Texture2D mask = null, Texture2D displacementMap = null, MaterialEffect.MaterialTypes type = 0, float emissiveStrength = 0)
        {
            MaterialEffect mat = new MaterialEffect(Shaders.ClearGBufferEffect);
            mat.Initialize(color, roughness, metallic, albedoMap, normalMap, roughnessMap, metallicMap, mask, displacementMap, type, emissiveStrength);
            return mat;
        }
        
        private Model ProcessModel(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    MaterialEffect matEffect = new MaterialEffect(meshPart.Effect);

                    if (!(meshPart.Effect is BasicEffect))
                    {
                        throw new Exception("Can only process models with basic effect");
                    }

                    BasicEffect oEffect = meshPart.Effect as BasicEffect;

                    if (oEffect.TextureEnabled)
                        matEffect.AlbedoMap = oEffect.Texture;

                    matEffect.DiffuseColor = oEffect.DiffuseColor;

                    meshPart.Effect = matEffect;
                }
            }

            return model;
        }
        
    }

}
