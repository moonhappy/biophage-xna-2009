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
    /// Indicates whether the load was successful or not.
    /// </summary>
    public sealed class AsyncSceneLoadReturn
    {
        /// <summary>
        /// Indicates whether the load was successful or not.
        /// </summary>
        public bool loadSuccessful;

        /// <summary>
        /// Indicates if the loading operation has completed.
        /// </summary>
        public bool loadingCompleted;

        /// <summary>
        /// Indicates if an error occured.
        /// </summary>
        public bool loadErrorOccured;

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="loadResult">
        /// If true the load was successful, otherwise an error
        /// occured.
        /// </param>
        /// <param name="loadComplete">
        /// If true, the scene has been completely loaded.
        /// </param>
        public AsyncSceneLoadReturn(bool loadResult, bool loadComplete, bool error)
        {
            loadSuccessful = loadResult;
            loadingCompleted = loadComplete;
            loadErrorOccured = error;
        }
    }

    /// <summary>
    /// Param container allows asynchronous scene loader to be extended.
    /// </summary>
    public class AsyncSceneLoadParam
    {
        public Scene scn;
        public object extras;

        public AsyncSceneLoadParam(Scene setScn, object setExtras)
        {
            scn = setScn;
            extras = setExtras;
        }
    }

    /// <summary>
    /// When submitted, resources for a scene will be asynchronously
    /// loaded.
    /// </summary>
    /// <remarks>
    /// For each scene loaded, a return package will be produced.
    /// This allows the caller to calculate what percentage of the scene
    /// has (or hasn't) been loaded. This allows a loading bar to be
    /// displayed interactively on the current scene.
    /// </remarks>
    public class AsyncSceneLoad : AsyncTask
    {
        /// <summary>
        /// Will load the scene param.
        /// </summary>
        public override void DoWork()
        {
            //unbox the scene param
            if (m_param == null)
            {
                m_asyncMgr.ProduceReturn(
                    new AsyncReturnPackage(m_id,
                        (object)(new AsyncSceneLoadReturn(true, true, true))));
                return;
            }

            Scene scnToLoad = ((AsyncSceneLoadParam)m_param).scn;

            SceneManager scnMgr = scnToLoad.Stage.SceneMgr;
            Stage curStage = scnToLoad.Stage;
            Scene curScene = curStage.CurrentScene;

            //load the scene

            //get load and unload branches,
            //  basically Load = A - (A n B) ; Unload = B - (A n B) ;
            //  where A is the scene branch to load and B is the scene
            //  branch to unload (the current scene branch).
            Stack<ILoadable> toLoad, toUnload;
            if (curStage != null)
            {
                BranchDiff(scnToLoad, curScene, out toLoad, out toUnload);
            }
            else
            {
                Scene scnNode = scnToLoad;
                toLoad = new Stack<ILoadable>();
                while (scnNode != null)
                {
                    toLoad.Push(scnNode);
                    scnNode = scnNode.ParentScene;
                }
                toLoad.Push(scnToLoad.Stage);
            }

            //now load each scene
            foreach (ILoadable scn in toLoad)
            {
                if (!(scn is Stage))
                    if (!scn.IsLoaded)
                    {
                        scnToLoad.Load();
                        //submit return package - this causes problems
                        //m_asyncMgr.ProduceReturn(
                        //    new AsyncReturnPackage(
                        //        m_id,
                        //        ((Scene)scn).Id,
                        //        (object)(new AsyncSceneLoadReturn(
                        //            scn.Load(),
                        //            false, false))
                        //            )
                        //        );
                    }
            }


            //all done, so signal
            m_asyncMgr.ProduceReturn(
                    new AsyncReturnPackage(m_id,
                        (object)(new AsyncSceneLoadReturn(true, true, false))));
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
