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
    /// Represents a handle to a RenderTarget2D resource. This class is
    /// implicitly thread safe providing a serialised access policy.
    /// </summary>
    public class RenderTargetResHandle : ResourceHandle<RenderTargetRes>
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
        /// <param name="renderTargetIdentifier">
        /// Unique identifier for the render target object instance.
        /// </param>
        public RenderTargetResHandle(DebugManager debugMgr, ResourceManager resourceMgr,
                                    string renderTargetIdentifier)
            : base(debugMgr, resourceMgr, "RenderTargets", renderTargetIdentifier)
        { }

        #endregion

        #region accessors

        public int Width
        {
            get { return ((RenderTargetRes)m_resource).Width; }
            set { ((RenderTargetRes)m_resource).Width = value; }
        }

        public int Height
        {
            get { return ((RenderTargetRes)m_resource).Height; }
            set { ((RenderTargetRes)m_resource).Height = value; }
        }

        public bool DiscardContents
        {
            get { return ((RenderTargetRes)m_resource).DiscardContents; }
            set { ((RenderTargetRes)m_resource).DiscardContents = value; }
        }

        #endregion
    }
}
