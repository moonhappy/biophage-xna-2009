/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.GameObjects.Assets;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;
using LNA.GameEngine.Resources.Playable;

using Microsoft.Xna.Framework.Input;

namespace LNA.GameEngine.Objects.UI.Menu
{
    /// <summary>
    /// Represents a vertical aligned menu window.
    /// </summary>
    public class MenuWindow : Asset
    {
        #region constants

        public const float MinHeight = 0.0000125f;
        public const float ConstWidth = 1f;
        public const float ConstCornerRadius = 0.025f;

        #endregion

        #region fields

        protected Menu m_menu;
        protected RoundCornerQuadAsset m_menuWndFace;
        protected MenuObject m_menuObjSelected;
        protected int m_menuObjSelectedId;

        protected GamePadState m_previousGPState;
#if !XBOX
        protected KeyboardState m_previousKBState;
#endif
        //helper icons
        protected TextureAsset m_selectIcon;
        protected FontTextureAsset m_selectIconFont;
        protected FontTextureAsset m_selectIconFontOutline;
        protected TextureAsset m_backIcon;
        protected FontTextureAsset m_backIconFont;
        protected FontTextureAsset m_backIconFontOutline;

        protected float m_menuWndWidth;

        //sound effects
        protected SoundResHandle m_sndButtonAction;
        protected SoundResHandle m_sndMoveSelection;
        protected SoundResHandle m_sndSetPrevMWindow;

        //font
        protected SpriteFontResHandle m_menuWndFont;
        protected bool m_registeredInput = false;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Arguement constructor.
        /// </summary>
        /// <param name="id">
        /// Menu window Id.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="font">
        /// Font to use for the icon helpers.
        /// </param>
        /// <param name="sndButtonAction">
        /// Sound to play when a menu button is 'clicked'.
        /// </param>
        /// <param name="sndMoveAction">
        /// Sound to play when moving selection.
        /// </param>
        /// <param name="menu">
        /// Reference to the menu this menu window belongs to.
        /// </param>
        /// <param name="addToMenu">
        /// If true, the menu window will be automatically added to menu.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics manager.
        /// </param>
        public MenuWindow(uint id,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            SpriteFontResHandle font, 
                            SoundResHandle sndButtonAction, SoundResHandle sndMoveAction,
                            SoundResHandle sndSetPrevMWindow,
                            Menu menu, bool addToMenu,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
            : base(id, Microsoft.Xna.Framework.Vector3.Zero, debugMgr, resourceMgr, null, false)
        {
            //set fields
            m_menu = menu;
            m_menuObjSelected = null;
            m_menuWndWidth = ConstWidth;
            m_sndButtonAction = sndButtonAction;
            m_sndMoveSelection = sndMoveAction;
            m_sndSetPrevMWindow = sndSetPrevMWindow;
            m_menuWndFont = font;

            //asserts
            m_debugMgr.Assert(m_menu != null, "MenuWindow:Constructor - 'menu' is null.");
            
            //misc
            m_menuWndFace = new RoundCornerQuadAsset(
                uint.MaxValue - id, m_position,
                m_menuWndWidth, m_menuWndWidth, ConstCornerRadius,
                Microsoft.Xna.Framework.Graphics.Color.DarkSlateGray,
                debugMgr, resourceMgr, null, false,
                graphicsMgr);

            //helper icons
            m_selectIcon = new TextureAsset(
                uint.MaxValue - id,
                Microsoft.Xna.Framework.Vector3.Zero,
                0.075f, 0.07f,
                "Content\\Common\\ButtonImages", "xboxControllerButtonA",
                debugMgr, resourceMgr, null, false,
                graphicsMgr);
            m_selectIconFont = new FontTextureAsset(
                uint.MaxValue - id - (uint)1,
                Microsoft.Xna.Framework.Vector3.Zero,
                0.65f, 0.6f,
                Microsoft.Xna.Framework.Graphics.Color.White, "- SELECT", font,
                true, debugMgr, resourceMgr, null, false,
                graphicsMgr, spriteBatch);
            m_selectIconFontOutline = new FontTextureAsset(
                uint.MaxValue - id - (uint)11,
                Microsoft.Xna.Framework.Vector3.Zero,
                0.65f, 0.6f,
                Microsoft.Xna.Framework.Graphics.Color.Black, "- SELECT", font,
                true, debugMgr, resourceMgr, null, false,
                graphicsMgr, spriteBatch);

            m_backIcon = new TextureAsset(
                uint.MaxValue - id - (uint)2,
                Microsoft.Xna.Framework.Vector3.Zero,
                0.075f, 0.07f,
                "Content\\Common\\ButtonImages", "xboxControllerButtonB",
                debugMgr, resourceMgr, null, false,
                graphicsMgr);
            m_backIconFont = new FontTextureAsset(
                uint.MaxValue - id - (uint)3,
                Microsoft.Xna.Framework.Vector3.Zero,
                0.65f, 0.6f,
                Microsoft.Xna.Framework.Graphics.Color.White, "- BACK", font,
                true, debugMgr, resourceMgr, null, false,
                graphicsMgr, spriteBatch);
            m_backIconFontOutline = new FontTextureAsset(
                uint.MaxValue - id - (uint)13,
                Microsoft.Xna.Framework.Vector3.Zero,
                0.65f, 0.6f,
                Microsoft.Xna.Framework.Graphics.Color.Black, "- BACK", font,
                true, debugMgr, resourceMgr, null, false,
                graphicsMgr, spriteBatch);

            if (addToMenu)
                m_menu.AddMenuWindow(this);

            //log
            //m_debugMgr.WriteLogEntry("MenuWindow:Constructor - done.");
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Common code for position settting.
        /// </summary>
        private void CommonSetPos(float currYPos, float yPosDelta)
        {
            //go through and set pos for all objs
            foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
            {
                ((MenuObject)mobjKVP.Value).Position = new Microsoft.Xna.Framework.Vector3(0f, currYPos, 0f);
                currYPos -= yPosDelta;
                ((MenuObject)mobjKVP.Value).IsSelected = false;
            }

            //set window face height
            float menuWndHeight = MinHeight + (m_childGameObjs.Count * (MenuObject.HeightDelta + MinHeight));
            m_menuWndFace.Height = menuWndHeight;

            //make the first button selected
            foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
            {
                if (!(mobjKVP.Value is MenuLabel))
                {
                    m_menuObjSelected = (MenuObject)mobjKVP.Value;
                    m_menuObjSelected.IsSelected = true;
                    m_menuObjSelectedId = (int)mobjKVP.Key;
                    break;
                }
            }
            //position the helper icons
            m_selectIcon.Position = new Microsoft.Xna.Framework.Vector3(-0.475f, -(menuWndHeight / 2f) - 0.075f, 0f);
            m_selectIconFont.Position = new Microsoft.Xna.Framework.Vector3(-0.275f, -(menuWndHeight / 2f) - 0.075f, 0f);
            m_selectIconFontOutline.Position = new Microsoft.Xna.Framework.Vector3(-0.27f, -(menuWndHeight / 2f) - 0.08f, 0f);

            m_backIcon.Position = new Microsoft.Xna.Framework.Vector3(0f, -(menuWndHeight / 2f) - 0.075f, 0f);
            m_backIconFont.Position = new Microsoft.Xna.Framework.Vector3(0.2f, -(menuWndHeight / 2f) - 0.075f, 0f);
            m_backIconFontOutline.Position = new Microsoft.Xna.Framework.Vector3(0.205f, -(menuWndHeight / 2f) - 0.08f, 0f);
        }

        /// <summary>
        /// Sets the positions for each menu object when there are an even
        /// number of menu objects.
        /// </summary>
        private void SetEvenObjsPos()
        {
            float yPosDelta = MenuObject.HeightDelta + MinHeight;

            //set top Y pos as current Y pos
            float currYPos = ((float)((m_childGameObjs.Count / 2) - 1) * yPosDelta) +
                (0.5f * (MenuObject.HeightDelta + MinHeight));

            CommonSetPos(currYPos, yPosDelta);
        }

        /// <summary>
        /// Sets the positions for each menu object when there are an odd
        /// number of menu objects.
        /// </summary>
        private void SetOddObjsPos()
        {
            float yPosDelta = MenuObject.HeightDelta + MinHeight;

            //set top Y pos as current Y pos
            float currYPos = ((float)((m_childGameObjs.Count - 1) / 2)) * yPosDelta;

            CommonSetPos(currYPos, yPosDelta);
        }

        /// <summary>
        /// Initialises the menu window.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                if ((!m_menuWndFace.Init())
                        || (!m_selectIcon.Init()) ||
                        (!m_selectIconFont.Init()) ||
                        (!m_selectIconFontOutline.Init())||
                        (!m_backIcon.Init()) ||
                        (!m_backIconFont.Init()) ||
                        (!m_backIconFontOutline.Init())
                    )
                    retVal = false;

                //set the window objects' positions
                if ((m_childGameObjs.Count % 2f) == 0f)
                    //even num of menu objs
                    SetEvenObjsPos();
                else
                    //odd num of menu objs
                    SetOddObjsPos();

                //init all objects
                foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
                {
                    if (!mobjKVP.Value.Init())
                        retVal = false;
                }

                if (!base.Init())
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        /// <summary>
        /// Reinitialises the menu window.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Reinit()
        {
            bool retVal = true;

            if ((!m_menuWndFace.Reinit())
                    || (!m_selectIcon.Reinit()) ||
                    (!m_selectIconFont.Reinit()) ||
                    (!m_selectIconFontOutline.Reinit()) ||
                    (!m_backIcon.Reinit()) ||
                    (!m_backIconFont.Reinit()) ||
                    (!m_backIconFontOutline.Reinit()) 
                )
                retVal = false;

            //set the window objects' positions
            if ((m_childGameObjs.Count % 2f) == 0f)
                //even num of menu objs
                SetEvenObjsPos();
            else
                //odd num of menu objs
                SetOddObjsPos();

            foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
            {
                if (!mobjKVP.Value.Reinit())
                    retVal = false;
            }

            if (!base.Reinit())
                retVal = false;

            if (retVal)
                m_isInit = true;
            else
                m_isInit = false;

            return retVal;
        }

        #region loading

        /// <summary>
        /// Loads the menu window.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if ((!m_menuWndFace.Load())
                        || (!m_selectIcon.Load()) ||
                        (!m_selectIconFont.Load()) ||
                        (!m_selectIconFontOutline.Load()) ||
                        (!m_backIcon.Load()) ||
                        (!m_backIconFont.Load()) ||
                        (!m_backIconFontOutline.Load()) 
                    )
                    retVal = false;

                if (m_sndButtonAction != null)
                    if (!m_sndButtonAction.Load())
                        retVal = false;
                if (m_sndMoveSelection != null)
                    if (!m_sndMoveSelection.Load())
                        retVal = false;
                if (m_sndSetPrevMWindow != null)
                    if (!m_sndSetPrevMWindow.Load())
                        retVal = false;

                foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
                {
                    if (!mobjKVP.Value.Load())
                        retVal = false;
                }

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        /// <summary>
        /// Unloads the menu window.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                if ((!m_menuWndFace.Unload())
                        || (!m_selectIcon.Unload()) ||
                        (!m_selectIconFont.Unload()) ||
                        (!m_selectIconFontOutline.Unload()) ||
                        (!m_backIcon.Unload()) ||
                        (!m_backIconFont.Unload()) ||
                        (!m_backIconFontOutline.Unload())
                    )
                    retVal = false;

                if (m_sndButtonAction != null)
                    if (!m_sndButtonAction.Unload())
                        retVal = false;
                if (m_sndMoveSelection != null)
                    if (!m_sndMoveSelection.Unload())
                        retVal = false;
                if (m_sndSetPrevMWindow != null)
                    if (!m_sndSetPrevMWindow.Unload())
                        retVal = false;

                foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
                {
                    if (!mobjKVP.Value.Unload())
                        retVal = false;
                }

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        /// <summary>
        /// Deinitialises the menu window.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Deinit()
        {
            bool retVal = true;
            if (m_isInit)
            {
                if ((!m_menuWndFace.Deinit())
                        || (!m_selectIcon.Deinit()) ||
                        (!m_selectIconFont.Deinit()) ||
                        (!m_selectIconFontOutline.Deinit()) ||
                        (!m_backIcon.Deinit()) ||
                        (!m_backIconFont.Deinit()) ||
                        (!m_backIconFontOutline.Deinit()) 
                    )
                    retVal = false;

                foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
                {
                    if (!mobjKVP.Value.Deinit())
                        retVal = false;
                }

                if (!base.Deinit())
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
        /// Reference to the menu.
        /// </summary>
        public Menu GetMenu
        {
            get { return m_menu; }
        }

        /// <summary>
        /// The assigned width of the menu window.
        /// </summary>
        public float Width
        {
            get { return m_menuWndWidth; }
        }

        /// <summary>
        /// Override of GameObject active flag so that inputs can be configuered.
        /// </summary>
        public override bool Active
        {
            get 
            {
                m_registeredInput = false;
                return base.Active; 
            }
            set
            {
                base.Active = value;

                //make sure there is no input 'jumps'
                m_previousGPState = Microsoft.Xna.Framework.Input.GamePad.GetState(m_menu.ControlingPlayerIndex);
#if !XBOX
                m_previousKBState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
#endif
            }
        }

        /// <summary>
        /// The description of the selected menu windoe object.
        /// </summary>
        public string Description
        {
            get
            {
                return m_menuObjSelected.Description;
            }
        }

        public bool RegisteredInput
        {
            get { return m_registeredInput; }
        }

        #endregion

        #region window_objs

        /// <summary>
        /// Adds a menu object to the menu window.
        /// </summary>
        /// <param name="menuObj">
        /// Reference to the menu object to add to the menu window.
        /// </param>
        public void AddMenuObj(MenuObject menuObj)
        {
            AddMenuObj(menuObj, false);
        }

        /// <summary>
        /// Adds a menu object to the menu window.
        /// </summary>
        /// <param name="menuObj">
        /// Reference to the menu object to add to the menu window.
        /// </param>
        /// <param name="reinitWindow">
        /// If true, the window will be updated to display the menu object.
        /// Only do this if no other menu objects will be added before the
        /// next draw routine, otherwise a significant performance impact
        /// will occur.
        /// </param>
        public void AddMenuObj(MenuObject menuObj, bool reinitWindow)
        {
            m_debugMgr.Assert(menuObj != null,
                "MenuWindow:AddMenuObj - 'menuObj' is null.");
            m_childGameObjs.Add((uint)m_childGameObjs.Count, (GameObject)menuObj);
            menuObj.GetMenuWindow = this;
            if (reinitWindow)
                Reinit();
        }

        public override GameObject GetChildObj(uint gameObjId)
        {
            GameObject gobj = null;
            foreach (KeyValuePair<uint, GameObject> gobjKVP in m_childGameObjs)
            {
                if (gobjKVP.Value.Id == gameObjId)
                {
                    gobj = gobjKVP.Value;
                    break;
                }
            }
            return gobj;
        }

        #endregion

        #region game_loop

        /// <summary>
        /// Handles input for menu window.
        /// </summary>
        /// <param name="newGPState">
        /// Reference to the latest game pade state.
        /// </param>
        /// <param name="newKBState">
        /// Reference to the latest keyboard state.
        /// </param>
        public void Input(  ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
            ,ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
            )
        {
            m_registeredInput = false;

            //Game Pad input
            int nextMenuObj;

            // go back
            if ((newGPState.IsButtonDown(Buttons.B) &&
                    m_previousGPState.IsButtonUp(Buttons.B))
#if !XBOX
 || (newKBState.IsKeyDown(Keys.Escape) &&
                    m_previousKBState.IsKeyUp(Keys.Escape))
#endif
)
            {
                if (m_menu.SetCurrentToPreviousWindow())
                    if (m_sndSetPrevMWindow != null)
                        m_sndSetPrevMWindow.Play();
                m_registeredInput = true;
            }

            //do action
            else if ((newGPState.IsButtonDown(Buttons.A) &&
                        m_previousGPState.IsButtonUp(Buttons.A))
#if !XBOX
 || (newKBState.IsKeyDown(Keys.Enter)) &&
                        (m_previousKBState.IsKeyUp(Keys.Enter))
#endif
)
            {
                m_menuObjSelected.ActionEvent();
                if (m_sndButtonAction != null)
                    m_sndButtonAction.Play();
                m_registeredInput = true;
            }

            //if down pressed - DPAD and left thumb stick
            else if (((newGPState.IsButtonDown(Buttons.DPadDown) &&
                        m_previousGPState.IsButtonUp(Buttons.DPadDown)) ||

                        ((newGPState.ThumbSticks.Left.Y < -0.5f)) &&
                        (m_previousGPState.ThumbSticks.Left.Y >= -0.5f))
#if !XBOX
 || (newKBState.IsKeyDown(Keys.Down) &&
                        m_previousKBState.IsKeyUp(Keys.Down))
#endif
)
            {
                nextMenuObj = m_menuObjSelectedId + 1;
                bool keepGoing = true;
                while ((nextMenuObj < m_childGameObjs.Count) && keepGoing)
                {
                    //check next obj is selectabel
                    if (!(((MenuObject)(m_childGameObjs[(uint)nextMenuObj])).IsSelectable))
                        //keep going
                        nextMenuObj += 1;
                    else
                    {
                        m_menuObjSelected.IsSelected = false;
                        m_menuObjSelected = (MenuObject)m_childGameObjs[(uint)nextMenuObj];
                        m_menuObjSelected.IsSelected = true;
                        m_menuObjSelectedId = nextMenuObj;
                        keepGoing = false;
                        if (m_sndMoveSelection != null)
                            m_sndMoveSelection.Play();
                        m_registeredInput = true;
                    }
                }
            }

            //if up pressed - DPAD and left thumb stick
            else if (((newGPState.IsButtonDown(Buttons.DPadUp) &&
                        m_previousGPState.IsButtonUp(Buttons.DPadUp)) ||

                        ((newGPState.ThumbSticks.Left.Y > 0.5f)) &&
                        (m_previousGPState.ThumbSticks.Left.Y <= 0.5f))
#if !XBOX
 || (newKBState.IsKeyDown(Keys.Up) &&
                        m_previousKBState.IsKeyUp(Keys.Up))
#endif
)
            {
                nextMenuObj = ((m_menuObjSelectedId - 1) > -1) ?
                    (m_menuObjSelectedId - 1) :
                    0;
                bool keepGoing = true;
                while ((nextMenuObj >= 0) && keepGoing)
                {
                    //check next obj is selectable
                    if (!(((MenuObject)(m_childGameObjs[(uint)nextMenuObj])).IsSelectable))
                    {
                        if (nextMenuObj == 0)
                            break;
                        //keep going
                        nextMenuObj = ((nextMenuObj - 1) > -1) ?
                            (nextMenuObj - 1) :
                            0;
                    }
                    else
                    {
                        m_menuObjSelected.IsSelected = false;
                        m_menuObjSelected = (MenuObject)m_childGameObjs[(uint)nextMenuObj];
                        m_menuObjSelected.IsSelected = true;
                        m_menuObjSelectedId = nextMenuObj;
                        keepGoing = false;
                        if (m_sndMoveSelection != null)
                            m_sndMoveSelection.Play();
                        m_registeredInput = true;
                    }
                }
                if (keepGoing)
                {
                    //must be unselectable obj at top, go the other way to find the next obj to select
                    keepGoing = true;
                    while ((nextMenuObj < m_childGameObjs.Count) && keepGoing)
                    {
                        //check next obj is selectable
                        if (!(((MenuObject)(m_childGameObjs[(uint)nextMenuObj])).IsSelectable))
                            //keep going
                            nextMenuObj += 1;
                        else
                        {
                            m_menuObjSelected.IsSelected = false;
                            m_menuObjSelected = (MenuObject)m_childGameObjs[(uint)nextMenuObj];
                            m_menuObjSelected.IsSelected = true;
                            m_menuObjSelectedId = nextMenuObj;
                            keepGoing = false;
                            if (m_sndMoveSelection != null)
                                m_sndMoveSelection.Play();
                            m_registeredInput = true;
                        }
                    }
                }
            }
#if !XBOX
            //remember old states
            m_previousKBState = newKBState;
#endif
            m_previousGPState = newGPState;
        }

        /// <summary>
        /// Updates the menu window.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //call each menu objects' update method
            foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
            {
                if (mobjKVP.Value.Active)
                    mobjKVP.Value.Update(gameTime);
            }
        }

        /// <summary>
        /// Animation type update routine for the menu window.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //call each menu objects' animate method
            foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
            {
                if (mobjKVP.Value.Active)
                    mobjKVP.Value.Animate(gameTime);
            }
        }

        /// <summary>
        /// Draw routine for the menu window.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// Reference to the XNA graphics device.
        /// </param>
        /// <param name="camera">
        /// Reference to the scene camera state.
        /// </param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                                    CameraGObj camera)
        {
            //first draw the window face quad
            m_menuWndFace.DoDraw(gameTime, graphicsDevice, camera);

            ////then draw the menu objects
            foreach (KeyValuePair<uint, GameObject> mobjKVP in m_childGameObjs)
            {
                mobjKVP.Value.DoDraw(gameTime, graphicsDevice, camera);
            }

            //finally the helper icons - only if active though
            if (Active)
            {
                m_selectIcon.DoDraw(gameTime, graphicsDevice, camera);
                m_selectIconFontOutline.DoDraw(gameTime, graphicsDevice, camera);
                m_selectIconFont.DoDraw(gameTime, graphicsDevice, camera);
                m_backIcon.DoDraw(gameTime, graphicsDevice, camera);
                m_backIconFontOutline.DoDraw(gameTime, graphicsDevice, camera);
                m_backIconFont.DoDraw(gameTime, graphicsDevice, camera);
            }
        }

        #endregion

        #endregion
    }
}
