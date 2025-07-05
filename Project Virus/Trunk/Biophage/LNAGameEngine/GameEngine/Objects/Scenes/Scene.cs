/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.UI;
using LNA.GameEngine.Objects.UI.Menu;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;
using LNA.GameEngine.Objects.GameObjects.Assets;

namespace LNA.GameEngine.Objects.Scenes
{
    /// <summary>
    /// The Scene class provides the interface between the game 
    /// programmer and the LNA game engine.
    /// </summary>
    /// <remarks>
    /// Scene lists all the game assets and contains a UI and Menu 
    /// system. Scene provides the Update and Render methods that are
    /// called by the game engine for every frame. Scenes must be used
    /// to define a game level, main menu, or cut scene.
    /// </remarks>
    public abstract class Scene : IInitialise, ISceneTree
    {
        #region fields

        protected uint m_id;
        protected DebugManager m_debugMgr;
        protected SceneManager m_sceneMgr;
        protected ResourceManager m_resMgr;

        protected bool m_isInit;
        protected bool m_isLoaded;
        protected bool m_isPaused;
        protected bool m_isVisible;

        protected Stage m_stage;
        protected Scene m_parent;
        protected Dictionary<uint, Scene> m_childScenes;

        protected LNA.GameEngine.Objects.GameObjects.CameraGObj m_camera;
        protected Dictionary<uint, IGameObject> m_gameObjects;
        protected Menu m_menu;

        protected QuadAsset m_fadeOverlay;
        protected Menu m_msgBox;
        protected MenuLabel m_msgLabel;
        protected MenuButton m_msgButton;
        protected Menu m_prompt;
        protected MenuLabel m_promptLabel;
        protected MenuButton m_promptYesButton;
        protected MenuButton m_promptNoButton;

        protected Microsoft.Xna.Framework.Graphics.Color m_clearColour;
        protected Microsoft.Xna.Framework.Graphics.Color m_initClearColour;

        protected Microsoft.Xna.Framework.Input.GamePadState m_prevGPState;
#if !XBOX
        protected Microsoft.Xna.Framework.Input.KeyboardState m_prevKBState;
#endif

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor #1. This is for top level scene 
        /// construction (IE: no parent scene).
        /// </summary>
        /// <param name="id">
        /// ID of the scene, this must be unique.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the Debug Manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics device.
        /// </param>
        /// <param name="initialColour">
        /// Initial clear colour of the scene.
        /// </param>
        /// <param name="stage">
        /// Reference to this Scene's Stage object. This cannot be null
        /// otherwise an assertion error will be thrown.
        /// </param>
        /// <param name="menu">
        /// Reference to the scene's menu. Since no parent scene exists,
        /// a menu must be provided.
        /// </param>
        public Scene(   uint id,
                        DebugManager debugMgr, ResourceManager resourceMgr,
                        Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                        Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                        Microsoft.Xna.Framework.Graphics.Color initialColour,
                        SpriteFontResHandle fontMsgBoxPrompt,
                        Stage stage, Menu menu)
        {
            m_debugMgr = debugMgr;
            
            //set fields
            m_resMgr = resourceMgr;
            m_stage = stage;
            m_id = id;
            m_isInit = false;
            m_isLoaded = false;
            m_isPaused = false;
            m_isVisible = true;
            m_parent = null;
            m_childScenes = new Dictionary<uint, Scene>();
            m_gameObjects = new Dictionary<uint, IGameObject>();
            m_menu = menu;

            //message/prompt boxes stuff
            m_fadeOverlay = new QuadAsset(uint.MaxValue,
                Microsoft.Xna.Framework.Vector3.Zero, 2f, 2f,
                new Microsoft.Xna.Framework.Graphics.Color(0f, 0f, 0f, 0.5f),
                m_debugMgr, m_resMgr, this, false, graphicsMgr);

            #region msg_box

            m_msgBox = new Menu(
                uint.MaxValue, m_debugMgr, m_resMgr, 
                graphicsMgr, spriteBatch, null, 
                stage.SceneMgr.Game.LeadPlayerIndex);

            MenuWindow msgBoxWnd = new MenuWindow(10, m_debugMgr, m_resMgr, 
                fontMsgBoxPrompt, null, null, null, m_msgBox, true, graphicsMgr, 
                stage.SceneMgr.Game.spriteBatch);

            m_msgLabel = new MenuLabel(1, "Message", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr,
                stage.SceneMgr.Game.spriteBatch, msgBoxWnd, true);

            m_msgButton = new MenuButton(2, "OK", "", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr, stage.SceneMgr.Game.spriteBatch,
                msgBoxWnd, true);
            m_msgButton.UIAction += DefaultUIAction;

            m_msgBox.SetDefaultWindow(10);

            #endregion

            #region prompt_box

            m_prompt = new Menu(
                uint.MaxValue, m_debugMgr, m_resMgr, 
                graphicsMgr, spriteBatch, null, 
                stage.SceneMgr.Game.LeadPlayerIndex);

            MenuWindow promptWnd = new MenuWindow(10, m_debugMgr, m_resMgr,
                fontMsgBoxPrompt, null, null, null, m_prompt, true, graphicsMgr,
                stage.SceneMgr.Game.spriteBatch);

            m_promptLabel = new MenuLabel(1, "Prompt", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr,
                stage.SceneMgr.Game.spriteBatch, promptWnd, true);

            m_promptYesButton = new MenuButton(2, "YES", "", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr, stage.SceneMgr.Game.spriteBatch,
                promptWnd, true);
            m_promptYesButton.UIAction += DefaultUIAction;

            m_promptNoButton = new MenuButton(3, "NO", "", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr,
                stage.SceneMgr.Game.spriteBatch, promptWnd, true);
            m_promptNoButton.UIAction += DefaultUIAction;

            m_prompt.SetDefaultWindow(10);

            #endregion

            //Asserts
            m_debugMgr.Assert(m_resMgr != null, 
                "Scene:constructor - resource manager is null.");
            m_debugMgr.Assert(m_menu != null,
                "Scene:Constructor - menu is null.");
            m_debugMgr.Assert(m_stage != null, 
                "Scene:Constructor - param 'stage' was null");
            m_debugMgr.Assert(graphicsMgr != null, 
                "Scene:Constructor - param 'graphicsDevice' is null.");

            m_camera = new CameraGObj(
                uint.MaxValue, 
                m_debugMgr, m_resMgr,
                this, false,
                graphicsMgr.GraphicsDevice.Viewport.AspectRatio);

            m_initClearColour = initialColour;
            m_clearColour = initialColour;

            m_sceneMgr = m_stage.SceneMgr;

            //add to stage
            m_stage.AddChildScene(this);

            //log
            string sLog = "Scene:Construction - Id:";
            sLog += m_id;
            sLog += " done.";
            m_debugMgr.WriteLogEntry(sLog);
        }

        /// <summary>
        /// Argument constructor #2. A child Scene can only be constructed
        /// if it can be given a reference to its Stage (root node) and
        /// parent Scene.
        /// </summary>
        /// <param name="id">
        /// ID of the scene, this must be unique.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the Debug Manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics device.
        /// </param>
        /// <param name="initialColour">
        /// Initial clear colour of the scene.
        /// </param>
        /// <param name="stage">
        /// Reference to this Scene's Stage object. This cannot be null
        /// otherwise an assertion error will be thrown.
        /// </param>
        /// <param name="parentScene">
        /// Reference to this Scene's parent Scene.
        /// </param>
        /// <param name="menu">
        /// Reference to this scene's menu. If null is passed, this scene's
        /// parent scene menu will be used by default.
        /// </param>
        public Scene(   uint id,
                        DebugManager debugMgr, ResourceManager resourceMgr,
                        Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                        Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                        Microsoft.Xna.Framework.Graphics.Color initialColour,
                        SpriteFontResHandle fontMsgBoxPrompt,
                        Stage stage, Scene parentScene, Menu menu)
        {
            m_debugMgr = debugMgr;

            //set fields
            m_resMgr = resourceMgr;
            m_stage = stage;
            m_id = id;
            m_isInit = false;
            m_isLoaded = false;
            m_isPaused = false;
            m_isVisible = true;
            m_parent = parentScene;
            m_childScenes = new Dictionary<uint, Scene>();
            m_gameObjects = new Dictionary<uint, IGameObject>();

            //message/prompt boxes stuff
            m_fadeOverlay = new QuadAsset(uint.MaxValue,
                Microsoft.Xna.Framework.Vector3.Zero, 2f, 2f,
                new Microsoft.Xna.Framework.Graphics.Color(0f, 0f, 0f, 0.5f),
                m_debugMgr, m_resMgr, this, false, graphicsMgr);

            #region msg_box

            m_msgBox = new Menu(
                uint.MaxValue, m_debugMgr, m_resMgr, 
                graphicsMgr, spriteBatch, null, 
                stage.SceneMgr.Game.LeadPlayerIndex);

            MenuWindow msgBoxWnd = new MenuWindow(10, m_debugMgr, m_resMgr,
                fontMsgBoxPrompt, null, null, null, m_msgBox, true, graphicsMgr,
                stage.SceneMgr.Game.spriteBatch);

            m_msgLabel = new MenuLabel(1, "Message", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr,
                stage.SceneMgr.Game.spriteBatch, msgBoxWnd, true);

            m_msgButton = new MenuButton(2, "OK", "", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr, stage.SceneMgr.Game.spriteBatch,
                msgBoxWnd, true);
            m_msgButton.UIAction += DefaultUIAction;

            m_msgBox.SetDefaultWindow(10);

            #endregion

            #region prompt_box

            m_prompt = new Menu(
                uint.MaxValue, m_debugMgr, m_resMgr, 
                graphicsMgr, spriteBatch, null, 
                stage.SceneMgr.Game.LeadPlayerIndex);

            MenuWindow promptWnd = new MenuWindow(10, m_debugMgr, m_resMgr,
                fontMsgBoxPrompt, null, null, null, m_prompt, true, graphicsMgr,
                stage.SceneMgr.Game.spriteBatch);

            m_promptLabel = new MenuLabel(1, "Prompt", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr,
                stage.SceneMgr.Game.spriteBatch, promptWnd, true);

            m_promptYesButton = new MenuButton(2, "YES", "", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr, stage.SceneMgr.Game.spriteBatch,
                promptWnd, true);
            m_promptYesButton.UIAction += DefaultUIAction;

            m_promptNoButton = new MenuButton(3, "NO", "", fontMsgBoxPrompt,
                m_debugMgr, m_resMgr, graphicsMgr,
                stage.SceneMgr.Game.spriteBatch, promptWnd, true);
            m_promptNoButton.UIAction += DefaultUIAction;

            m_prompt.SetDefaultWindow(10);

            #endregion

            //Asserts
            m_debugMgr.Assert(m_resMgr != null, 
                "Scene:constructor - resource manager is null.");
            m_debugMgr.Assert(m_stage != null, 
                "Scene:Constructor - param 'stage' was null");
            m_debugMgr.Assert(graphicsMgr != null, 
                "Scene:Constructor - param 'graphicsDevice' is null.");
            m_debugMgr.Assert(m_parent != null, 
                "Scene:Constructor - param 'parentScene' is null.");

            //other sets
            if (menu == null)
                //use parent scene's menu
                m_menu = m_parent.GetMenu;
            else
            {
                m_menu = menu;
            }

            m_camera = new CameraGObj(
                uint.MaxValue,
                m_debugMgr, m_resMgr,
                this, false,
                graphicsMgr.GraphicsDevice.Viewport.AspectRatio);

            m_initClearColour = initialColour;
            m_clearColour = initialColour;

            m_sceneMgr = m_stage.SceneMgr;

            //add to stage
            m_stage.AddChildScene(this);

            //log
            string sLog = "Scene:Construction - Id:";
            sLog += m_id;
            sLog += " done.";
            m_debugMgr.WriteLogEntry(sLog);
        }

        void DefaultUIAction(object sender, EventArgs e)
        {
            MenuButton mb = (MenuButton)sender;
            mb.GetMenuWindow.GetMenu.Active = false;
        }

        #endregion

        #region initialisation

        /// <summary>
        /// All initialisation code for this Scene should be defined in this
        /// method.
        /// </summary>
        /// <returns>
        /// True if the initialisation completed without error, otherwise
        /// false.
        /// </returns>
        public virtual bool Init()
        {
            //m_debugMgr.WriteLogEntry("Scene:Init - done.");

            bool retVal = true;
            if (!m_isInit)
            {
                //init all gobjs
                foreach (KeyValuePair<uint, IGameObject> gobj in m_gameObjects)
                {
                    if (!gobj.Value.Init())
                        retVal = false;
                }

                //init camera and menu
                if (    (!m_camera.Init())  ||
                        (!m_menu.Init())    ||
                        (!m_fadeOverlay.Init()) ||
                        (!m_prompt.Init())  ||
                        (!m_msgBox.Init())  )
                    retVal = false;

                //init colour
                m_clearColour = m_initClearColour;
                m_isPaused = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        /// <summary>
        /// Reinitialises this Scene. If the scene has not yet been created,
        /// then the 'Init' method will be called in place of this method.
        /// </summary>
        /// <returns>
        /// True if this Scene was reinitialised without error, otherwise
        /// false.
        /// </returns>
        public virtual bool Reinit()
        {
            //m_debugMgr.WriteLogEntry("Scene:Reset - done.");

            bool retVal = true;
            //reinit all gobjs
            foreach (KeyValuePair<uint, IGameObject> gobj in m_gameObjects)
            {
                if (!gobj.Value.Reinit())
                    retVal = false;
            }

            //reinit camera and menu
            if (    (!m_camera.Reinit())    ||
                    (!m_menu.Reinit())      ||
                    (!m_fadeOverlay.Reinit())   ||
                    (!m_prompt.Reinit())    ||
                    (!m_msgBox.Reinit())    )
                retVal = false;

            //reinit colour
            m_clearColour = m_initClearColour;

            if (retVal)
                m_isInit = true;
            else
                m_isInit = false;

            return retVal;
        }

        #region loading

        /// <summary>
        /// Loads all resources mapped to this Scene.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public virtual bool Load()
        {
            //m_debugMgr.WriteLogEntry("Scene:Load - doing.");

            bool retVal = true;
            if (!m_isLoaded)
            {
                //load all gobjs
                foreach (KeyValuePair<uint, IGameObject> gobj in m_gameObjects)
                {
                    if (!gobj.Value.Load())
                        retVal = false;
                }

                //load camera and menu
                if (    (!m_camera.Load())  ||
                        (!m_menu.Load())    ||
                        (!m_fadeOverlay.Load()) ||
                        (!m_prompt.Load())  ||
                        (!m_msgBox.Load())  )
                    retVal = false;

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        /// <summary>
        /// Queries whether this Scene has been loaded.
        /// </summary>
        /// <returns>
        /// True if this Scene has been loaded, otherwise false.
        /// </returns>
        public bool IsLoaded
        {
            get { return m_isLoaded; }
        }

        /// <summary>
        /// Unloads all resources mapped to this Scene.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public virtual bool Unload()
        {
            //m_debugMgr.WriteLogEntry("Scene:UnLoad - doing.");

            bool retVal = true;
            if (m_isLoaded)
            {
                //unload all gobjs
                foreach (KeyValuePair<uint, IGameObject> gobj in m_gameObjects)
                {
                    if (!gobj.Value.Unload())
                        retVal = false;
                }

                //unload 
                if (    (!m_camera.Unload())    ||
                        (!m_menu.Unload())      ||
                        (!m_fadeOverlay.Unload())   ||
                        (!m_prompt.Unload())    ||
                        (!m_msgBox.Unload())    )
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
        /// Will invalidate this Scene object. This Scene should not be used
        /// if this method is called, unless the Scene's 'Init' method is
        /// called again.
        /// </summary>
        /// <returns>
        /// True if no error occure, otherwise false.
        /// </returns>
        public virtual bool Deinit()
        {
            //m_debugMgr.WriteLogEntry("Scene:Deinit - doing.");

            bool retVal = true;
            if (m_isInit)
            {
                //deinit all gobjs
                foreach (KeyValuePair<uint, IGameObject> gobj in m_gameObjects)
                {
                    if (!gobj.Value.Deinit())
                        retVal = false;
                }

                //deinit camera and menu
                if (    (!m_camera.Deinit())    ||
                        (!m_menu.Deinit())      ||
                        (!m_fadeOverlay.Deinit())   ||
                        (!m_prompt.Deinit())    ||
                        (!m_msgBox.Deinit())    )
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
        /// Reference to this Scene's stage object.
        /// </summary>
        public Stage Stage
        {
            get { return m_stage; }
        }

        /// <summary>
        /// The scene's camera.
        /// </summary>
        public CameraGObj Camera
        {
            get { return m_camera; }
            set
            {
                if (value is CameraGObj)
                {
                    //init, load, and activate new
                    //  camera in accordance with old
                    bool retVal = true;

                    if (m_camera.IsInit)
                        retVal = value.Init();
                    if (!retVal)
                    {
                        m_debugMgr.WriteLogEntry(
                            "Scene:Camera - new cam init failed.");
                        return;
                    }

                    if (m_camera.IsLoaded)
                        retVal = value.Load();
                    if (!retVal)
                    {
                        m_debugMgr.WriteLogEntry(
                            "Scene:Camera - new cam load failed.");
                        return;
                    }

                    if (m_camera.Active)
                        value.Active = true;

                    //set
                    m_camera = value;
                }
                else
                    m_debugMgr.WriteLogEntry(
                        "Scene:Camera - value is not a CameraGObj type.");
            }
        }

        /// <summary>
        /// The clear colour of the scene.
        /// </summary>
        public Microsoft.Xna.Framework.Graphics.Color Colour
        {
            get { return m_clearColour; }
            set { m_clearColour = value; }
        }

        /// <summary>
        /// Queries whether this Scene has been initialised.
        /// </summary>
        public bool IsInit
        {
            get { return m_isInit; }
        }

        /// <summary>
        /// If true, the scene will no longer be updated (except
        /// the menu).
        /// </summary>
        public bool IsPaused
        {
            get { return m_isPaused; }
            set { m_isPaused = value; }
        }

        /// <summary>
        /// If true, the scene will not be rendered to the screen,
        /// however the message box and prompt will always be rendered.
        /// </summary>
        public bool IsVisible
        {
            get { return m_isVisible; }
            set { m_isVisible = value; }
        }

        /// <summary>
        /// Reference to this scene's menu.
        /// </summary>
        public Menu GetMenu
        {
            get { return m_menu; }
        }

        /// <summary>
        /// Reference to the collection of game objects.
        /// </summary>
        public Dictionary<uint, IGameObject> GameObjects
        {
            get { return m_gameObjects; }
        }

        #region msg_box

        /// <summary>
        /// Tests if the message box is active.
        /// </summary>
        public bool MsgBoxActive
        {
            get { return m_msgBox.Active; }
        }

        /// <summary>
        /// Reference to the message box menu.
        /// </summary>
        public Menu MsgBox
        {
            get { return m_msgBox; }
        }

        /// <summary>
        /// Reference of the message box button.
        /// </summary>
        public MenuButton GetMsgBoxButton
        {
            get { return m_msgButton; }
        }

        #endregion

        #region prompt

        /// <summary>
        /// Tests if the prompt box is active.
        /// </summary>
        public bool PromptActive
        {
            get { return m_prompt.Active; }
        }

        /// <summary>
        /// Reference to the prompt box menu.
        /// </summary>
        public Menu Prompt
        {
            get { return m_prompt; }
        }

        /// <summary>
        /// Reference to the prompt's YES button.
        /// </summary>
        public MenuButton GetPromptYESButton
        {
            get { return m_promptYesButton; }
        }

        /// <summary>
        /// Reference to the prompt's NO button.
        /// </summary>
        public MenuButton GetPromptNOButton
        {
            get { return m_promptNoButton; }
        }

        #endregion

        public QuadAsset GetFadeOverlay
        {
            get { return m_fadeOverlay; }
        }

        #endregion

        #region message_prompt_boxs

        /// <summary>
        /// Call this if you want to show a pop-up message.
        /// </summary>
        /// <param name="message"></param>
        public void ShowMessage(string message)
        {
            m_msgButton.ClearEvents();
            m_msgLabel.LabelString = message;
            m_msgButton.UIAction += DefaultUIAction;
            m_msgBox.Active = true;
        }

        /// <summary>
        /// Call this to show a prompt.
        /// </summary>
        /// <param name="query"></param>
        public void ShowPrompt(string query)
        {
            m_promptYesButton.ClearEvents();
            m_promptNoButton.ClearEvents();
            m_promptLabel.LabelString = query;
            m_promptYesButton.UIAction += DefaultUIAction;
            m_promptNoButton.UIAction += DefaultUIAction;
            m_prompt.Active = true;
        }

        #endregion

        #region scene_tree

        /// <summary>
        /// Adds a Scene object reference as a child node of this Scene.
        /// </summary>
        /// <param name="childScene">
        /// Scene to add as a child scene node.
        /// </param>
        public void AddChildScene(Scene childScene)
        {
            m_debugMgr.Assert(childScene != null, 
                "Scene:AddChildScene - param 'child' was null");
            //add to tree collection
            m_childScenes.Add(childScene.Id, childScene);

            //m_debugMgr.WriteLogEntry("Scene:AddChildScene - done.");
        }

        /// <summary>
        /// Returns a Scene reference to a child node with matching Scene ID
        /// value.
        /// </summary>
        /// <param name="childId">
        /// The child Scene ID to match.
        /// </param>
        /// <returns>
        /// Reference to the matching child Scene to ID passed. Returns
        /// 'null' if no matching child scene.
        /// </returns>
        public Scene GetChildScene(uint childId)
        {
            //find and return (if found)
            Scene childSceneRef;
            m_childScenes.TryGetValue(childId, out childSceneRef);

            //m_debugMgr.WriteLogEntry("Scene:GetChildScene - done.");
            return childSceneRef;
        }

        /// <summary>
        /// Reference to this Scene's parent Scene object.
        /// </summary>
        public Scene ParentScene
        {
            get { return m_parent; }
        }

        #endregion

        #region gobj_collection

        /// <summary>
        /// Adds a game object to the scene.
        /// </summary>
        /// <param name="gobj">
        /// Reference to the game object.
        /// </param>
        public void AddGameObj(IGameObject gobj)
        {
            m_debugMgr.Assert(gobj != null, 
                "Scene:AddGameObj - param 'gobj' was null");
            //add to collection
            m_gameObjects.Add(gobj.Id, gobj);

            //m_debugMgr.WriteLogEntry("Scene:AddGameObj - done.");
        }

        /// <summary>
        /// Returns a reference to a game object if Id match found in
        /// collection.
        /// </summary>
        /// <param name="gobjId">
        /// Game object Id to find match.
        /// </param>
        /// <returns>
        /// Reference to the game object if Id match found.
        /// </returns>
        public IGameObject GetGameObj(uint gobjId)
        {
            //find and return (if found)
            IGameObject gobjRef;
            m_gameObjects.TryGetValue(gobjId, out gobjRef);

            //m_debugMgr.WriteLogEntry("Scene:GetGameObj - done.");
            return gobjRef;
        }

        /// <summary>
        /// Removes the reference to the game object, if Id match found.
        /// Shallow remove.
        /// </summary>
        /// <param name="gobjId">
        /// Game object Id to match and remove from collection.
        /// </param>
        /// <returns>
        /// True if Id match and removed, otherwise false.
        /// </returns>
        public bool RemoveGameObj(uint gobjId)
        {
            //m_debugMgr.WriteLogEntry("Scene:RemoveGameObj - doing.");
            return m_gameObjects.Remove(gobjId);
        }

        #endregion

        #region input

        /// <summary>
        /// When called, the previous input states will be updated to latest.
        /// </summary>
        public void SetPrevInputStates()
        {
            if (m_isInit)
            {
                m_prevGPState = Microsoft.Xna.Framework.Input.GamePad.GetState(Stage.SceneMgr.Game.LeadPlayerIndex);
#if !XBOX
                m_prevKBState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
#endif
            }
        }

        /// <summary>
        /// This is where input handling must be handled.
        /// </summary>
        /// <param name="newGPState">
        /// Reference to the latest game pad state structure.
        /// </param>
        /// <param name="newKBState">
        /// Reference to the latest keyboard state structure.
        /// </param>
        public abstract void Input(Microsoft.Xna.Framework.GameTime gameTime,
                                    ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
                                    ,ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
            );

        #endregion

        #region game_loop

        /// <summary>
        /// This is the first method called by the game loop during the
        /// 'Update' stage of the cycle. Here you must define the logic
        /// for new input events and other scene logic.
        /// </summary>
        /// <remarks>
        /// Always remember that each game object of this scene will have
        /// its Update method called directly after this method. Each game
        /// object's Update method will be processed concurrently, so you
        /// should minimise the amount of game logic here if you want to
        /// maximise the performance of your game on multicored CPU
        /// systems (like the Xbox360).
        /// </remarks>
        /// <param name="gameTime">
        /// XNA time for the frame.
        /// </param>
        public abstract void Update(Microsoft.Xna.Framework.GameTime gameTime);

        /// <summary>
        /// Invokes the suspended 'synchronous' worker threads to process
        /// each game objects' Update methods.
        /// </summary>
        /// <remarks>
        /// Whilst this is happening, the main thread (which handles the
        /// Scenes) will be suspended in this method. When all the game
        /// objects have been processed by the worker threads, all the
        /// worker threads will be suspended and the main thread will be
        /// awaken in this method.
        /// </remarks>
        /// <param name="gametime">
        /// XNA game time for the frame.
        /// </param>
        public void UpdateGObjs(Microsoft.Xna.Framework.GameTime gametime)
        {
            //m_debugMgr.WriteLogEntry("Scene:UpdateGObjs - doing.");

            if (m_gameObjects.Count == 0)
                return;

            SynchroniseWithThreadPool syncThreadPool = 
                new SynchroniseWithThreadPool(m_gameObjects.Count);

            uint i = 0;
            foreach (KeyValuePair<uint, IGameObject> gobj in m_gameObjects)
            {
                //doneEvents[i] = new ManualResetEvent(false);
                //check the game object is active
                if (gobj.Value.Active)
                {
                    //set reference to the 'update completed' signal
                    gobj.Value.SetUpdate(syncThreadPool);
                    //dispatch
                    ThreadPool.QueueUserWorkItem(gobj.Value.DoUpdate, (object)gametime);
                }
                else
                    //game object isn't active so manually signal completion
                    syncThreadPool.TaskCompleted();

                //inc
                i++;
            }

            //wait until all game object updates have completed before proceding
            //  The Xbox ThreadPool affines worker threads to hardware threads 1,3,4,5.
            //  This being the main thread will contend on hardware thread 1.
            //  Also, xbox doesn't support the waithandle class (why not?), so implement
            //  my own 'WaitAll' functionality.
            syncThreadPool.stallCondition.WaitOne();
        }

        /// <summary>
        /// This is the last method called in the Update stage of the game
        /// loop cycle. It will be called directly after all the game
        /// objects have been processed (update and AI methods called).
        /// </summary>
        /// <remarks>
        /// All "last-minute" logic should be defined here; eg, game objects
        /// that aren't grouped cannot communicate directly to each other or
        /// to the scene, when this method is called you can check for and
        /// pass messages to and from game objects to the scene or other
        /// game objects safely.
        /// </remarks>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public abstract void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime);

        /// <summary>
        /// This is the main method entry for the rendering procedure of the
        /// game loop. Each game object will have its Render method invoked
        /// here.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// XNA graphics device.
        /// </param>
        public virtual void Draw(   Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            //m_debugMgr.WriteLogEntry("Scene:Render - doing.");
                graphicsDevice.Clear(m_clearColour);

                Stage.SceneMgr.Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                Stage.SceneMgr.Game.GraphicsDevice.RenderState.AlphaBlendOperation = Microsoft.Xna.Framework.Graphics.BlendFunction.Add;
                Stage.SceneMgr.Game.GraphicsDevice.RenderState.SourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceAlpha;
                Stage.SceneMgr.Game.GraphicsDevice.RenderState.DestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceAlpha;
                Stage.SceneMgr.Game.GraphicsDevice.RenderState.SeparateAlphaBlendEnabled = false;

                Stage.SceneMgr.Game.GraphicsDevice.RenderState.AlphaTestEnable = true;
                Stage.SceneMgr.Game.GraphicsDevice.RenderState.AlphaFunction = Microsoft.Xna.Framework.Graphics.CompareFunction.Greater;
                Stage.SceneMgr.Game.GraphicsDevice.RenderState.ReferenceAlpha = 0;

                foreach (KeyValuePair<uint, IGameObject> gobj in m_gameObjects)
                {
                    gobj.Value.DoDraw(gameTime, graphicsDevice, m_camera);
                }

                //draw the menu
                if (m_menu != null)
                    if (m_menu.Active)
                        m_menu.DoDraw(gameTime, graphicsDevice, m_camera);
        }

        #endregion

        #endregion
    }
}
