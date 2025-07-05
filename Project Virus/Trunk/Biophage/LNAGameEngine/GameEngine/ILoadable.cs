/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

namespace LNA.GameEngine
{
    /// <summary>
    /// Loadable class objects must provide the following load/unload
    /// operations.
    /// </summary>
    public interface ILoadable
    {
        bool Load();
        bool IsLoaded { get; }
        bool Unload();
    }
}
