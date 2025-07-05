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

namespace Biophage.Game.Stages.Game
{
    public class CommonGameResScn : Scene
    {
        #region feilds

        //have a handle for each model res
        protected ModelResHandle m_modelRBC;
        protected ModelResHandle m_modelPlatelet;
        protected ModelResHandle m_modelBigCellSilo;
        protected ModelResHandle m_modelBigCellTank;

        protected ModelResHandle m_modelSmallHybrid;
        protected ModelResHandle m_modelMediumHybrid;
        protected ModelResHandle m_modelBigHybrid;

        protected ModelResHandle m_modelWhiteBloodCell;

        public HUDOverlay m_hud = null;

        #endregion

        #region construction

        public CommonGameResScn(uint id,
                                    DebugManager debugMgr, ResourceManager resourceMgr,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                    Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                    SpriteFontResHandle sceneFont,
                                    Stage stage, Scene parent)
            : base(id, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
            Microsoft.Xna.Framework.Graphics.Color.Black,
            sceneFont, stage, parent, null)
        {
            m_modelRBC = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\RedBloodCell\\", "RedBloodCell");
            m_modelPlatelet = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Platelet\\", "Platelet");
            m_modelBigCellSilo = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Silo\\", "Silo");
            m_modelBigCellTank = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Tank\\", "Tank");

            m_modelSmallHybrid = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Hybrids\\", "SmallHybrid");
            m_modelMediumHybrid = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Hybrids\\", "MediumHybrid");
            m_modelBigHybrid = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Hybrids\\", "BigHybrid");

            m_modelWhiteBloodCell = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\WhiteBloodCell\\", "WhiteBloodCell");
        }

        #endregion

        #region creation

        public static CommonGameResScn Create(DebugManager debugMgr, ResourceManager resourceMgr,
            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
            Stage stage, Scene parent)
        {
            CommonGameResScn scn = new CommonGameResScn(
                GlobalConstants.COMMRES_SCN_ID,
                debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                stage, parent);

            return scn;
        }

        #endregion

        #region loading

        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if ((!m_modelRBC.Load()) ||
                        (!m_modelPlatelet.Load()) ||
                        (!m_modelBigCellSilo.Load()) ||
                        (!m_modelBigCellTank.Load()) ||
                        (!m_modelSmallHybrid.Load()) ||
                        (!m_modelMediumHybrid.Load()) ||
                        (!m_modelBigHybrid.Load()) ||
                        (!m_modelWhiteBloodCell.Load()))
                    retVal = false;

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        public override bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                if ((!m_modelRBC.Unload()) ||
                        (!m_modelPlatelet.Unload()) ||
                        (!m_modelBigCellSilo.Unload()) ||
                        (!m_modelBigCellTank.Unload()) ||
                        (!m_modelSmallHybrid.Unload()) ||
                        (!m_modelMediumHybrid.Unload()) ||
                        (!m_modelBigHybrid.Unload()) ||
                        (!m_modelWhiteBloodCell.Unload()))
                    retVal = false;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
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
