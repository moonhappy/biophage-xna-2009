/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Drawable;

namespace LNA.GameEngine.Objects.GameObjects.Assets
{
    public class ModelAsset : Asset
    {
        #region fields

        protected ModelResHandle m_modelHResource;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="objId">
        /// Model object Id.
        /// </param>
        /// <param name="initialPosition">
        /// The initial position for the model.
        /// </param>
        /// <param name="resFDir">
        /// The file directory path of the resource.
        /// </param>
        /// <param name="resFName">
        /// The file name of the resource.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the scene object that this object belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        public ModelAsset(  uint objId, Microsoft.Xna.Framework.Vector3 initialPosition,
                            string resFDir, string resFName,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Scene scene, bool addToScene)
            : base(objId, initialPosition, debugMgr, resourceMgr, scene, addToScene)
        {
            m_modelHResource = new ModelResHandle(  m_debugMgr, resourceMgr, 
                                                    resFDir, resFName);

            //m_debugMgr.WriteLogEntry("ModelGObj:Constructor - done.");
        }

        #endregion

        #region loading

        /// <summary>
        /// Loads the game object (and model resource).
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Load()
        {
            //m_debugMgr.WriteLogEntry("ModelGObj:Load - doing.");

            bool retVal = true;
            if (!m_isLoaded)
            {
                retVal = m_modelHResource.Load();
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
            //m_debugMgr.WriteLogEntry("ModelGObj:Unload - doing.");

            bool retVal = true;
            if (m_isLoaded)
            {
                retVal = m_modelHResource.Unload();
                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        #region game_loop

        /// <summary>
        /// Override this method to give the model object update logic.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        /// <summary>
        /// Override this method to give the model object animation behaviour.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        /// <summary>
        /// Draws the model resource.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// The XNA graphics device.
        /// </param>
        /// <param name="camera">
        /// The camera view to help with rendering.
        /// </param>
        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                                    CameraGObj camera)
        {
            //draw model
            m_modelHResource.Draw(gameTime, m_worldTransform, camera.ProjectionMatrix, camera.ViewMatrix);
        }

        #endregion

        #endregion
    }
}
