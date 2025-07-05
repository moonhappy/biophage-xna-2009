/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Resources;

namespace LNA.GameEngine.Objects.Scenes
{
    /// <summary>
    /// Stage class acts as the root node for a scene tree structure.
    /// </summary>
    /// <remarks>
    /// This scene tree structure provides a way to organise complicated
    /// game level and setting hiearchies in a simple and consistent
    /// manner; this, intern, should make organising content loading
    /// easier to manage.
    /// </remarks>
    public class Stage : IInitialise, ISceneTree
    {
        #region fields

        protected uint m_id;
        protected DebugManager m_debugMgr;
        protected SceneManager m_sceneMgr;
        
        protected bool m_isInit;
        protected bool m_isLoaded;

        protected Dictionary<uint, Scene> m_childScenes;
        protected Scene m_currentScene;

        protected CameraGObj m_defaultCamera;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Unique Id to set to this Stage object.
        /// </param>
        /// <param name="debugMgr">
        /// Pass by reference value to the debug manager.
        /// </param>
        /// <param name="sceneMgr">
        /// Pass by reference value to the scene manager.
        /// </param>
        public Stage(   uint id,
                        DebugManager debugMgr, ResourceManager resourceMgr, SceneManager sceneMgr,
                        Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr)
        {
            m_debugMgr = debugMgr;

            //check assert params first for fast-fail
            //  scene manager must be valid - check assertion
            m_sceneMgr = sceneMgr;
            m_debugMgr.Assert(m_sceneMgr != null, 
                "Stage:Constructor - scene manager was null.");

            //set all other fields
            m_id = id;
            m_isInit = false;
            m_isLoaded = false;
            m_childScenes = new Dictionary<uint, Scene>();
            m_currentScene = null;
            m_defaultCamera = new CameraGObj(
                uint.MaxValue, debugMgr, resourceMgr, null, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000f);


            //add self to manager
            sceneMgr.AddStage(this);

            //log
            string sLog = "Stage:Construction - Id:";
            sLog += m_id;
            sLog += " done.";
            m_debugMgr.WriteLogEntry(sLog);
        }

        #endregion

        #region initialisation

        /// <summary>
        /// All initialisation code for this Stage object should be defined
        /// in this method.
        /// </summary>
        /// <returns>
        /// True if the initialisation/creation completed without error,
        /// otherwise false.
        /// </returns>
        public bool Init()
        {
            //m_debugMgr.WriteLogEntry("Stage:Init - doing.");

            bool retVal = true;
            if (!m_isInit)
            {
                m_debugMgr.Assert(m_currentScene != null, 
                    "Stage:Init - current scene is null.");
                retVal = m_currentScene.Init();

                if (!m_defaultCamera.Init())
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        /// <summary>
        /// Queries whether this Stage has been created.
        /// </summary>
        public bool IsInit
        {
            get { return m_isInit; }
        }

        /// <summary>
        /// Reinitialise this Stage. If the stage has not yet been created,
        /// then the 'Init' method will be called in place of this method.
        /// </summary>
        /// <returns>
        /// True if this Stage was reinitialised without error, otherwise
        /// false.
        /// </returns>
        public bool Reinit()
        {
            //m_debugMgr.WriteLogEntry("Stage:Reinit - doing.");

            m_debugMgr.Assert(m_currentScene != null, 
                "Stage:Reinit - current scene is null.");
            bool retVal = m_currentScene.Reinit();

            if (!m_defaultCamera.Reinit())
                retVal = false;

            if (retVal)
                m_isInit = true;
            else
                m_isInit = false;

            return retVal;
        }

        #region loading

        /// <summary>
        /// Loads all resources mapped to this Stage.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool Load()
        {
            //m_debugMgr.WriteLogEntry("Stage:Load - doing.");

            bool retVal = true;
            if (!m_isLoaded)
            {
                m_debugMgr.Assert(m_currentScene != null, 
                    "Stage:Load - current scene is null.");
                retVal = m_currentScene.Load();

                if (!m_defaultCamera.Load())
                    retVal = false;

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        /// <summary>
        /// Queries whether this Stage has been loaded.
        /// </summary>
        public bool IsLoaded
        {
            get { return m_isLoaded; }
        }

        /// <summary>
        /// Unloads all resources mapped to this Stage.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool Unload()
        {
            //m_debugMgr.WriteLogEntry("Stage:UnLoad - doing.");

            bool retVal = true;
            if (m_isLoaded)
            {
                m_debugMgr.Assert(m_currentScene != null, 
                    "Stage:Unload - current scene is null.");
                retVal = m_currentScene.Unload();

                if (!m_defaultCamera.Unload())
                    retVal = false;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        /// <summary>
        /// Will invalidate this Stage object. This Stage should not be used
        /// if this method is called, unless the Stage's 'Init' method is 
        /// called again.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool Deinit()
        {
            //m_debugMgr.WriteLogEntry("Stage:Deinit - doing.");

            bool retVal = true;
            if (m_isInit)
            {
                m_debugMgr.Assert(m_currentScene != null, 
                    "Stage:Deinit - current scene is null.");
                retVal = m_currentScene.Deinit();

                if (!m_defaultCamera.Deinit())
                    retVal = false;

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Id value.
        /// </summary>
        public uint Id
        {
            get { return m_id; }
        }

        /// <summary>
        /// Reference to the scene manager object.
        /// </summary>
        public SceneManager SceneMgr
        {
            get { return m_sceneMgr; }
        }

        #endregion

        #region scene_tree

        /// <summary>
        /// Adds a Scene object reference as a child node of this Stage.
        /// </summary>
        /// <param name="childScene">
        /// Pass by reference value to the child Scene node to add to this
        /// Stage object.
        /// </param>
        public void AddChildScene(Scene childScene)
        {
            m_debugMgr.Assert(childScene != null, 
                "Stage:AddChildScene - param 'childScene' was null.");
            
            //add to tree collection
            m_childScenes.Add(childScene.Id, childScene);

            //m_debugMgr.WriteLogEntry("Stage:AddChildScene - done.");
        }

        /// <summary>
        /// Returns a reference to child scene with matching Id.
        /// </summary>
        /// <param name="childId">
        /// Child scene Id to find match.
        /// </param>
        /// <returns>
        /// Reference to the child scene node with matching Id.
        /// </returns>
        public Scene GetChildScene(uint childId)
        {
            //find and return (if found)
            Scene childSceneRef = null;
            m_childScenes.TryGetValue(childId, out childSceneRef);

            //m_debugMgr.WriteLogEntry("Stage:GetChildScene - done.");
            return childSceneRef;
        }

        /// <summary>
        /// Reference to the current scene object.
        /// </summary>
        public Scene CurrentScene
        {
            get { return m_currentScene; }
        }

        /// <summary>
        /// Call to change the current scene. If a current scene is set, it
        /// will not be unloaded or Deinited.
        /// </summary>
        /// <param name="nextSceneId">
        /// Id of the next scene.
        /// </param>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool SetCurrentScene(uint nextSceneId)
        {
            return SetCurrentScene(nextSceneId, false, false);
        }

        /// <summary>
        /// Call to change the current scene. Depending on how the 
        /// programmer wants to handle content loading, parameter options
        /// exist to to either unload and/or Deinit the current scene before
        /// the scene change.
        /// </summary>
        /// <param name="nextSceneId">
        /// Id of the next scene.
        /// </param>
        /// <param name="unloadLastScene">
        /// If true, unload current scene before change.
        /// </param>
        /// <param name="DeinitLastScene">
        /// If true, Deinit current scene before change.
        /// </param>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public bool SetCurrentScene(    uint nextSceneId,
                                        bool unloadLastScene, bool deinitLastScene)
        {
            //m_debugMgr.WriteLogEntry("Stage:SetCurrentScene - doing.");

            //check next scene Id match
            Scene nextScene;
            bool returnFlag = true;

            if (!m_childScenes.TryGetValue(nextSceneId, out nextScene))
            {
                //Game doesn't need to be killed if match not found as it can resume
                //  with the current scene. Debugging should check this error.
                string sLogEntry = "Stage:SetCurrentScene - no Id match found for param 'nextSceneId'=";
                sLogEntry += nextSceneId.ToString();
                m_debugMgr.WriteLogEntry(sLogEntry);
                //check assertion that current scene exists to fall back on
                m_debugMgr.Assert(m_currentScene != null,
                    "Stage:SetCurrentScene - field 'm_currentScene' is null.");
                return false;
            }

            //scene cannot be null
            m_debugMgr.Assert(nextScene != null,
                "Stage:SetCurrentScene - varible 'nextScene' is null.");


            //init next scene if not already done
            if (!nextScene.IsInit)
                if (!nextScene.Init())
                {
                    returnFlag = false;
                    m_debugMgr.WriteLogEntry("Stage:SetCurrentScene - next scene creation failed.");
                }

            //load next scene if not already done
            //  - loading is an important operation, so throw assert error if this fails,
            //  - load via scene manager
            Stack<ILoadable> scnsToUnload = null;
            //if (!nextScene.IsLoaded)
            //{
                m_debugMgr.Assert(m_sceneMgr.LoadScene(nextScene, out scnsToUnload),
                    "Stage:SetCurrentScene - next scene load failed.");
            //}


            if (m_currentScene != null)
            {
                //unload current scene if requested
                if (unloadLastScene)
                {
                    if (scnsToUnload != null)
                    {
                        if (!m_sceneMgr.UnloadScene(scnsToUnload))
                        {
                            returnFlag = false;
                            m_debugMgr.WriteLogEntry("Stage:SetCurrentScene - current scene unload failed.");
                        }
                    }
                }

                //Deinit current scene if requested
                if (deinitLastScene)
                {
                    if (!m_currentScene.Deinit())
                    {
                        returnFlag = false;
                        m_debugMgr.WriteLogEntry("Stage:SetCurrentScene - current scene Deinit failed.");
                    }
                }
            }

            //set current scene to the passed next scene
            m_currentScene = nextScene;

            //make sure input states are adjusted
            m_currentScene.SetPrevInputStates();

            return returnFlag;
        }

        #endregion

        #region game_loop

        /// <summary>
        /// Update procedure of the game loop.
        /// </summary>
        /// <param name="time">
        /// XNA game time for the frame.
        /// </param>
        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_debugMgr.Assert(m_currentScene != null, 
                "Stage:Update - m_currentScene is null.");

            //get inputs
            Microsoft.Xna.Framework.Input.GamePadState gpState = Microsoft.Xna.Framework
                .Input.GamePad.GetState(SceneMgr.Game.LeadPlayerIndex);
#if !XBOX
             Microsoft.Xna.Framework.Input.KeyboardState kbState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
#endif

            //input updates 
            //- prompt has priority, then msg box, then scene menu, then scene
            if (SceneMgr.Game.IsActive)
            {
                if (m_currentScene.PromptActive)
                {
                    //prompt box is always active
                    m_currentScene.Prompt.Input(ref gpState
#if !XBOX
                        , ref kbState
#endif
                        );
                    m_currentScene.Prompt.Update(gameTime);
                    m_currentScene.Prompt.Animate(gameTime);
                }
                else if (m_currentScene.MsgBoxActive)
                {
                    //message box is always active
                    m_currentScene.MsgBox.Input(ref gpState
#if !XBOX
                        , ref kbState
#endif
                        );
                    m_currentScene.MsgBox.Update(gameTime);
                    m_currentScene.MsgBox.Animate(gameTime);
                }
                else if (m_currentScene.GetMenu != null)
                {
                    if (m_currentScene.GetMenu.Active)
                    {
                        m_currentScene.GetMenu.Input(ref gpState
#if !XBOX
, ref kbState
#endif
);
                        m_currentScene.GetMenu.Update(gameTime);
                        m_currentScene.GetMenu.Animate(gameTime);
                    }
                    else
                    {
#if !XBOX
                        m_currentScene.Input(gameTime, ref gpState, ref kbState);
#else
                        m_currentScene.Input(gameTime, ref gpState);
#endif
                    }
                }
                else
                {
                    //in case menu is not assigned
#if !XBOX
                    m_currentScene.Input(gameTime, ref gpState, ref kbState);
#else
                    m_currentScene.Input(gameTime, ref gpState);
#endif
                }
            }

            if (!m_currentScene.IsPaused)
            {
                //call scene's Update
                m_currentScene.Update(gameTime);
                //call scene's GObjUpdate
                m_currentScene.UpdateGObjs(gameTime);
                //call scene's PostUpdate
                m_currentScene.PostUpdate(gameTime);
            }

            //update inputs
            m_currentScene.SetPrevInputStates();
        }

        /// <summary>
        /// Render procedure of the game loop.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// XNA graphics device.
        /// </param>
        public void Draw(   Microsoft.Xna.Framework.GameTime gameTime,
                            Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            m_debugMgr.Assert(m_currentScene != null, 
                "Stage:Render - m_currentScene is null.");

            //call scene's Render
            if (m_currentScene.IsVisible)
                m_currentScene.Draw(gameTime,graphicsDevice);

            //draw message / prompts
            if (m_currentScene.MsgBoxActive || m_currentScene.PromptActive)
                m_currentScene.GetFadeOverlay.DoDraw(gameTime, graphicsDevice, m_defaultCamera);

            if (m_currentScene.MsgBoxActive)
                m_currentScene.MsgBox.DoDraw(gameTime, graphicsDevice, null);

            if (m_currentScene.PromptActive)
                m_currentScene.Prompt.DoDraw(gameTime, graphicsDevice, null);
        }

        #endregion

        #endregion
    }
}
