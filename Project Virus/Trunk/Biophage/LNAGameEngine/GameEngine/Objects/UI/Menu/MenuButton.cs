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

namespace LNA.GameEngine.Objects.UI.Menu
{
    /// <summary>
    /// Represents a menu button.
    /// </summary>
    public class MenuButton : MenuObject
    {
        #region fields

        protected RoundCornerQuadAsset m_buttonFace;

        protected string m_buttonLabelString;
        protected FontTextureAsset m_buttonLabel;
        protected SpriteFontResHandle m_buttonFont;

        protected string m_buttonDescription;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Menu object Id.
        /// </param>
        /// <param name="buttonLabel">
        /// Button label string.
        /// </param>
        /// <param name="buttonDescription">
        /// Button operation description.
        /// </param>
        /// <param name="buttonFont">
        /// The font of the menu.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics manager.
        /// </param>
        /// <param name="spriteBatch">
        /// Reference to the XNA sprite batch system.
        /// </param>
        /// <param name="menuWnd">
        /// Reference to the menu window.
        /// </param>
        /// <param name="addToMenuWindow">
        /// If true, this object will be automatically added to
        /// the menu window.
        /// </param>
        public MenuButton(  uint id,
                            string buttonLabel, string buttonDescription,
                            SpriteFontResHandle buttonFont,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                            MenuWindow menuWnd, bool addToMenuWindow)
            : base(id, true, debugMgr, resourceMgr, menuWnd, addToMenuWindow)
        {
            //set fields
            m_isSelected = false;
            m_buttonLabelString = buttonLabel;
            m_buttonFont = buttonFont;
            m_buttonDescription = buttonDescription;

            float cornerDiameter = 2f * ConstCornerRadius;
            m_buttonFace = new RoundCornerQuadAsset(
                uint.MaxValue - id, Microsoft.Xna.Framework.Vector3.Zero,
                menuWnd.Width - 0.05f, HeightDelta - cornerDiameter, ConstCornerRadius,
                Microsoft.Xna.Framework.Graphics.Color.DarkSlateGray,
                debugMgr, resourceMgr, null, false, graphicsMgr);

            m_buttonFace.Visible = false;

            m_buttonLabel = new FontTextureAsset(
                uint.MaxValue - id - (uint)1, Microsoft.Xna.Framework.Vector3.Zero,
                1f, 1f,
                Microsoft.Xna.Framework.Graphics.Color.White,
                m_buttonLabelString, buttonFont, true,
                debugMgr, resourceMgr, null, false,
                graphicsMgr, spriteBatch);

            //log
            //m_debugMgr.WriteLogEntry("MenuButton:Constructor - done.");
        }

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Menu object Id.
        /// </param>
        /// <param name="buttonLabel">
        /// Button label string.
        /// </param>
        /// <param name="buttonDescription">
        /// Button description.
        /// </param>
        /// <param name="buttonFont">
        /// The font of the menu.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics manager.
        /// </param>
        /// <param name="spriteBatch">
        /// Reference to the XNA sprite batch system.
        /// </param>
        /// <param name="menuWnd">
        /// Reference to the menu window.
        /// </param>
        /// <param name="addToMenuWindow">
        /// If true, this object will be automatically added to
        /// the menu window.
        /// </param>
        public MenuButton(uint id,
                            string buttonLabel, string buttonDescription, 
                            SpriteFontResHandle buttonFont,
                            float buttonWidth, float buttonHeight, float cornerRadius,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                            MenuWindow menuWnd, bool addToMenuWindow)
            : base(id, true, debugMgr, resourceMgr, menuWnd, addToMenuWindow)
        {
            //set fields
            m_isSelected = false;
            m_buttonLabelString = buttonLabel;
            m_buttonFont = buttonFont;
            m_buttonDescription = buttonDescription;

            float cornerDiameter = 2f * cornerRadius;
            m_buttonFace = new RoundCornerQuadAsset(
                uint.MaxValue - id, Microsoft.Xna.Framework.Vector3.Zero,
                buttonWidth, buttonHeight, cornerRadius,
                Microsoft.Xna.Framework.Graphics.Color.DarkSlateGray,
                debugMgr, resourceMgr, null, false, graphicsMgr);

            m_buttonFace.Visible = false;

            m_buttonLabel = new FontTextureAsset(
                uint.MaxValue - id - (uint)1, Microsoft.Xna.Framework.Vector3.Zero,
                1f, 1f,
                Microsoft.Xna.Framework.Graphics.Color.White,
                m_buttonLabelString, buttonFont, true,
                debugMgr, resourceMgr, null, false, 
                graphicsMgr, spriteBatch);

            //log
            //m_debugMgr.WriteLogEntry("MenuButton:Constructor - done.");
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// If the button is selected.
        /// </summary>
        public override bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                m_isSelected = value;

                if (m_isSelected)
                {
                    //button selected
                    m_buttonFace.Visible = true;
                    m_buttonLabel.Colour = Microsoft.Xna.Framework.Graphics.Color.Black;
                }
                else
                {
                    //button unselecteed
                    m_buttonFace.Visible = false;
                    m_buttonLabel.Colour = Microsoft.Xna.Framework.Graphics.Color.White;
                }
            }
        }

        /// <summary>
        /// Description of the button.
        /// </summary>
        public override string Description
        {
            get { return m_buttonDescription; }
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
                if (    (!m_buttonFace.Init()) ||
                        (!m_buttonLabel.Init()) ||
                        (!base.Init()))
                    retVal = false;

                m_buttonFace.Colour = Microsoft.Xna.Framework.Graphics.Color.SlateGray;
                if (m_isSelected)
                    m_buttonLabel.Colour = Microsoft.Xna.Framework.Graphics.Color.Black;
                else
                    m_buttonLabel.Colour = Microsoft.Xna.Framework.Graphics.Color.White;

                //make local transforms
                m_buttonFace.Position = m_position;
                m_buttonLabel.Position = m_position;

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

                if (    (!m_buttonFace.Reinit())    ||
                        (!m_buttonLabel.Reinit())   ||
                        (!base.Reinit())            )
                    retVal = false;

                m_buttonFace.Colour = Microsoft.Xna.Framework.Graphics.Color.SlateGray;
                if (m_isSelected)
                    m_buttonLabel.Colour = Microsoft.Xna.Framework.Graphics.Color.Black;
                else
                    m_buttonLabel.Colour = Microsoft.Xna.Framework.Graphics.Color.White;

                //make local transforms
                m_buttonFace.Position = m_position;
                m_buttonLabel.Position = m_position;

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
                if (    (!m_buttonFace.Load())  ||
                        (!m_buttonLabel.Load()) )
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
                if (    (!m_buttonFace.Unload()) ||
                        (!m_buttonLabel.Unload()))
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
                if (    (!m_buttonFace.Deinit())    ||
                        (!m_buttonLabel.Deinit())   ||
                        (!base.Deinit())            )
                    retVal = false;

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        #endregion

        #region game_loop

        /// <summary>
        /// Update logic for the menu object.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing.
        }

        /// <summary>
        /// Animation type update logic for the menu object.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing.
        }

        /// <summary>
        /// Draw routine for the menu object.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// Reference to the XNA graphics device.
        /// </param>
        /// <param name="camera">
        /// Reference to the camera.
        /// </param>
        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                                    CameraGObj camera)
        {
            //draw the button face
            m_buttonFace.DoDraw(gameTime, graphicsDevice, camera);

            //draw the button label
            m_buttonLabel.DoDraw(gameTime, graphicsDevice, camera);
        }

        #endregion
        
        #endregion
    }
}
