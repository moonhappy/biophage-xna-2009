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
    /// The main menu scene, does somethings in the background
    /// and always shows the menu.
    /// </summary>
    public class MainMenuScn : Scene
    {
        #region fields

        //based on the XNA tutorial - Network state management
        private bool haveShownGuide = false;
        private bool haveShownMarketplace = false;
        private bool showSignIn = false;

        private bool showGPadSelector = true;

        private TextureAsset backgroundWallBkg;
        private TextureAsset mainMenuTitle;
        private TextureAsset gamePadSelector;

        #endregion

        #region events

        public event EventHandler<EventArgs> ProfileSignedIn;

        #endregion

        #region methods

        #region construction

        public MainMenuScn( uint id,
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
                uint.MaxValue, debugMgr, resourceMgr, this, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000f);

            backgroundWallBkg = new TextureAsset(
                1, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "Background",
                debugMgr, resourceMgr, this, true,
                graphicsMgr);

            mainMenuTitle = new TextureAsset(
                2, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "MainMenuTitle",
                debugMgr, resourceMgr, this, true,
                graphicsMgr);
            mainMenuTitle.Active = false;

            gamePadSelector = new TextureAsset(
                3, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage\\", "GPadSelectionTitle",
                debugMgr, resourceMgr, this, true,
                graphicsMgr);
        }

        #endregion

        #region creation

        public static MainMenuScn Create(   DebugManager debugMgr, ResourceManager resourceMgr,
                                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                            Scene parent)
        {
            MainMenuScn scn = new MainMenuScn(
                GlobalConstants.MAIN_MENU_SCN_ID, debugMgr, resourceMgr, 
                graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr,resourceMgr,"Content\\Fonts\\","PromptFont"),
                parent);

            return scn;
        }

        #endregion

        #region field accessors

        public bool ShowSignIn
        {
            get { return showSignIn; }
            set { showSignIn = value; }
        }

        #endregion

        #region game loop

        #region helpers

        /// <summary>
        /// Helper checks whether a valid player profile is signed in. Copied from XNA tutorial
        /// </summary>
        private bool ValidProfileSignedIn()
        {
            // If there is no profile signed in, that is never good.
            Microsoft.Xna.Framework.GamerServices.SignedInGamer gamer = 
                Microsoft.Xna.Framework.GamerServices.Gamer.SignedInGamers[Stage.SceneMgr.Game.LeadPlayerIndex];

            if (gamer == null)
                return false;

            // If we want to play in a Live session, also make sure the profile is
            // signed in to Live, and that it has the privilege for online gameplay.
            Stages.Main.GameplaySettingsScn settingsScn = (Stages.Main.GameplaySettingsScn)
                Stage.GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);
            if (LnaNetworkSessionComponent.IsOnlineSessionType(settingsScn.sessionDetails.netSessionType))
            {
                if (!gamer.IsSignedInToLive)
                    return false;

                if (!gamer.Privileges.AllowOnlineSessions)
                    return false;
            }

            // Okeydokey, this looks good.
            return true;
        }

        /// <summary>
        /// LIVE networking is not supported in trial mode. Rather than just giving
        /// the user an error, this function asks if they want to purchase the full
        /// game, then takes them to Marketplace where they can do that. Once the
        /// Guide is active, the user can either make the purchase, or cancel it.
        /// When the Guide closes, ProfileSignInScreen.Update will notice that
        /// Guide.IsVisible has gone back to false, at which point it will check if
        /// the game is still in trial mode, and either exit the screen or proceed
        /// forward accordingly.
        /// </summary>
        /// <remarks>
        /// Just copied directly from tutorial.
        /// </remarks>
        private  void ShowMarketplace()
        {
            //show the goto marketplace window
            GetMenu.SetCurrentWindow(GlobalConstants.MMGOTOMP_WND_ID);
        }

        #endregion

        public override void Input( Microsoft.Xna.Framework.GameTime gameTime,
                                    ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
                                    , ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
                                    )
        {
            //if show game pad is true - await for someone to press 'a' and start
            if (showGPadSelector)
            {
                Microsoft.Xna.Framework.Input.GamePadState gpState;

                for (int i = 0; i < 4; ++i)
                {
                    gpState = Microsoft.Xna.Framework.Input.GamePad.GetState((Microsoft.Xna.Framework.PlayerIndex)i);
                    if (gpState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A))
                    {
                        Stage.SceneMgr.Game.LeadPlayerIndex = (Microsoft.Xna.Framework.PlayerIndex)i;
                        showGPadSelector = false;
                        break;
                    }

#if !XBOX
                    if (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
                        showGPadSelector = false;
#endif
                }
            }
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //sign in profile
            if (showSignIn)
            {
                Stages.Main.GameplaySettingsScn settingsScn = (Stages.Main.GameplaySettingsScn)
                    Stage.GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);

                if (GetMenu.Active)
                    GetMenu.Active = false;

                if (ValidProfileSignedIn())
                {
                    // As soon as we detect a suitable profile is signed in,
                    // we raise the profile signed in event, then go away.
                    if (ProfileSignedIn != null)
                        ProfileSignedIn(this, EventArgs.Empty);

                    showSignIn = false;
                    GetMenu.Active = true;
                }
                else if (!Microsoft.Xna.Framework.GamerServices.Guide.IsVisible)
                {
                    // If we are in trial mode, and they want to play online, and a profile
                    // is signed in, take them to marketplace so they can purchase the game.
                    if ((Microsoft.Xna.Framework.GamerServices.Guide.IsTrialMode) &&
                        (LnaNetworkSessionComponent.IsOnlineSessionType(settingsScn.sessionDetails.netSessionType)) &&
                        (Microsoft.Xna.Framework.GamerServices.Gamer.SignedInGamers[Stage.SceneMgr.Game.LeadPlayerIndex] != null) &&
                        (!haveShownMarketplace))
                    {
                        ShowMarketplace();

                        haveShownMarketplace = true;
                    }
                    else if (!haveShownGuide && !haveShownMarketplace)
                    {
                        // No suitable profile is signed in, and we haven't already shown
                        // the Guide. Let's show it now, so they can sign in a profile.
                        Microsoft.Xna.Framework.GamerServices.Guide.ShowSignIn(1,
                                LnaNetworkSessionComponent.IsOnlineSessionType(settingsScn.sessionDetails.netSessionType));

                        haveShownGuide = true;
                    }
                    else
                    {
                        // Hmm. No suitable profile is signed in, but we already showed
                        // the Guide, and the Guide isn't still visible. There is only
                        // one thing that can explain this: they must have cancelled the
                        // Guide without signing in a profile. We'd better just exit,
                        // which will leave us on the same menu as before.
                        showSignIn = false;
                        GetMenu.Active = true;
                    }
                }
            }
        }

        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            //main menu should always be active in this scene when g-pad selector is disabled
            if (showGPadSelector)
            {
                if (GetMenu.Active)
                {
                    GetMenu.Active = false;
                    mainMenuTitle.Visible = false;
                    gamePadSelector.Visible = true;
                }
            }
            else if (!GetMenu.Active)
            {
                GetMenu.Active = true;
                mainMenuTitle.Visible = true;
                gamePadSelector.Visible = false;
            }

            base.Draw(gameTime, graphicsDevice);
        }

        #endregion

        #endregion
    }
}
