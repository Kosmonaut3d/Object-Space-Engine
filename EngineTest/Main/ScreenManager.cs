﻿using BEPUphysics;
using EngineTest.Recources;
using EngineTest.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Main
{
    /// <summary>
    /// Manages our different screens and passes information accordingly
    /// </summary>
    public class ScreenManager
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Renderer.Renderer _renderer;
        private MainLogic _logic;
        private EditorLogic _editorLogic;
        private Assets _assets;
        private DebugScreen _debug;
        private UILogic _uiLogic;

        private EditorLogic.EditorReceivedData _editorReceivedDataBuffer;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Initialize(GraphicsDevice graphicsDevice, Space space)
        {
            _renderer.Initialize(graphicsDevice, _assets);
            _logic.Initialize(_assets, space);
            _editorLogic.Initialize(graphicsDevice);
            _debug.Initialize(graphicsDevice);
            _uiLogic.Initialize(graphicsDevice);
        }

        //Update per frame
        public void Update(GameTime gameTime, bool isActive)
        {
            _logic.Update(gameTime, isActive);
            _editorLogic.Update(gameTime, _logic.BasicEntities, _logic.PointLights, _logic.DirectionalLights, _editorReceivedDataBuffer, _logic.MeshMaterialLibrary);
            _renderer.Update(gameTime, isActive);

            _debug.Update(gameTime);
            _uiLogic.Update(gameTime);
        }

        //Load content
        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _renderer = new Renderer.Renderer();
            _logic = new MainLogic();
            _editorLogic = new EditorLogic();
            _assets = new Assets();
            _debug = new DebugScreen();
            _uiLogic = new UILogic();

            Shaders.Load(content);
            _assets.Load(content, graphicsDevice);
            _renderer.Load(content);
            _logic.Load(content);
            _debug.LoadContent(content);
            _uiLogic.Load(content);
        }

        public void Unload(ContentManager content)
        {
            content.Dispose();
        }
        
        public void Draw(GameTime gameTime)
        {
            //Our renderer gives us information on what id is currently hovered over so we can update / manipulate objects in the logic functions
            _renderer.Draw(_logic.Camera, _logic.MeshMaterialLibrary, _logic.BasicEntities, _logic.PointLights, _logic.DirectionalLights, _editorLogic.GetEditorData(), gameTime);
            _debug.Draw(gameTime);
            _uiLogic.Draw(gameTime);
        }

        public void UpdateResolution()
        {
            _renderer.UpdateResolution();
        }
    }
}
