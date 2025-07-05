/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace LNA.GameEngine.Resources
{
    /// <summary>
    /// LNA game resource objects should inherit this interface.
    /// </summary>
    public interface IGameResource : IGameResourceHandle
    {
        bool ForceUnload();
    }
}
