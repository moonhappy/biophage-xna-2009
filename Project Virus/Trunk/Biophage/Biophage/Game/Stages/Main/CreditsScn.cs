using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    /// Credits scene.
    /// </summary>
    class CreditsScn : Scene
    {
        #region fields

        #endregion

        #region methods

        #region construction

        public CreditsScn(  uint id,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                            SpriteFontResHandle sceneFont,
                            Scene parent)
            : base(id, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
            Microsoft.Xna.Framework.Graphics.Color.Black, sceneFont,
            parent.Stage, parent, null)
        {
            m_camera = new CameraGObj(
                uint.MaxValue, debugMgr, resourceMgr, this,
                false, graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000.0f);
        }

        #endregion

        #region creation

        public static CreditsScn Create(    DebugManager debugMgr, ResourceManager resourceMgr,
                                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                            Scene parent)
        {
            CreditsScn scn = new CreditsScn(
                GlobalConstants.CREDITS_SCN_ID, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                parent);

            TextureAsset backgroundWall = new TextureAsset(
               1, Microsoft.Xna.Framework.Vector3.Zero,
               1.92f, 1.08f, "Content\\MainStage", "Background",
               debugMgr, resourceMgr, scn, true,
               graphicsMgr);

            TextureAsset creditsTitle = new TextureAsset(
                2, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "CreditsTitle",
                debugMgr, resourceMgr, scn, true,
                graphicsMgr);

            return scn;
        }

        #endregion

        #region game loop

        public override void Input(Microsoft.Xna.Framework.GameTime gameTime,
                                    ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
                                    , ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
                                    )
        {
            if (    (newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                    m_prevGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B)) ||

                    (newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Back) &&
                    m_prevGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.Back))
                )
            {
                GetMenu.Active = true;
                Stage.SetCurrentScene(GlobalConstants.MAIN_MENU_SCN_ID);
            }

#if !XBOX
            if (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) &&
                m_prevKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                GetMenu.Active = true;
                Stage.SetCurrentScene(GlobalConstants.MAIN_MENU_SCN_ID);
            }
#endif
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //don't show main menu
            if (GetMenu.Active)
                GetMenu.Active = false;
        }

        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        #endregion

        #endregion
    }
}
