/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.Scenes;

namespace LNA.GameEngine.Core.AsyncTasks
{
    /// <summary>
    /// Indicates whether the unload was successful or not.
    /// </summary>
    public sealed class AsyncSceneUnloadReturn
    {
        /// <summary>
        /// Indicates whether the unload was successful or not.
        /// </summary>
        public bool unloadSuccessful;

        /// <summary>
        /// Indicates if the unloading operation has completed.
        /// </summary>
        public bool unloadingCompleted;

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="unloadResult">
        /// If true the load was successful, otherwise an error
        /// occured.
        /// </param>
        /// <param name="unloadComplete">
        /// If true, the scene has been completely loaded.
        /// </param>
        public AsyncSceneUnloadReturn(bool unloadResult, bool unloadComplete)
        {
            unloadSuccessful = unloadResult;
            unloadingCompleted = unloadComplete;
        }
    }

    /// <summary>
    /// When submitted, resources for a scene will be asynchronously
    /// unloaded.
    /// </summary>
    /// <remarks>
    /// For each scene unloaded, a return package will be produced.
    /// This allows the caller to calculate what percentage of the scene
    /// has (or hasn't) been unloaded.
    /// </remarks>
    class AsyncSceneUnload : AsyncTask
    {
        /// <summary>
        /// Will unload the scene param.
        /// </summary>
        public override void DoWork()
        {
            //unbox the scene param
            Scene scnToUnload = (Scene)m_param;

            SceneManager scnMgr = scnToUnload.Stage.SceneMgr;
            Stage curStage = scnMgr.CurrentStage;
            Scene curScene = curStage.CurrentScene;

            //unload the scene

            //get load and unload branches,
            //  basically Load = A - (A n B) ; Unload = B - (A n B) ;
            //  where A is the scene branch to unload and B is the scene
            //  branch to load (the current scene branch).
            Stack<ILoadable> toUnload, toLoad;
            if (curStage != null)
            {
                BranchDiff(scnToUnload, curScene, out toUnload, out toLoad);
            }
            else
            {
                Scene scnNode = scnToUnload;
                toUnload = new Stack<ILoadable>();
                while (scnNode != null)
                {
                    toUnload.Push(scnNode);
                    scnNode = scnNode.ParentScene;
                }
                toUnload.Push(scnToUnload.Stage);
            }

            //now unload each scene
            foreach (ILoadable scn in toUnload)
            {
                if (!(scn is Stage))
                    if (scn.IsLoaded)
                    {
                        //submit return package
                        m_asyncMgr.ProduceReturn(
                            new AsyncReturnPackage(
                                m_id,
                                ((Scene)scn).Id,
                                (object)(new AsyncSceneUnloadReturn(
                                    scn.Unload(),
                                    false))
                                    )
                                );
                    }
            }


            //all done, so signal
            m_asyncMgr.ProduceReturn(
                    new AsyncReturnPackage(m_id,
                        (object)(new AsyncSceneUnloadReturn(true, true))));
        }

        /// <summary>
        /// Lists all non-mutual scene nodes between two scene objects.
        /// </summary>
        /// <remarks>
        /// scnDo = A - (A n B) ; scnUndo = B - (A n B) ; where A and B are
        /// two scene node branches. This is usefull for content loading as
        /// only the content that is different between two scene's will be 
        /// loaded/unloaded. The word "do" can either mean load or unload
        /// depending on the intended action.
        /// </remarks>
        /// <param name="scnDo">
        /// Reference to the "do" scene node.
        /// </param>
        /// <param name="scnUndo">
        /// Reference to the "undo" scene node.
        /// </param>
        /// <param name="doStack">
        /// Stack containing ILnaLoadable objects that need their "do"
        /// method called.
        /// </param>
        /// <param name="undoStack">
        /// Stack containing ILnaLoadable objects that need their "undo"
        /// method called.
        /// </param>
        private void BranchDiff(Scene scnDo, Scene scnUndo,
                                out Stack<ILoadable> doStack,
                                out Stack<ILoadable> undoStack)
        {
            //m_debugMgr.WriteLogEntry("AsyncSceneLoad:BranchDiff - doing.");

            doStack = new Stack<ILoadable>();
            undoStack = new Stack<ILoadable>();
            if (scnDo == scnUndo)
                return;

            //add all scene nodes of do branch.
            Scene scnNode = scnDo;
            while (scnNode != null)
            {
                doStack.Push(scnNode);
                scnNode = scnNode.ParentScene;
            }
            doStack.Push(scnDo.Stage);

            //add all scene nodes of undo branch.
            scnNode = scnUndo;
            while (scnNode != null)
            {
                undoStack.Push(scnNode);
                scnNode = scnNode.ParentScene;
            }
            undoStack.Push(scnUndo.Stage);

            //remove intersection scene nodes
            while (doStack.Peek() == undoStack.Peek())
            {
                doStack.Pop(); undoStack.Pop();
                if (doStack.Count == 0) break;
                if (undoStack.Count == 0) break;
            }
        }
    }
}
