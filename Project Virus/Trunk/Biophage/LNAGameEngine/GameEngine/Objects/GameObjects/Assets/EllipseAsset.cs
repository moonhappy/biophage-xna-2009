/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;

namespace LNA.GameEngine.Objects.GameObjects.Assets
{
    /// <summary>
    /// Represents an ellipse primitive.
    /// </summary>
    public class EllipseAsset : QuadAsset
    {
        #region methods

        #region construction

        /// <summary>
        /// Argument constructor,
        /// </summary>
        /// <param name="id">
        /// Game object Id.
        /// </param>
        /// <param name="initialPosition">
        /// The initial position for the circle.
        /// </param>
        /// <param name="colour">
        /// Colour of the circle pane.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the scene this game object belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics device manager.
        /// </param>
        public EllipseAsset(uint id, Microsoft.Xna.Framework.Vector3 initialPosition,
                            float initialWidth, float initialHeight,
                            Microsoft.Xna.Framework.Graphics.Color colour,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Scene scene, bool addToScene,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr)
            : base(id, initialPosition, initialWidth, initialHeight, colour, debugMgr, resourceMgr, scene, addToScene, graphicsMgr)
        {
            //set other fields
            m_effect = new EffectResHandle(debugMgr, resourceMgr, "Content\\Common\\Shaders\\", "Ellipse");

            //m_debugMgr.WriteLogEntry("EllipseAsset:Constructor - done.");
        }

        #endregion

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
                //load the effect file
                retVal = m_effect.Load();

                Microsoft.Xna.Framework.Graphics.Effect effect = 
                    (Microsoft.Xna.Framework.Graphics.Effect)m_effect.GetResource;
                effect.CurrentTechnique = effect.Techniques["EllipseEffect"];

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        #endregion

        #endregion
    }
}
