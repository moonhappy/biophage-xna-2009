/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNA.GameEngine.Resources
{
    /// <summary>
    /// Represents a handle to a Resource object. This class is
    /// impliciltly thread safe providing a serialised access policy.
    /// </summary>
    /// <typeparam name="T">
    /// Resource object type.
    /// </typeparam>
    public class ResourceHandle<T> : IGameResourceHandle
        where T : Resource, new()
    {
        #region fields

        protected DebugManager m_dbgMgr;
        protected ResourceManager m_resMgr;
        protected T m_resource;

        //A resource handle is active if it's load method is called
        //  otherwise the resource handle won't interact with the
        //  resource.
        protected bool m_isActive;

        #endregion

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
        public ResourceHandle(  DebugManager debugMgr, ResourceManager resourceMgr,
                                string resFileDirectoryPath, string resFileName)
        {
            //set fields
            m_dbgMgr = debugMgr;
            m_dbgMgr.Assert(resourceMgr != null, 
                "ResourceHandle:Constructor - parameter 'resourceMgr' is null.");
            m_resMgr = resourceMgr;
            m_isActive = false;

            //get the resource
            m_resource = m_resMgr.GetResource<T>(resFileDirectoryPath, resFileName);

            //log
            //m_dbgMgr.WriteLogEntry("ResourceHandle:Constructor - done.");
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Resource file name.
        /// </summary>
        public string FileName
        {
            get { return ((IGameResource)(m_resource)).FileName; }
        }

        /// <summary>
        /// Resource file direcory path.
        /// </summary>
        public string FileDirectoryPath
        {
            get { return ((IGameResource)(m_resource)).FileDirectoryPath; }
        }

        /// <summary>
        /// Reference to the XNA resource object.
        /// </summary>
        /// <remarks>
        /// Try to not use this method in your game.
        /// </remarks>
        public object GetResource
        {
            get { return m_resource.GetResource; }
        }

        #endregion

        #region loading

        /// <summary>
        /// Loads the resource into memory (if not already done).
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool Load()
        {
            //m_dbgMgr.WriteLogEntry("ResourceHandle:Load - doing.");

            //Only set handle as active if no error occured
            //  during resource loading.
            if (m_resource.Load())
            {
                m_isActive = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Queries whether the resource is already loaded.
        /// </summary>
        public bool IsLoaded
        {
            //Handle only needs to be concerned if it has
            //  the resource loaded. This stops the game
            //  programmer from recursively calling unload
            //  until the resource mistakenly unloads whilest
            //  other handles are using it.
            get { return m_isActive; }
        }

        /// <summary>
        /// Releases this "active" handle from the resource. If the resource
        /// has no more active handles, it will be released from memory.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool Unload()
        {
            //m_dbgMgr.WriteLogEntry("ResourceHandle:Unload - doing.");

            m_isActive = false;
            return m_resource.Unload();
        }

        #endregion

        #endregion
    }
}
