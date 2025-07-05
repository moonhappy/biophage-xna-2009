/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.GameObjects.Assets;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;
using LNA.GameEngine.Resources.Playable;

using Microsoft.Xna.Framework.Input;

namespace LNA.GameEngine.Objects.UI.Menu
{
    /// <summary>
    /// Represents
    /// </summary>
    public class MenuToggle : MenuObject
    {
        #region fields

        protected MenuButton m_nextButton;
        protected MenuButton m_prevButton;
        protected MenuButton m_buttonSelected;

        protected string[] m_valuesArray;
        protected int m_currentValueIndex = 0;

        protected FontTextureAsset m_toggleLabel;
        protected SpriteFontResHandle m_toggleFont;

        protected GamePadState m_prevGPState;
        protected KeyboardState m_prevKBState;

        protected SoundResHandle m_sndMoveSelection;

        protected Dictionary<string, string> m_toggleDescriptions;
        protected Dictionary<int, string> m_toggleDescrFast;

        #endregion

        #region methods

        #region construction

        public MenuToggle(  uint id,
                            string[] toggleValues, int defValIndex, 
                            Dictionary<string, string> toggleDescriptions,
                            SpriteFontResHandle toggleFont,
                            SoundResHandle sndMoveSelection,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                            MenuWindow menuWnd, bool addToMenuWindow)
            : base(id, true, debugMgr, resourceMgr, menuWnd, addToMenuWindow)
        {
            //set fields
            m_isSelected = false;
            m_valuesArray = toggleValues;
            m_currentValueIndex = defValIndex;
            m_toggleFont = toggleFont;
            m_sndMoveSelection = sndMoveSelection;
            m_toggleDescriptions = toggleDescriptions;

            //asserts
            m_debugMgr.Assert(m_valuesArray != null,
                "MenuToggle:Constructor - array of values is null.");
            m_debugMgr.Assert((m_currentValueIndex >= 0) && (m_currentValueIndex < m_valuesArray.Length),
                "MenuToggle:Constructor - default value index is out of range.");
            m_debugMgr.Assert(m_toggleDescriptions != null,
                "MenuToggle:Constructor - 'toggleDescriptions' is null.");

            //consolidate to fast descriptions
            m_toggleDescrFast = new Dictionary<int, string>(m_toggleDescriptions.Count);
            ReadjustDescriptions();

            m_toggleLabel = new FontTextureAsset(
                uint.MaxValue - id - (uint)1, Microsoft.Xna.Framework.Vector3.Zero,
                1f, 1f,
                Microsoft.Xna.Framework.Graphics.Color.White,
                m_valuesArray[m_currentValueIndex], toggleFont, true,
                debugMgr, resourceMgr, null, false, graphicsMgr, spriteBatch);

            float buttonSideLength = (HeightDelta - ConstCornerRadius) * 0.75f;
            float xAxisDelta = (menuWnd.Width * 0.5f) - 0.05f;

            m_nextButton = new MenuButton(
                uint.MaxValue - id - (uint)2, ">", "", m_toggleFont, 
                buttonSideLength, buttonSideLength, ConstCornerRadius,
                m_debugMgr, resourceMgr, 
                graphicsMgr, spriteBatch, menuWnd, false);
            m_nextButton.UIAction += new UIEventAction(m_nextButton_UIAction);

            m_prevButton = new MenuButton(
                uint.MaxValue - id - (uint)3, "<", "", m_toggleFont, 
                buttonSideLength, buttonSideLength, ConstCornerRadius,
                m_debugMgr, resourceMgr, 
                graphicsMgr, spriteBatch, menuWnd, false);
            m_prevButton.UIAction += new UIEventAction(m_prevButton_UIAction);

            m_buttonSelected = m_nextButton;

            //log
            //m_debugMgr.WriteLogEntry("MenuButton:Constructor - done.");
        }

        void ReadjustDescriptions()
        {
            m_toggleDescrFast.Clear();

            for (int i = 0; i < m_valuesArray.Length; ++i)
            {
                //find description of string
                if (m_toggleDescriptions.ContainsKey(m_valuesArray[i]))
                    m_toggleDescrFast.Add(i, m_toggleDescriptions[m_valuesArray[i]]);
                else
                    m_toggleDescrFast.Add(i, ""); //fill with empty description
            }
        }

        void m_prevButton_UIAction(object sender, EventArgs e)
        {
            TogglePrevValue();
        }

        void m_nextButton_UIAction(object sender, EventArgs e)
        {
            ToggleNextValue();
        }

        #endregion

        #region field_accessors

        public override bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                m_isSelected = value;

                m_buttonSelected.IsSelected = IsSelected;
            }
        }

        public string[] ValuesArray
        {
            get { return m_valuesArray; }
            set
            {
                m_valuesArray = value;
                m_currentValueIndex = 0;
                //set new label
                m_toggleLabel.GetString = m_valuesArray[m_currentValueIndex];

                //readjust descriptions
                ReadjustDescriptions();
            }
        }

        public string CurrentValue
        {
            get { return m_valuesArray[m_currentValueIndex]; }
        }

        public MenuButton NextButton
        {
            get { return m_nextButton; }
        }

        public MenuButton PrevButton
        {
            get { return m_prevButton; }
        }

        public override string Description
        {
            get { return m_toggleDescrFast[m_currentValueIndex]; }
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Initialises the menu object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                if (    (!m_nextButton.Init())  ||
                        (!m_prevButton.Init())  ||
                        (!m_toggleLabel.Init()) ||
                        (!base.Init()))
                    retVal = false;

                m_buttonSelected.IsSelected = IsSelected;

                //make local transforms
                float xAxisDelta = (m_menuWnd.Width * 0.5f) - 0.05f;

                m_nextButton.Position = new Microsoft.Xna.Framework.Vector3(xAxisDelta, 0f, 0f);
                m_prevButton.Position = new Microsoft.Xna.Framework.Vector3(-xAxisDelta, 0f, 0f);

                m_nextButton.Position += m_position;
                m_prevButton.Position += m_position;
                m_toggleLabel.Position = m_position;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        /// <summary>
        /// Reinitialises the menu object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Reinit()
        {
            bool retVal = true;

            if (    (!m_nextButton.Reinit())    ||
                    (!m_prevButton.Reinit())    ||
                    (!m_toggleLabel.Reinit())   ||
                    (!base.Reinit()))
                retVal = false;

            m_buttonSelected.IsSelected = IsSelected;

            //make local transforms
            float xAxisDelta = (m_menuWnd.Width * 0.5f) - 0.05f;

            m_nextButton.Position = new Microsoft.Xna.Framework.Vector3(xAxisDelta, 0f, 0f);
            m_prevButton.Position = new Microsoft.Xna.Framework.Vector3(-xAxisDelta, 0f, 0f);

            m_nextButton.Position += m_position;
            m_prevButton.Position += m_position;
            m_toggleLabel.Position = m_position;

            if (retVal)
                m_isInit = true;
            else
                m_isInit = false;

            return retVal;
        }

        #region loading

        /// <summary>
        /// Loads the menu object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if (    (!m_nextButton.Load())  ||
                        (!m_prevButton.Load())  ||
                        (!m_toggleLabel.Load()) )
                    retVal = false;

                if (m_sndMoveSelection != null)
                    if (!m_sndMoveSelection.Load())
                        retVal = false;

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        /// <summary>
        /// Unloads the menu object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                if (    (!m_nextButton.Unload())    ||
                        (!m_prevButton.Unload())    ||
                        (!m_toggleLabel.Unload())   )
                    retVal = false;

                if (m_sndMoveSelection != null)
                    if (!m_sndMoveSelection.Unload())
                        retVal = false;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        /// <summary>
        /// Deinitialises the menu object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Deinit()
        {
            bool retVal = true;
            if (m_isInit)
            {
                if (    (!m_nextButton.Deinit())    ||
                        (!m_prevButton.Deinit())    ||
                        (!m_toggleLabel.Deinit())   ||
                        (!base.Deinit()))
                    retVal = false;

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        #endregion

        #region toggle

        public void ToggleNextValue()
        {
            m_currentValueIndex++;
            if (m_currentValueIndex >= m_valuesArray.Length)
                m_currentValueIndex = m_valuesArray.Length - 1;

            //set new label
            m_toggleLabel.GetString = m_valuesArray[m_currentValueIndex];
        }

        public void TogglePrevValue()
        {
            m_currentValueIndex--;
            if (m_currentValueIndex < 0)
                m_currentValueIndex = 0;

            //set new label
            m_toggleLabel.GetString = m_valuesArray[m_currentValueIndex];
        }

        public void ToggleStart()
        {
            m_currentValueIndex = 0;

            //set new label
            m_toggleLabel.GetString = m_valuesArray[m_currentValueIndex];
        }

        public void ToggleEnd()
        {
            m_currentValueIndex = m_valuesArray.Length - 1;

            //set new label
            m_toggleLabel.GetString = m_valuesArray[m_currentValueIndex];
        }

        #endregion

        #region custom_action

        /// <summary>
        /// Calling this will do nothing, it is replaced by a defered call so that
        /// events will be raised after increment or decrement.
        /// </summary>
        public override void ActionEvent()
        {
            //do nothing
        }

        void ActualActionEvent()
        {
            //check active
            base.ActionEvent();
        }

        #endregion

        #region game_loop

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //call updates
            if (m_nextButton.Active)
                m_nextButton.Update(gameTime);
            if (m_prevButton.Active)
                m_prevButton.Update(gameTime);

            #region input

            if (IsSelected)
            {
                // Handle input for XBOX360 controller
                GamePadState gPadState = GamePad.GetState(m_menuWnd.GetMenu.ControlingPlayerIndex);

                //do action
                if (    (gPadState.Buttons.A == ButtonState.Pressed) &&
                        (m_prevGPState.Buttons.A == ButtonState.Released))
                {
                    m_buttonSelected.ActionEvent();
                    //raise own action event
                    ActualActionEvent();
                }

            //if right pressed - DPAD and left thumb stick
                else if ((  (gPadState.DPad.Right == ButtonState.Pressed) &&
                            (m_prevGPState.DPad.Right == ButtonState.Released)) ||

                        (   (gPadState.ThumbSticks.Left.X > 0.5f)) &&
                            (m_prevGPState.ThumbSticks.Left.X <= 0.5f))
                {
                    //select the 'next' toggle button
                    m_buttonSelected.IsSelected = false;
                    m_buttonSelected = m_nextButton;
                    m_buttonSelected.IsSelected = true;
                    if (m_sndMoveSelection != null)
                        m_sndMoveSelection.Play();
                    m_menuWnd.GetMenu.m_renderContextInvalid = true;
                }

                //if left pressed - DPAD and left thumb stick
                else if ((  (gPadState.DPad.Left == ButtonState.Pressed) &&
                            (m_prevGPState.DPad.Left == ButtonState.Released)) ||

                        (   (gPadState.ThumbSticks.Left.X < -0.5f)) &&
                            (m_prevGPState.ThumbSticks.Left.X >= -0.5f))
                {
                    //select the 'next' toggle button
                    m_buttonSelected.IsSelected = false;
                    m_buttonSelected = m_prevButton;
                    m_buttonSelected.IsSelected = true;
                    if (m_sndMoveSelection != null)
                        m_sndMoveSelection.Play();
                    m_menuWnd.GetMenu.m_renderContextInvalid = true;
                }

                //3.2 Handle input for keyboard
#if !XBOX
                //KEYBOARD

                KeyboardState keyboardState = Keyboard.GetState();

                //do action
                if (    (keyboardState.IsKeyDown(Keys.Enter)) &&
                        (m_prevKBState.IsKeyUp(Keys.Enter)))
                {
                    m_buttonSelected.ActionEvent();
                    //raise own action event
                    ActualActionEvent();
                }

                //if right pressed
                else if (   keyboardState.IsKeyDown(Keys.Right) &&
                            m_prevKBState.IsKeyUp(Keys.Right))
                {
                    //select the 'next' toggle button
                    m_buttonSelected.IsSelected = false;
                    m_buttonSelected = m_nextButton;
                    m_buttonSelected.IsSelected = true;
                    if (m_sndMoveSelection != null)
                        m_sndMoveSelection.Play();
                    m_menuWnd.GetMenu.m_renderContextInvalid = true;
                }

                //if left pressed - DPAD and left thumb stick
                else if (   keyboardState.IsKeyDown(Keys.Left) &&
                            m_prevKBState.IsKeyUp(Keys.Left))
                {
                    //select the 'next' toggle button
                    m_buttonSelected.IsSelected = false;
                    m_buttonSelected = m_prevButton;
                    m_buttonSelected.IsSelected = true;
                    if (m_sndMoveSelection != null)
                        m_sndMoveSelection.Play();
                    m_menuWnd.GetMenu.m_renderContextInvalid = true;
                }

                //remember old states
                m_prevKBState = keyboardState;
#endif
                m_prevGPState = gPadState;
            }
            #endregion
        }

        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //animate buttons
            if (m_nextButton.Active)
                m_nextButton.Animate(gameTime);
            if (m_prevButton.Active)
                m_prevButton.Animate(gameTime);
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            //draw the buttons
            m_nextButton.DoDraw(gameTime, graphicsDevice, camera);
            m_prevButton.DoDraw(gameTime, graphicsDevice, camera);

            //draw the button label
            m_toggleLabel.DoDraw(gameTime, graphicsDevice, camera);
        }

        #endregion

        #endregion
    }
}
