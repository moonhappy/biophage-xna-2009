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
    /// Custom EventArgs class used by the NetworkBusyScreen.OperationCompleted event.
    /// </summary>
    /// <remarks>
    /// Copied directly from XNA tutorial.
    /// </remarks>
    public class OperationCompletedEventArgs : EventArgs
    {
        #region Properties


        /// <summary>
        /// Gets or sets the IAsyncResult associated with
        /// the network operation that has just completed.
        /// </summary>
        public IAsyncResult AsyncResult
        {
            get { return asyncResult; }
            set { asyncResult = value; }
        }

        IAsyncResult asyncResult;


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new event arguments class.
        /// </summary>
        public OperationCompletedEventArgs(IAsyncResult asyncResult)
        {
            this.asyncResult = asyncResult;
        }


        #endregion
    }

    public class NetworkLoadingScn : Scene
    {
        #region fields

        protected IAsyncResult asyncResult = null;

        Microsoft.Xna.Framework.Vector2 FontPos;
        Microsoft.Xna.Framework.Vector2 FontOrigin;

        protected SpriteFontResHandle m_font;

        public const string m_string = "Please wait...";

        #endregion

        #region Events

        public event EventHandler<OperationCompletedEventArgs> OperationCompleted;

        #endregion

        #region methods

        #region construction

        public NetworkLoadingScn(   uint id,
                                    DebugManager debugMgr, ResourceManager resourceMgr,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                    Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                    SpriteFontResHandle sceneFont,
                                    Scene parentScn)
            : base(id, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
            Microsoft.Xna.Framework.Graphics.Color.Black, sceneFont,
            parentScn.Stage, parentScn, null)
        {
            m_camera = new CameraGObj(
                uint.MaxValue, debugMgr, resourceMgr, this, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000f);

            TextureAsset bkgrd = new TextureAsset(
                2, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage\\", "Background",
                debugMgr, resourceMgr, this, true, graphicsMgr);

            m_fadeOverlay = new QuadAsset(
                1, Microsoft.Xna.Framework.Vector3.Zero, 2f, 2f,
                new Microsoft.Xna.Framework.Graphics.Color(0f, 0f, 0f, 0.5f),
                m_debugMgr, m_resMgr, this, true, graphicsMgr);
            m_fadeOverlay.Visible = true;

            GetMsgBoxButton.UIAction += new UIEventAction(GetMsgBoxButton_UIAction);


            m_font = new SpriteFontResHandle(
                debugMgr, resourceMgr, "Content\\Fonts\\", "MenuFont");
        }

        void GetMsgBoxButton_UIAction(object sender, EventArgs e)
        {
            //go back to main menu
            Stage.SetCurrentScene(GlobalConstants.MAIN_MENU_SCN_ID);
        }

        #endregion

        #region creation

        public static NetworkLoadingScn Create( DebugManager debugMgr, ResourceManager resourceMgr,
                                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                                Scene parent)
        {
            NetworkLoadingScn scn = new NetworkLoadingScn(
                GlobalConstants.NET_LOADING_SCN_ID, debugMgr, resourceMgr,
                graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                parent);

            return scn;
        }

        public void ClearEvents()
        {
            OperationCompleted = null;
        }

        #endregion

        #region loading

        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                retVal = base.Load();

                if (!m_font.Load())
                    retVal = false;

                FontPos = new Microsoft.Xna.Framework.Vector2(
                    Stage.SceneMgr.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f,
                    Stage.SceneMgr.Game.GraphicsDevice.PresentationParameters.BackBufferHeight / 2f);

                FontOrigin = ((Microsoft.Xna.Framework.Graphics.SpriteFont)m_font.GetResource).MeasureString(m_string) / 2;

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

                if (!m_font.Unload())
                    retVal = false;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        #region field accessors

        public IAsyncResult AsyncResult
        {
            get { return asyncResult; }
            set { asyncResult = value; }
        }

        //public Menu SetMenu
        //{
        //    set
        //    {
        //        m_menu = value;
        //        if (m_isInit && (!m_menu.IsInit))
        //            m_menu.Init();
        //        if (m_isLoaded && (!m_menu.IsLoaded))
        //            m_menu.Load();
        //    }
        //}

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
            // Has our asynchronous operation completed?
            if ((asyncResult != null) && asyncResult.IsCompleted)
            {
                // If so, raise the OperationCompleted event.
                if (OperationCompleted != null)
                    OperationCompleted(this,
                        new OperationCompletedEventArgs(asyncResult));

                //let op complete handle transition

                asyncResult = null;
            }
        }

        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            //deactivate menu
            if (GetMenu != null)
                GetMenu.Active = false;

            base.Draw(gameTime, graphicsDevice);

            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch = Stage.SceneMgr.Game.spriteBatch;

            spriteBatch.Begin();

            spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_font.GetResource, 
                m_string, FontPos, Microsoft.Xna.Framework.Graphics.Color.White, 
                0f, FontOrigin, 1f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0.5f);

            spriteBatch.End();
        }

        #endregion

        #endregion
    }
}
