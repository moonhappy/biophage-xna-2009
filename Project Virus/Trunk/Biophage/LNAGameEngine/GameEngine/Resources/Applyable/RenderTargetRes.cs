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
    /// This resource provides access to a RenderTarget2D. A unique render
    /// target will be allocated for each unique string identity.
    /// </summary>
    public class RenderTargetRes : Resource
    {
        #region fields

        protected int m_width = 600;
        protected int m_height = 600;
        protected bool m_discardContents = true;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RenderTargetRes()
            : base()
        { }

        #endregion

        #region field_accessors

        /// <summary>
        /// Width of the RenderTarget2D. The resource must be reloaded
        /// for this parameter to take effect.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set 
            { 
                m_width = value;
                if ((m_width <= 0) || (m_width > m_graphicsMgr.PreferredBackBufferWidth))
                {
                    m_dbgMgr.WriteLogEntry("RenderTargetTextureRes:Width - invalid width value.");
                    m_width = 600;
                }
            }
        }

        /// <summary>
        /// Height of the RenderTarget2D. The resource must be reloaded
        /// for this parameter to take effect.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set
            {
                m_height = value;
                if ((m_height <= 0) || (m_height > m_graphicsMgr.PreferredBackBufferHeight))
                {
                    m_dbgMgr.WriteLogEntry("RenderTargetTextureRes:Height - invalid height value.");
                    m_height = 600;
                }
            }
        }

        public bool DiscardContents
        {
            get { return m_discardContents; }
            set { m_discardContents = value; }
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
            //m_dbgMgr.WriteLogEntry("MaterialRes:Load - doing.");
            m_dbgMgr.Assert(m_isInit,
                "RenderTargetTextureRes:Load - resource has not been created/initialised.");

            //make method as a single atomic action
            //load via ResourceLoader
            if (!m_isLoaded)
            {
                lock (m_resLoader)
                {
                    if (m_discardContents)
                        m_resource = (object)(new Microsoft.Xna.Framework.Graphics.RenderTarget2D(
                            m_graphicsMgr.GraphicsDevice, m_width, m_height, 1,
                            m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferFormat,
                            Microsoft.Xna.Framework.Graphics.RenderTargetUsage.DiscardContents));
                    else
                        m_resource = (object)(new Microsoft.Xna.Framework.Graphics.RenderTarget2D(
                            m_graphicsMgr.GraphicsDevice, m_width, m_height, 1,
                            m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferFormat,
                            Microsoft.Xna.Framework.Graphics.RenderTargetUsage.PreserveContents));
                }
                if (m_resource == null)
                {
                    m_dbgMgr.WriteLogEntry("RenderTargetTextureRes:Load - 'm_resource' is null.");
                    return false;
                }
                m_isLoaded = true;
            }

            m_numLoadHandles++;
            return true;
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
        public override bool Unload()
        {
            //m_dbgMgr.WriteLogEntry("Resource:Unload - doing.");
            m_dbgMgr.Assert(m_isInit,
                "RenderTargetTextureRes:Unload - resource has not been created/initialised.");

            if (m_isLoaded)
            {
                //do some checking
                if (m_numLoadHandles == 0)
                    m_dbgMgr.WriteLogEntry("RenderTargetTextureRes:Unload - logic error, no active handles but unload called.");
                else
                    m_numLoadHandles--;

                //unload if no more handles
                if (m_numLoadHandles == 0)
                {
                    m_resource = null;
                    lock (m_resLoader)
                    {
                        ((Microsoft.Xna.Framework.Graphics.RenderTarget2D)m_resource).Dispose();
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
        public override bool ForceUnload()
        {
            //m_dbgMgr.WriteLogEntry("Resource:ForceUnload - doing.");
            m_dbgMgr.Assert(m_isInit,
                "RenderTargetTextureRes:ForceUnload - resource has not been created/initialised.");

            if (m_isLoaded)
            {
                m_resource = null;
                lock (m_resLoader)
                {
                    ((Microsoft.Xna.Framework.Graphics.RenderTarget2D)m_resource).Dispose();
                }
                m_isLoaded = false;
            }

            return true;
        }

        #endregion

        #endregion
    }
}
