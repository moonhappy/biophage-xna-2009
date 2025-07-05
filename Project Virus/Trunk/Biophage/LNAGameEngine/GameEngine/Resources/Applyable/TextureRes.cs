/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNA.GameEngine.Resources.Applyable
{
    /// <summary>
    /// Represents an XNA texture resource. This class is implicitly
    /// thread safe providing a serialised access policy.
    /// </summary>
    public sealed class TextureRes : Resource
    {
        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TextureRes()
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
            //m_dbgMgr.WriteLogEntry("TextureRes:Load - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "TextureRes:Load - resource has not been created/initialised.");

            //load via ResourceLoader
            if (!m_isLoaded)
            {
                lock (m_resLoader)
                {
                    m_resource = (object)m_resLoader.Load<Microsoft.Xna.Framework.Graphics.Texture2D>(m_filePath);
                }
                if (m_resource == null)
                {
                    m_dbgMgr.WriteLogEntry("TextureRes:Load - 'm_resource' is null.");
                    return false;
                }
                m_isLoaded = true;
            }

            m_numLoadHandles++;
            return true;
        }

        #endregion

        #endregion
    }
}
