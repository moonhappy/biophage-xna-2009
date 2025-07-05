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
using System.Globalization;

namespace Biophage.Game.Stages.Game
{
    #region additionals

    public class HUDResources
    {
        #region fields

        TextureResHandle m_A_Button;
        public TextureResHandle ButtonA
        {
            get { return m_A_Button; }
        }

        TextureResHandle m_B_Button;
        public TextureResHandle ButtonB
        {
            get { return m_B_Button; }
        }

        TextureResHandle m_X_Button;
        public TextureResHandle ButtonX
        {
            get { return m_X_Button; }
        }

        TextureResHandle m_Y_Button;
        public TextureResHandle ButtonY
        {
            get { return m_Y_Button; }
        }

        TextureResHandle m_LB_Button;
        public TextureResHandle ButtonLB
        {
            get { return m_LB_Button; }
        }

        TextureResHandle m_RB_Button;
        public TextureResHandle ButtonRB
        {
            get { return m_RB_Button; }
        }

        TextureResHandle m_RTS_Button;
        public TextureResHandle ButtonRightTS
        {
            get { return m_RTS_Button; }
        }

        bool m_isLoaded = false;

        #endregion

        #region methods

        public HUDResources(DebugManager debugMgr, ResourceManager resMgr)
        {
            m_A_Button = new TextureResHandle(debugMgr, resMgr, "Content\\Common\\ButtonImages\\", "xboxControllerButtonA");
            m_B_Button = new TextureResHandle(debugMgr, resMgr, "Content\\Common\\ButtonImages\\", "xboxControllerButtonB");
            m_X_Button = new TextureResHandle(debugMgr, resMgr, "Content\\Common\\ButtonImages\\", "xboxControllerButtonX");
            m_Y_Button = new TextureResHandle(debugMgr, resMgr, "Content\\Common\\ButtonImages\\", "xboxControllerButtonY");

            m_LB_Button = new TextureResHandle(debugMgr, resMgr, "Content\\Common\\ButtonImages\\", "xboxControllerLeftShoulder");
            m_RB_Button = new TextureResHandle(debugMgr, resMgr, "Content\\Common\\ButtonImages\\", "xboxControllerRightShoulder");

            m_RTS_Button = new TextureResHandle(debugMgr, resMgr, "Content\\Common\\ButtonImages\\", "xboxControllerRightThumbstick");
        }

        public bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if (    (!m_A_Button.Load())    ||
                        (!m_B_Button.Load())    ||
                        (!m_X_Button.Load())    ||
                        (!m_Y_Button.Load())    ||
                        (!m_LB_Button.Load())   ||
                        (!m_RB_Button.Load())   ||
                        (!m_RTS_Button.Load()))
                    retVal = false;

                m_isLoaded = (retVal) ? true : false;
            }

            return retVal;
        }

        public bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                if (    (!m_A_Button.Unload())  ||
                        (!m_B_Button.Unload())  ||
                        (!m_X_Button.Unload())  ||
                        (!m_Y_Button.Unload())  ||
                        (!m_LB_Button.Unload()) ||
                        (!m_RB_Button.Unload()) ||
                        (!m_RTS_Button.Unload()))
                    retVal = false;

                m_isLoaded = (retVal) ? false : true;
            }

            return retVal;
        }

        #endregion
    }

    #endregion

    /// <summary>
    /// Manages the HUD of the game. Also provides extra calculations.
    /// </summary>
    public class HUDOverlay : GameObject
    {
        #region fields

        protected ResourceManager m_resMgr;
        protected Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;
        protected Microsoft.Xna.Framework.Graphics.SpriteBatch m_spriteBatch;
        
        protected BiophageGameBaseScn m_scn;
        public BiophageGameBaseScn BioScene
        {
            get { return m_scn; }
            set
            {
                m_scn = value;
                m_clusterMenu.m_scn = value;
            }
        }

        protected CameraGObj m_hudCam;
        protected bool m_isWidescreen;
        protected SpriteFontResHandle m_hudFont;

        public Common.Virus m_myVirus;

        protected string m_statsString;

        protected HUDResources m_hudResources;

        public Common.InGameMenu m_clusterMenu = null;

        protected TextureResHandle m_resCapsidIcon;
        protected TextureResHandle m_resImmuneIcon;
        protected TextureResHandle m_resMedicationIcon;

#if DEBUG
        protected FramerateGObj m_frameRateGObj;
#endif

        //these fields are set by the scene
        public bool showImmuneAlert = false;
        public int immuneAlertCountDown = 0;

        public bool showMedicationAlert = false;
        public int medicationAlertCountDown = 0;

        public bool ShowCapsidAlert
        {
            get 
            {
                if (m_myVirus != null)
                    return m_myVirus.m_virusCapsid.Active;
                else
                    return false;
            }
        }
        public int CapsidCountDown
        {
            get 
            {
                if (m_myVirus != null)
                    return (int)m_myVirus.m_virusCapsid.remainingLifeSecs;
                else
                    return 0;
            }
        }

        #region draw positions

        protected Microsoft.Xna.Framework.Vector2 m_statsStrPos = new Microsoft.Xna.Framework.Vector2(33, 33);

        protected Microsoft.Xna.Framework.Rectangle m_iconSlot1;
        protected Microsoft.Xna.Framework.Vector2 m_labelSlot1;

        protected Microsoft.Xna.Framework.Rectangle m_iconSlot2;
        protected Microsoft.Xna.Framework.Vector2 m_labelSlot2;

        protected Microsoft.Xna.Framework.Rectangle m_iconSlot3;
        protected Microsoft.Xna.Framework.Vector2 m_labelSlot3;

        protected Microsoft.Xna.Framework.Rectangle m_iconSlot4;
        protected Microsoft.Xna.Framework.Vector2 m_labelSlot4;

        protected Microsoft.Xna.Framework.Rectangle m_iconSlot5;
        protected Microsoft.Xna.Framework.Vector2 m_labelSlot5;

        protected Microsoft.Xna.Framework.Rectangle m_iconSlot6;
        protected Microsoft.Xna.Framework.Vector2 m_labelSlot6;

        protected Microsoft.Xna.Framework.Rectangle m_iconSlot7;
        protected Microsoft.Xna.Framework.Vector2 m_labelSlot7;

        #endregion

        #endregion

        #region methods

        #region construction

        public HUDOverlay(DebugManager debugMgr, ResourceManager resMgr, Scene scn,
            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
            : base(uint.MaxValue, debugMgr, resMgr, (Scene)scn, true)
        {
            m_resMgr = resMgr;
            m_graphicsMgr = graphicsMgr;
            m_spriteBatch = spriteBatch;
            m_hudResources = new HUDResources(debugMgr, resMgr);

            m_resCapsidIcon = new TextureResHandle(debugMgr, resMgr, "Content\\HUD\\", "HudAlertCapsid");
            m_resImmuneIcon = new TextureResHandle(debugMgr, resMgr, "Content\\HUD\\", "HudAlertImmuneSys");
            m_resMedicationIcon = new TextureResHandle(debugMgr, resMgr, "Content\\HUD\\", "HudAlertMedication");

            m_clusterMenu = new Common.InGameMenu(
                        debugMgr, resMgr, graphicsMgr, spriteBatch,
                        scn);
            m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_DIVIDE_WND_ID);
            m_clusterMenu.Active = false;

#if DEBUG
            m_frameRateGObj = new FramerateGObj(
                uint.MaxValue, "Content\\Fonts\\", "HUDFont", debugMgr, resMgr,
                scn, false, graphicsMgr);
#endif

            //asserts
            m_debugMgr.Assert(m_resMgr != null, "HUD:Constructor - res mgr is null.");
            m_debugMgr.Assert(m_graphicsMgr != null, "HUD:Constructor - graphics mgr is null.");
            m_debugMgr.Assert(m_spriteBatch != null, "HUD:Constructor - sprite batch is null.");

            m_hudCam = new CameraGObj(
                uint.MaxValue, debugMgr, resMgr, scn, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 0.5f, 50f);

            m_hudFont = new SpriteFontResHandle(
                m_debugMgr, m_resMgr, "Content\\Fonts\\", "HUDFont");

            m_statsString = "";

            m_isWidescreen = Microsoft.Xna.Framework.Graphics.GraphicsAdapter.DefaultAdapter.IsWideScreen;

            //HUD must be rendered last - since its an overlay
            Visible = false;
        }

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                if ((!m_hudCam.Init()) || (!m_clusterMenu.Init())
#if DEBUG
                    || (!m_frameRateGObj.Init())
#endif
                    )
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        public override bool Reinit()
        {
            bool retVal = true;
            
                if ((!m_hudCam.Reinit()) || (!m_clusterMenu.Reinit())
#if DEBUG
                    || (!m_frameRateGObj.Reinit())
#endif
                    )
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;

            return retVal;
        }

        #region loading

        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if (    (!m_hudCam.Load())  ||
                        (!m_hudFont.Load()) ||
                        (!m_hudResources.Load()) ||
                        (!m_clusterMenu.Load()) ||
                        (!m_resCapsidIcon.Load()) ||
                        (!m_resImmuneIcon.Load()) ||
                        (!m_resMedicationIcon.Load())
#if DEBUG
                    || (!m_frameRateGObj.Load())
#endif
                    )
                    retVal = false;

                #region draw positions

                int iconHeight = 
                    (int)(((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonA.GetResource).Height / 2);
                int iconWidth = 
                    (int)(((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonA.GetResource).Width / 2);

                int fontHeight = ((Microsoft.Xna.Framework.Graphics.SpriteFont)m_hudFont.GetResource).LineSpacing;

                int spacerX = 210;
                int spacerY = 20;

                int posX = 33;
                int posY = m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight
                    - iconHeight - spacerY;

                m_iconSlot1 = new Microsoft.Xna.Framework.Rectangle(posX,
                    posY - iconHeight - spacerY,
                    iconWidth, iconHeight);
                m_labelSlot1 = new Microsoft.Xna.Framework.Vector2(m_iconSlot1.X, m_iconSlot1.Y);
                m_labelSlot1.X += iconWidth + 3; m_labelSlot1.Y += (int)((iconHeight - fontHeight) / 2);

                m_iconSlot2 = new Microsoft.Xna.Framework.Rectangle(posX, posY, iconWidth, iconHeight);
                m_labelSlot2 = new Microsoft.Xna.Framework.Vector2(m_iconSlot2.X, m_iconSlot2.Y);
                m_labelSlot2.X += iconWidth + 3; m_labelSlot2.Y += (int)((iconHeight - fontHeight) / 2);

                m_iconSlot3 = new Microsoft.Xna.Framework.Rectangle(posX + spacerX,
                    posY - iconHeight - spacerY,
                    iconWidth, iconHeight);
                m_labelSlot3 = new Microsoft.Xna.Framework.Vector2(m_iconSlot3.X, m_iconSlot3.Y);
                m_labelSlot3.X += iconWidth + 3; m_labelSlot3.Y += (int)((iconHeight - fontHeight) / 2);

                m_iconSlot4 = new Microsoft.Xna.Framework.Rectangle(posX + spacerX, posY, iconWidth, iconHeight);
                m_labelSlot4 = new Microsoft.Xna.Framework.Vector2(m_iconSlot4.X, m_iconSlot4.Y);
                m_labelSlot4.X += iconWidth + 3; m_labelSlot4.Y += (int)((iconHeight - fontHeight) / 2);

                iconWidth = 
                    (int)(((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonLB.GetResource).Width / 2);

                m_iconSlot5 = new Microsoft.Xna.Framework.Rectangle(posX + (2 * spacerX),
                    posY - iconHeight - spacerY,
                    iconWidth, iconHeight);
                m_labelSlot5 = new Microsoft.Xna.Framework.Vector2(m_iconSlot5.X, m_iconSlot5.Y);
                m_labelSlot5.X += iconWidth + 3; m_labelSlot5.Y += (int)((iconHeight - fontHeight) / 2);

                m_iconSlot6 = new Microsoft.Xna.Framework.Rectangle(posX + (2 * spacerX),
                    posY,
                    iconWidth, iconHeight);
                m_labelSlot6 = new Microsoft.Xna.Framework.Vector2(m_iconSlot6.X, m_iconSlot6.Y);
                m_labelSlot6.X += iconWidth + 3; m_labelSlot6.Y += (int)((iconHeight - fontHeight) / 2);

                iconHeight = 
                    (int)(((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonRightTS.GetResource).Height / 3);
                iconWidth = 
                    (int)(((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonRightTS.GetResource).Width / 3);

                m_iconSlot7 = new Microsoft.Xna.Framework.Rectangle(posX,
                    posY - iconHeight - (4 * spacerY),
                    iconWidth, iconHeight);
                m_labelSlot7 = new Microsoft.Xna.Framework.Vector2(m_iconSlot7.X, m_iconSlot7.Y);
                m_iconSlot7.X += iconWidth + 3; m_iconSlot7.Y += (int)((iconHeight - fontHeight) / 2);

                #endregion

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
                if (    (!m_hudCam.Unload())    ||
                        (!m_hudFont.Unload())   ||
                        (!m_hudResources.Unload()) ||
                        (!m_clusterMenu.Unload()) ||
                        (!m_resCapsidIcon.Unload()) ||
                        (!m_resImmuneIcon.Unload()) ||
                        (!m_resMedicationIcon.Unload())
#if DEBUG
                    || (!m_frameRateGObj.Unload())
#endif
                    )
                    retVal = false;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        public override bool Deinit()
        {
            bool retVal = true;
            if (m_isInit)
            {
                if ((!m_hudCam.Deinit()) || (!m_clusterMenu.Deinit())
#if DEBUG
                    || (!m_frameRateGObj.Deinit())
#endif
                    )
                    retVal = false;

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        #endregion

        #region game loop

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
#if DEBUG
            m_frameRateGObj.Update(gameTime);
#endif
        }

        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
#if DEBUG
            m_frameRateGObj.Animate(gameTime);
#endif
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            if (m_myVirus != null)
            {
                m_spriteBatch.Begin();

                DrawHUDInfo();

                #region show options

                if (m_myVirus.SelectedCluster != null)
                {
                    switch (m_myVirus.SelectedCluster.stateData.actionState)
                    {
                        case Biophage.Game.Stages.Game.Common.CellActionState.IDLE:
                            DrawClusterIdleOptions();
                            break;

                        case Biophage.Game.Stages.Game.Common.CellActionState.CHASING_UCELL_TOINFECT:
                        case Biophage.Game.Stages.Game.Common.CellActionState.CHASING_ENEMY_TO_BATTLE:
                        case Biophage.Game.Stages.Game.Common.CellActionState.CHASING_CLUST_TO_COMBINE:
                        case Biophage.Game.Stages.Game.Common.CellActionState.EVADING_ENEMY:
                            DrawClusterInActionOptions();
                            break;

                        //this scenario is when the player is controlling the cursor
                        case Biophage.Game.Stages.Game.Common.CellActionState.WAITING_FOR_ORDER:
                            DrawClusterWaitingOptions();
                            break;

                        case Biophage.Game.Stages.Game.Common.CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED:
                            DrawClusterWaitingOptions();
                            DrawClusterWaitingMyClustSelect();
                            break;

                        case Biophage.Game.Stages.Game.Common.CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED:
                            DrawClusterWaitingOptions();
                            DrawClusterWaitingEnemyClusterSelected();
                            break;

                        case Biophage.Game.Stages.Game.Common.CellActionState.WAITING_WITH_UCELL_SELECTED:
                            DrawClusterWaitingOptions();
                            DrawClusterWaitingUCellSelected();
                            break;

                        default:
                            break; //do nothing - shouldn't even occur
                    }
                }

                #endregion

                m_spriteBatch.End();
            }
        }

        private void DrawHUDInfo()
        {
            string showInfectPercent = "INFECTION: %" + 
                m_myVirus.virusStateData.infectPercentage.ToString("F2", CultureInfo.InvariantCulture);
            string showCurrentRank = "RANK: " + m_myVirus.virusStateData.rank.ToString();

            m_statsString = showInfectPercent + " | " + showCurrentRank;

#if DEBUG
            if (m_scn.m_sessionDetails.isMultiplayer)
            {
                m_statsString += "\n" + "Av Net Up = " +
                    m_scn.m_sessionDetails.netSessionComponent.GetNetworkSession.BytesPerSecondSent
                    + " | Av Net Down = " +
                    m_scn.m_sessionDetails.netSessionComponent.GetNetworkSession.BytesPerSecondReceived;
            }
            m_frameRateGObj.IncrementFrameCounter();
            m_statsString += "\n" + "fps = " + m_frameRateGObj.FramesPerSecond;
#endif

            //HUD stats
            DrawHUDString(m_statsStrPos, m_statsString, m_myVirus.virusStateData.colour);

            //countdowns - virus capsid
            Microsoft.Xna.Framework.Vector2 alertPos = Microsoft.Xna.Framework.Vector2.Zero;
            Microsoft.Xna.Framework.Vector2 alertPosStr = Microsoft.Xna.Framework.Vector2.Zero;
            alertPos.X = m_graphicsMgr.PreferredBackBufferWidth - 140;
            alertPosStr.X = alertPos.X + 50f;
            alertPosStr.Y = 7f;
            
            if (showImmuneAlert)
            {
                alertPos.Y = 20f; alertPosStr.Y = alertPos.Y + 7f;
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_resImmuneIcon.GetResource,
                    alertPos, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(alertPosStr, immuneAlertCountDown.ToString(), m_myVirus.virusStateData.colour);
            }
            if (showMedicationAlert)
            {
                alertPos.Y = 70f; alertPosStr.Y = alertPos.Y + 7f;
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_resMedicationIcon.GetResource,
                    alertPos, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(alertPosStr, medicationAlertCountDown.ToString(), m_myVirus.virusStateData.colour);
            }
            if (ShowCapsidAlert)
            {
                alertPos.Y = 120f; alertPosStr.Y = alertPos.Y + 7f;
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_resCapsidIcon.GetResource,
                    alertPos, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(alertPosStr, CapsidCountDown.ToString(), m_myVirus.virusStateData.colour);
            }
        }

        private void DrawClusterWaitingUCellSelected()
        {
            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonA.GetResource,
                m_iconSlot1, Microsoft.Xna.Framework.Graphics.Color.White);
            DrawHUDString(m_labelSlot1, "INFECT", Microsoft.Xna.Framework.Graphics.Color.White);
        }

        private void DrawClusterWaitingMyClustSelect()
        {
            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonA.GetResource,
                m_iconSlot1, Microsoft.Xna.Framework.Graphics.Color.White);
            DrawHUDString(m_labelSlot1, "COMBINE WITH", Microsoft.Xna.Framework.Graphics.Color.White);
        }

        private void DrawClusterWaitingEnemyClusterSelected()
        {
            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonA.GetResource,
                m_iconSlot1, Microsoft.Xna.Framework.Graphics.Color.White);
            DrawHUDString(m_labelSlot1, "BATTLE", Microsoft.Xna.Framework.Graphics.Color.White);

            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonX.GetResource,
                m_iconSlot2, Microsoft.Xna.Framework.Graphics.Color.White);
            DrawHUDString(m_labelSlot2, "EVADE", Microsoft.Xna.Framework.Graphics.Color.White);

            string clustName;
            Common.CellCluster enemClust = (Common.CellCluster)m_myVirus.SelectedCluster.stateData.actionReObject;

            if (enemClust.stateData.numWhiteBloodCell == 0)
                clustName = enemClust.stateData.virusRef.virusStateData.ownerName;
            else
                clustName = "White blood cell";

            DrawHUDString(
                new Microsoft.Xna.Framework.Vector2(
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth / 2,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight / 2),
                clustName, Microsoft.Xna.Framework.Graphics.Color.Red);
        }

        void DrawHUDString(Microsoft.Xna.Framework.Vector2 pos, string str,Microsoft.Xna.Framework.Graphics.Color sColour)
        {
            m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_hudFont.GetResource,
                str, pos, Microsoft.Xna.Framework.Graphics.Color.Black);

            pos.X -= 1;
            pos.Y -= 1;

            m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_hudFont.GetResource,
                str, pos, sColour);
        }

        void DrawClusterIdleOptions()
        {
            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonA.GetResource, 
                m_iconSlot1, Microsoft.Xna.Framework.Graphics.Color.White);
            DrawHUDString(m_labelSlot1, "DIVIDE CELLS", Microsoft.Xna.Framework.Graphics.Color.White);

            if (m_myVirus.SelectedCluster.CanHybreed)
            {
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonY.GetResource,
                    m_iconSlot2, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(m_labelSlot2, "HYBRID CELLS", Microsoft.Xna.Framework.Graphics.Color.White);
            }

            if (m_myVirus.SelectedCluster.stateData.numCellsTotal > 1)
            {
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonB.GetResource,
                    m_iconSlot3, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(m_labelSlot3, "SPLIT CLUSTER", Microsoft.Xna.Framework.Graphics.Color.White);
            }

            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonX.GetResource, 
                m_iconSlot4, Microsoft.Xna.Framework.Graphics.Color.White);
            DrawHUDString(m_labelSlot4, "ACTION SELECT", Microsoft.Xna.Framework.Graphics.Color.White);

            if (m_myVirus.virusStateData.clusters.Count > 1)
            {
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonLB.GetResource,
                    m_iconSlot5, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(m_labelSlot5, "PREVIOUS CLUSTER", Microsoft.Xna.Framework.Graphics.Color.White);

                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonRB.GetResource,
                    m_iconSlot6, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(m_labelSlot6, "NEXT CLUSTER", Microsoft.Xna.Framework.Graphics.Color.White);

                if (m_myVirus.m_underAttack)
                {
                    m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonRightTS.GetResource,
                        m_iconSlot7, Microsoft.Xna.Framework.Graphics.Color.White);
                    DrawHUDString(m_labelSlot7, "CLUSTER UNDER ATTACK", Microsoft.Xna.Framework.Graphics.Color.White);
                }
            }
        }

        void DrawClusterWaitingOptions()
        {
            //always show
            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonB.GetResource,
                m_iconSlot4, Microsoft.Xna.Framework.Graphics.Color.White);
            DrawHUDString(m_labelSlot4, "CANCEL ACTION", Microsoft.Xna.Framework.Graphics.Color.White);
        }

        void DrawClusterInActionOptions()
        {
            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonA.GetResource,
                m_iconSlot1, Microsoft.Xna.Framework.Graphics.Color.White);
            DrawHUDString(m_labelSlot1, "DIVIDE CELLS", Microsoft.Xna.Framework.Graphics.Color.White);

            if (m_myVirus.SelectedCluster.CanHybreed)
            {
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonY.GetResource,
                    m_iconSlot2, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(m_labelSlot2, "HYBRID CELLS", Microsoft.Xna.Framework.Graphics.Color.White);
            }

            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonB.GetResource,
                m_iconSlot4, Microsoft.Xna.Framework.Graphics.Color.White);
            DrawHUDString(m_labelSlot4, "CANCEL ACTION", Microsoft.Xna.Framework.Graphics.Color.White);

            if (m_myVirus.virusStateData.clusters.Count > 1)
            {
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonLB.GetResource,
                    m_iconSlot5, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(m_labelSlot5, "PREVIOUS CLUSTER", Microsoft.Xna.Framework.Graphics.Color.White);

                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonRB.GetResource,
                    m_iconSlot6, Microsoft.Xna.Framework.Graphics.Color.White);
                DrawHUDString(m_labelSlot6, "NEXT CLUSTER", Microsoft.Xna.Framework.Graphics.Color.White);

                if (m_myVirus.m_underAttack)
                {
                    m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hudResources.ButtonRightTS.GetResource,
                        m_iconSlot7, Microsoft.Xna.Framework.Graphics.Color.White);
                    DrawHUDString(m_labelSlot7, "CLUSTER UNDER ATTACK", Microsoft.Xna.Framework.Graphics.Color.White);
                }
            }
        }

        #endregion

        #endregion
    }
}
