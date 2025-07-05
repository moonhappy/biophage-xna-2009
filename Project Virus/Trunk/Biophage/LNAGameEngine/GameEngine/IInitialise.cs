/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

namespace LNA.GameEngine
{
    /// <summary>
    /// Game components must provide the following initialisation
    /// methods.
    /// </summary>
    public interface IInitialise : ILoadable, IObject
    {
        bool Init();
        bool IsInit { get; }
        bool Reinit();
        bool Deinit();
    }
}
