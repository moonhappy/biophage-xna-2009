/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Core;
using LNA.GameEngine.Core.AsyncTasks;

namespace LNA.GameEngine.Objects.Scenes
{
    /// <summary>
    /// The SceneMgr provides the interface between the LNA core and the
    /// game scenes.
    /// </summary>
    public class SceneManager
    {
        #region fields

        protected DebugManager m_debugMgr;
        protected AsyncTaskManager m_asyncTaskMgr;

        protected Stage m_currentStage;
        protected Dictionary<uint, Stage> m_stages;

        protected bool m_continueLoop;

        protected LnaGame m_game;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="asyncTaskMgr">
        /// Reference to the asynchronous task manager.
        /// </param>
        /// <param name="game">
        /// Reference to the game component.
        /// </param>
        public SceneManager(DebugManager debugMgr, AsyncTaskManager asyncTaskMgr, LnaGame game)
        {
            //Set fields
            m_debugMgr = debugMgr;
            m_asyncTaskMgr = asyncTaskMgr;
            m_currentStage = null;
            m_stages = new Dictionary<uint,Stage>();
            m_continueLoop = true;
            m_game = game;
            
            //asserts
            m_debugMgr.Assert(m_asyncTaskMgr != null,
                "SceneManager:Constructor - Asynchronous task manager is null.");
            m_debugMgr.Assert(game != null,
                "SceneManager:Constructor - LnaGame component is null.");

            //log
            m_debugMgr.WriteLogEntry("SceneMgr:Constructor - done.");
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Read-only access to the asynchronous task manager.
        /// </summary>
        public AsyncTaskManager AsyncTaskMgr
        {
            get { return m_asyncTaskMgr; }
        }

        /// <summary>
        /// Read-only reference to the LnaGame component.
        /// </summary>
        public LnaGame Game
        {
            get { return m_game; }
        }

        #endregion

        #region stage_control

        /// <summary>
        /// Returns reference to the current Stage.
        /// </summary>
        public Stage CurrentStage
        {
            get { return m_currentStage; }
        }

        /// <summary>
        /// Sets or changes the current active stage to the stage with a
        /// matching Id. This method will not unload or Deinit the current
        /// stage.
        /// </summary>
        /// <param name="nextStageId">
        /// Id of the next stage.
        /// </param>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool SetCurrentStage(uint nextStageId)
        {
            return SetCurrentStage(nextStageId, false, false);
        }

        /// <summary>
        /// Sets or changes the current active stage to the stage with a
        /// matching Id to the Id parameter.
        /// </summary>
        /// <param name="nextStageId">
        /// Id of the next stage.
        /// </param>
        /// <param name="unloadLastStage">
        /// If true, the current active stage will be unloaded.
        /// </param>
        /// <param name="deinitLastStage">
        /// If true, the current active stage will be deinitialised.
        /// </param>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool SetCurrentStage(    uint nextStageId, 
                                        bool unloadLastStage, bool deinitLastStage)
        {
            //m_debugMgr.WriteLogEntry("SceneMgr:SetCurrentStage - doing.");

            Stage nextStage;
            bool returnFlag = true;

            //check next stage Id match
            if (!m_stages.TryGetValue(nextStageId, out nextStage))
            {
                //Game doesn't need to be killed if match not found as it can resume
                //  with the current stage. Debugging should check this error.
                string sLogEntry = "SceneMgr:SetCurrentStage - no Id match found for param 'nextStageId'=";
                sLogEntry += nextStageId.ToString();
                m_debugMgr.WriteLogEntry(sLogEntry);
                //check assertion that current stage exists to fall back on
                m_debugMgr.Assert(m_currentStage != null,
                    "SceneMgr:SetCurrentStage - field 'm_currentStage' is null.");
                return false;
            }

            //init next stage if not already done
            if (!nextStage.IsInit)
                if (!nextStage.Init())
                {
                    returnFlag = false;
                    m_debugMgr.WriteLogEntry(
                        "SceneMgr:SetCurrentStage - next stage init failed.");
                }

            //load next stage if not already done
            //  - loading is an important operation, so throw assert error if this fails
            //  (this assert error should occur in stage itself, but always be prepared!).
            if (!nextStage.IsLoaded)
                m_debugMgr.Assert(nextStage.Load(),
                    "SceneMgr:SetCurrentStage - next stage load failed.");

            //set current stage to the next stage
            Stage oldStage = m_currentStage;
            m_currentStage = nextStage;

            if (oldStage != null)
            {
                //unload current stage if requested
                if (unloadLastStage)
                    if (!oldStage.Unload())
                    {
                        returnFlag = false;
                        m_debugMgr.WriteLogEntry(
                            "SceneMgr:SetCurrentStage - old stage unload failed.");
                    }

                //deinit current stage if requested
                if (deinitLastStage)
                    if (!oldStage.Deinit())
                    {
                        returnFlag = false;
                        m_debugMgr.WriteLogEntry(
                            "SceneMgr:SetCurrentStage - old stage deinit failed.");
                    }
            }

            return returnFlag;
        }

        /// <summary>
        /// Adds a Stage object to the Scene Manager.
        /// </summary>
        /// <param name="stage">
        /// Reference to the Stage object to add.
        /// </param>
        public void AddStage(Stage stage)
        {
            m_debugMgr.Assert(stage != null, 
                "SceneMgr:AddStage - param 'stage' was null.");
            
            m_stages.Add(stage.Id, stage);

            //m_debugMgr.WriteLogEntry("SceneMgr:AddStage - done.");
        }

        /// <summary>
        /// Returns a reference to Stage with matching Id.
        /// </summary>
        /// <param name="stageId">
        /// Stage Id to find match.
        /// </param>
        /// <returns>
        /// Stage reference with matching stage Id.
        /// </returns>
        public Stage GetStage(uint stageId)
        {
            //find and return (if found)
            Stage stageRef;
            m_stages.TryGetValue(stageId, out stageRef);

            //m_debugMgr.WriteLogEntry("SceneMgr:GetStage - done.");
            return stageRef;
        }

        /// <summary>
        /// Removes the Stage reference if matching ID is listed (IE:
        /// shallow remove).
        /// </summary>
        /// <param name="stageId">
        /// The Stage ID to match.
        /// </param>
        /// <returns>
        /// True if Stage object matched and removed, false otherwise.
        /// </returns>
        public bool RemoveStage(uint stageId)
        {
            //m_debugMgr.WriteLogEntry("SceneMgr:RemoveStage - done.");
            bool retVal;
            retVal = m_stages.Remove(stageId);
            return retVal;
        }

        /// <summary>
        /// Reference to the collection of stages.
        /// </summary>
        public Dictionary<uint, Stage> GetStageCollection
        {
            get { return m_stages; }
        }

        #endregion

        #region scene_loading

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
            //m_debugMgr.WriteLogEntry("SceneMgr:BranchDiff - doing.");

            //assert params are valid
            m_debugMgr.Assert(scnDo != null, 
                "SceneMgr:BranchDiff - 'scnDo' is null.");
            m_debugMgr.Assert(scnUndo != null, 
                "SceneMgr:BranchDiff - 'scnUndo' is null.");
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

        /// <summary>
        /// Calls the 'Load' method for each scene node listed on the load
        /// branch stack, in its order.
        /// </summary>
        /// <param name="loadBranch">
        /// Reference to the stack that lists all the scenes that will have
        /// their 'Load' method called. NOTE: the stack will remain intact
        /// when this method returns.
        /// </param>
        /// <returns>
        /// True if no load error occured, otherwise false.
        /// </returns>
        public bool LoadScene(Stack<ILoadable> loadBranch)
        {
            //m_debugMgr.WriteLogEntry("SceneMgr:LoadScene - load branch doing.");

            //assert
            m_debugMgr.Assert(loadBranch != null, 
                "SceneMgr:LoadScene - 'loadBranch' is null.");

                //invoke the load methods - pop the stage object first though
                bool returnVal = true;
                foreach (ILoadable scn in loadBranch)
                {
                    if (!(scn is Stage))
                        if (!scn.IsLoaded)
                            if (!scn.Load())
                                returnVal = false;
                }
                return returnVal;
        }

        /// <summary>
        /// Loads a scene including all scene nodes down the branch that are
        /// not mutual to the current scene.
        /// </summary>
        /// <param name="sceneLoad">
        /// Reference to the scene object to load.
        /// </param>
        /// <param name="toUnload">
        /// Stack that lists all non-mutual nodes of the current scene to the
        /// 'loaded' scene. This helps indicate what nodes must be unloaded
        /// when the 'loaded' scene is set to the current scene.
        /// </param>
        /// <returns>
        /// True if no load error occured, otherwise false.
        /// </returns>
        public bool LoadScene(Scene sceneLoad, out Stack<ILoadable> toUnload)
        {
            //m_debugMgr.WriteLogEntry("SceneMgr:LoadScene - doing.");

            //get load and unload branches,
            //  basically Load = A - (A n B) ; Unload = B - (A n B) ;
            //  where A is the scene branch to load and B is the scene
            //  branch to unload (the current scene branch).
            Stack<ILoadable> toLoad;
            toUnload = null;
            if (m_currentStage != null)
            {
                BranchDiff(sceneLoad, m_currentStage.CurrentScene,
                    out toLoad, out toUnload);
            }
            else
            {
                Scene scnNode = sceneLoad;
                toLoad = new Stack<ILoadable>();
                while (scnNode != null)
                {
                    toLoad.Push(scnNode);
                    scnNode = scnNode.ParentScene;
                }
                toLoad.Push(sceneLoad.Stage);
            }

            return LoadScene(toLoad);
        }

        /// <summary>
        /// Calls the 'Unload' method for each scene node listed on the
        /// unload branch stack, in its order.
        /// </summary>
        /// <param name="unloadBranch">
        /// Reference to the stack that lists all the scenes that will have
        /// their 'Unload' method called. NOTE: the stack will remain intact
        /// when this method returns.
        /// </param>
        /// <returns>
        /// True if no load error occured, otherwise false.
        /// </returns>
        public bool UnloadScene(Stack<ILoadable> unloadBranch)
        {
            //m_debugMgr.WriteLogEntry("SceneMgr:UnloadScene - unload branch doing.");

            //assert
            m_debugMgr.Assert(unloadBranch != null, 
                "SceneMgr:UnloadScene - 'unloadBranch' is null.");

                //invoke the load methods
                bool returnVal = true;
                foreach (ILoadable scn in unloadBranch)
                {
                    if (!(scn is Stage))
                        if (scn.IsLoaded)
                            if (!scn.Unload())
                                returnVal = false;
                }
                return returnVal;
        }

        /// <summary>
        /// Unloads a scene including all scene nodes down the branch that
        /// are not mutual to the current scene.
        /// </summary>
        /// <remarks>
        /// This method is not consistent with the 'LoadScene' method. IE:
        /// It does not provide a 'toLoad' stack. This is due to the fact
        /// that the 'toLoad' stack would contain scene nodes non-mutual to
        /// the 'unloaded' scene branch of the current scene - which will
        /// all already be loaded. So the 'toLoad' stack would not serve any
        /// purposes.
        /// </remarks>
        /// <param name="sceneUnload">
        /// Reference to the scene object to unload.
        /// </param>
        /// <returns>
        /// True if no load error occured, otherwise false.
        /// </returns>
        public bool UnloadScene(Scene sceneUnload)
        {
            //m_debugMgr.WriteLogEntry("SceneMgr:UnloadScene - doing.");

            //get load and unload branches,
            //  basically Load = A - (A n B) ; Unload = B - (A n B) ;
            //  where A is the scene branch to load (the current scene branch)
            //  and B is the scene branch to unload.
            Stack<ILoadable> toUnload, toLoad;
            BranchDiff(sceneUnload, m_currentStage.CurrentScene,
                out toUnload, out toLoad);

            return UnloadScene(toUnload);
        }

        #endregion

        #region game_loop

        /// <summary>
        /// If true continue the game loop, otherwise stop and exit.
        /// </summary>
        public bool ContinueLoop
        {
            get { return m_continueLoop; }
        }

        /// <summary>
        /// Call this method to exit the game.
        /// </summary>
        public void ExitGame()
        {
            m_continueLoop = false;
        }

        #endregion

        #endregion
    }
}
