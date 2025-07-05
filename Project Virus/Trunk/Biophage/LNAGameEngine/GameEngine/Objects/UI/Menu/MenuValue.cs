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
    /// Represents a menu value integer.
    /// </summary>
    public class MenuValue : MenuObject
    {
        #region fields

        protected MenuButton m_incButton;
        protected MenuButton m_decButton;
        protected MenuButton m_buttonSelected;

        protected int m_currentValue = 0;
        protected int m_maxValue;
        protected int m_minValue;

        protected FontTextureAsset m_valueLabel;
        protected SpriteFontResHandle m_valueFont;

        protected GamePadState m_prevGPState;
        protected KeyboardState m_prevKBState;

        protected SoundResHandle m_sndMoveSelection;

        protected string m_valueDescription;

        #endregion

        #region methods

        #region construction

        public MenuValue(   uint id,
                            int minValue,int maxValue, int defValue, string valueDescription,
                            SpriteFontResHandle valueFont, SoundResHandle sndMoveSelection,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                            MenuWindow menuWnd, bool addToMenuWindow)
            : base(id, true, debugMgr, resourceMgr, menuWnd, addToMenuWindow)
        {
            //set fields
            m_isSelected = false;
            m_maxValue = maxValue;
            m_minValue = minValue;
            m_currentValue = defValue;
            m_valueFont = valueFont;
            m_sndMoveSelection = sndMoveSelection;
            m_valueDescription = valueDescription;

            //asserts
            m_debugMgr.Assert(m_minValue <= m_maxValue,
                "MenuValue:Constructor - minimal value must be less than, or equal to, max value.");
            m_debugMgr.Assert((m_currentValue >= m_minValue) && (m_currentValue <= m_maxValue),
                "MenuValue:Constructor - default value is out of range.");
            m_debugMgr.Assert(m_valueDescription != null,
                "MenuValue:Constructor - description is null.");

            m_valueLabel = new FontTextureAsset(
                uint.MaxValue - id - (uint)1, Microsoft.Xna.Framework.Vector3.Zero,
                1f, 1f,
                Microsoft.Xna.Framework.Graphics.Color.White,
                m_currentValue.ToString(), valueFont, true,
                debugMgr, resourceMgr, null, false, 
                graphicsMgr, spriteBatch);

            float buttonSideLength = (HeightDelta - ConstCornerRadius) * 0.75f;
            float xAxisDelta = (menuWnd.Width * 0.5f) - 0.05f;

            m_incButton = new MenuButton(
                uint.MaxValue - id - (uint)2, "+", "", m_valueFont, 
                buttonSideLength, buttonSideLength, ConstCornerRadius,
                m_debugMgr, resourceMgr, 
                graphicsMgr, spriteBatch, menuWnd, false);
            m_incButton.UIAction += new UIEventAction(m_nextButton_UIAction);

            m_decButton = new MenuButton(
                uint.MaxValue - id - (uint)3, "-", "", m_valueFont, 
                buttonSideLength, buttonSideLength, ConstCornerRadius,
                m_debugMgr, resourceMgr, 
                graphicsMgr, spriteBatch, menuWnd, false);
            m_decButton.UIAction += new UIEventAction(m_prevButton_UIAction);

            m_buttonSelected = m_incButton;

            //log
            //m_debugMgr.WriteLogEntry("MenuButton:Constructor - done.");
        }

        void m_prevButton_UIAction(object sender, EventArgs e)
        {
            DecrementValue();
        }

        void m_nextButton_UIAction(object sender, EventArgs e)
        {
            IncrementValue();
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

        public int MinValue
        {
            get { return m_minValue; }
            set
            {
                if (value <= m_maxValue)
                    m_minValue = value;
                else
                    m_debugMgr.WriteLogEntry(
                        "MenuValue:MinValue - can't set min value that is greater than max value.");
            }
        }

        public int MaxValue
        {
            get { return m_maxValue; }
            set
            {
                if (value >= m_minValue)
                    m_maxValue = value;
                else
                    m_debugMgr.WriteLogEntry(
                        "MenuValue:MaxValue - can't set max value that is less than min value.");
            }
        }

        public int CurrentValue
        {
            get { return m_currentValue; }
            set
            {
                if ((value < m_minValue) && (value > m_maxValue))
                    m_debugMgr.Assert(false,
                        "MenuValue:CurrentValue - current value cannot be greater than or less then ranges.");

                m_currentValue = value;

                m_valueLabel.GetString = m_currentValue.ToString();
            }
        }

        public MenuButton IncrementButton
        {
            get { return m_incButton; }
        }

        public MenuButton DecrementButton
        {
            get { return m_decButton; }
        }

        public override string Description
        {
            get { return m_valueDescription; }
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
                if (    (!m_incButton.Init())  ||
                        (!m_decButton.Init())  ||
                        (!m_valueLabel.Init()) ||
                        (!base.Init()))
                    retVal = false;

                m_buttonSelected.IsSelected = IsSelected;

                //make local transforms
                float xAxisDelta = (m_menuWnd.Width * 0.5f) - 0.05f;

                m_incButton.Position = new Microsoft.Xna.Framework.Vector3(xAxisDelta, 0f, 0f);
                m_decButton.Position = new Microsoft.Xna.Framework.Vector3(-xAxisDelta, 0f, 0f);

                m_incButton.Position += m_position;
                m_decButton.Position += m_position;
                m_valueLabel.Position = m_position;

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

            if (    (!m_incButton.Reinit())    ||
                    (!m_decButton.Reinit())    ||
                    (!m_valueLabel.Reinit())   ||
                    (!base.Reinit()))
                retVal = false;

            m_buttonSelected.IsSelected = IsSelected;

            //make local transforms
            float xAxisDelta = (m_menuWnd.Width * 0.5f) - 0.05f;

            m_incButton.Position = new Microsoft.Xna.Framework.Vector3(xAxisDelta, 0f, 0f);
            m_decButton.Position = new Microsoft.Xna.Framework.Vector3(-xAxisDelta, 0f, 0f);

            m_incButton.Position += m_position;
            m_decButton.Position += m_position;
            m_valueLabel.Position = m_position;

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
                if (    (!m_incButton.Load())  ||
                        (!m_decButton.Load())  ||
                        (!m_valueLabel.Load()) )
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
                if (    (!m_incButton.Unload())    ||
                        (!m_decButton.Unload())    ||
                        (!m_valueLabel.Unload())   )
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
                if (    (!m_incButton.Deinit())    ||
                        (!m_decButton.Deinit())    ||
                        (!m_valueLabel.Deinit())   ||
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

        public void IncrementValue()
        {
            m_currentValue++;
            if (m_currentValue > m_maxValue)
                m_currentValue = m_maxValue;

            //set new label
            m_valueLabel.GetString = m_currentValue.ToString();
        }

        public void DecrementValue()
        {
            m_currentValue--;
            if (m_currentValue < m_minValue)
                m_currentValue = m_minValue;

            //set new label
            m_valueLabel.GetString = m_currentValue.ToString();
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
            if (m_incButton.Active)
                m_incButton.Update(gameTime);
            if (m_decButton.Active)
                m_decButton.Update(gameTime);

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
                    m_buttonSelected = m_incButton;
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
                    m_buttonSelected = m_decButton;
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
                    m_buttonSelected = m_incButton;
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
                    m_buttonSelected = m_decButton;
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
            if (m_incButton.Active)
                m_incButton.Animate(gameTime);
            if (m_decButton.Active)
                m_decButton.Animate(gameTime);
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            //draw the buttons
            m_incButton.DoDraw(gameTime, graphicsDevice, camera);
            m_decButton.DoDraw(gameTime, graphicsDevice, camera);

            //draw the button label
            m_valueLabel.DoDraw(gameTime, graphicsDevice, camera);
        }

        #endregion

        #endregion
    }
}
