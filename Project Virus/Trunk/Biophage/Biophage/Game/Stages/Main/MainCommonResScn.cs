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
    public class MainCommonResScn : Scene
    {
        #region construction

        public MainCommonResScn(uint id,
                                    DebugManager debugMgr, ResourceManager resourceMgr,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                    Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                    SpriteFontResHandle sceneFont, Stage stage, Menu menu)
            : base(id, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
            Microsoft.Xna.Framework.Graphics.Color.Black,
            sceneFont, stage, menu)
        {

        }

        #endregion

        #region creation

        public static MainCommonResScn Create(  DebugManager debugMgr, ResourceManager resourceMgr,
                                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                                Stage stage, Menu menu)
        {
            MainCommonResScn scn = new MainCommonResScn(
                GlobalConstants.MAIN_COMMON_RES_SCN_ID, debugMgr, resourceMgr,
                graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                stage, menu);

            TextureAsset backgroundWallMain = new TextureAsset(
                1, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "Background",
                debugMgr, resourceMgr, scn, true,
                graphicsMgr);

            return scn;
        }

        #endregion

        #region game loop

        public override void Input( Microsoft.Xna.Framework.GameTime gameTime,
                                    ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
                                    ,ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
            )
        {
            //do nothing
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        #endregion
    }
}
