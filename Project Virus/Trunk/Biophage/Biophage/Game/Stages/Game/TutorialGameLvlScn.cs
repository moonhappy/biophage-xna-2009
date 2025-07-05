using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
    public class TutorialGameLvlScn : TrialGameLvlScn
    {
        #region constants

        private const float TUT_TITLE_Y = 50f;
        private const float TUT_TEXT_Y = 150f;

        #endregion

        #region fields

        bool showTutorialScreen = true;
        int tutorialScreenId = 0;

        bool tutJustStarted = true;

        //audio objects
        private Microsoft.Xna.Framework.Audio.AudioEngine m_audEngine = null;
        private Microsoft.Xna.Framework.Audio.SoundBank m_audSoundBank = null;
        private Microsoft.Xna.Framework.Audio.WaveBank m_audWaveBank = null;

        private Microsoft.Xna.Framework.Audio.Cue[] m_audCues;

        //continue icon
        private TextureResHandle m_iconContinue;
        private Microsoft.Xna.Framework.Rectangle m_iconContinueSlot;
        private Microsoft.Xna.Framework.Vector2 m_labelContinueSlot;

        #region tutorial text
        SpriteFontResHandle m_tutTitleFont;
        SpriteFontResHandle m_tutTextFont;

        Microsoft.Xna.Framework.Vector2 TitleFontPos;
        Microsoft.Xna.Framework.Vector2 TitleFontOrigin;

        Microsoft.Xna.Framework.Vector2 TextFontPos;
        Microsoft.Xna.Framework.Vector2 TextFontOrigin;
        #endregion

        #region cell images

        private TextureResHandle m_tutRBCimage;
        private TextureResHandle m_tutPLTimage;
        private TextureResHandle m_tutTNKimage;
        private TextureResHandle m_tutSILimage;

        private Microsoft.Xna.Framework.Rectangle m_cellImgSlot;

        #endregion

        #endregion

        #region methods

        #region construction

        public TutorialGameLvlScn(uint id, DebugManager debugMgr, ResourceManager resourceMgr,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                    Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                    SpriteFontResHandle sceneFont,
                                    Scene parent)
            : base(id, debugMgr, resourceMgr, graphicsMgr, spriteBatch, sceneFont, parent)
        {
            m_tutTitleFont = new SpriteFontResHandle(
                debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont");
            m_tutTextFont = new SpriteFontResHandle(
                debugMgr, resourceMgr, "Content\\Fonts\\", "HUDFont");

            TitleFontPos = new Microsoft.Xna.Framework.Vector2(
                graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f, 
                TUT_TITLE_Y);

            TextFontPos = new Microsoft.Xna.Framework.Vector2(
                graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f,
                TUT_TEXT_Y);

            //sounds
            #region sounds

            m_audEngine = new Microsoft.Xna.Framework.Audio.AudioEngine("Content\\Sounds\\Tutorial\\TutorialSNDs.xgs");
            m_audWaveBank = new Microsoft.Xna.Framework.Audio.WaveBank(m_audEngine,
                "Content\\Sounds\\Tutorial\\TutWAVs.xwb");
            m_audSoundBank = new Microsoft.Xna.Framework.Audio.SoundBank(m_audEngine,
                "Content\\Sounds\\Tutorial\\TutSNDs.xsb");

            m_debugMgr.Assert(m_audEngine != null, "TutorialGameLvlScn:Load - m_audEngine is null.");
            m_debugMgr.Assert(m_audWaveBank != null, "TutorialGameLvlScn:Load - m_audWaveBank is null.");
            m_debugMgr.Assert(m_audSoundBank != null, "TutorialGameLvlScn:Load - m_audSoundBank is null.");

            m_audCues = new Microsoft.Xna.Framework.Audio.Cue[16];
            for (int i = 0; i < m_audCues.Length; ++i)
            {
                m_audCues[i] = null;
            }

            #endregion

            #region cell images

            m_tutRBCimage = new TextureResHandle(
                m_debugMgr, m_resMgr, "Content\\TutImages\\", "RBCimage");
            m_tutPLTimage = new TextureResHandle(
                m_debugMgr, m_resMgr, "Content\\TutImages\\", "PLTimage");
            m_tutTNKimage = new TextureResHandle(
                m_debugMgr, m_resMgr, "Content\\TutImages\\", "TNKimage");
            m_tutSILimage = new TextureResHandle(
                m_debugMgr, m_resMgr, "Content\\TutImages\\", "SILimage");

            #endregion

            m_iconContinue = new TextureResHandle(
                m_debugMgr, m_resMgr, "Content\\Common\\ButtonImages\\", "xboxControllerButtonA");
        }

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                #region from base

                CreatePhysics();

                m_physicsWorld.Gravity = new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f);

                Common.LevelEnvironment env = new Common.LevelEnvironment(
                    m_levelEnvId,
                    new ModelResHandle(m_debugMgr, m_resMgr, "Content\\Models\\Levels\\", "TrialLevel"),
                    m_debugMgr, m_resMgr, this,
                    new JigLibX.Collision.MaterialProperties(0.8f, 0.8f, 0.7f),
                    100f, Microsoft.Xna.Framework.Vector3.Zero);

                //set psuedo random cell positions
                Random rnd = new Random(0);
                Microsoft.Xna.Framework.Vector3 cellLoc;

                //uninfected cells - 10 rbcs above one another
                Common.Cells.UninfectedCell uCell;
                for (byte i = 0; i < 20; i++)
                {
                    cellLoc = new Microsoft.Xna.Framework.Vector3(
                        (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                    if (cellLoc.Length() != 0)
                        cellLoc.Normalize();
                    cellLoc *= rnd.Next(-50, 50);

                    uCell = new Common.Cells.UninfectedCell(
                        UCellGobjID(i),
                        m_rbcStaticData,
                        new ModelResHandle(m_debugMgr, m_resMgr, "Content\\Models\\RedBloodCell\\", "RedBloodCell"),
                        m_debugMgr, m_resMgr, this, true, cellLoc,
                        Microsoft.Xna.Framework.Quaternion.Identity,
                        new JigLibX.Geometry.Box(cellLoc, Microsoft.Xna.Framework.Matrix.Identity,
                            new Microsoft.Xna.Framework.Vector3(2f, 0.5f, 2f)),
                        new JigLibX.Collision.MaterialProperties(0.8f, 0.8f, 0.7f),
                        1f);

                    m_uninfectedCells.AddLast(i);
                    m_ucellObjs.AddLast(uCell);
                }
                for (byte i = 20; i < 30; i++)
                {
                    cellLoc = new Microsoft.Xna.Framework.Vector3(
                        (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                    if (cellLoc.Length() != 0)
                        cellLoc.Normalize();
                    cellLoc *= rnd.Next(-50, 50);

                    uCell = new Common.Cells.UninfectedCell(
                        UCellGobjID(i),
                        m_pcStaticData,
                        new ModelResHandle(m_debugMgr, m_resMgr, "Content\\Models\\Platelet\\", "Platelet"),
                        m_debugMgr, m_resMgr, this, true, cellLoc,
                        Microsoft.Xna.Framework.Quaternion.Identity,
                        new JigLibX.Geometry.Box(cellLoc, Microsoft.Xna.Framework.Matrix.Identity,
                            new Microsoft.Xna.Framework.Vector3(2f, 0.5f, 2f)),
                        new JigLibX.Collision.MaterialProperties(0.8f, 0.8f, 0.7f),
                        1f);

                    m_uninfectedCells.AddLast(i);
                    m_ucellObjs.AddLast(uCell);
                }

                #region bases

                m_camera = new Common.FollowCamera(
                    m_camId, m_debugMgr, m_resMgr, this,
                    m_graphicsMgr,
                    Microsoft.Xna.Framework.Vector3.Zero,
                    Microsoft.Xna.Framework.Vector3.Up,
                    Microsoft.Xna.Framework.Vector3.Forward,
                    0.0125f, 220f, 10f);

                m_cursor = new Common.PlayerCursor(
                    m_cursorId, Microsoft.Xna.Framework.Vector3.Zero,
                    m_debugMgr, m_resMgr, this);

                //init all gobjs
                foreach (KeyValuePair<uint, IGameObject> gobj in m_gameObjects)
                {
                    if (!gobj.Value.Init())
                        retVal = false;
                }

                //init camera and menu
                if ((!m_camera.Init()) ||
                        (!m_menu.Init()) ||
                        (!m_fadeOverlay.Init()) ||
                        (!m_prompt.Init()) ||
                        (!m_msgBox.Init()))
                    retVal = false;

                //init colour
                m_clearColour = m_initClearColour;
                m_isPaused = false;

                #endregion

                #endregion

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }
            return retVal;
        }

        #region loading

        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                retVal = base.Load();

                if ((!m_tutTitleFont.Load()) ||
                    (!m_tutTextFont.Load()) ||
                    
                    (!m_iconContinue.Load()) ||
                    
                    (!m_tutRBCimage.Load()) ||
                    (!m_tutPLTimage.Load()) ||
                    (!m_tutTNKimage.Load()) ||
                    (!m_tutSILimage.Load()))
                    retVal = false;

                #region continue postions

                int iconHeight =
                    (int)(((Microsoft.Xna.Framework.Graphics.Texture2D)m_iconContinue.GetResource).Height / 2);
                int iconWidth =
                    (int)(((Microsoft.Xna.Framework.Graphics.Texture2D)m_iconContinue.GetResource).Width / 2);

                int fontHeight = ((Microsoft.Xna.Framework.Graphics.SpriteFont)m_tutTextFont.GetResource).LineSpacing;

                int spacerX = 400;
                int spacerY = 20;

                int posX = 33;
                int posY = m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight
                    - iconHeight - spacerY;

                m_iconContinueSlot = new Microsoft.Xna.Framework.Rectangle(posX + spacerX, posY, iconWidth, iconHeight);
                m_labelContinueSlot = new Microsoft.Xna.Framework.Vector2(m_iconContinueSlot.X, m_iconContinueSlot.Y);
                m_labelContinueSlot.X += iconWidth + 3; m_labelContinueSlot.Y += (int)((iconHeight - fontHeight) / 2);

                #endregion

                //cell image demo positions
                iconHeight = iconWidth = 300;
                posX = m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth / 2;
                posY = m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight / 3;

                m_cellImgSlot = new Microsoft.Xna.Framework.Rectangle(posX - (iconWidth / 2), posY, iconWidth, iconHeight);

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
                retVal = base.Unload();

                if ((!m_tutTitleFont.Unload()) ||
                    (!m_tutTextFont.Unload()) ||
                    
                    (!m_iconContinue.Unload()) ||

                    (!m_tutRBCimage.Unload()) ||
                    (!m_tutPLTimage.Unload()) ||
                    (!m_tutTNKimage.Unload()) ||
                    (!m_tutSILimage.Unload()))
                    retVal = false;

                //sounds
                #region sounds

                for (int i = 0; i < m_audCues.Length; ++i)
                {
                    m_audCues[i] = null;
                }

                #endregion

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }
            return retVal;
        }

        #endregion

        #endregion

        #region creation

        public static TutorialGameLvlScn CreateTutorial(DebugManager debugMgr, ResourceManager resourceMgr,
                                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                                Scene parent)
        {
            TutorialGameLvlScn scn = new TutorialGameLvlScn(
                GlobalConstants.TUTORIAL_LVL_SCN_ID, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                parent);

            return scn;
        }

        #endregion

        #region overrides

        public override void SetSession(SessionDetails sessionDetails)
        {
            base.SetSession(sessionDetails);

            //reset tutorial
            tutJustStarted = true;
            showTutorialScreen = true;
            tutorialScreenId = 0;
        }

        public override void HostCreateNewClusterFromUCell(byte ucellId, byte virusId)
        {
            base.HostCreateNewClusterFromUCell(ucellId, virusId);

            //show the next screen
            if (tutorialScreenId == 2)
            {
                showTutorialScreen = true;
                IsPaused = true;
                if (m_audCues[tutorialScreenId] == null)
                    m_audCues[tutorialScreenId] = m_audSoundBank.GetCue(tutorialScreenId.ToString());

                m_audCues[tutorialScreenId].Play();
            }
        }

        public override void HostClusterInfectUCell(byte ucellId, byte clusterId)
        {
            base.HostClusterInfectUCell(ucellId, clusterId);

            //show the next screen
            if (tutorialScreenId == 11)
            {
                showTutorialScreen = true;
                IsPaused = true;
                if (m_audCues[tutorialScreenId] == null)
                    m_audCues[tutorialScreenId] = m_audSoundBank.GetCue(tutorialScreenId.ToString());

                m_audCues[tutorialScreenId].Play();
            }
        }

        public override void HostClusterDivideCells(Common.CellCluster cluster, 
            byte addRBC, byte addPLT, byte addTNK, byte addSIL)
        {
            base.HostClusterDivideCells(cluster, addRBC, addPLT, addTNK, addSIL);

            //show the next screen
            if (tutorialScreenId == 14)
            {
                showTutorialScreen = true;
                IsPaused = true;
                if (m_audCues[tutorialScreenId] == null)
                    m_audCues[tutorialScreenId] = m_audSoundBank.GetCue(tutorialScreenId.ToString());

                m_audCues[tutorialScreenId].Play();
            }
        }

        public override void HostSplitCluster(Common.CellCluster srcCluster, 
            byte splRBC, byte splPLT, byte splTNK, byte splSIL, byte splSHY, byte splMHY, byte splBHY)
        {
            base.HostSplitCluster(srcCluster, splRBC, splPLT, splTNK, splSIL, splSHY, splMHY, splBHY);

            //show the next screen
            if (tutorialScreenId == 15)
            {
                showTutorialScreen = true;
                IsPaused = true;
                if (m_audCues[tutorialScreenId] == null)
                    m_audCues[tutorialScreenId] = m_audSoundBank.GetCue(tutorialScreenId.ToString());

                m_audCues[tutorialScreenId].Play();
            }
        }

        #endregion

        #region game loop

        #region input

        public override void Input(Microsoft.Xna.Framework.GameTime gameTime, 
            ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
            , ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
            )
        {
            //this allows the tutorial to trim off undesired actions from the player
            Microsoft.Xna.Framework.Input.GamePadState changedNewGPState = newGPState;
#if !XBOX
            Microsoft.Xna.Framework.Input.KeyboardState changedNewKBState = newKBState;
            Microsoft.Xna.Framework.Input.Keys[] kbKeys;
#endif

            #region trim inputs

            if ((tutorialScreenId == 10) || (tutorialScreenId == 11))
            {
                if (!m_cursor.Visible)
                {
                    //only allow action select to be pressed
                    changedNewGPState = new Microsoft.Xna.Framework.Input.GamePadState(
                        newGPState.ThumbSticks, newGPState.Triggers,
                        new Microsoft.Xna.Framework.Input.GamePadButtons(
                            ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X)) ?
                                Microsoft.Xna.Framework.Input.Buttons.X : Microsoft.Xna.Framework.Input.Buttons.RightStick)),
                                newGPState.DPad);

#if !XBOX
                    kbKeys = new Microsoft.Xna.Framework.Input.Keys[1];
                    if (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X))
                        kbKeys[0] = Microsoft.Xna.Framework.Input.Keys.X;
                    else
                        kbKeys[0] = Microsoft.Xna.Framework.Input.Keys.U;

                    changedNewKBState = new Microsoft.Xna.Framework.Input.KeyboardState(
                        kbKeys);
#endif
                }

            }

            else if ((tutorialScreenId == 13) || (tutorialScreenId == 14))
            {
                if (!GetHUD.m_clusterMenu.Active)
                {
                    //only allow divide cells to be pressed
                    changedNewGPState = new Microsoft.Xna.Framework.Input.GamePadState(
                        newGPState.ThumbSticks, newGPState.Triggers,
                        new Microsoft.Xna.Framework.Input.GamePadButtons(
                            ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A)) ?
                                Microsoft.Xna.Framework.Input.Buttons.A : Microsoft.Xna.Framework.Input.Buttons.RightStick)),
                                newGPState.DPad);

#if !XBOX
                    kbKeys = new Microsoft.Xna.Framework.Input.Keys[1];
                    if (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
                        kbKeys[0] = Microsoft.Xna.Framework.Input.Keys.A;
                    else
                        kbKeys[0] = Microsoft.Xna.Framework.Input.Keys.U;

                    changedNewKBState = new Microsoft.Xna.Framework.Input.KeyboardState(
                        kbKeys);
#endif
                }
            }

            else if (tutorialScreenId == 15)
            {
                if (!GetHUD.m_clusterMenu.Active)
                {
                    //only allow split cluster to be pressed
                    changedNewGPState = new Microsoft.Xna.Framework.Input.GamePadState(
                        newGPState.ThumbSticks, newGPState.Triggers,
                        new Microsoft.Xna.Framework.Input.GamePadButtons(
                            ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B)) ?
                                Microsoft.Xna.Framework.Input.Buttons.B : Microsoft.Xna.Framework.Input.Buttons.RightStick)),
                                newGPState.DPad);

#if !XBOX
                    kbKeys = new Microsoft.Xna.Framework.Input.Keys[1];
                    if (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B))
                        kbKeys[0] = Microsoft.Xna.Framework.Input.Keys.B;
                    else
                        kbKeys[0] = Microsoft.Xna.Framework.Input.Keys.U;

                    changedNewKBState = new Microsoft.Xna.Framework.Input.KeyboardState(
                        kbKeys);
#endif
                }
            }

            #endregion

            #region game menu

            if (newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Start)
#if !XBOX
 || newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)
#endif
)
            {
                GetMenu.Active = true;
                //pause game
                IsPaused = true;
            }

            #endregion

            if (showTutorialScreen)
            {
                #region increment the tutorial state
                if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) &&
                    m_prevGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.A))
#if !XBOX
                    || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter) &&
                        m_prevKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Enter))
#endif
                        )
                {
                    IncrementTutorial();
                }
                #endregion
            }
            else
            {
                #region base input with trimed input
                base.Input(gameTime, ref changedNewGPState 
#if !XBOX
                ,ref changedNewKBState
#endif
                );
                #endregion
            }
        }

        private void IncrementTutorial()
        {
            //1. stop current sound
            if ((tutorialScreenId >= 0) && (tutorialScreenId < m_audCues.Length))
            {
                if (m_audCues[tutorialScreenId] != null)
                {
                    m_audCues[tutorialScreenId].Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
                    m_audCues[tutorialScreenId] = null;
                }
            }
            else if (tutorialScreenId != 16)
                tutorialScreenId = -1;

            //2. do next tutorial state
            tutorialScreenId++;

            switch (tutorialScreenId)
            {
                case 2:
                case 10:
                case 11:
                case 13:
                case 14:
                case 15:
                case 17:
                    showTutorialScreen = false;
                    break;
                default:
                    showTutorialScreen = true;
                    break;
            }

            if (tutorialScreenId == 17)
                tutorialScreenId = 16;

            if (showTutorialScreen)
            {
                if (tutorialScreenId < m_audCues.Length)
                {
                    if (m_audCues[tutorialScreenId] == null)
                        m_audCues[tutorialScreenId] = m_audSoundBank.GetCue(tutorialScreenId.ToString());

                    m_audCues[tutorialScreenId].Play();
                }
            }
            else
                IsPaused = false;
        }

        #endregion

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //check if first starting tutorial
            if (tutJustStarted)
            {
                tutJustStarted = false;
                if (m_audCues[tutorialScreenId] == null)
                    m_audCues[tutorialScreenId] = m_audSoundBank.GetCue(tutorialScreenId.ToString());

                m_audCues[tutorialScreenId].Play();
            }

            if (tutorialScreenId == 10)
            {
                if ((!showTutorialScreen) && m_cursor.Visible)
                {
                    showTutorialScreen = true;

                    if (m_audCues[tutorialScreenId] == null)
                        m_audCues[tutorialScreenId] = m_audSoundBank.GetCue(tutorialScreenId.ToString());

                    m_audCues[tutorialScreenId].Play();
                }
            }
            else if (tutorialScreenId == 13)
            {
                if ((!showTutorialScreen) && GetHUD.m_clusterMenu.Active)
                {
                    showTutorialScreen = true;

                    if (m_audCues[tutorialScreenId] == null)
                        m_audCues[tutorialScreenId] = m_audSoundBank.GetCue(tutorialScreenId.ToString());

                    m_audCues[tutorialScreenId].Play();
                }
            }

            if (showTutorialScreen != IsPaused)
                IsPaused = showTutorialScreen;
            else
                base.Update(gameTime);

            //audio engine update
            m_audEngine.Update();
        }

        protected override void HostCheckGameConditions(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //the tutorial is open ended - but check conditions

            //if virus capsid dies - restart
            if (!myVirus.Active)
            {
                Unload();
                Deinit();

                Init();
                Load();

                SetSession(m_sessionDetails);

                tutorialScreenId = 1;
                showTutorialScreen = true;
            }
        }

        #region draw routines

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            #region draw the game

            graphicsDevice.Clear(m_clearColour);

            //draw level
            m_gameObjects[m_levelEnvId].DoDraw(gameTime, graphicsDevice, Camera);

            //draw the sorted gobjs
            foreach (KeyValuePair<float, IGameObject> gobjKVP in m_drawableObjs)
            {
                gobjKVP.Value.DoDraw(gameTime, graphicsDevice, Camera);
            }

            //draw cursor
            m_cursor.DoDraw(gameTime, graphicsDevice, Camera);

            //do the bloom effect
            DrawBloomEffect(gameTime, graphicsDevice);

            //draw the HUD
            if (!m_finishedRanksScrn.isGameOver)
                GetHUD.Draw(gameTime, graphicsDevice, m_camera);

            //draw cluster menu - if avaliable
            if (myVirus.SelectedCluster != null)
                if (GetHUD.m_clusterMenu.Active)
                {
                    //make sure cluster is not dead
                    if (GetHUD.m_clusterMenu.Cluster.Active)
                        GetHUD.m_clusterMenu.DoDraw(gameTime, graphicsDevice, m_camera);
                    else
                    {
                        GetHUD.m_clusterMenu.Active = false;
                    }
                }

            //draw the finished ranks screen - if game is over
            if (m_finishedRanksScrn.isGameOver)
                m_finishedRanksScrn.Draw(gameTime, graphicsDevice, Camera);

            #endregion

            //draw tutorial screen
            if (showTutorialScreen)
                DrawTutorialScreen(gameTime, graphicsDevice);

            //draw the menu
            if (m_menu.Active)
                m_menu.DoDraw(gameTime, graphicsDevice, m_camera);
        }

        private void DrawTutorialScreen(    Microsoft.Xna.Framework.GameTime gameTime, 
                                            Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            //draw overlay
            m_fadeOverlay.DoDraw(gameTime, graphicsDevice, m_tutOverlayCam);

            //draw Tutorial writings
            m_spriteBatch.Begin();

            DrawTutorialTitle(graphicsDevice);
            DrawTutorialText(graphicsDevice);

            m_spriteBatch.End();
        }

        private void DrawTutorialTitle(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            TitleFontOrigin = ((Microsoft.Xna.Framework.Graphics.SpriteFont)m_tutTitleFont.GetResource)
                .MeasureString(GlobalConstants.TUT_TITLES[tutorialScreenId]) / 2;

            TitleFontPos.Y = TitleFontOrigin.Y + TUT_TITLE_Y;

            m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_tutTitleFont.GetResource,
                GlobalConstants.TUT_TITLES[tutorialScreenId], TitleFontPos,
                Microsoft.Xna.Framework.Graphics.Color.White,
                0f, TitleFontOrigin, 1f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0.5f);
        }

        private void DrawTutorialText(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            TextFontOrigin = ((Microsoft.Xna.Framework.Graphics.SpriteFont)m_tutTextFont.GetResource)
                .MeasureString(GlobalConstants.TUT_TEXTS[tutorialScreenId]) / 2;

            TextFontPos.Y = TextFontOrigin.Y + TUT_TEXT_Y;

            m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_tutTextFont.GetResource,
                GlobalConstants.TUT_TEXTS[tutorialScreenId], TextFontPos,
                Microsoft.Xna.Framework.Graphics.Color.White,
                0f, TextFontOrigin, 1f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0.5f);

            //draw 'CONTINUE' & icon
            m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_iconContinue.GetResource,
                m_iconContinueSlot, Microsoft.Xna.Framework.Graphics.Color.White);
            m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_tutTextFont.GetResource,
                "CONTINUE", m_labelContinueSlot, Microsoft.Xna.Framework.Graphics.Color.Green);

            //draw cell images
            if (tutorialScreenId == 5)
            {
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_tutRBCimage.GetResource,
                    m_cellImgSlot, Microsoft.Xna.Framework.Graphics.Color.White);
            }
            else if (tutorialScreenId == 6)
            {
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_tutPLTimage.GetResource,
                    m_cellImgSlot, Microsoft.Xna.Framework.Graphics.Color.White);
            }
            else if (tutorialScreenId == 7)
            {
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_tutTNKimage.GetResource,
                    m_cellImgSlot, Microsoft.Xna.Framework.Graphics.Color.White);
            }
            else if (tutorialScreenId == 8)
            {
                m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_tutSILimage.GetResource,
                    m_cellImgSlot, Microsoft.Xna.Framework.Graphics.Color.White);
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
