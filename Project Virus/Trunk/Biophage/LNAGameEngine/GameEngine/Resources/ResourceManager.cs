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
    /// The ResourceManager class is used to manage the game resources.
    /// This class is implicitly thread safe providing a serialised
    /// access policy.
    /// </summary>
    public class ResourceManager
    {
        #region fields

        protected DebugManager m_dbgMgr;
        protected ResourceLoader m_resLoader;
        protected Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;

        protected Dictionary<string, Resource> m_resCollection;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument contructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics manager.
        /// </param>
        /// <param name="serviceProvider">
        /// Service provider that the ResourceLoader/XNA:ContentManager
        /// class requires.
        /// </param>
        public ResourceManager( DebugManager debugMgr, 
                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr, 
                                IServiceProvider serviceProvider)
        {
            //set fields
            m_dbgMgr = debugMgr;
            m_resCollection = new Dictionary<string, Resource>();
            m_graphicsMgr = graphicsMgr;

            //create the resource loader
            m_resLoader = new ResourceLoader(debugMgr, serviceProvider);

            //log
            m_dbgMgr.WriteLogEntry("ResourceManager:Constructor - done.");
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Returns a reference to the ResourceLoader class.
        /// </summary>
        /// <remarks>
        /// This access is intended only for the Resource objects to
        /// physically load or unload the file resource into memory. DO NOT
        /// access the ResourceLoader directly unless you know what you are
        /// doing!
        /// </remarks>
        public ResourceLoader ResourceLoader
        {
            get { return m_resLoader; }
        }

        #endregion

        #region res_handler

        /// <summary>
        /// Returns a reference to the Resource object. If an instance of
        /// the same Resource exists (if found in one of the collections),
        /// a reference to that Resource object will be returned. Otherwise
        /// a new Resource object will be contructed and a reference to it
        /// will be returned.
        /// </summary>
        /// <remarks>
        /// This method is intended to only be called in a ResourceHandle's
        /// constructor. This garuntees that all ResourceHandles have a
        /// local reference pointing to the same Resource object. This
        /// method will attempt to find a match to the Id param, if no match
        /// is found the file path string will be used as the criteria (if
        /// this occurs, a logic error must exist and will be logged).
        /// </remarks>
        /// <typeparam name="T">
        /// The type of Resource object to get; eg: TextureRes, SoundRes,
        /// etc...
        /// </typeparam>
        /// <param name="resFileDirPath">
        /// The resource file's directory path.
        /// </param>
        /// <param name="resFileName">
        /// The file name of the resource.
        /// </param>
        /// <returns>
        /// Reference to the resource object.
        /// </returns>
        public T GetResource<T>(string resFileDirPath, string resFileName)
            where T : Resource, new()
        {
            //m_dbgMgr.WriteLogEntry("ResourceManager:GetResource - doing.");

            //check if already exists

            //set file path
            //  check that there is an end directive
            string sFilePath = resFileDirPath;
            if (sFilePath[sFilePath.Length - 1] != '\\')
                sFilePath += "\\";
            sFilePath += resFileName;
            if (m_resCollection.ContainsKey(sFilePath))
            {
                return (T)m_resCollection[sFilePath];
            }

            //No instance found, create new Resource
            T res = new T();
            res.Init(m_dbgMgr, this, m_graphicsMgr,resFileDirPath, resFileName);

            //add new resource to collection
            m_resCollection.Add(sFilePath, (Resource)res);

            //return the reference
            return res;
        }

        #endregion

        #endregion
    }
}
