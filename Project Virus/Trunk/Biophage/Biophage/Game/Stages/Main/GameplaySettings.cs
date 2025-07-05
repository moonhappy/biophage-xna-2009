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
    /// This scene should only be accessible by the host.
    /// This also includes single player.
    /// </summary>
    public class GameplaySettingsScn : Scene
    {
        #region fields

        //this field will permantly be accessable
        public Game.SessionDetails sessionDetails;

        protected bool m_isSessionSet = false;
        protected bool m_isTrial;

        private bool m_leaveSessionActionSet = false;

        #region menu elements

        MenuToggle toggleGameType;
        MenuToggle toggleLevel;
        MenuValue botNum;

        #endregion

        #endregion

        #region methods

        #region construction

        public GameplaySettingsScn(uint id, bool isTrial,
                                    DebugManager debugMgr, ResourceManager resourceMgr,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                    Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                    SpriteFontResHandle sceneFont,
                                    Scene parentScn)
            : base(id,
                    debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                    Microsoft.Xna.Framework.Graphics.Color.Black,
                    sceneFont,
                    parentScn.Stage, parentScn, null)
        {
            m_camera = new CameraGObj(
                uint.MaxValue, debugMgr, resourceMgr, this,
                false, graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000.0f);

            m_isTrial = isTrial;


            //make menu
            m_menu = new Menu(
                GlobalConstants.SESSION_SETTINGS_MENU, debugMgr, resourceMgr,
                graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "HUDFont"),
                Stage.SceneMgr.Game.LeadPlayerIndex);

            //make the gameplay settings menu
            SpriteFontResHandle menuFont = new SpriteFontResHandle(
                debugMgr, resourceMgr, "Content\\Fonts\\", "MenuFont");

            #region game play wnd

            MenuWindow gpSettingsWnd = new MenuWindow(
                GlobalConstants.SS_MAIN_WND, debugMgr, resourceMgr,
                menuFont,
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuBack"), 
                m_menu, true, graphicsMgr,
                Stage.SceneMgr.Game.spriteBatch);

            MenuLabel gameTypeLabel = new MenuLabel(
                GlobalConstants.SSMAIN_GAMETYPE_LABEL, "Select Game Type:", menuFont,
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd, true);

            toggleGameType = new MenuToggle(
                GlobalConstants.SSMAIN_GAMETYPE_TGL, GlobalConstants.GP_TYPES_STR_ARRAY, 0,
                GenDescriptions(GlobalConstants.GP_TYPES_STR_ARRAY, GlobalConstants.GP_TYPES_DESC_ARRAY),
                menuFont, 
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuMove"),
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd, true);

            MenuButton buttonGameTypeSettings = new MenuButton(
                GlobalConstants.SSMAIN_GTYPE_SETTINGS_BUT, "Conditions", "Change game type conditions.",
                menuFont, debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd, true);
            buttonGameTypeSettings.UIAction += buttonGameTypeSettings_UIAction;

            MenuLabel levelLabel = new MenuLabel(
                GlobalConstants.SSMAIN_LEVEL_LABEL, "Game Level:", menuFont,
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd, true);

            string[] toggleStrArr = null;
            if (Microsoft.Xna.Framework.GamerServices.Guide.IsTrialMode)
            {
                toggleStrArr = new string[1];
                toggleStrArr[0] = GlobalConstants.GP_LEVELS_STR_ARRAY[0];
            }
            else
                toggleStrArr = GlobalConstants.GP_LEVELS_STR_ARRAY;

            toggleLevel = new MenuToggle(
               GlobalConstants.SSMAIN_LEVEL_TGL, toggleStrArr, 0,
               GenDescriptions(GlobalConstants.GP_LEVELS_STR_ARRAY, GlobalConstants.GP_LEVELS_DESC_ARRAY),
               menuFont,
               new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuMove"), 
               debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd, true);

            MenuLabel botLabel = new MenuLabel(
                GlobalConstants.SSMAIN_BOT_LABEL, "Num Bots:", menuFont,
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd, true);

            botNum = new MenuValue(
                GlobalConstants.SSMAIN_BOTNUM_VAL, GlobalConstants.GP_MIN_BOTS, GlobalConstants.GP_MAX_BOTS, 1, "",
                menuFont,
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuMove"), 
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd,
                true);

            MenuLabel continueLabel = new MenuLabel(
                GlobalConstants.SSMAIN_CONTINUE_LABEL, "Continue?", menuFont,
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd,
                true);

            MenuButton startGameButton = new MenuButton(
                GlobalConstants.SSMAIN_PLAY_BUT, "YES", "", menuFont, debugMgr,
                resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd, true);
            startGameButton.UIAction += startGameButton_UIAction;

            MenuButton cancelButton = new MenuButton(
                GlobalConstants.SSMAIN_CANCEL_BUT, "NO", "", menuFont,
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, gpSettingsWnd, true);
            cancelButton.UIAction += cancelButton_UIAction;

            #endregion

            #region timed match wnd

            MenuWindow timedMatchWnd = new MenuWindow(
                GlobalConstants.SSMAIN_TIMED_SWND, debugMgr, resourceMgr, 
                menuFont,
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuBack"),
                m_menu, true, graphicsMgr, Stage.SceneMgr.Game.spriteBatch);

            MenuLabel timedMatchLabel = new MenuLabel(
                GlobalConstants.SSMAIN_TIMED_LABEL, "Time (mins):", menuFont,
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, timedMatchWnd, true);

            MenuValue timedMatchVal = new MenuValue(
                GlobalConstants.SSMAIN_TIMED_VAL,
                GlobalConstants.GP_TIMED_MIN_TIME, GlobalConstants.GP_TIMED_MAX_TIME,
                GlobalConstants.GP_TIMED_DEF_TIME, "", menuFont,
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuMove"), 
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, timedMatchWnd, true);

            MenuButton timedMatchOk = new MenuButton(
                GlobalConstants.SSMAIN_TIMED_VAL + (uint)1, "OK", "", menuFont, debugMgr, resourceMgr,
                graphicsMgr, Stage.SceneMgr.Game.spriteBatch, timedMatchWnd, true);
            timedMatchOk.UIAction += Ok_UIAction;

            #endregion

            #region illness match wnd

            MenuWindow illnessWnd = new MenuWindow(
                GlobalConstants.SSMAIN_ILL_SWND, debugMgr, resourceMgr, 
                menuFont,
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuBack"),
                m_menu, true, graphicsMgr, Stage.SceneMgr.Game.spriteBatch);

            MenuLabel illnessLabel = new MenuLabel(
                GlobalConstants.SSMAIN_ILL_LABEL, "Infection (%):", menuFont,
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, illnessWnd, true);

            MenuValue illnessVal = new MenuValue(
                GlobalConstants.SSMAIN_ILL_VAL,
                GlobalConstants.GP_ILL_MIN_INFECT, GlobalConstants.GP_ILL_MAX_INFECT,
                GlobalConstants.GP_ILL_DEF_INFECT, "", menuFont,
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuMove"), 
                debugMgr, resourceMgr, graphicsMgr, Stage.SceneMgr.Game.spriteBatch, illnessWnd, true);

            MenuButton illnessOk = new MenuButton(
                GlobalConstants.SSMAIN_ILL_VAL + (uint)1, "OK", "", menuFont, debugMgr, resourceMgr,
                graphicsMgr, Stage.SceneMgr.Game.spriteBatch, illnessWnd, true);
            illnessOk.UIAction += Ok_UIAction;

            #endregion

            m_menu.SetDefaultWindow(GlobalConstants.SS_MAIN_WND);
        }

        /// <summary>
        /// Assumes that the descriptions array maps to the keys correctly.
        /// </summary>
        private Dictionary<string, string> GenDescriptions(string[] p, string[] p_2)
        {
            m_debugMgr.Assert(p.Length == p_2.Length,
                "GameplaySettings:GenDescriptions - arrays have different number of elements.");

            Dictionary<string, string> descs = new Dictionary<string, string>(p.Length);

            for (int i = 0; i < p.Length; ++i)
            {
                descs.Add(p[i], p_2[i]);
            }

            return descs;
        }

        void cancelButton_UIAction(object sender, EventArgs e)
        {
            //go back to main menu
            CloseSession();
        }

        void startGameButton_UIAction(object sender, EventArgs e)
        {
            //set details
            GlobalConstants.GameLevel lvl = GlobalConstants.GameLevel.TRIAL;
            string lvlStr = ((MenuToggle)((MenuWindow)GetMenu.GetChildObj(GlobalConstants.SS_MAIN_WND))
                .GetChildObj(GlobalConstants.SSMAIN_LEVEL_TGL)).CurrentValue;
            GlobalConstants.GameplayType gpType = GlobalConstants.GameplayType.TIMED_MATCH;
            string gpTypeStr = ((MenuToggle)((MenuWindow)GetMenu.GetChildObj(GlobalConstants.SS_MAIN_WND))
                .GetChildObj(GlobalConstants.SSMAIN_GAMETYPE_TGL)).CurrentValue;
            byte typeSetting = (byte)GlobalConstants.GP_TIMED_MIN_TIME;
            byte numBots = (byte)((MenuValue)((MenuWindow)GetMenu.GetChildObj(GlobalConstants.SS_MAIN_WND))
                .GetChildObj(GlobalConstants.SSMAIN_BOTNUM_VAL)).CurrentValue;

            for (int i = 0; i < GlobalConstants.GP_LEVELS_STR_ARRAY.Length; i++)
            {
                if (lvlStr == GlobalConstants.GP_LEVELS_STR_ARRAY[i])
                {
                    lvl = (GlobalConstants.GameLevel)i;
                    break;
                }
            }
            for (int i = 0; i < GlobalConstants.GP_TYPES_STR_ARRAY.Length; i++)
            {
                if (gpTypeStr == GlobalConstants.GP_TYPES_STR_ARRAY[i])
                {
                    gpType = (GlobalConstants.GameplayType)i;
                    break;
                }
            }
            switch (gpType)
            {
                case GlobalConstants.GameplayType.TIMED_MATCH:
                    typeSetting = (byte)((MenuValue)((MenuWindow)GetMenu.GetChildObj(GlobalConstants.SSMAIN_TIMED_SWND))
                .GetChildObj(GlobalConstants.SSMAIN_TIMED_VAL)).CurrentValue;
                    break;
                case GlobalConstants.GameplayType.ILLNESS:
                    typeSetting = (byte)((MenuValue)((MenuWindow)GetMenu.GetChildObj(GlobalConstants.SSMAIN_ILL_SWND))
                .GetChildObj(GlobalConstants.SSMAIN_ILL_VAL)).CurrentValue;
                    break;

                default:
                    break;
            }

            sessionDetails.gameLevel = lvl;
            sessionDetails.type = gpType;
            sessionDetails.typeSettings = typeSetting;
            sessionDetails.numBots = numBots;

            if (sessionDetails.isMultiplayer)
            {
                //open the lobby scene
                Stage.SetCurrentScene(GlobalConstants.LOBBY_SCN_ID);
            }
            else
            {
                //start the game
                Stage.SetCurrentScene(GlobalConstants.LOADING_SCN_ID);

                LoadingScn lScn = (LoadingScn)Stage.CurrentScene;
                lScn.SceneLoadDetails(sessionDetails);
            }
        }

        void Ok_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;

            //go back
            mb.GetMenuWindow.GetMenu.SetCurrentToPreviousWindow();
        }

        void buttonGameTypeSettings_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            string typeSelected = ((MenuToggle)mb.GetMenuWindow
                .GetChildObj(GlobalConstants.SSMAIN_GAMETYPE_TGL)).CurrentValue;

            if (typeSelected == GlobalConstants.GP_TYPES_STR_ARRAY[0])
            {
                //timed match
                mb.GetMenuWindow.GetMenu.SetCurrentWindow(GlobalConstants.SSMAIN_TIMED_SWND);
            }
            else if (typeSelected == GlobalConstants.GP_TYPES_STR_ARRAY[1])
            {
                //Illness
                mb.GetMenuWindow.GetMenu.SetCurrentWindow(GlobalConstants.SSMAIN_ILL_SWND);
            }
        }

        #endregion

        #region creation

        public static GameplaySettingsScn Create(DebugManager debugMgr, ResourceManager resourceMgr,
                                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                                    Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                                    Scene parent)
        {
            GameplaySettingsScn scn = new GameplaySettingsScn(
                GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID,
                Microsoft.Xna.Framework.GamerServices.Guide.IsTrialMode,
                debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                parent);

            //continue with background
            TextureAsset backgroundWall = new TextureAsset(
              1, Microsoft.Xna.Framework.Vector3.Zero,
              1.92f, 1.08f, "Content\\MainStage", "Background",
              debugMgr, resourceMgr, scn, true,
              graphicsMgr);

            return scn;
        }

        #endregion

        #region initialisation

        public override bool Deinit()
        {
            bool retVal = base.Deinit();

            //gotta reset session
            m_isSessionSet = false;

            return retVal;
        }

        #endregion

        #region session settings

        public void NewSession(Game.SessionDetails newSessionDetails)
        {
            sessionDetails = newSessionDetails;
            m_debugMgr.Assert(sessionDetails != null,
                "GameplaySettings:NewSession - sessionDetails is null.");
            m_isSessionSet = true;

            //if single player - min bots is 1
            if (!sessionDetails.isMultiplayer)
            {
                MenuValue numBots = (MenuValue)GetMenu.GetMenuWindow(GlobalConstants.SS_MAIN_WND)
                    .GetChildObj(GlobalConstants.SSMAIN_BOTNUM_VAL);
                numBots.MinValue = 1;
            }
            else
            {
                MenuValue numBots = (MenuValue)GetMenu.GetMenuWindow(GlobalConstants.SS_MAIN_WND)
                    .GetChildObj(GlobalConstants.SSMAIN_BOTNUM_VAL);
                numBots.MinValue = 0;
            }

            //reset default bot num
            botNum.CurrentValue = 1;
        }

        void LeavingSession(object sender, EventArgs e)
        {
            m_isSessionSet = false;
        }

        public void CloseSession()
        {
            if (m_isSessionSet)
            {
                if (sessionDetails.isMultiplayer)
                    //end session
                    sessionDetails.netSessionComponent.RequestLeaveSession();
                else
                {
                    //go back to main menu
                    Stage.SceneMgr.GetStage(GlobalConstants.GAME_STAGE_ID).SetCurrentScene(GlobalConstants.COMMRES_SCN_ID, true, true);
                    Stage.SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).SetCurrentScene(GlobalConstants.MAIN_MENU_SCN_ID);
                    Stage.SceneMgr.SetCurrentStage(GlobalConstants.MAIN_STAGE_ID);
                }
            }
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
            m_debugMgr.Assert(m_isSessionSet, "GameplaySettings:Update - session details are not set.");

            if ((!m_leaveSessionActionSet) && (sessionDetails.isMultiplayer))
            {
                sessionDetails.netSessionComponent.LeavingSession += LeavingSession;
                m_leaveSessionActionSet = true;
            }

            //check if trial mode has been disabled - and show all levels
            if ((!Microsoft.Xna.Framework.GamerServices.Guide.IsTrialMode) && m_isTrial)
            {
                m_isTrial = false;
                MenuToggle levelToggle = (MenuToggle)GetMenu.GetMenuWindow(GlobalConstants.SS_MAIN_WND)
                    .GetChildObj(GlobalConstants.SSMAIN_LEVEL_TGL);
                levelToggle.ValuesArray = GlobalConstants.GP_LEVELS_STR_ARRAY;
            }
        }

        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            //menu should always be active
            if (!GetMenu.Active)
                GetMenu.Active = true;

            base.Draw(gameTime, graphicsDevice);
        }

        #endregion

        #endregion
    }
}
