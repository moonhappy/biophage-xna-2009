using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using LNA;
using LNA.GameEngine;
using LNA.GameEngine.Core;
using LNA.GameEngine.Core.AsyncTasks;
using LNA.GameEngine.Objects;
using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.GameObjects.Assets;
using LNA.GameEngine.Objects.GameObjects.Sprites;
using LNA.GameEngine.Objects.UI;
using LNA.GameEngine.Objects.UI.Menu;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;
using LNA.GameEngine.Resources.Drawable;
using LNA.GameEngine.Resources.Playable;

namespace Biophage.Game.Stages.Main
{
    /// <summary>
    /// The first screen of the game - used to async load the startup
    /// content.
    /// </summary>
    public class StartSplashScn : Scene
    {
        #region fields

        private bool m_mainMenuLoadSubmited = false;

        #endregion

        #region methods

        #region construction

        public StartSplashScn(  uint id,
                                DebugManager debugMgr, ResourceManager resourceMgr,
                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                SpriteFontResHandle sceneFont,
                                Scene parent)
            : base(id, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
            Microsoft.Xna.Framework.Graphics.Color.Black, sceneFont,
            parent.Stage, parent, null)
        {
            //set camera
            m_camera = new CameraGObj(
                uint.MaxValue, debugMgr, resourceMgr,
                this, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000f);
        }

        #endregion

        #region creation

        /// <summary>
        /// Creates the scene.
        /// </summary>
        public static StartSplashScn Create(DebugManager debugMgr, ResourceManager resourceMgr,
                                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                                Stage stage, Scene parent)
        {
            StartSplashScn scn = new StartSplashScn(
                GlobalConstants.START_SPLASH_SCN_ID,
                debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                parent);

            TextureAsset background = new TextureAsset(
                1, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "Background",
                debugMgr, resourceMgr, scn, true,
                graphicsMgr);

            TextureAsset splashTexture = new TextureAsset(
                2, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "StartSplashTitle",
                debugMgr, resourceMgr, scn, true,
                graphicsMgr);

            TextureAsset disclaimer = new TextureAsset(
                3, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage\\", "Disclaimer",
                debugMgr, resourceMgr, scn, true, graphicsMgr);

            return scn;
        }

        #endregion

        #region game loop

        public override void Input( Microsoft.Xna.Framework.GameTime gameTime,
                                    ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
                                    , ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
            )
        {
            //do nothing
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //load main menu screen
            if (!m_mainMenuLoadSubmited)
            {
                m_mainMenuLoadSubmited = true;
            }

            //all done? change to menu scene
            else
            {
                if (gameTime.TotalRealTime.Seconds > GlobalConstants.MIN_DISCLAIMER_TIMEOUT_SECS)
                {
                    m_gameObjects[3].Visible = false;
                }
                if (gameTime.TotalRealTime.Seconds > GlobalConstants.MIN_SPLASH_TIMEOUT_SECS)
                {
                    Stage.SetCurrentScene(GlobalConstants.MAIN_MENU_SCN_ID);
                    Stage.CurrentScene.GetMenu.Active = true;
                }
            }
        }

        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            ///do nothing
        }

        #endregion

        #endregion
    }
}
