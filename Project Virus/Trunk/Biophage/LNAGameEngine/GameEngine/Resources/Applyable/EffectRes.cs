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
    /// Represents an XNA Material resource. This class is implicitly
    /// thread safe providing a serialised access policy.
    /// </summary>
    public sealed class EffectRes : Resource
    {
        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EffectRes()
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
            //m_dbgMgr.WriteLogEntry("MaterialRes:Load - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "EffectRes:Load - resource has not been created/initialised.");

            //make method as a single atomic action
            //load via ResourceLoader
            if (!m_isLoaded)
            {
                lock (m_resLoader)
                {
                    m_resource = (object)m_resLoader.Load<Microsoft.Xna.Framework.Graphics.Effect>(m_filePath);
                }
                if (m_resource == null)
                {
                    m_dbgMgr.WriteLogEntry("MaterialRes:Load - 'm_resource' is null.");
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
