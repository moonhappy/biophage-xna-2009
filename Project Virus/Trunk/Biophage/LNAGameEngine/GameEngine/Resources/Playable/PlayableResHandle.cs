/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNA.GameEngine.Resources.Playable
{
    /// <summary>
    /// Represents a handle to a "playable" resource object. This class
    /// is implicitly thread safe providing a  serialised access policy.
    /// </summary>
    /// <typeparam name="T">
    /// PlayableRes object type.
    /// </typeparam>
    public class PlayableResHandle<T> : ResourceHandle<T>
        where T : PlayableRes, new()
    {
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
        public PlayableResHandle(   DebugManager debugMgr, ResourceManager resourceMgr,
                                    string resFileDirectoryPath, string resFileName)
            : base(debugMgr, resourceMgr, resFileDirectoryPath, resFileName)
        {
            //m_dbgMgr.WriteLogEntry("PlayableResHandle:Constructor - done.");
        }

        #endregion

        #region playable

        /// <summary>
        /// Plays the resource once.
        /// </summary>
        public void Play()
        {
            //m_dbgMgr.WriteLogEntry("PlayableResHandle:Play - doing.");

            if (m_isActive)
                m_resource.Play();
        }

        /// <summary>
        /// Plays the resource indefinitly, or until 'Stop' is called.
        /// </summary>
        public void PlayLoop()
        {
            //m_dbgMgr.WriteLogEntry("PlayableResHandle:PlayLoop - doing.");

            if (m_isActive)
                m_resource.PlayLoop();
        }

        /// <summary>
        /// Pauses the resource. If 'Play' or 'PlayLoop' methods are called
        /// the resource will resume.
        /// </summary>
        public void Pause()
        {
            //m_dbgMgr.WriteLogEntry("PlayableResHandle:Plause - doing.");

            if (m_isActive)
                m_resource.Pause();
        }

        /// <summary>
        /// Stops the resource, whether or not the sound is currently
        /// playing.
        /// </summary>
        public void Stop()
        {
            //m_dbgMgr.WriteLogEntry("PlayableResHandle:Stop - doing.");

            if (m_isActive)
                m_resource.Stop();
        }

        #endregion

        #endregion
    }
}
