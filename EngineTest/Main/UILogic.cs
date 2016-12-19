using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineTest.Recources;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EngineTest.Main
{
    public class UILogic
    {
        private UserInterface UIManager;
        private SpriteBatch spriteBatch;

        private Panel panel;
        private Paragraph fps;
        private CheckBox toggleShader;

        public void Initialize(GraphicsDevice graphics)
        {
            UIManager.Initialize();
            spriteBatch = new SpriteBatch(graphics);

            UserInterface.SCALE = 0.7f;
            UIManager.ShowCursor = false;

            panel = new Panel(new Vector2(500,600), PanelSkin.Simple, Anchor.TopRight);
            panel.Draggable = false;
            UIManager.AddEntity(panel);



            panel.AddChild(new Paragraph("Test UI"));
            panel.SetPosition(Anchor.TopRight, new Vector2(0,0));

            panel.AddChild(fps = new Paragraph("fps :"+GameStats.fps_avg));

            panel.AddChild(toggleShader = new CheckBox("Update Shader", Anchor.Auto, null, null, true));
            toggleShader.Scale = 0.5f;
            toggleShader.OnValueChange = (Entity entity) =>
            {
                CheckBox cb = (CheckBox) entity;
                GameSettings.g_UpdateShading = cb.Checked;
            };

            CheckBox seamFixToggle;
            panel.AddChild(seamFixToggle = new CheckBox("Fix Seams", Anchor.Auto, null, null, true));

            panel.AddChild(toggleShader = new CheckBox("Rotate Model", Anchor.Auto, null, null, true));
            toggleShader.Scale = 0.5f;
            toggleShader.OnValueChange = (Entity entity) =>
            {
                CheckBox cb = (CheckBox)entity;
                GameSettings.s_rotateModel = cb.Checked;
            };

            panel.AddChild(new LineSpace(1));
            
            Paragraph sliderValue = new Paragraph("Texture resolution: 512");
            panel.AddChild(sliderValue);
            

            Slider slider = new Slider(1, 12, SliderSkin.Default);
            slider.Value = 9;
            slider.StepsCount = 11;
            slider.OnValueChange = (Entity entity) =>
            {
                Slider slid = (Slider) entity;
                GameSettings.g_texResolution = (int) Math.Pow(2, slid.Value);
                sliderValue.Text = "Texture resolution: " + GameSettings.g_texResolution;
            };
            panel.AddChild(slider);

            Paragraph environmentMap = new Paragraph("ambient intensity: 1.8");
            panel.AddChild(environmentMap);
            
            Slider envIntensitySlider = new Slider(0, 400, SliderSkin.Default);
            envIntensitySlider.StepsCount = 400;
            envIntensitySlider.Value = 180;
            envIntensitySlider.OnValueChange = (Entity entity) =>
            {
                Slider slid = (Slider)entity;
                GameSettings.g_EnvironmentIntensity = slid.Value/100.0f;
                environmentMap.Text = "ambient intensity: " + GameSettings.g_EnvironmentIntensity;
            };
            panel.AddChild(envIntensitySlider);

            Paragraph seamFixSteps = new Paragraph("seam dilate pixels: 1");
            panel.AddChild(seamFixSteps);

            Slider seamFixStepsSlider = new Slider(0, 6, SliderSkin.Default);
            seamFixStepsSlider.StepsCount = 6;
            seamFixStepsSlider.Value = 1;
            seamFixStepsSlider.OnValueChange = (Entity entity) =>
            {
                Slider slid = (Slider)entity;
                GameSettings.g_SeamSearchSteps = slid.Value;
                seamFixSteps.Text = "seam dilate pixels: " + GameSettings.g_SeamSearchSteps;
            };
            panel.AddChild(seamFixStepsSlider);


            seamFixToggle.OnValueChange = (Entity entity) =>
            {
                CheckBox cb = (CheckBox)entity;
                GameSettings.g_FixSeams = cb.Checked;
                seamFixStepsSlider.Disabled = !cb.Checked;
            };

        }

        public void Load(ContentManager content)
        {
            UIManager = new UserInterface(content);

        }

        public void Update(GameTime gameTime)
        {
            if (!GameSettings.u_updateUI) return;

            UIManager.Update(gameTime);

            fps.Text = "fps " + Math.Round( GameStats.fps_avg,1);
        }

        public void Draw(GameTime gameTime)
        {
            if (!GameSettings.u_updateUI) return;
            UIManager.Draw(spriteBatch);
        }
    }
}
