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
    /// Represents the base definition for a "playable" resource type.
    /// Extends the abstract Resource class with 'Play', 'PlayLoop', and
    /// 'Stop' methods.
    /// </summary>
    /// <remarks>
    /// An example "playable" resource would be sound resource type. The
    /// sound resource can be played to the speakers either once or
    /// indefinitly. The sound can also be stoped if it is playing.
    /// </remarks>
    public abstract class PlayableRes : Resource
    {
        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PlayableRes()
            : base()
        { }

        #endregion

        #region playable

        public abstract void Play();
        public abstract void PlayLoop();
        public abstract void Pause();
        public abstract void Stop();

        #endregion

        #endregion
    }
}
