/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;

namespace LNA.GameEngine.Objects.Scenes
{
    /// <summary>
    /// Class objects that are a node on a scene tree should inherit
    /// this interface.
    /// </summary>
    public interface ISceneTree
    {
        void AddChildScene(Scene childScene);
        Scene GetChildScene(uint childId);
        //bool RemoveChildScene(uint childId);
        //Dictionary<uint, Scene> GetChildSceneCollection();
    }
}
