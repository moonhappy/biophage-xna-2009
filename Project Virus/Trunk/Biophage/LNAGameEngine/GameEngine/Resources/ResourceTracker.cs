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
    /// Used internally for the ResourceLoader class. Contains the
    /// actual XNA resource object and a list of disposibles of the
    /// resource object. This class is NOT implicitly thread safe.
    /// </summary>
    public class ResourceTracker
    {
        #region fields

        /// <summary>
        /// The actual XNA resource object.
        /// </summary>
        public object m_resource;

        /// <summary>
        /// List of disposible objects of the resource.
        /// </summary>
        public List<IDisposable> m_disposables;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ResourceTracker()
        {
            //set fields
            m_disposables = new List<IDisposable>();
            m_resource = null;
        }

        #endregion

        /// <summary>
        /// To keep track of resources that must have their 'Dispose' method
        /// invoked when they are unloaded.
        /// </summary>
        /// <param name="disposable">
        /// Reference of the object to dispose.
        /// </param>
        public void TrackDisposables(IDisposable disposable)
        {
            m_disposables.Add(disposable);
        }

        #endregion
    }
}
