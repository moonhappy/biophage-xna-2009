/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNA.GameEngine.Resources.Drawable
{
    /// <summary>
    /// Represents the base definition for a "drawable" resource type.
    /// Extends the abstract Resource class with a "Draw" method.
    /// </summary>
    /// <remarks>
    /// An example "drawable" resource would be a model resource. The
    /// draw routine for a model is fairly complex then most other
    /// resources as it consists of many other child resources (like
    /// texture and materials) and is considered as "boiler-plate" code.
    /// </remarks>
    public abstract class DrawableRes : Resource
    {
        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DrawableRes()
            : base()
        { }

        #endregion

        #region drawable

        /// <summary>
        /// Draws the resource.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="worldMat">
        /// The world matrix transformation.
        /// This is 'Model space' -> 'World space'.
        /// </param>
        /// <param name="projectionMat">
        /// The projection matrix transformation.
        /// This is 'World space' -> 'Camera space'.
        /// </param>
        /// <param name="viewMat">
        /// The view matrix transformation.
        /// This is 'Camera space' -> 'View space'. The GPU will be able
        /// to cull in view space as it is normalised.
        /// </param>
        public abstract void Draw(  Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Matrix worldMat,
                                    Microsoft.Xna.Framework.Matrix projectionMat,
                                    Microsoft.Xna.Framework.Matrix viewMat);

        #endregion

        #endregion
    }
}
