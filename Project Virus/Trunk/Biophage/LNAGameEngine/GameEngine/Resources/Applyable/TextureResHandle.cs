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
    /// Represents a handle to a texture resource. This class is
    /// implicitly thread safe providing a serialised access policy.
    /// </summary>
    public class TextureResHandle : ResourceHandle<TextureRes>
    {
        #region construction

        /// <summary>
        /// Invokes the 'GetResource' method in ResourceManager to retrieve
        /// a reference to the Resource object.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="resFileDirectoryPath">
        /// File directory path to the resource file.
        /// </param>
        /// <param name="resFileName">
        /// File name of the resource file.
        /// </param>
        public TextureResHandle(    DebugManager debugMgr, ResourceManager resMgr,
                                    string resFileDirectoryPath, string resFileName)
            : base( debugMgr, resMgr, resFileDirectoryPath, resFileName)
        {
            //m_dbgMgr.WriteLogEntry("TextureResHandle:Constructor - done.");
        }

        #endregion
    }
}
