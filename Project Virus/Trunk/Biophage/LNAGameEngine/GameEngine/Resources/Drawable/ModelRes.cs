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
    /// Represents an XNA model resource. This class is implicitly
    /// thread safe providing a serialised access policy.
    /// </summary>
    public sealed class ModelRes : DrawableRes
    {
        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ModelRes()
            : base()
        { }

        #endregion

        #region loading

        /// <summary>
        /// Loads the resource into memory.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Load()
        {
            //m_dbgMgr.WriteLogEntry("ModelRes:Load - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "ModelRes:Load - resource has not been created/initialised.");

            //load via ResourceLoader
            if (!m_isLoaded)
            {
                lock (m_resLoader)
                {
                    m_resource = (object)m_resLoader.Load<Microsoft.Xna.Framework.Graphics.Model>(m_filePath);
                }
                if (m_resource == null)
                {
                    m_dbgMgr.WriteLogEntry("ModelRes:Load - 'm_resource' is null.");
                    return false;
                }
                m_isLoaded = true;
            }

            m_numLoadHandles++;
            return true;
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
        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Matrix worldMat, 
                                    Microsoft.Xna.Framework.Matrix projectionMat, 
                                    Microsoft.Xna.Framework.Matrix viewMat)
        {
            //m_dbgMgr.WriteLogEntry("ModelRes:Draw - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "ModelRes:Draw - resource has not been initialised/created.");

            Microsoft.Xna.Framework.Graphics.Model model = (Microsoft.Xna.Framework.Graphics.Model)m_resource;
            foreach (Microsoft.Xna.Framework.Graphics.ModelMesh mesh in model.Meshes)
            {
                foreach (Microsoft.Xna.Framework.Graphics.BasicEffect effect in mesh.Effects)
                {
                    //Lighting
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    //load drawspace translation maticies
                    effect.World = worldMat;
                    effect.Projection = projectionMat;
                    effect.View = viewMat;
                }

                mesh.Draw();
            }
        }

        #endregion

        #endregion
    }
}
