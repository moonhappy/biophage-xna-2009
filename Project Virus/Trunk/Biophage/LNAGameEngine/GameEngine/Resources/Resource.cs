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
    /// Represents the base definition of a Resource class. This class
    /// is implicitly thread safe providing a serialised access policy.
    /// </summary>
    public abstract class Resource : IGameResource
    {
        #region fields

        protected bool m_isInit;
        protected DebugManager m_dbgMgr;
        protected ResourceLoader m_resLoader;

        //boxed XNA resource
        protected object m_resource;

        protected string m_fileName;
        protected string m_fileDirPath;
        //path is a combination of the directory and file name
        protected string m_filePath;

        protected bool m_isLoaded;
        protected uint m_numLoadHandles;

        protected Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Default constructor. 'Init' method must be called directly
        /// afterwards.
        /// </summary>
        public Resource()
        {
            //set fields
            m_isInit = false;
            m_resource = null;
            m_isLoaded = false;
            m_numLoadHandles = 0;
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Initialises the resource.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="fileDirectoryPath">
        /// The file directory path of the reource.
        /// </param>
        /// <param name="fileName">
        /// The file name of the resource.
        /// </param>
        public virtual void Init(   DebugManager debugMgr, ResourceManager resourceMgr,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                    string fileDirectoryPath, string fileName)
        {
            //set fields
            m_dbgMgr = debugMgr;
            m_fileName = fileName;
            m_fileDirPath = fileDirectoryPath;
            m_resLoader = resourceMgr.ResourceLoader;

            //set file path
            //  check that there is an end directive
            if (m_fileDirPath[m_fileDirPath.Length - 1] != '\\')
                m_fileDirPath += "\\";
            //  set to file path
            m_filePath = m_fileDirPath;
            m_filePath += m_fileName;

            m_isInit = true;

            m_graphicsMgr = graphicsMgr;

            //log
            //m_dbgMgr.WriteLogEntry("Resource:Create - done.");
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// True if the resource has been initialised.
        /// </summary>
        public bool IsInit
        {
            get { return m_isInit; }
        }

        /// <summary>
        /// Resource file name.
        /// </summary>
        public string FileName
        {
            get { return m_fileName; }
        }

        /// <summary>
        /// Resource file directory path.
        /// </summary>
        public string FileDirectoryPath
        {
            get { return m_fileDirPath; }
        }

        /// <summary>
        /// Reference to the XNA resource object.
        /// </summary>
        /// <remarks>
        /// Try to not use this method in your game.
        /// </remarks>
        public object GetResource
        {
            get { return m_resource; }
        }

        #endregion

        #region loading

        public abstract bool Load();

        /// <summary>
        /// Queries whether the resource has been loaded to memory.
        /// </summary>
        public bool IsLoaded
        {
            get { return m_isLoaded; }
        }

        /// <summary>
        /// Safely unloads the resource from memory.
        /// </summary>
        /// <remarks>
        /// The resource wont be released from memory if there are still
        /// "active" handles to the resource. By "active" I am refering to
        /// handles that have invoked a load call of this resource and may
        /// still be using the resource.
        /// </remarks>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public virtual bool Unload()
        {
            //m_dbgMgr.WriteLogEntry("Resource:Unload - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "Resource:Unload - resource has not been created/initialised.");

            if (m_isLoaded)
            {
                //do some checking
                if (m_numLoadHandles == 0)
                    m_dbgMgr.WriteLogEntry("Resource:Unload - logic error, no active handles but unload called.");
                else
                    m_numLoadHandles--;

                //unload if no more handles
                if (m_numLoadHandles == 0)
                {
                    m_resource = null;
                    lock (m_resLoader)
                    {
                        m_resLoader.Unload(m_filePath);
                    }
                    m_isLoaded = false;
                }
            }

            return true;
        }

        /// <summary>
        /// Forces the resource to unload from memory.
        /// </summary>
        /// <remarks>
        /// Will forcefully release the resource from memory, even if there
        /// are "active" handles to this resource. By "active" I am
        /// refering to handles that have invoked a load call of this
        /// resource and may still be using the resource.
        /// </remarks>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public virtual bool ForceUnload()
        {
            //m_dbgMgr.WriteLogEntry("Resource:ForceUnload - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "Resource:ForceUnload - resource has not been created/initialised.");

            if (m_isLoaded)
            {
                m_resource = null;
                lock (m_resLoader)
                {
                    m_resLoader.Unload(m_filePath);
                }
                m_isLoaded = false;
            }

            return true;
        }

        #endregion

        #endregion
    }
}
