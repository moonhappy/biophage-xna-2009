/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

namespace LNA.GameEngine
{
    /// <summary>
    /// Class objects that are a unique "item" in the LNA framework
    /// should inherit this interface.
    /// </summary>
    public interface IObject
    {
        uint Id { get; }
    }
}
