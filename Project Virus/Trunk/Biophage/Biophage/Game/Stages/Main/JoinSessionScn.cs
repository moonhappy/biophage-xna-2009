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
    /// Allows the client to select an active session.
    /// </summary>
    public class JoinSessionScn : Scene
    {
        #region fields

        protected Microsoft.Xna.Framework.Net.AvailableNetworkSessionCollection m_availableSessions;

        #endregion

        #region methods

        #region construction

        public JoinSessionScn(  uint id,
                                DebugManager debugMgr, ResourceManager resourceMgr,
                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                SpriteFontResHandle sceneFont,
                                Scene parentScn, Menu menu)
            : base(id, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
            Microsoft.Xna.Framework.Graphics.Color.Black, sceneFont,
            parentScn.Stage, parentScn, menu)
        {
            m_camera = new CameraGObj(
                uint.MaxValue, debugMgr, resourceMgr, this, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000.0f);
        }

        #endregion

        #region creation

        public static JoinSessionScn Create(    DebugManager debugMgr,ResourceManager resourceMgr,
                                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                                Scene parent)
        {
            JoinSessionScn scn = new JoinSessionScn(
                GlobalConstants.CLIENT_JOIN_SCN_ID, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                parent,
                new Menu(uint.MaxValue, debugMgr, resourceMgr,
                    graphicsMgr, spriteBatch,
                    new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "HUDFont"),
                    parent.Stage.SceneMgr.Game.LeadPlayerIndex));

            TextureAsset backgroundWall = new TextureAsset(
                1, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "Background",
                debugMgr, resourceMgr, scn, true,
                graphicsMgr);

            TextureAsset joinTitle = new TextureAsset(
                2, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "JoinTitle",
                debugMgr, resourceMgr, scn, true,
                graphicsMgr);

            return scn;
        }

        #endregion

        #region initialisation

        public void SetAvaliableSessions(Microsoft.Xna.Framework.Net.AvailableNetworkSessionCollection availableSessions)
        {
            if (m_availableSessions != null)
            {
                //we must make a new menu
                m_menu = new Menu(uint.MaxValue, m_debugMgr, m_resMgr,
                    Stage.SceneMgr.Game.GraphicsMgr, Stage.SceneMgr.Game.spriteBatch,
                    new SpriteFontResHandle(m_debugMgr, m_resMgr, "Content\\Fonts\\", "HUDFont"),
                    Stage.SceneMgr.Game.LeadPlayerIndex);
            }

            m_availableSessions = availableSessions;

            SpriteFontResHandle mFont = new SpriteFontResHandle(
                m_debugMgr, m_resMgr, "Content\\Fonts\\", "MenuFont");

            MenuWindow joinSessWnd = new MenuWindow(
                10, m_debugMgr, m_resMgr, 
                mFont,
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "MenuBack"),
                GetMenu, true, Stage.SceneMgr.Game.GraphicsMgr,
                Stage.SceneMgr.Game.spriteBatch);

            //add a button for each session that can be joined
            MenuButton mb = null;
            uint id = 1;
            foreach (Microsoft.Xna.Framework.Net.AvailableNetworkSession avSession in availableSessions)
            {
                mb = new MenuButtonAvSession(id, avSession.HostGamertag, "",
                    mFont, m_debugMgr, m_resMgr, 
                    Stage.SceneMgr.Game.GraphicsMgr, Stage.SceneMgr.Game.spriteBatch, 
                    joinSessWnd, true, avSession);

                mb.UIAction += AvailableSessionMenuEntrySelected;

                id++;
            }

            //add the cancel button
            mb = new MenuButton(id, "CANCEL", "", mFont, m_debugMgr, m_resMgr,
                Stage.SceneMgr.Game.GraphicsMgr, Stage.SceneMgr.Game.spriteBatch,
                joinSessWnd, true);
            mb.UIAction += mb_UIAction;

            joinSessWnd.Init();
            joinSessWnd.Load();

            GetMenu.SetDefaultWindow(10);
            GetMenu.Init();
            GetMenu.Load();
        }

        void mb_UIAction(object sender, EventArgs e)
        {
            m_availableSessions.Dispose();

            //go back to main menu
            Stage.SceneMgr.GetStage(GlobalConstants.GAME_STAGE_ID).SetCurrentScene(GlobalConstants.COMMRES_SCN_ID);
            Stage.SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).SetCurrentScene(GlobalConstants.MAIN_MENU_SCN_ID);
            Stage.SceneMgr.SetCurrentStage(GlobalConstants.MAIN_STAGE_ID);
        }

        void AvailableSessionMenuEntrySelected(object sender, EventArgs e)
        {
            // Which menu entry was selected?
            MenuButtonAvSession mb = (MenuButtonAvSession)sender;
            Microsoft.Xna.Framework.Net.AvailableNetworkSession availableSession = mb.GetSession;

            try
            {
                // Begin an asynchronous join network session operation.
                IAsyncResult asyncResult = Microsoft.Xna.Framework.Net.NetworkSession.BeginJoin(availableSession,
                                                                    null, null);

                // Activate the network busy screen, which will display
                // an animation until this operation has completed.
                Stages.Main.NetworkLoadingScn networkloadingScn = (Stages.Main.NetworkLoadingScn)
                    Stage.GetChildScene(GlobalConstants.NET_LOADING_SCN_ID);
                networkloadingScn.AsyncResult = asyncResult;

                networkloadingScn.ClearEvents();
                networkloadingScn.OperationCompleted += JoinOperationCompleted;

                networkloadingScn.Stage.SetCurrentScene(networkloadingScn.Id);
            }
            catch (Exception exception)
            {
                m_debugMgr.WriteLogEntry("JoinSessionScn:SessionSelected - network error joining session. " +
                        exception.ToString());
                Stage.CurrentScene.ShowMessage("Error creating session");
            }
        }

        void JoinOperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            LnaNetworkSessionComponent networkSessionComp = LnaNetworkSessionComponent.FindSessionComponent(Stage.SceneMgr.Game);

            try
            {
                // End the asynchronous join network session operation.
                Microsoft.Xna.Framework.Net.NetworkSession networkSession = 
                    Microsoft.Xna.Framework.Net.NetworkSession.EndJoin(e.AsyncResult);

                // Create a component that will manage the session we just joined.
                m_debugMgr.Assert(networkSessionComp != null, "JoinSession:OpComplete - network component not created.");
                networkSessionComp.Create(networkSession);
                BiophageGame bgame = (BiophageGame) Stage.SceneMgr.Game;

                // Create a component that will manage the session we just created.
                Stages.Main.GameplaySettingsScn gpScn = (Stages.Main.GameplaySettingsScn)Stage.GetChildScene(
                    GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);
                gpScn.sessionDetails.netSessionComponent = networkSessionComp;

                //if session ends, go to main menu
                networkSessionComp.LeavingSession += networkSessionComp_LeavingSession;

                // Go to the lobby screen. We pass null as the controlling player,
                // because the lobby screen accepts input from all local players
                // who are in the session, not just a single controlling player.
                Stage.SetCurrentScene(GlobalConstants.LOBBY_SCN_ID);

                m_availableSessions.Dispose();
            }
            catch (Exception exception)
            {
                m_debugMgr.WriteLogEntry("JoinSessionScn:OpComplete - network error joining session. " +
                        exception.ToString());
                Stage.CurrentScene.ShowMessage("Error creating session");
            }
        }

        void networkSessionComp_LeavingSession(object sender, EventArgs e)
        {
            //re-add network component
            Stage.SceneMgr.Game.Components.Add(new LnaNetworkSessionComponent(m_debugMgr, m_resMgr, Stage.SceneMgr,
                Stage.SceneMgr.Game, new SpriteFontResHandle(m_debugMgr, m_resMgr, "Content\\Fonts\\", "PromptFont")));

            Stage.SceneMgr.GetStage(GlobalConstants.GAME_STAGE_ID).SetCurrentScene(GlobalConstants.COMMRES_SCN_ID, true, true);
            Stage.SceneMgr.GetStage(GlobalConstants.MAIN_STAGE_ID).SetCurrentScene(GlobalConstants.MAIN_MENU_SCN_ID);
            Stage.SceneMgr.SetCurrentStage(GlobalConstants.MAIN_STAGE_ID);

            Stage.SceneMgr.CurrentStage.CurrentScene.GetMenu.SetDefaultWindow(GlobalConstants.MMMAIN_WND_ID);
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

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            //main menu should always be active in this scene
            if (!GetMenu.Active)
                GetMenu.Active = true;

            base.Draw(gameTime, graphicsDevice);
        }

        #endregion

        #endregion
    }
}
