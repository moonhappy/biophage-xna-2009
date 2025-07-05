/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//TODO: extend sound functionality

namespace LNA.GameEngine.Resources.Playable
{
    /// <summary>
    /// Represents an XNA sound resource. This class is implicilty
    /// thread safe providing a serilaised access policy.
    /// </summary>
    public class SoundRes : PlayableRes
    {
        #region fields

        protected Microsoft.Xna.Framework.Audio.SoundEffectInstance m_soundInst;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SoundRes()
            : base()
        {
            m_soundInst = null;
        }

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
            //m_dbgMgr.WriteLogEntry("SoundRes:Load - doing.");
            m_dbgMgr.Assert(m_isInit,
                "SoundRes:Load - resource has not been initialised/created.");

            //load via ResourceLoader
            if (!m_isLoaded)
            {
                lock (m_resLoader)
                {
                    m_resource = (object)m_resLoader.Load<Microsoft.Xna.Framework.Audio.SoundEffect>(m_filePath);
                }
                if (m_resource == null)
                {
                    m_dbgMgr.WriteLogEntry("SoundRes:Load - 'm_resource' is null.");
                    return false;
                }
                m_isLoaded = true;
            }

            m_numLoadHandles++;
            return true;
        }

        /// <summary>
        /// Unloads the resource from memory.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Unload()
        {
            m_dbgMgr.Assert(m_isInit,
                "SoundRes:Unload - resource has not been initialised/created.");

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
                    m_soundInst = null;
                }
            }

            return true;
        }

        public override bool ForceUnload()
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
                m_soundInst = null;
            }

            return true;
        }

        #endregion

        #region playable

        /// <summary>
        /// Plays the sound once.
        /// </summary>
        public override void Play()
        {
            //m_dbgMgr.WriteLogEntry("SoundRes:Play - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "SoundRes:Play - resource has not been initialised/created.");

            //unbox as sound and play
            Microsoft.Xna.Framework.Audio.SoundEffect sound = (Microsoft.Xna.Framework.Audio.SoundEffect)m_resource;
            if (m_soundInst != null)
            {
                m_soundInst.Stop();
                m_soundInst.Play();
            }
            else
            {
                m_soundInst = sound.CreateInstance();
                m_soundInst.Play();
            }
        }

        /// <summary>
        /// Plays the sound indefinitly, or until 'Stop' is called.
        /// </summary>
        public override void PlayLoop()
        {
            //m_dbgMgr.WriteLogEntry("SoundRes:PlayLoop - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "SoundRes:PlayLoop - resource has not been initialised/created.");

            //unbox as sound and play
            Microsoft.Xna.Framework.Audio.SoundEffect sound = (Microsoft.Xna.Framework.Audio.SoundEffect)m_resource;
            if (m_soundInst != null)
            {
                m_soundInst.Stop();
                m_soundInst.Play();
            }
            else
            {
                m_soundInst = sound.CreateInstance();
                m_soundInst.IsLooped = true;
                m_soundInst.Play();
            }
        }

        /// <summary>
        /// Pauses the sound if it is playing.
        /// </summary>
        public override void Pause()
        {
            //m_dbgMgr.WriteLogEntry("SoundRes:Pause - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "SoundRes:Pause - resource has not been initialised/created.");

            //only pause if instance exists
            if (m_soundInst != null)
                m_soundInst.Pause();
        }

        /// <summary>
        /// Stops the sound, whether or not the sound is currently playing.
        /// </summary>
        public override void Stop()
        {
            //m_dbgMgr.WriteLogEntry("SoundRes:Stop - doing.");
            m_dbgMgr.Assert(m_isInit, 
                "SoundRes:Stop - resource has not been initialised/created.");

            //only pause if instance exists
            if (m_soundInst != null)
                m_soundInst.Stop();
        }

        #endregion

        #endregion
    }
}
