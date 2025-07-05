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
    /// Represents a menu label.
    /// </summary>
    public class MenuLabel : MenuObject
    {
        #region fields

        protected string m_labelString;
        protected FontTextureAsset m_labelStringTexture;
        protected SpriteFontResHandle m_labelFont;
        protected QuadAsset m_labelFace;

        protected string m_labelDescription = "";

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Id of the menu label boject.
        /// </param>
        /// <param name="labelString">
        /// String to apply to the label.
        /// </param>
        /// <param name="labelFont">
        /// String font for rendering the label.
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
        /// <param name="menuWnd">
        /// Reference to the menu window this label belongs to.
        /// </param>
        /// <param name="addToMenuWindow">
        /// If true, this object will be automatically assigned to its menu
        /// window. Otherwise it must be done manually.
        /// </param>
        public MenuLabel(   uint id,
                            string labelString, SpriteFontResHandle labelFont,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                            MenuWindow menuWnd, bool addToMenuWindow)
            : base(id, false, debugMgr, resourceMgr, menuWnd, addToMenuWindow)
        {
            //set fields
            m_isSelected = false;
            m_labelString = labelString;
            m_labelFont = labelFont;

            m_labelStringTexture = new FontTextureAsset(
                uint.MaxValue - id, Microsoft.Xna.Framework.Vector3.Zero,
                1f, 1f,
                Microsoft.Xna.Framework.Graphics.Color.Black,
                m_labelString, labelFont, true,
                debugMgr, resourceMgr, null, false, graphicsMgr, spriteBatch);

            m_labelFace = new QuadAsset(
                uint.MaxValue - id - (uint)1, Microsoft.Xna.Framework.Vector3.Zero,
                menuWnd.Width + 0.05f, HeightDelta,
                Microsoft.Xna.Framework.Graphics.Color.Silver,
                debugMgr, resourceMgr, null, false, graphicsMgr);

            //log
            //m_debugMgr.WriteLogEntry("MenuLabel:Constructor - done.");
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// If the button is selected. Should always be false.
        /// </summary>
        public override bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                //spit the dummy
                m_debugMgr.Assert(!value,
                    "MenuLabel:IsSelected - being assigned as selected, which is not allowed for labels.");
            }
        }

        public override bool IsSelectable
        {
            get { return base.IsSelectable; }
        }

        public string LabelString
        {
            get { return m_labelString; }
            set
            {
                m_labelString = value;
                m_labelStringTexture.GetString = m_labelString;
            }
        }

        public override string Description
        {
            get { return m_labelDescription; }
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
                if (    (!m_labelStringTexture.Init())  ||
                        (!m_labelFace.Init())           ||
                        (!base.Init())                  )
                    retVal = false;
                
                //m_labelStringTexture.Colour = Microsoft.Xna.Framework.Graphics.Color.Black;

                //make local transforms
                m_labelFace.Position = m_position;
                m_labelStringTexture.Position = m_position;

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

            if (    (!m_labelStringTexture.Reinit())    ||
                    (!m_labelFace.Reinit())             ||
                    (!base.Reinit())                    )
                retVal = false;

            //m_labelStringTexture.Colour = Microsoft.Xna.Framework.Graphics.Color.White;

            //make local transforms
            m_labelFace.Position = m_position;
            m_labelStringTexture.Position = m_position;

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
                if (    (!m_labelStringTexture.Load())  || 
                        (!m_labelFace.Load())           )
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
                if (    (!m_labelStringTexture.Unload())    ||
                        (!m_labelFace.Unload())             )
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
                if (    (!m_labelStringTexture.Deinit())    ||
                        (!m_labelFace.Deinit())             ||
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
            //draw the face
            m_labelFace.DoDraw(gameTime, graphicsDevice, camera);

            //draw the label
            m_labelStringTexture.DoDraw(gameTime, graphicsDevice, camera);
        }

        #endregion

        public override void ActionEvent()
        {
            //chuck a hissy
            m_debugMgr.Assert(false,
                "MenuLabel:ActionEvent - labels should not have actions.");
        }

        #endregion
    }
}
