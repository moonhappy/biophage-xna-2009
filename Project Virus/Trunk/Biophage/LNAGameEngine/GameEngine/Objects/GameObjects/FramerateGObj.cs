/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;

namespace LNA.GameEngine.Objects.GameObjects
{
    /// <summary>
    /// Displays the framerate to the screen.
    /// </summary>
    public class FramerateGObj : GameObject
    {
        #region fields

        protected Microsoft.Xna.Framework.Graphics.SpriteBatch m_spriteBatch;
        protected SpriteFontResHandle m_spriteFontRes;

        protected int frameRate = 0;
        protected int frameCounter = 0;
        protected TimeSpan elapsedTime = TimeSpan.Zero;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Game object Id.
        /// </param>
        /// <param name="fontFileDirectoryPath">
        /// SpriteFont resource directory path.
        /// </param>
        /// <param name="fontFileName">
        /// SpriteFont resource file name.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the scene that this game object belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics manager.
        /// </param>
        public FramerateGObj(   uint id, string fontFileDirectoryPath, string fontFileName,
                                DebugManager debugMgr, ResourceManager resourceMgr,
                                Scene scene, bool addToScene,
                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr)
            : base(id, debugMgr, resourceMgr, scene, addToScene)
        {
            m_spriteFontRes = new SpriteFontResHandle(
                debugMgr, resourceMgr, 
                fontFileDirectoryPath, fontFileName);

            m_spriteBatch = scene.Stage.SceneMgr.Game.spriteBatch;
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Initialises the game object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Init()
        {
            if (!m_isInit)
                m_isInit = true;

            return true;
        }

        /// <summary>
        /// Reinitialises the game object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Reinit()
        {
            m_isInit = true;
            return true;
        }

        #region loading

        /// <summary>
        /// Loads the game object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                retVal = m_spriteFontRes.Load();
                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        /// <summary>
        /// Unloads the game object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                retVal = m_spriteFontRes.Load();
                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        public override bool Deinit()
        {
            m_isInit = false;
            return true;
        }

        #endregion

        #region framerate

        public void IncrementFrameCounter()
        {
            frameCounter++;
        }

        public string FramesPerSecond
        {
            get { return frameRate.ToString(); }
        }

        #endregion

        #region game_loop

        /// <summary>
        /// Updates the game object.
        /// </summary>
        /// <remarks>
        /// Based on Shawn Hargreaves framerate component
        /// </remarks>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="gameTime">
        /// Not implemented
        /// </param>
        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            return;
        }

        /// <summary>
        /// Renders the sprite to the screen.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// Reference to XNA graphics device manager.
        /// </param>
        /// <param name="camera">
        /// Reference to the current camera.
        /// </param>
        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            IncrementFrameCounter();

            string fps = string.Format("fps: {0}", frameRate);

            m_spriteBatch.Begin();

            m_spriteBatch.DrawString(
                (Microsoft.Xna.Framework.Graphics.SpriteFont)m_spriteFontRes.GetResource,
                fps, new Microsoft.Xna.Framework.Vector2(33, 33), Microsoft.Xna.Framework.Graphics.Color.Black);
            m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_spriteFontRes.GetResource,
                fps, new Microsoft.Xna.Framework.Vector2(32, 32), Microsoft.Xna.Framework.Graphics.Color.White);

            m_spriteBatch.End();

        }

        #endregion

        #endregion
    }
}
