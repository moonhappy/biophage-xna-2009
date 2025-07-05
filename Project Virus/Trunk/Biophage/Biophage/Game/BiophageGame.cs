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

namespace Biophage.Game
{
    /// <summary>
    /// The main game class.
    /// </summary>
    class BiophageGame : LnaGame
    {
        #region methods

        #region core

        LnaNetworkSessionComponent netComp;

        public override void GameMain(int numCpus)
        {
            //create the stages
            Stage mainStage = new Stage(GlobalConstants.MAIN_STAGE_ID, DebugMgr, ResourceMgr, SceneMgr, GraphicsMgr);
            Stage gameStage = new Stage(GlobalConstants.GAME_STAGE_ID, DebugMgr, ResourceMgr, SceneMgr, GraphicsMgr);

            //setup mainStage
            SetupMainStage(mainStage);

            //setup gameStage
            SetupGameStage(gameStage);

            //set main as current stage
            SceneMgr.SetCurrentStage(GlobalConstants.MAIN_STAGE_ID);

            //gotta add the network component 
            netComp = new LnaNetworkSessionComponent(DebugMgr, ResourceMgr, SceneMgr, this,
                new SpriteFontResHandle(DebugMgr, ResourceMgr, "Content\\Fonts", "PromptFont"));
            Components.Add(netComp);
        }

        protected override void InitDisplay()
        {
#if !XBOX
            //PC
#if DEBUG
            GraphicsMgr.IsFullScreen = false;
#else
            GraphicsMgr.IsFullScreen = true;
#endif
#else
            //XBOX
#endif
        }

        #endregion

        #region construction

        #region main stage

        private Menu MakeMainMenu()
        {
            Menu mainMenu = new Menu(GlobalConstants.MAIN_MENU_ID, DebugMgr, ResourceMgr,
                GraphicsMgr, spriteBatch,
                new SpriteFontResHandle(DebugMgr, ResourceMgr, "Content\\Fonts\\", "HUDFont"),
                LeadPlayerIndex);

            SpriteFontResHandle mainMenuFont = new SpriteFontResHandle(
                DebugMgr, ResourceMgr, "Content\\Fonts\\", "MenuFont");

            #region main window

            MenuWindow mainWnd = new MenuWindow(
                GlobalConstants.MMMAIN_WND_ID, DebugMgr, ResourceMgr, 
                mainMenuFont, 
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"),
                mainMenu, true, GraphicsMgr, spriteBatch);

            MenuButton buttonSP = new MenuButton(
                GlobalConstants.MMSP_BUT_ID, "SINGLE PLAYER", "", mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainWnd, true);
            buttonSP.UIAction += buttonSP_UIAction;

            MenuButton buttonMP = new MenuButton(
                GlobalConstants.MMMP_BUT_ID, "MULTIPLAYER", "", mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainWnd, true);
            buttonMP.UIAction += buttonMP_UIAction;

            MenuButton buttonExtras = new MenuButton(
                GlobalConstants.MMEXTR_BUT_ID, "EXTRAS", "", mainMenuFont, DebugMgr, ResourceMgr,
                GraphicsMgr, spriteBatch, mainWnd, true);
            buttonExtras.UIAction += buttonExtras_UIAction;

            MenuButton buttonExit = new MenuButton(
                GlobalConstants.MMEXIT_BUT_ID, "EXIT", "", mainMenuFont, DebugMgr, ResourceMgr,
                GraphicsMgr, spriteBatch, mainWnd, true);
            buttonExit.UIAction += buttonExit_UIAction;

            #endregion

            #region single player window

            MenuWindow spWnd = new MenuWindow(
                GlobalConstants.MMSP_WND_ID, DebugMgr, ResourceMgr, 
                mainMenuFont, 
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"),
                mainMenu, true, GraphicsMgr, spriteBatch);

            MenuButton buttonSPPlay = new MenuButton(
                GlobalConstants.MMSPPLAY_BUT_ID, "PLAY", "", mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, spWnd, true);
            buttonSPPlay.UIAction += buttonSPPlay_UIAction;

            MenuButton buttonSPTutorial = new MenuButton(
                GlobalConstants.MMSPTUTORIAL_BUT_ID, "TUTORIAL", "", mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, spWnd, true);
            buttonSPTutorial.UIAction += buttonSPTutorial_UIAction;

            #endregion

            #region multiplayer window

            MenuWindow mpFirstWnd = new MenuWindow(
                GlobalConstants.MMMP_WND_ID, DebugMgr, ResourceMgr, 
                mainMenuFont, 
                new SoundResHandle(
                DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"),
                mainMenu, true, GraphicsMgr, spriteBatch);

            MenuButton mpStartLAN = new MenuButton(
                GlobalConstants.MMMP_LAN_BUT_ID, "LOCAL", "Local Area Network\nsession.\n(aka SystemLink)",
                mainMenuFont, DebugMgr, ResourceMgr,
                GraphicsMgr, spriteBatch, mpFirstWnd, true);
            mpStartLAN.UIAction += mpStartLAN_UIAction;

            MenuButton mpStartNET = new MenuButton(
                GlobalConstants.MMMP_NET_BUT_ID, "INTERNET", "LIVE session.\n(aka PlayerMatch)",
                mainMenuFont, DebugMgr, ResourceMgr,
                GraphicsMgr, spriteBatch, mpFirstWnd, true);
            mpStartNET.UIAction += mpStartNET_UIAction;

            #region LAN

            MenuWindow mpWndLocal = new MenuWindow(
                GlobalConstants.MMMP_LAN_WND_ID, DebugMgr, ResourceMgr, 
                mainMenuFont, 
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"),
                mainMenu, true, GraphicsMgr, spriteBatch);

            MenuButton bLANHost = new MenuButton(
                GlobalConstants.MMMPLAN_HOST_BUT_ID, "HOST", "Host a new LAN\nmultiplayer session.",
                mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mpWndLocal, true);
            bLANHost.UIAction += bLANHost_UIAction;

            MenuButton bLANJoin = new MenuButton(
                GlobalConstants.MMMPLAN_JOIN_BUT_ID, "JOIN", "Join a LAN multiplayer\nsession.",
                mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mpWndLocal, true);
            bLANJoin.UIAction += bLANJoin_UIAction;

            #endregion

            #region NET

            MenuWindow mpWndNet = new MenuWindow(
                GlobalConstants.MMMP_NET_WND_ID, DebugMgr, ResourceMgr, 
                mainMenuFont,
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"),
                mainMenu, true, GraphicsMgr, spriteBatch);

            MenuButton bNETHost = new MenuButton(
                GlobalConstants.MMMPNET_HOST_BUT_ID, "HOST", "Host a new INTERNET\nmultiplayer session.",
                mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mpWndNet, true);
            bNETHost.UIAction += bNETHost_UIAction;

            MenuButton bNETJoin = new MenuButton(
                GlobalConstants.MMMPNET_JOIN_BUT_ID, "JOIN", "Join a INTERNET\nmultiplayer session.",
                mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mpWndNet, true);
            bNETJoin.UIAction += bNETJoin_UIAction;

            #endregion

            #endregion

            #region extra window

            MenuWindow extrasWnd = new MenuWindow(
                GlobalConstants.MMEXTR_WND_ID, DebugMgr, ResourceMgr, 
                mainMenuFont,
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"),
                mainMenu, true, GraphicsMgr, spriteBatch);

            MenuButton buttonCredits = new MenuButton(
                GlobalConstants.MMEXTRCREDS_BUT_ID, "CREDITS", "", mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, extrasWnd, true);
            buttonCredits.UIAction += buttonCredits_UIAction;

            //MenuButton buttonPropaganda = new MenuButton(
            //    GlobalConstants.MMEXTRPROP_BUT_ID, "PROPAGANDA", "", mainMenuFont,
            //    DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, extrasWnd, true);
            //buttonPropaganda.UIAction += buttonPropaganda_UIAction;

            #endregion

            #region live marketplace

            //only shown when requested in main menu scene
            MenuWindow gotoMarketplaceWnd = new MenuWindow(
                GlobalConstants.MMGOTOMP_WND_ID, DebugMgr, ResourceMgr, 
                mainMenuFont,
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"),
                mainMenu, true, GraphicsMgr, spriteBatch);

            MenuLabel labelGoToMarket = new MenuLabel(
                GlobalConstants.MMGTMARKET_LABEL_ID, "GO TO MARKET?",
                mainMenuFont, DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch,
                gotoMarketplaceWnd, true);

            MenuButton buttonGoToMarket = new MenuButton(
                GlobalConstants.MMGTMARKET_BUT_YES_ID, "YES", "", mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, gotoMarketplaceWnd, true);
            buttonGoToMarket.UIAction += buttonGoToMarket_UIAction;

            MenuButton buttonCancelMarket = new MenuButton(
                GlobalConstants.MMGTMARKET_BUT_NO_ID, "NO", "", mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, gotoMarketplaceWnd, true);
            buttonCancelMarket.UIAction += buttonCancelMarket_UIAction;

            #endregion

            #region quit prompt

            MenuWindow quitGameWnd = new MenuWindow(
                GlobalConstants.MMQUIT_PROMT_WND_ID, DebugMgr, ResourceMgr,
                mainMenuFont,
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"), 
                mainMenu, true, GraphicsMgr, spriteBatch);

            MenuLabel quitGameLabel = new MenuLabel(
                GlobalConstants.MMQUIT_LABEL_ID,
                "ARE YOU SURE?", mainMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, quitGameWnd, true);

            MenuButton buttonQuitYes = new MenuButton(
                GlobalConstants.MMQUIT_BUT_YES_ID,
                "YES", "", mainMenuFont, DebugMgr, ResourceMgr,
                GraphicsMgr, spriteBatch, quitGameWnd, true);
            buttonQuitYes.UIAction += buttonQuitYes_UIAction;

            MenuButton buttonQuitNo = new MenuButton(
                GlobalConstants.MMQUIT_BUT_NO_ID,
                "NO", "", mainMenuFont, DebugMgr, ResourceMgr,
                GraphicsMgr, spriteBatch, quitGameWnd, true);
            buttonQuitNo.UIAction += buttonQuitNo_UIAction;

            #endregion

            mainMenu.SetDefaultWindow(GlobalConstants.MMMAIN_WND_ID);

            return mainMenu;
        }

        #region button actions

        #region main window button actions

        void buttonExit_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.SetCurrentWindow(GlobalConstants.MMQUIT_PROMT_WND_ID);
        }

        void buttonExtras_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.SetCurrentWindow(GlobalConstants.MMEXTR_WND_ID);
        }

        void buttonMP_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.SetCurrentWindow(GlobalConstants.MMMP_WND_ID);
        }

        void buttonSP_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.SetCurrentWindow(GlobalConstants.MMSP_WND_ID);
        }

        #endregion

        #region single player button actions

        void buttonSPPlay_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;

            //get settings scene
            Stages.Main.GameplaySettingsScn settingsScn = 
                (Stages.Main.GameplaySettingsScn)SceneMgr.CurrentStage.
                GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);

            //set initial setting details
            Stages.Game.SessionDetails sessionDetails = new Stages.Game.SessionDetails();
            sessionDetails.isMultiplayer = false;
            sessionDetails.isHost = true;
            settingsScn.NewSession(sessionDetails);

            //change to settings scene scene
            SceneMgr.CurrentStage.SetCurrentScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);
        }

        void buttonSPTutorial_UIAction(object sender, EventArgs e)
        {
            //set initial setting details
            Stages.Game.SessionDetails sessionDetails = new Stages.Game.SessionDetails();

            //though we bypass the settings scn - we will still need to set a new session to it
            Stages.Main.GameplaySettingsScn settingsScn =
                (Stages.Main.GameplaySettingsScn)SceneMgr.CurrentStage.
                GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);
            settingsScn.NewSession(sessionDetails);

            sessionDetails.isMultiplayer = false;
            sessionDetails.isHost = true;
            sessionDetails.gameLevel = GlobalConstants.GameLevel.TUTORIAL;
            sessionDetails.type = GlobalConstants.GameplayType.LAST_STANDING;
            sessionDetails.typeSettings = 0;
            sessionDetails.numBots = 0;

            //start the game
            SceneMgr.CurrentStage.SetCurrentScene(GlobalConstants.LOADING_SCN_ID);
            Stages.Main.LoadingScn lScn = (Stages.Main.LoadingScn)SceneMgr.CurrentStage.CurrentScene;
                lScn.SceneLoadDetails(sessionDetails);
        }

        #endregion

        #region multiplayer button actions

        void mpStartNET_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.SetCurrentWindow(GlobalConstants.MMMP_NET_WND_ID);
        }

        void mpStartLAN_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.SetCurrentWindow(GlobalConstants.MMMP_LAN_WND_ID);
        }

        void bLANHost_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;

            //get settings scene
            Stages.Main.GameplaySettingsScn settingsScn = (Stages.Main.GameplaySettingsScn)SceneMgr.CurrentStage.
                GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);

            //set initial setting details
            Stages.Game.SessionDetails sessionDetails = new Stages.Game.SessionDetails();
            sessionDetails.isMultiplayer = true;
            sessionDetails.isHost = true;
            sessionDetails.netSessionType = Microsoft.Xna.Framework.Net.NetworkSessionType.SystemLink;
            settingsScn.NewSession(sessionDetails);

            //check if live account is logged in
            Stages.Main.MainMenuScn mmScn = (Stages.Main.MainMenuScn)
                SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).GetChildScene(GlobalConstants.MAIN_MENU_SCN_ID);
            mmScn.ShowSignIn = true;
        }

        void bLANJoin_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;

            //get settings scene
            Stages.Main.GameplaySettingsScn settingsScn = (Stages.Main.GameplaySettingsScn)SceneMgr.CurrentStage.
                GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);

            //set initial setting details
            Stages.Game.SessionDetails sessionDetails = new Stages.Game.SessionDetails();
            sessionDetails.isMultiplayer = true;
            sessionDetails.isHost = false;
            sessionDetails.netSessionType = Microsoft.Xna.Framework.Net.NetworkSessionType.SystemLink;
            settingsScn.NewSession(sessionDetails);

            //check if live account is logged in
            Stages.Main.MainMenuScn mmScn = (Stages.Main.MainMenuScn)
                SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).GetChildScene(GlobalConstants.MAIN_MENU_SCN_ID);
            mmScn.ShowSignIn = true;
        }

        void bNETHost_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;

            //get settings scene
            Stages.Main.GameplaySettingsScn settingsScn = (Stages.Main.GameplaySettingsScn)SceneMgr.CurrentStage.
                GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);

            //set initial setting details
            Stages.Game.SessionDetails sessionDetails = new Stages.Game.SessionDetails();
            sessionDetails.isMultiplayer = true;
            sessionDetails.isHost = true;
            sessionDetails.netSessionType = Microsoft.Xna.Framework.Net.NetworkSessionType.PlayerMatch;
            settingsScn.NewSession(sessionDetails);

            //check if live account is logged in
            Stages.Main.MainMenuScn mmScn = (Stages.Main.MainMenuScn)
                SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).GetChildScene(GlobalConstants.MAIN_MENU_SCN_ID);
            mmScn.ShowSignIn = true;
        }

        void bNETJoin_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;

            //get settings scene
            Stages.Main.GameplaySettingsScn settingsScn = (Stages.Main.GameplaySettingsScn)SceneMgr.CurrentStage.
                GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);

            //set initial setting details
            Stages.Game.SessionDetails sessionDetails = new Stages.Game.SessionDetails();
            sessionDetails.isMultiplayer = true;
            sessionDetails.isHost = false;
            sessionDetails.netSessionType = Microsoft.Xna.Framework.Net.NetworkSessionType.PlayerMatch;
            settingsScn.NewSession(sessionDetails);

            //check if live account is logged in
            Stages.Main.MainMenuScn mmScn = (Stages.Main.MainMenuScn)
                SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).GetChildScene(GlobalConstants.MAIN_MENU_SCN_ID);
            mmScn.ShowSignIn = true;
        }

        #endregion

        #region extras button actions

        //void buttonPropaganda_UIAction(object sender, EventArgs e)
        //{
        //    SceneMgr.CurrentStage.SetCurrentScene(GlobalConstants.PROPAGANDA_SCN_ID);
        //}

        void buttonCredits_UIAction(object sender, EventArgs e)
        {
            SceneMgr.CurrentStage.SetCurrentScene(GlobalConstants.CREDITS_SCN_ID);
        }

        #endregion

        #region goto marketplace button actions

        void buttonCancelMarket_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.SetCurrentToPreviousWindow();
        }

        void buttonGoToMarket_UIAction(object sender, EventArgs e)
        {
            //open the marketplace
            Microsoft.Xna.Framework.GamerServices.Guide.ShowMarketplace(LeadPlayerIndex);
        }

        #endregion

        #region quit prompt button actions

        void buttonQuitNo_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.SetCurrentToPreviousWindow();
        }

        void buttonQuitYes_UIAction(object sender, EventArgs e)
        {
            //exit game
            SceneMgr.ExitGame();
        }

        #endregion

        #endregion

        private void SetupMainStage(Stage stgRef)
        {
            // - mainCommonResScn
            // + - || startSplashScn
            // + - mainMenuScn
            // + - creditsScn
            // + - propagandaScn
            // + - loadingScn

            #region common res scene

            //the main menu and scene
            Stages.Main.MainCommonResScn mainCommonResScn = Stages.Main.MainCommonResScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, stgRef, MakeMainMenu());

            #endregion

            #region start splash scene

            //the start scene
            Stages.Main.StartSplashScn startSplashScn = Stages.Main.StartSplashScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, stgRef, mainCommonResScn);

            #endregion

            #region main menu scene

            //the background scene - when displaying the menu
            Stages.Main.MainMenuScn mainMenuScn = Stages.Main.MainMenuScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainCommonResScn);

            mainMenuScn.ProfileSignedIn += mainMenuScn_ProfileSignedIn;

            #endregion

            #region game load scene

            //the loading scene
            Stages.Main.LoadingScn loadingScn = Stages.Main.LoadingScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainCommonResScn);

            #endregion

            #region credits

            //credits
            Stages.Main.CreditsScn creditsScn = Stages.Main.CreditsScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainCommonResScn);

            #endregion

            #region propaganda

            //propaganda
            Stages.Main.PropagandaScn propagandaScn = Stages.Main.PropagandaScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainCommonResScn);

            #endregion

            #region gameplay settings

            Stages.Main.GameplaySettingsScn gameplaySettingsScn = Stages.Main.GameplaySettingsScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainCommonResScn);

            #endregion

            #region lobby

            Stages.Main.GameLobbyScn lobbyScn = Stages.Main.GameLobbyScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainCommonResScn);

            #endregion

            #region network loading

            Stages.Main.NetworkLoadingScn networkLoadingScn = Stages.Main.NetworkLoadingScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainCommonResScn);

            #endregion

            #region join session

            Stages.Main.JoinSessionScn joinSessionScn = Stages.Main.JoinSessionScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainCommonResScn);

            #endregion

            //set current and return
            stgRef.SetCurrentScene(GlobalConstants.START_SPLASH_SCN_ID);
        }

        #region main to network control

        void mainMenuScn_ProfileSignedIn(object sender, EventArgs e)
        {
            //wait till network activity complete
            Stages.Main.GameplaySettingsScn settingsScn = (Stages.Main.GameplaySettingsScn)SceneMgr.CurrentStage.
                GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);

            if (settingsScn.sessionDetails.isHost)
            {
                try
                {
                    // Begin an asynchronous create network session operation.
                    IAsyncResult asyncResult;
                    
                        asyncResult = Microsoft.Xna.Framework.Net.NetworkSession.BeginCreate(
                            settingsScn.sessionDetails.netSessionType,
                            1, GlobalConstants.NET_MAX_PLAYERS, null, null);
                    

                    // Activate the network busy screen, which will display
                    // an animation until this operation has completed.
                    Stages.Main.NetworkLoadingScn networkloadingScn = (Stages.Main.NetworkLoadingScn)
                        SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).GetChildScene(GlobalConstants.NET_LOADING_SCN_ID);
                    networkloadingScn.AsyncResult = asyncResult;

                    networkloadingScn.ClearEvents();
                    networkloadingScn.OperationCompleted += CreateSessionComplete;

                    networkloadingScn.Stage.SetCurrentScene(networkloadingScn.Id);
                }
                catch (Exception exception)
                {
                    m_debugManager.WriteLogEntry("BiophageGame:ProfileSignedIn - network error creating host session. " +
                        exception.ToString());
                    SceneMgr.CurrentStage.CurrentScene.ShowMessage("Error creating session");
                    SceneMgr.CurrentStage.CurrentScene.GetMsgBoxButton.UIAction += networkComp_LeavingSession;
                }
            }
            else
            {
                try
                {
                    // Begin an asynchronous find network sessions operation.
                    IAsyncResult asyncResult;

                        asyncResult = Microsoft.Xna.Framework.Net.NetworkSession.BeginFind(
                            settingsScn.sessionDetails.netSessionType,
                            1, null, null, null);

                    // Activate the network busy screen, which will display
                    // an animation until this operation has completed.
                    Stages.Main.NetworkLoadingScn netloadingScn = (Stages.Main.NetworkLoadingScn)
                        SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).GetChildScene(GlobalConstants.NET_LOADING_SCN_ID);
                    netloadingScn.AsyncResult = asyncResult;

                    netloadingScn.ClearEvents();
                    netloadingScn.OperationCompleted += FoundSessionsComplete;

                    netloadingScn.Stage.SetCurrentScene(netloadingScn.Id);
                }
                catch (Exception exception)
                {
                    m_debugManager.WriteLogEntry("BiophageGame:ProfileSignedIn - network error finding sessions. " +
                        exception.ToString());
                    SceneMgr.CurrentStage.CurrentScene.ShowMessage("Error finding sessions");
                    SceneMgr.CurrentStage.CurrentScene.GetMsgBoxButton.UIAction += networkComp_LeavingSession;
                }
            }
        }

        void FoundSessionsComplete(object sender, Biophage.Game.Stages.Main.OperationCompletedEventArgs e)
        {
            try
            {
                // End the asynchronous find network sessions operation.
                Microsoft.Xna.Framework.Net.AvailableNetworkSessionCollection availableSessions =
                                                Microsoft.Xna.Framework.Net.NetworkSession.EndFind(e.AsyncResult);

                if (availableSessions.Count == 0)
                {
                    // If we didn't find any sessions, display an error.
                    availableSessions.Dispose();

                    SceneMgr.CurrentStage.CurrentScene.ShowMessage("No sessions found");
                    SceneMgr.CurrentStage.CurrentScene.GetMsgBoxButton.UIAction += GetMsgBoxButton_UIAction;
                }
                else
                {
                    // If we did find some sessions, proceed to the JoinSessionScreen.
                    SceneMgr.CurrentStage.SetCurrentScene(GlobalConstants.CLIENT_JOIN_SCN_ID);
                    Stages.Main.JoinSessionScn joinScn = (Stages.Main.JoinSessionScn)SceneMgr.CurrentStage.CurrentScene;
                    joinScn.SetAvaliableSessions(availableSessions);
                }
            }
            catch (Exception exception)
            {
                m_debugManager.WriteLogEntry("BiophageGame:FoundSessionComplete - network error. " + exception.ToString());
                SceneMgr.CurrentStage.CurrentScene.ShowMessage("Network error occured");
                SceneMgr.CurrentStage.CurrentScene.GetMsgBoxButton.UIAction += networkComp_LeavingSession;
            }
        }

        void GetMsgBoxButton_UIAction(object sender, EventArgs e)
        {
            SceneMgr.GetStage(GlobalConstants.GAME_STAGE_ID).SetCurrentScene(GlobalConstants.COMMRES_SCN_ID);
            SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).SetCurrentScene(GlobalConstants.MAIN_MENU_SCN_ID);
            SceneMgr.SetCurrentStage(GlobalConstants.MAIN_STAGE_ID);
        }

        void CreateSessionComplete(object sender, Biophage.Game.Stages.Main.OperationCompletedEventArgs e)
        {
            try
            {
                // End the asynchronous create network session operation.
                Microsoft.Xna.Framework.Net.NetworkSession networkSession =
                    Microsoft.Xna.Framework.Net.NetworkSession.EndCreate(e.AsyncResult);

                // Create a component that will manage the session we just created.
                LnaNetworkSessionComponent networkComp = LnaNetworkSessionComponent.FindSessionComponent(this);
                m_debugManager.Assert(networkComp != null, "BiophageGame:NetworkLoadingScn - network component not created.");
                networkComp.Create(networkSession);

                //if session ends, go to main menu
                networkComp.LeavingSession += networkComp_LeavingSession;

                // Go to the lobby screen. We pass null as the controlling player,
                // because the lobby screen accepts input from all local players
                // who are in the session, not just a single controlling player.
                SceneMgr.CurrentStage.SetCurrentScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);
                Stages.Main.GameplaySettingsScn gpScn = (Stages.Main.GameplaySettingsScn)SceneMgr.CurrentStage.CurrentScene;
                gpScn.sessionDetails.netSessionComponent = networkComp;
            }
            catch (Exception exception)
            {
                m_debugManager.WriteLogEntry("BiophageGame:OpComplete - network error. " + 
                    exception.ToString());
                SceneMgr.CurrentStage.CurrentScene.ShowMessage("Network error");
                SceneMgr.CurrentStage.CurrentScene.GetMsgBoxButton.UIAction += networkComp_LeavingSession;
            }
        }

        void networkComp_LeavingSession(object sender, EventArgs e)
        {
            //re-add network component
            netComp = new LnaNetworkSessionComponent(DebugMgr, ResourceMgr, SceneMgr, this,
               new SpriteFontResHandle(DebugMgr, ResourceMgr, "Content\\Fonts\\", "PromptFont"));
            Components.Add(netComp);

            SceneMgr.GetStage(GlobalConstants.GAME_STAGE_ID).SetCurrentScene(GlobalConstants.COMMRES_SCN_ID, true, true);
            SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).SetCurrentScene(GlobalConstants.MAIN_MENU_SCN_ID);
            SceneMgr.SetCurrentStage(GlobalConstants.MAIN_STAGE_ID);

            SceneMgr.CurrentStage.CurrentScene.GetMenu.SetDefaultWindow(GlobalConstants.MMMAIN_WND_ID);
        }

        #endregion

        #endregion

        #region game stage

        private Menu MakeGameMenu()
        {
            Menu gameMenu = new Menu(GlobalConstants.GAME_MENU_ID, DebugMgr, ResourceMgr,
                GraphicsMgr, spriteBatch,
                new SpriteFontResHandle(DebugMgr, ResourceMgr, "Content\\Fonts\\", "HUDFont"),
                LeadPlayerIndex);

            SpriteFontResHandle gameMenuFont = new SpriteFontResHandle(
                DebugMgr, ResourceMgr, "Content\\Fonts\\", "MenuFont");

            #region main window

            MenuWindow mainWnd = new MenuWindow(
                GlobalConstants.GMMAIN_WND_ID, DebugMgr, ResourceMgr, 
                gameMenuFont,
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"),
                gameMenu, true, GraphicsMgr, spriteBatch);

            MenuButton buttonResume = new MenuButton(
                GlobalConstants.GMMAIN_RESUME_BUT_ID, "RESUME GAME", "", gameMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainWnd, true);
            buttonResume.UIAction += buttonResume_UIAction;

            MenuButton buttonQuit = new MenuButton(
                GlobalConstants.GMMAIN_QUIT_BUT_ID, "QUIT GAME", "", gameMenuFont,
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, mainWnd, true);
            buttonQuit.UIAction += buttonQuit_UIAction;

            #endregion

            #region prompt close session

            MenuWindow quitPromptWnd = new MenuWindow(
                GlobalConstants.GMQUIT_WND_ID, DebugMgr, ResourceMgr,
                gameMenuFont,
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(DebugMgr, ResourceMgr, "Content\\Sounds\\", "MenuBack"), 
                gameMenu, true, GraphicsMgr, spriteBatch);

            MenuLabel quitPromptLabel = new MenuLabel(
                GlobalConstants.GMQUIT_LABEL_ID, "Are You Sure?",
                gameMenuFont, DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch,
                quitPromptWnd, true);

            MenuButton buttonSessionQuitYes = new MenuButton(
                GlobalConstants.GMQUIT_BUT_YES_ID, "YES", "",
                gameMenuFont, DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch,
                quitPromptWnd, true);
            buttonSessionQuitYes.UIAction += buttonSessionQuitYes_UIAction;

            MenuButton buttonSessionQuitNo = new MenuButton(
                GlobalConstants.GMQUIT_BUT_NO_ID, "NO", "",
                gameMenuFont, DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch,
                quitPromptWnd, true);
            buttonSessionQuitNo.UIAction += buttonSessionQuitNo_UIAction;

            #endregion

            gameMenu.SetCurrentWindow(GlobalConstants.GMMAIN_WND_ID);

            return gameMenu;
        }

        #region quit session prompt

        void buttonSessionQuitNo_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.SetCurrentToPreviousWindow();
        }

        void buttonSessionQuitYes_UIAction(object sender, EventArgs e)
        {
            //gotta close the session and return back to the main menu
            MenuButton mb = (MenuButton)sender;
            Menu gm = mb.GetMenuWindow.GetMenu;

            gm.SetDefaultWindow(GlobalConstants.GMMAIN_WND_ID);
            gm.Active = false;

            Stages.Main.GameplaySettingsScn gpScn = (Stages.Main.GameplaySettingsScn)SceneMgr
                .GetStage(GlobalConstants.MAIN_STAGE_ID).GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);

            gpScn.CloseSession();
        }

        #endregion

        #region game menu button actions

        void buttonQuit_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            Menu gm = mb.GetMenuWindow.GetMenu;

            gm.SetCurrentWindow(GlobalConstants.GMQUIT_WND_ID);
        }

        void buttonResume_UIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;

            SceneMgr.CurrentStage.CurrentScene.GetMenu.Active = false;
            SceneMgr.CurrentStage.CurrentScene.IsPaused = false;
        }

        #endregion

        private void SetupGameStage(Stage stgRef)
        {
            // - blankScn
            // - gameMenuScn
            // + - commonResScn
            //   + - trialLevelScn

            #region game menu res scene

            //the game menu scene
            Stages.Game.GameMenuScene gameMenuScn = Stages.Game.GameMenuScene.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, stgRef, MakeGameMenu());

            #endregion

            #region common res scene

            //common resources scene
            Stages.Game.CommonGameResScn commonResScn = Stages.Game.CommonGameResScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, stgRef, gameMenuScn);

            #endregion

            #region trial game level scene

            //trial level
            Stages.Game.TrialGameLvlScn trialLevelScn = Stages.Game.TrialGameLvlScn.Create(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, commonResScn);

            #endregion

            #region tutorial level scene

            Stages.Game.TutorialGameLvlScn tutorialLevelScn = Stages.Game.TutorialGameLvlScn.CreateTutorial(
                DebugMgr, ResourceMgr, GraphicsMgr, spriteBatch, commonResScn);

            #endregion

            //set current
            stgRef.SetCurrentScene(GlobalConstants.COMMRES_SCN_ID);
        }

        #endregion

        #endregion

        #endregion
    }
}
