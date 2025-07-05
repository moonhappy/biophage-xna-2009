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
    /// Represents a handle to a material resource. This class is
    /// implicitly thread safe providing a serialised access policy.
    /// </summary>
    public class ModelResHandle : DrawableResHandle<ModelRes>
    {
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
        public ModelResHandle(  DebugManager debugMgr, ResourceManager resourceMgr,
                                string resFileDirectoryPath, string resFileName)
            : base( debugMgr, resourceMgr, resFileDirectoryPath, resFileName)
        {
            //m_dbgMgr.WriteLogEntry("ModelResHandle:Constructor - done.");
        }

        #endregion
    }
}
