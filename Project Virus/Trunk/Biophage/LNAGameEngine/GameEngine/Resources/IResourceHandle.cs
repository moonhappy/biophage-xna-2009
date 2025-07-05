/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace LNA.GameEngine.Resources
{
    /// <summary>
    /// LNA game resource handle objects should inherit this interface.
    /// </summary>
    public interface IGameResourceHandle : ILoadable
    {
        string FileName { get; }
        string FileDirectoryPath { get; }
        object GetResource { get; }
    }
}
