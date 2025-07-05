/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects;
using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;

namespace LNA.GameEngine.Objects.GameObjects.Sprites
{
    /// <summary>
    /// Extends the game object to allow spritebatch drawing.
    /// </summary>
    public abstract class SpriteGObj : GameObject
    {
        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Sprite Id.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the scene the sprite belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        public SpriteGObj(  uint id,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Scene scene, bool addToScene)
            : base(id, debugMgr, resourceMgr, scene, addToScene)
        { }

        #endregion

        /// <summary>
        /// This method allows sprites to be rendered efficiently by using
        /// the XNA SpriteBatch class (with effect).
        /// </summary>
        /// <param name="spriteBatch">
        /// Reference to the XNA SpriteBatch class.
        /// </param>
        public abstract void SpriteDraw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch);

        #endregion
    }
}
