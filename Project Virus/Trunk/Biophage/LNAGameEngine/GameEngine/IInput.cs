/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNA.GameEngine
{
    /// <summary>
    /// Represents a class capable of handling input routines.
    /// </summary>
    public interface IInput
    {
        /// <summary>
        /// Input handling method.
        /// </summary>
        /// <param name="newGPState">
        /// Reference to the latest game pad state structure.
        /// </param>
        /// <param name="prevGPState">
        /// Reference to the previous game pad state structure.
        /// </param>
        /// <param name="newKBState">
        /// Reference to the latest keyboard state structure.
        /// </param>
        /// <param name="prevKBState">
        /// Reference to the previous keyboard state structure.
        /// </param>
        void Input( ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                    ref Microsoft.Xna.Framework.Input.GamePadState prevGPState
#if !XBOX
                    ,ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                    ref Microsoft.Xna.Framework.Input.KeyboardState prevKBState
#endif
            );
    }
}
