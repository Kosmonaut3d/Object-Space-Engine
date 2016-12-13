using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using EngineTest.Entities;
using EngineTest.Main;
using EngineTest.Recources;
using EngineTest.Recources.Helper;
using EngineTest.Renderer.Helper;
using EngineTest.Renderer.RenderModules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace EngineTest.Renderer
{
    public class Renderer
    {
        #region VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Graphics & Helpers
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private QuadRenderer _quadRenderer;

        //Assets
        private Assets _assets;

        //View Projection
        private bool _viewProjectionHasChanged;
        private Vector3 _inverseResolution;
        
        //Projection Matrices and derivates used in shaders
        private Matrix _view;
        private Matrix _inverseView;
        private Matrix _projection;
        private Matrix _viewProjection;
        private Matrix _staticViewProjection;
        private Matrix _inverseViewProjection;
        private Matrix _previousViewProjection;
        private Matrix _currentViewToPreviousViewProjection;

        private bool _continous = true;

        //Bounding Frusta of our view projection, to calculate which objects are inside the view
        private BoundingFrustum _boundingFrustum;
        private BoundingFrustum _boundingFrustumShadow;

        //Used for the view space directions in our shaders. Far edges of our view frustum
        private readonly Vector3[] _cornersWorldSpace = new Vector3[8];
        private readonly Vector3[] _cornersViewSpace = new Vector3[8];
        private readonly Vector3[] _currentFrustumCorners = new Vector3[4];

        //Checkvariables to see which console variables have changed from the frame before
        private float _g_FarClip;
        private float _supersampling = 1;
        private bool _hologramDraw;
        private int _forceShadowFiltering;
        private bool _forceShadowSS;
        private bool _ssr = true;
        private bool _g_SSReflectionNoise;
        private bool _g_UseDepthStencilLightCulling;

        //Render modes
        public enum RenderModes { Final
        }

        //Render targets
        private RenderTarget2D _textureBuffer;
        //Performance Profiler

        private readonly Stopwatch _performanceTimer = new Stopwatch();
        private long _performancePreviousTime;

        #endregion

        #region FUNCTIONS

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            #region BASE FUNCTIONS

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  BASE FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize variables
        /// </summary>
        /// <param name="content"></param>
        public void Load(ContentManager content)
        {
            _inverseResolution = new Vector3(1.0f / GameSettings.g_ScreenWidth, 1.0f / GameSettings.g_ScreenHeight, 0);

        }

        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="assets"></param>
        public void Initialize(GraphicsDevice graphicsDevice, Assets assets)
        {
            _graphicsDevice = graphicsDevice;
            _quadRenderer = new QuadRenderer();
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _assets = assets;

            //Apply some base settings to overwrite shader defaults with game settings defaults
            GameSettings.ApplySettings();

            Shaders.GBufferEffectParameter_Material_EnvironmentMap.SetValue(_assets.EnvironmentMap);
            
            SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight, false);

            
        }

        /// <summary>
        /// Update our function
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="isActive"></param>
        public void Update(GameTime gameTime, bool isActive)
        {

        }

        #endregion

            #region RENDER FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  RENDER FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////

                #region MAIN DRAW FUNCTIONS
                ////////////////////////////////////////////////////////////////////////////////////////////////////
                //  MAIN DRAW FUNCTIONS
                ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        /// <param name="camera">view point of the renderer</param>
        /// <param name="meshMaterialLibrary">a class that has stored all our mesh data</param>
        /// <param name="entities">entities and their properties</param>
        /// <param name="pointLights"></param>
        /// <param name="directionalLights"></param>
        /// <param name="editorData">The data passed from our editor logic</param>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public void Draw(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities, List<PointLightSource> pointLights, List<DirectionalLightSource> directionalLights, EditorLogic.EditorSendData editorData, GameTime gameTime)
        {
            //Reset the stat counter, so we can count stats/information for this frame only
            ResetStats();
            
            //Update the mesh data for changes in physics etc.
            meshMaterialLibrary.FrustumCullingStartFrame(entities);

            //Check if we changed some drastic stuff for which we need to reload some elements
            CheckRenderChanges(directionalLights);

            //Render ShadowMaps
            //DrawShadowMaps(meshMaterialLibrary, entities, pointLights, directionalLights, camera);

            //Update our view projection matrices if the camera moved
            UpdateViewProjection(camera, meshMaterialLibrary, entities);

            if (Input.WasKeyPressed(Keys.V))
            {
                _continous = !_continous;
            }
            //Draw our meshes to the G Buffer
            DrawTextureBuffer(meshMaterialLibrary);

            DrawObjects(meshMaterialLibrary);
            
            //Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
            RenderMode();
            
            //Draw (debug) lines
            LineHelperManager.Draw(_graphicsDevice, _staticViewProjection);

            //Set up the frustum culling for the next frame
            meshMaterialLibrary.FrustumCullingFinalizeFrame(entities);
        }
                #endregion

                #region OBJECT SPACE RENDERING FUNCTIONS
                /////////////////////////////////////////////////////////////////////////////////////////////////////
                //  DEFERRED RENDERING FUNCTIONS, IN ORDER OF USAGE
                ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset our stat counting for this frame
        /// </summary>
        private void ResetStats()
        {
            GameStats.MaterialDraws = 0;
            GameStats.MeshDraws = 0;
            GameStats.LightsDrawn = 0;
            GameStats.shadowMaps = 0;
            GameStats.activeShadowMaps = 0;
            GameStats.EmissiveMeshDraws = 0;

            //Profiler
            if (GameSettings.d_profiler)
            {
                _performanceTimer.Restart();
                _performancePreviousTime = 0;
            }
            else if (_performanceTimer.IsRunning)
            {
                _performanceTimer.Stop();
            }
        }

        /// <summary>
        /// Check whether any GameSettings have changed that need setup
        /// </summary>
        /// <param name="dirLights"></param>
        private void CheckRenderChanges(List<DirectionalLightSource> dirLights)
        {
            if (Math.Abs(_g_FarClip - GameSettings.g_FarPlane) > 0.0001f)
            {
                _g_FarClip = GameSettings.g_FarPlane;
                Shaders.GBufferEffectParameter_FarClip.SetValue(_g_FarClip);
            }
            
            //Check if supersampling has changed
            if (Math.Abs(_supersampling - GameSettings.g_supersampling) > 0.0001f)
            {
                _supersampling = GameSettings.g_supersampling;
                SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight, false);
            }
            
            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileRenderChanges = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        
        /// <summary>
        /// Create the projection matrices
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="meshMaterialLibrary"></param>
        /// <param name="entities"></param>
        private void UpdateViewProjection(Camera camera, MeshMaterialLibrary meshMaterialLibrary, List<BasicEntity> entities)
        {
            _viewProjectionHasChanged = camera.HasChanged;
            
            //If the camera didn't do anything we don't need to update this stuff
            if (_viewProjectionHasChanged)
            {
                //We have processed the change, now setup for next frame as false
                camera.HasChanged = false;
                camera.HasMoved = false;

                //View matrix
                _view = Matrix.CreateLookAt(camera.Position, camera.Lookat, camera.Up);
                _inverseView = Matrix.Invert(_view);
                
                _projection = Matrix.CreatePerspectiveFieldOfView(camera.FieldOfView,
                    GameSettings.g_ScreenWidth / (float)GameSettings.g_ScreenHeight, 1, GameSettings.g_FarPlane);
                
                Shaders.GBufferEffectParameter_Camera.SetValue(camera.Position);

                _viewProjection = _view * _projection;

                //this is the unjittered viewProjection. For some effects we don't want the jittered one
                _staticViewProjection = _viewProjection;

                //Transformation for TAA - from current view back to the old view projection
                _currentViewToPreviousViewProjection = Matrix.Invert(_view) * _previousViewProjection;
                
                _previousViewProjection = _viewProjection;
                _inverseViewProjection = Matrix.Invert(_viewProjection);

                if (_boundingFrustum == null) _boundingFrustum = new BoundingFrustum(_staticViewProjection);
                else _boundingFrustum.Matrix = _staticViewProjection;
                
            }

            //We need to update whether or not entities are in our boundingFrustum and then cull them or not!
            meshMaterialLibrary.FrustumCulling(entities, _boundingFrustum, _viewProjectionHasChanged, camera.Position);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileUpdateViewProjection = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        
        /// <summary>
        /// Draw all our meshes to the GBuffer - albedo, normal, depth - for further computation
        /// </summary>
        /// <param name="meshMaterialLibrary"></param>
        private void DrawTextureBuffer(MeshMaterialLibrary meshMaterialLibrary)
        {
            if (!Input.WasKeyPressed(Keys.C) && !_continous) return;

            _graphicsDevice.SetRenderTarget(_textureBuffer);
            _graphicsDevice.Clear(Color.TransparentBlack);
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;
            meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.TextureBuffer, graphicsDevice: _graphicsDevice, viewProjection: _viewProjection, lightViewPointChanged: true, view: _view);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawGBuffer = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        private void DrawObjects(MeshMaterialLibrary meshMaterialLibrary)
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Clear(_continous ? Color.CadetBlue : Color.DarkViolet);
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            Shaders.GBufferEffectParameter_Material_Texture.SetValue(_textureBuffer);
            meshMaterialLibrary.Draw(renderType: MeshMaterialLibrary.RenderType.FinalMesh, graphicsDevice: _graphicsDevice, viewProjection: _viewProjection, lightViewPointChanged: true, view: _view);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawGBuffer = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }

        /// <summary>
        /// Draw the final rendered image, change the output based on user input to show individual buffers/rendertargets
        /// </summary>
        private void RenderMode()
        {
            //switch (GameSettings.g_RenderMode)
            //{
            //    default:

            //}

            DrawMapToScreenToFullScreen(_textureBuffer, null, 0.4f);

            //Performance Profiler
            if (GameSettings.d_profiler)
            {
                long performanceCurrentTime = _performanceTimer.ElapsedTicks;
                GameStats.d_profileDrawFinalRender = performanceCurrentTime - _performancePreviousTime;

                _performancePreviousTime = performanceCurrentTime;
            }
        }
        #endregion

        #endregion

            #region RENDERTARGET SETUP FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            //  RENDERTARGET SETUP FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update the resolution of our rendertargets
        /// </summary>
        public void UpdateResolution()
        {
            _inverseResolution = new Vector3(1.0f / GameSettings.g_ScreenWidth, 1.0f / GameSettings.g_ScreenHeight, 0);

            //SetUpRenderTargets(GameSettings.g_ScreenWidth, GameSettings.g_ScreenHeight, false);
        }

        private void SetUpRenderTargets(int width, int height, bool onlyEssentials)
        {
            _textureBuffer = new RenderTarget2D(_graphicsDevice, 512,512, true, SurfaceFormat.Color, DepthFormat.None);
        }

        private void UpdateRenderMapBindings(bool onlyEssentials)
        {
            
        }

            #endregion

            #region HELPER FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            //  HELPER FUNCTIONS
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            
        private void DrawMapToScreenToFullScreen(Texture2D map, BlendState blendState = null, float scale = 1)
        {
            if(blendState == null) blendState = BlendState.Opaque;

            int height;
            int width;
            if (Math.Abs(map.Width / (float)map.Height - GameSettings.g_ScreenWidth / (float)GameSettings.g_ScreenHeight) < 0.001)
            //If same aspectratio
            {
                height = GameSettings.g_ScreenHeight;
                width = GameSettings.g_ScreenWidth;
            }
            else
            {
                if (GameSettings.g_ScreenHeight < GameSettings.g_ScreenWidth)
                {
                    height = GameSettings.g_ScreenHeight;
                    width = GameSettings.g_ScreenHeight;
                }
                else
                {
                    height = GameSettings.g_ScreenWidth;
                    width = GameSettings.g_ScreenWidth;
                }
            }
            if (Math.Abs(scale - 1) > 0.001f)
            {
                width = (int) (scale * width);
                height = (int) (scale * height);
            }
            _graphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(0, blendState, _supersampling>1 ? SamplerState.LinearWrap : SamplerState.PointClamp);
            _spriteBatch.Draw(map, new Rectangle(0, 0, width, height), Color.White);
            _spriteBatch.End();
        }
        
        #endregion

        #endregion
    }

}

