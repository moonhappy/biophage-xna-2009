/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNA.GameEngine.Objects.GameObjects
{
    /// <summary>
    /// Game objects must provide the following methods.
    /// </summary>
    public interface IGameObject : IInitialise
    {
        bool Active { get; set; }
        bool Visible { get; set; }

        void Update(Microsoft.Xna.Framework.GameTime gameTime);
        void Animate(Microsoft.Xna.Framework.GameTime gameTime);

        void SetUpdate(SynchroniseWithThreadPool syncThreadPool);
        void DoUpdate(object gameTime);
        void DoDraw(    Microsoft.Xna.Framework.GameTime gameTime,
                        Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                        CameraGObj camera);
    }
}
