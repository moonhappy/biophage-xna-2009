/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;

namespace LNA.GameEngine.Objects.UI.Menu
{
    public class Menu : GameObject
    {
        #region fields

        //protected Dictionary<uint, MenuWindow> m_menuWnds;
        protected MenuWindow m_currentWnd;
        protected MenuWindow m_movingWnd;
        protected Stack<MenuWindow> m_previousWnds;

        protected CameraGObj m_currentWndView;
        protected CameraGObj m_movingWndView;
        protected CameraGObj m_previousWndsView;

        protected bool m_animInwards;

        protected Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;
        protected Microsoft.Xna.Framework.Graphics.SpriteBatch m_spriteBatch;

        protected SpriteFontResHandle m_descriptionsFont;

        protected Microsoft.Xna.Framework.PlayerIndex m_leadPlayerIndex;

        //to help lower xbox impact
        public bool m_renderContextInvalid = true;
        protected RenderTargetResHandle m_renderTarget;
        protected Microsoft.Xna.Framework.Graphics.Texture2D m_resolveTexture;
        Microsoft.Xna.Framework.Graphics.ResolveTexture2D m_prevFrame;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Menu Id.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics device manager.
        /// </param>
        /// <param name="spriteBatch">
        /// Reference to the XNA sprite batch system.
        /// </param>
        public Menu(    uint id, 
                        DebugManager debugMgr, ResourceManager resourceMgr,
                        Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                        Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                        SpriteFontResHandle descriptionsFont,
                        Microsoft.Xna.Framework.PlayerIndex controlingPlayerIndex)
            : base(id, debugMgr, resourceMgr, null, false)
        {
            //set fields
            //m_menuWnds = new Dictionary<uint, MenuWindow>();
            m_currentWnd = null;
            m_movingWnd = null;
            m_previousWnds = new Stack<MenuWindow>();
            m_isActive = false;
            m_animInwards = true;
            m_spriteBatch = spriteBatch;
            m_graphicsMgr = graphicsMgr;
            m_descriptionsFont = descriptionsFont;
            m_leadPlayerIndex = controlingPlayerIndex;
            m_renderTarget = new RenderTargetResHandle(
                debugMgr, resourceMgr, "MenuRenderTarget");
            m_renderTarget.Width = graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth;
            m_renderTarget.Height = graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight;
            m_renderTarget.DiscardContents = false;

            //cameras
            m_currentWndView = new CameraGObj(
                uint.MaxValue - id,
                debugMgr, resourceMgr,
                null, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 2f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 0.000000125f, 10000f);

            m_movingWndView = new CameraGObj(
                uint.MaxValue - id - 1,
                debugMgr, resourceMgr,
                null, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 2f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 0.000000125f, 10000f);

            m_previousWndsView = new CameraGObj(
                uint.MaxValue - id - 2,
                debugMgr, resourceMgr,
                null, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 4f),
                new Microsoft.Xna.Framework.Vector3(1.5f, 0f, 0f),
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 0.000000125f, 10000f);

            m_prevFrame = new Microsoft.Xna.Framework.Graphics.ResolveTexture2D(
                   m_graphicsMgr.GraphicsDevice,
                   m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth,
                   m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight,
                   1,
                   m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferFormat);
            m_resolveTexture = new Microsoft.Xna.Framework.Graphics.Texture2D(
                m_graphicsMgr.GraphicsDevice,
                m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth,
                m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight,
                1, Microsoft.Xna.Framework.Graphics.TextureUsage.None,
                m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferFormat);

            //log
            //m_debugMgr.WriteLogEntry("Menu:Constructor - done.");
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Initialises the menu.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                //init menu windows
                foreach (KeyValuePair<uint, GameObject> mWndKVP in m_childGameObjs)
                {
                    if (!mWndKVP.Value.Init())
                        retVal = false;
                }

                //cameras
                if (    (!m_currentWndView.Init())  ||
                        (!m_movingWndView.Init())     ||
                        (!m_previousWndsView.Init()))
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        /// <summary>
        /// Reinitialises the menu.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Reinit()
        {
            bool retVal = true;

                //init menu windows
                foreach (KeyValuePair<uint, GameObject> mWndKVP in m_childGameObjs)
                {
                    if (!mWndKVP.Value.Reinit())
                        retVal = false;
                }

                //cameras
                if (    (!m_currentWndView.Reinit())    ||
                        (!m_movingWndView.Reinit())       ||
                        (!m_previousWndsView.Reinit())  )
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            

            return retVal;
        }

        #region loading

        /// <summary>
        /// Loads the menu.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if (    (!m_currentWndView.Load())  ||
                        (!m_movingWndView.Load())     ||
                        (!m_previousWndsView.Load()) ||
                        (!m_renderTarget.Load()))
                    retVal = false;

                if (m_descriptionsFont != null)
                    if (!m_descriptionsFont.Load())
                        retVal = false;

                m_renderContextInvalid = true;

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        /// <summary>
        /// Unloads the menu.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                //unload menu windows
                foreach (KeyValuePair<uint, GameObject> mWndKVP in m_childGameObjs)
                {
                    if (!mWndKVP.Value.Unload())
                        retVal = false;
                }

                if (    (!m_currentWndView.Unload())    ||
                        (!m_movingWndView.Unload())       ||
                        (!m_previousWndsView.Unload()) ||
                        (!m_renderTarget.Unload()))
                    retVal = false;

                if (m_descriptionsFont != null)
                    if (!m_descriptionsFont.Unload())
                        retVal = false;

                m_renderContextInvalid = true;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        /// <summary>
        /// Deinitialises the menu.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Deinit()
        {
            bool retVal = true;
            if (m_isInit)
            {
                //deinit menu windows
                foreach (KeyValuePair<uint, GameObject> mWndKVP in m_childGameObjs)
                {
                    if (!mWndKVP.Value.Deinit())
                        retVal = false;
                }

                //cameras
                if (    (!m_currentWndView.Deinit())    ||
                        (!m_movingWndView.Deinit())       ||
                        (!m_previousWndsView.Deinit())  )
                    retVal = false;

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Active propertie of the menu.
        /// </summary>
        public override bool Active
        {
            get { return base.Active; }
            set
            {
                base.Active = value;

                if (value)
                {
                    //reactivate menu windows - to reconfigure inputs
                    foreach (KeyValuePair<uint, GameObject> mwndKVP in m_childGameObjs)
                    {
                        if (mwndKVP.Value.Active)
                            mwndKVP.Value.Active = true;
                    }
                }
            }
        }

        /// <summary>
        /// The player index assigned to control this menu.
        /// </summary>
        public Microsoft.Xna.Framework.PlayerIndex ControlingPlayerIndex
        {
            get { return m_leadPlayerIndex; }
        }

        #endregion

        #region menu_windows

        /// <summary>
        /// Adds a menu window to the menu.
        /// </summary>
        /// <param name="menuWnd">
        /// Reference to the menu window.
        /// </param>
        public void AddMenuWindow(MenuWindow menuWnd)
        {
            m_debugMgr.Assert(menuWnd != null,
                "Menu:AddMenuWindow - 'menuWnd' is null.");

            //m_menuWnds.Add(menuWnd.Id, menuWnd);
            m_childGameObjs.Add(menuWnd.Id, (GameObject)menuWnd);

            //make the window inactive ATM
            menuWnd.Active = false;
        }

        /// <summary>
        /// Returns a reference to the menu window with matching
        /// Id.
        /// </summary>
        /// <param name="menuId">
        /// Id to find match.
        /// </param>
        /// <returns>
        /// Reference to the menu window with matching Id.
        /// </returns>
        public MenuWindow GetMenuWindow(uint menuId)
        {
            //find and return
            GameObject retMenuWnd = null;
            m_childGameObjs.TryGetValue(menuId, out retMenuWnd);

            return (MenuWindow)retMenuWnd;
        }

        /// <summary>
        /// Reference to the current window.
        /// </summary>
        public MenuWindow CurrentWindow
        {
            get { return m_currentWnd; }
        }

        /// <summary>
        /// Sets the current window, or changes the current.
        /// </summary>
        /// <param name="nextWindowId">
        /// Id of the menu window to set as current.
        /// </param>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool SetCurrentWindow(uint nextWindowId)
        {
            //check next scene Id match
            MenuWindow nextWindow;
            bool returnFlag = true;

            //check if window is already current
            if (m_currentWnd != null)
                if (nextWindowId == m_currentWnd.Id)
                    return true;

            if (!m_childGameObjs.ContainsKey(nextWindowId))
            {
                //Game doesn't need to be killed if match not found as it can resume
                //  with the current window. Debugging should check this error.
                string sLogEntry = "Menu:SetCurrentWindow - no Id match found for param 'nextWindowId'=";
                sLogEntry += nextWindowId.ToString();
                m_debugMgr.WriteLogEntry(sLogEntry);

                return false;
            }
                
            nextWindow = (MenuWindow)m_childGameObjs[nextWindowId];

            //window cannot be null
            m_debugMgr.Assert(nextWindow != null,
                "Menu:SetCurrentWindow - varible 'nextWindow' is null.");

            //set current window to last (for move animation) - and deactivate it
            if (m_currentWnd == null)
            {
                m_currentWnd = nextWindow;
                m_currentWnd.Active = true;
                m_currentWnd.Load();
            }
            else
            {
                m_movingWnd = m_currentWnd;
                m_movingWnd.Active = false;

                //set current window to the next window - don't activate until animation finishes
                m_currentWnd = nextWindow;
                m_currentWnd.Load();

                //make sure 'last window view' camera is back to normal - and time is n/a
                m_movingWndView.Position = m_currentWndView.Position;
                m_movingWndView.LookingAt = m_currentWndView.LookingAt;
                m_movingWndView.UpdateCamera();

                //signal that anim should reveal the next window
                m_animInwards = true;
            }

            return returnFlag;
        }

        /// <summary>
        /// This does not transition to sub windows, rather
        /// it sets the window as the default.
        /// </summary>
        /// <param name="windowId">
        /// Id of the menu window to set as the default.
        /// </param>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool SetDefaultWindow(uint windowId)
        {
            //check next scene Id match
            MenuWindow nextWindow;
            bool returnFlag = true;

            //empty previous windows, set moving to null
            if (m_previousWnds.Count > 0)
                m_previousWnds.Peek().Unload();
            m_previousWnds.Clear();

            if (m_movingWnd != null)
                m_movingWnd.Unload();
            m_movingWnd = null;

            //check if window is already current
            if (m_currentWnd != null)
                if (windowId == m_currentWnd.Id)
                    return true;

            if (!m_childGameObjs.ContainsKey(windowId))
            {
                //Game doesn't need to be killed if match not found as it can resume
                //  with the current window. Debugging should check this error.
                string sLogEntry = "Menu:SetDefaultWindow - no Id match found for param 'nextWindowId'=";
                sLogEntry += windowId.ToString();
                m_debugMgr.WriteLogEntry(sLogEntry);

                return false;
            }

            nextWindow = (MenuWindow)m_childGameObjs[windowId];

            //window cannot be null
            m_debugMgr.Assert(nextWindow != null,
                "Menu:SetDefaultWindow - varible 'nextWindow' is null.");

            //set current window to the new default
            if (m_currentWnd != null)
            {
                m_currentWnd.Active = false;
                m_currentWnd.Unload();
            }
            m_currentWnd = nextWindow;
            m_currentWnd.Active = true;
            m_currentWnd.Load();

            return returnFlag;
        }

        /// <summary>
        /// Call when you want to transit to the previous window.
        /// </summary>
        /// <returns>
        /// True if changed to previous window, otherwise false will be
        /// returned if there is no previous windows.
        /// </returns>
        public bool SetCurrentToPreviousWindow()
        {
            //exit quickly if nothing on previous wnds stack
            if (m_previousWnds.Count == 0)
            {
                if (m_movingWnd == null)
                    return false;
                else
                    return true;
            }

            //make current inactive
            m_currentWnd.Active = false;

            //set moving window to the top previous window and pop
            m_movingWnd = m_previousWnds.Pop();

            //load new top if there
            if (m_previousWnds.Count > 0)
                m_previousWnds.Peek().Load();

            //signal that we should pan to the previous window
            m_animInwards = false;
            m_movingWndView.Position = m_previousWndsView.Position;
            m_movingWndView.LookingAt = m_previousWndsView.LookingAt;
            m_movingWndView.UpdateCamera();

            return true;
        }

        #endregion

        #region game_loop

        public void Input(  ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
                            ,ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
            )
        {
            //update menu windows
            foreach (KeyValuePair<uint, GameObject> mWndKVP in m_childGameObjs)
            {
                MenuWindow mwnd = (MenuWindow)mWndKVP.Value;
                if (mwnd.Active)
                {
                    mwnd.Input(ref newGPState
#if !XBOX
                        , ref newKBState
#endif
                        );
                }
                if (mwnd.RegisteredInput)
                    m_renderContextInvalid = true;
            }
        }

        /// <summary>
        /// Update the menu.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //update menu windows
            foreach (KeyValuePair<uint, GameObject> mWndKVP in m_childGameObjs)
            {
                if (mWndKVP.Value.Active)
                    mWndKVP.Value.Update(gameTime);
            }

            //cameras
            m_currentWndView.Update(gameTime);
            m_movingWndView.Update(gameTime);
            m_previousWndsView.Update(gameTime);
        }

        /// <summary>
        /// Animates the menu.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //animate the last window view camera
            if (m_movingWnd != null)
            {
                m_renderContextInvalid = true;

                if (!m_movingWnd.IsLoaded)
                    m_movingWnd.Load();

                //turn camera slightly
                Microsoft.Xna.Framework.Vector3 lookingAt = m_movingWndView.LookingAt;
                Microsoft.Xna.Framework.Vector3 camPosition = m_movingWndView.Position;
                if (m_animInwards)
                {
                    if ((lookingAt.X += 0.003f * gameTime.ElapsedRealTime.Milliseconds) >= 1.5f)
                    {
                        //finish animation procedure - make correction to X
                        lookingAt.X = 1.5f;
                        camPosition.Z = 4f;
                        //push to previous windows stack
                        if (m_previousWnds.Count > 0)
                            m_previousWnds.Peek().Unload();
                        m_previousWnds.Push(m_movingWnd);
                        //last window is now on top of stack
                        m_movingWnd = null;
                        //make new window active
                        m_currentWnd.Active = true;
                    }
                    else if ((camPosition.Z += 0.004f * gameTime.ElapsedRealTime.Milliseconds) >= 4f)
                    {
                        //finish animation procedure - make correction to X
                        lookingAt.X = 1.5f;
                        camPosition.Z = 4f;
                        //push to previous windows stack
                        if (m_previousWnds.Count > 0)
                            m_previousWnds.Peek().Unload();
                        m_previousWnds.Push(m_movingWnd);
                        //last window is now on top of stack
                        m_movingWnd = null;
                        //make new window active
                        m_currentWnd.Active = true;
                    }
                }
                else
                {
                    if ((lookingAt.X -= 0.003f * gameTime.ElapsedRealTime.Milliseconds) <= 0f)
                    {
                        //finish animation procedure - make correction to X
                        lookingAt.X = 0f;
                        camPosition.Z = 2f;
                        //last window is now current
                        m_currentWnd.Unload();
                        m_currentWnd = m_movingWnd;
                        m_movingWnd = null;
                        //make window active
                        m_currentWnd.Active = true;
                    }
                    else if ((camPosition.Z -= 0.004f * gameTime.ElapsedRealTime.Milliseconds) <= 2f)
                    {
                        //finish animation procedure - make correction to X
                        lookingAt.X = 0f;
                        camPosition.Z = 2f;
                        //last window is now current
                        m_currentWnd.Unload();
                        m_currentWnd = m_movingWnd;
                        m_movingWnd = null;
                        //make window active
                        m_currentWnd.Active = true;
                    }
                }

                //update cam
                m_movingWndView.Position = camPosition;
                m_movingWndView.LookingAt = lookingAt;
            }

            //animate objects
            //update menu windows
            foreach (KeyValuePair<uint, GameObject> mWndKVP in m_childGameObjs)
            {
                if (mWndKVP.Value.Active)
                    mWndKVP.Value.Animate(gameTime);
            }

            //cameras
            m_currentWndView.Animate(gameTime);
            m_movingWndView.Animate(gameTime);
            m_previousWndsView.Animate(gameTime);
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// Reference to the XNA graphics device manager.
        /// </param>
        /// <param name="camera">
        /// Camera is ignored with the rendering of the menu.
        /// </param>
        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            bool renderContextWasInvalid = m_renderContextInvalid;
            Microsoft.Xna.Framework.Graphics.RenderTarget2D rt =
                    (Microsoft.Xna.Framework.Graphics.RenderTarget2D)m_renderTarget.GetResource;

            #region draw to rendertarget
            if (m_renderContextInvalid)
            {
                m_renderContextInvalid = false;

                //save copy of background
                graphicsDevice.ResolveBackBuffer(m_prevFrame);

                graphicsDevice.SetRenderTarget(0, rt);
                {
                    graphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.TransparentBlack);

                    //draw the previous windows first - only the current top
                    if (m_previousWnds.Count > 0)
                        m_previousWnds.Peek().DoDraw(gameTime, graphicsDevice, m_previousWndsView);

                    //then the current window
                    if (m_currentWnd != null)
                        m_currentWnd.DoDraw(gameTime, graphicsDevice, m_currentWndView);

                    //then (if it exists) the last window in animation
                    if (m_movingWnd != null)
                        m_movingWnd.DoDraw(gameTime, graphicsDevice, m_movingWndView);

                    //finaly the description of the selected object - only if now moving windows
                    if (m_movingWnd == null)
                        DrawSelectedDescription(gameTime, graphicsDevice, camera);
                }

                graphicsDevice.SetRenderTarget(0, null);

                m_resolveTexture = rt.GetTexture();
            }
            #endregion

            #region draw to back buffer

            m_spriteBatch.Begin(Microsoft.Xna.Framework.Graphics.SpriteBlendMode.AlphaBlend);

            if (renderContextWasInvalid)
            {
                m_spriteBatch.Draw(m_prevFrame, 
                    Microsoft.Xna.Framework.Vector2.Zero, 
                    Microsoft.Xna.Framework.Graphics.Color.White);
            }

            m_spriteBatch.Draw(m_resolveTexture, 
                Microsoft.Xna.Framework.Vector2.Zero, 
                Microsoft.Xna.Framework.Graphics.Color.White);

            m_spriteBatch.End();

            #endregion
        }

        private void DrawSelectedDescription(   Microsoft.Xna.Framework.GameTime gameTime, 
                                                Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                                CameraGObj camera)
        {
            //determine lead in position - description will be to the right of the window
            if ((m_currentWnd.Description.Length > 0) && (m_descriptionsFont != null))
            {
                Microsoft.Xna.Framework.Vector2 descPos = new Microsoft.Xna.Framework.Vector2(
                    (float)(m_graphicsMgr.PreferredBackBufferWidth * 0.71),
                    (float)(m_graphicsMgr.PreferredBackBufferHeight * 0.5));

                m_spriteBatch.Begin();

                descPos.X -= 1;
                descPos.Y -= 1;

                m_spriteBatch.DrawString(
                    (Microsoft.Xna.Framework.Graphics.SpriteFont)m_descriptionsFont.GetResource,
                    m_currentWnd.Description,
                    descPos, Microsoft.Xna.Framework.Graphics.Color.Black);

                descPos.X += 1;

                m_spriteBatch.DrawString(
                    (Microsoft.Xna.Framework.Graphics.SpriteFont)m_descriptionsFont.GetResource,
                    m_currentWnd.Description,
                    descPos, Microsoft.Xna.Framework.Graphics.Color.Black);

                descPos.X += 1;

                m_spriteBatch.DrawString(
                    (Microsoft.Xna.Framework.Graphics.SpriteFont)m_descriptionsFont.GetResource,
                    m_currentWnd.Description,
                    descPos, Microsoft.Xna.Framework.Graphics.Color.Black);

                descPos.X -= 2;
                descPos.Y += 1;

                m_spriteBatch.DrawString(
                    (Microsoft.Xna.Framework.Graphics.SpriteFont)m_descriptionsFont.GetResource,
                    m_currentWnd.Description,
                    descPos, Microsoft.Xna.Framework.Graphics.Color.Black);

                descPos.X += 2;

                m_spriteBatch.DrawString(
                    (Microsoft.Xna.Framework.Graphics.SpriteFont)m_descriptionsFont.GetResource,
                    m_currentWnd.Description,
                    descPos, Microsoft.Xna.Framework.Graphics.Color.Black);

                descPos.X -= 2;
                descPos.Y += 1;

                m_spriteBatch.DrawString(
                    (Microsoft.Xna.Framework.Graphics.SpriteFont)m_descriptionsFont.GetResource,
                    m_currentWnd.Description,
                    descPos, Microsoft.Xna.Framework.Graphics.Color.Black);

                descPos.X += 1;

                m_spriteBatch.DrawString(
                    (Microsoft.Xna.Framework.Graphics.SpriteFont)m_descriptionsFont.GetResource,
                    m_currentWnd.Description,
                    descPos, Microsoft.Xna.Framework.Graphics.Color.Black);

                descPos.X += 1;

                m_spriteBatch.DrawString(
                    (Microsoft.Xna.Framework.Graphics.SpriteFont)m_descriptionsFont.GetResource,
                    m_currentWnd.Description,
                    descPos, Microsoft.Xna.Framework.Graphics.Color.Black);

                descPos.X -= 1;
                descPos.Y -= 1;

                m_spriteBatch.DrawString(
                    (Microsoft.Xna.Framework.Graphics.SpriteFont)m_descriptionsFont.GetResource,
                    m_currentWnd.Description,
                    descPos, Microsoft.Xna.Framework.Graphics.Color.White);

                m_spriteBatch.End();
            }
        }

        #endregion

        #endregion
    }
}
