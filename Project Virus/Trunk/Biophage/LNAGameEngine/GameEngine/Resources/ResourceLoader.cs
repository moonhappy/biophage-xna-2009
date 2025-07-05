/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

/*
    NOTE: The 'ResourceLoader' class makes use of some of the
    concepts explained in Aranda Morrison's article:
        "Advanced Content Management Systems in XNA"
    (Morrison, 2008, http://www.ziggyware.com/readarticle.php?article_id=231).
    Though this class is quite different to Aranda's content 
    manager, I owe much gratitude to Aranda's article that helped
    with the development of this class.
    Aranda's expressed rights/license of his work are as follows:
    
    "You are free to use the ContentTracker software in any way
    you like, free or commercial. The author is not responsible
    for any problems caused by the ContentTracker. If you use the
    ContentTracker in a compiled assembly, some credit would be
    nice but it's not required. If you release the ContentTracker
    along with other source code, credit to original author is
    required." (quoted 09/04/2009 @ 18.57EAST,
    from http://www.ziggyware.com/readarticle.php?article_id=231).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

namespace LNA.GameEngine.Resources
{
    /// <summary>
    /// Used to load or unload resources to/from memory. This class does
    /// not provide any thread safety garuntees, rather this class should
    /// be kept thread safe via the resource access itself.
    /// </summary>
    /// <remarks>
    /// This only provides basic management of loaded resources and is
    /// not intended to be used directly in the game.
    /// </remarks>
    public class ResourceLoader : Microsoft.Xna.Framework.Content.ContentManager
    {
        #region fields

        protected DebugManager m_dbgMgr;
        protected Dictionary<string, ResourceTracker> m_loadedResources;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="serviceProvider">
        /// Object that provides services to the Resource Loader.
        /// </param>
        public ResourceLoader(DebugManager debugMgr, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            m_dbgMgr = debugMgr;
            m_loadedResources = new Dictionary<string, ResourceTracker>();

            //log
            //m_dbgMgr.WriteLogEntry("ResourceLoader:Constructor - done.");
        }

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="serviceProvider">
        /// Object that provides services to the Resource Loader.
        /// </param>
        /// <param name="rootDirectory">
        /// The root file directory path of the project's resources.
        /// </param>
        public ResourceLoader(  DebugManager debugMgr, IServiceProvider serviceProvider, 
                                string rootDirectory)
            : base(serviceProvider, rootDirectory)
        {
            m_dbgMgr = debugMgr;
            m_loadedResources = new Dictionary<string, ResourceTracker>();

            //log
            //m_dbgMgr.WriteLogEntry("ResourceLoader:Constructor - done.");
        }

        #endregion

        #region content_managing

        /// <summary>
        /// Loads a resource into memory. A reference to the resource will
        /// be returned once it has been loaded.
        /// </summary>
        /// <remarks>
        /// Even if the resource has been loaded into memory by a previous
        /// call, this method will load another copy into memory and return
        /// a reference to another instance of the resource. That is why you
        /// should only use resource handles in your game so that the 
        /// resources are properly handled.
        /// </remarks>
        /// <typeparam name="T">
        /// The resource type to load.
        /// </typeparam>
        /// <param name="resFilePath">
        /// Specifies the file path of the resource to load.
        /// </param>
        /// <returns>
        /// Reference to the loaded resource.
        /// </returns>
        public override T Load<T>(string resFilePath)
        {
            //m_dbgMgr.WriteLogEntry("ResourceLoader:Load - doing.");

            //check if resource object is already loaded
            if (m_loadedResources.ContainsKey(resFilePath))
            {
                return (T)m_loadedResources[resFilePath].m_resource;
            }

            //resource must not have been loaded, so load it now
            //  Any failures/exception with loading should be treated as very serious
            //  so cause an Assert error.
            ResourceTracker res = new ResourceTracker();
            try
            {
                // Read the resource from disk.
                res.m_resource = ReadAsset<T>(resFilePath, res.TrackDisposables);
            }
            catch (ObjectDisposedException)
            {
                m_dbgMgr.Assert(false,
                    "ResourceLoader:Load - resource trying to load has been disposed.");
            }
            catch (ArgumentNullException)
            {
                m_dbgMgr.Assert(false,
                    "ResourceLoader:Load - parameter 'resFilePath' is invalid.");
            }
            catch (Microsoft.Xna.Framework.Content.ContentLoadException)
            {
                m_dbgMgr.Assert(false,
                    "ResourceLoader:Load - content type mismatch, check file name or resource type.");
            }

            //add to collection
            m_loadedResources.Add(resFilePath, res);

            // Return loaded asset
            return (T)res.m_resource;
        }

        /// <summary>
        /// Clean up all resources.
        /// </summary>
        public override void Unload()
        {
            //m_dbgMgr.WriteLogEntry("ResourceLoader:Unload - doing.");

            // Dispose all IDisposables now
            Dictionary<string, ResourceTracker>.Enumerator enumer = m_loadedResources.GetEnumerator();
            while (enumer.MoveNext())
            {
                // Destroy tracked disposables
                foreach (IDisposable disposable in enumer.Current.Value.m_disposables)
                {
                    disposable.Dispose();
                }

                //Dispose the actual asset, if possible
                if (enumer.Current.Value.m_resource is IDisposable)
                    ((IDisposable)enumer.Current.Value.m_resource).Dispose();

                //clear the disposables list
                enumer.Current.Value.m_disposables.Clear();
            }

            //clear the list
            m_loadedResources.Clear();
        }

        /// <summary>
        /// Release a resource from system to free up memory.
        /// </summary>
        /// <param name="resFilePath">
        /// Specifies the file path of the resource to unload.
        /// </param>
        public void Unload(string resFilePath)
        {
            //string sLog = "ResourceLoader:Unload - doing for file=";
            //sLog += resFilePath;
            //m_dbgMgr.WriteLogEntry(sLog);

            if (m_loadedResources.ContainsKey(resFilePath))
            {
                ResourceTracker tracker = m_loadedResources[resFilePath];

                // Destroy tracked disposables
                foreach (IDisposable disposable in tracker.m_disposables)
                {
                    disposable.Dispose();
                }

                // Dispose the actual asset, if possible
                if (tracker.m_resource is IDisposable)
                    ((IDisposable)tracker.m_resource).Dispose();

                // Remove from collection
                m_loadedResources.Remove(resFilePath);
            }
        }

        #endregion

        #endregion
    }
}
