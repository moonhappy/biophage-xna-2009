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
    /// Repesents an XNA SpriteFont resource for rasterising font.
    /// </summary>
    public class SpriteFontRes : Resource
    {
        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SpriteFontRes()
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
            //m_dbgMgr.WriteLogEntry("SpriteFontRes:Load - doing.");
            m_dbgMgr.Assert(m_isInit,
                "SpriteFontRes:Load - resource has not been initialised.");

            //make method as a single atomic action
            //load via ResourceLoader
            if (!m_isLoaded)
            {
                lock (m_resLoader)
                {
                    m_resource = (object)m_resLoader.Load<Microsoft.Xna.Framework.Graphics.SpriteFont>(m_filePath);
                }
                if (m_resource == null)
                {
                    m_dbgMgr.WriteLogEntry("SpriteFontRes:Load - 'm_resource' is null.");
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
