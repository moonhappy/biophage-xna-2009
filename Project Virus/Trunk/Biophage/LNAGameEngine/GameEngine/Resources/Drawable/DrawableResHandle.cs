/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNA.GameEngine.Resources.Drawable
{
    /// <summary>
    /// Represents a handle to a "drawable" resource object. This class
    /// is implicitly thread safe providing a serialised access policy.
    /// </summary>
    /// <typeparam name="T">
    /// DrawableRes object type.
    /// </typeparam>
    public class DrawableResHandle<T> : ResourceHandle<T>
        where T : DrawableRes, new()
    {
        #region methods

        #region construction

        /// <summary>
        /// Invokes the 'GetResource' method in ResourceManager to retrieve
        /// a reference to the Resource object.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="resFileDirectoryPath">
        /// File directory path to the resource file.
        /// </param>
        /// <param name="resFileName">
        /// File name of the resource file.
        /// </param>
        public DrawableResHandle(   DebugManager debugMgr, ResourceManager resourceMgr,
                                    string resFileDirectoryPath, string resFileName)
            : base(debugMgr, resourceMgr, resFileDirectoryPath, resFileName)
        {
            //m_dbgMgr.WriteLogEntry("DrawableResHandle:Constructor - done.");
        }

        #endregion

        #region drawable

        /// <summary>
        /// Draws the resource.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="worldMat">
        /// The world matrix transformation.
        /// This is 'Model space' -> 'World space'.
        /// </param>
        /// <param name="projectionMat">
        /// The projection matrix transformation.
        /// This is 'World space' -> 'Camera space'.
        /// </param>
        /// <param name="viewMat">
        /// The view matrix transformation.
        /// This is 'Camera space' -> 'View space'. The GPU will be able
        /// to cull in view space as it is normalised.
        /// </param>
        public void Draw(   Microsoft.Xna.Framework.GameTime gameTime,
                            Microsoft.Xna.Framework.Matrix worldMat,
                            Microsoft.Xna.Framework.Matrix projectionMat,
                            Microsoft.Xna.Framework.Matrix viewMat)
        {
            //m_dbgMgr.WriteLogEntry("DrawableResHandle:Draw - doing.");

            if (m_isActive)
                m_resource.Draw(gameTime, worldMat, projectionMat, viewMat);
        }

        #endregion

        #endregion
    }
}
