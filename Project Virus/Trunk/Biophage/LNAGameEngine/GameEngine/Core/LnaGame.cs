/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace LNA.GameEngine.Core
{
    /// <summary>
    /// Game engine library core.
    /// </summary>
    public abstract class LnaGame : Game
    {
        #region fields

        //--- XNA ---//
        protected GraphicsDeviceManager         m_graphicsMgr;
        public SpriteBatch                      spriteBatch;

        //--- LNA ---//
        protected DebugManager                  m_debugManager;
        protected Objects.Scenes.SceneManager   m_sceneManager;
        protected Resources.ResourceManager     m_resourceManager;

        protected AsyncTasks.AsyncTaskManager   m_asyncTaskManager;

        protected PlayerIndex m_leadPlayerIndex;

        #endregion

        #region methods

        #region api

        /// <summary>
        /// Override this method to add all custom game scenes, stages, and
        /// objects to the game engine.
        /// </summary>
        /// <param name="numCpus">
        /// Number of hardware threads the system provides (usually the number
        /// of processor cores).
        /// </param>
        public abstract void GameMain(int numCpus);

        #endregion

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LnaGame()
        {
            m_debugManager = new DebugManager();

            //--- XNA default ---//
            m_graphicsMgr = new GraphicsDeviceManager(this);
            m_graphicsMgr.PreparingDeviceSettings += m_graphicsMgr_PreparingDeviceSettings;
            m_graphicsMgr.ApplyChanges();

            Content.RootDirectory = "Content";

            //--- LNA ---//
            m_asyncTaskManager = new AsyncTasks.AsyncTaskManager(m_debugManager);
            m_sceneManager = new Objects.Scenes.SceneManager(m_debugManager, m_asyncTaskManager, this);
            m_resourceManager = new Resources.ResourceManager(m_debugManager, m_graphicsMgr, Content.ServiceProvider);

            //stops XNA from deciding when display should be called
            IsFixedTimeStep = false;

            m_leadPlayerIndex = PlayerIndex.One; //one is the default

            //network
            Components.Add(new GamerServicesComponent(this));
        }

        /// <summary>
        /// Adapts resolution to output display.
        /// </summary>
        /// <remarks>
        /// XBOX will scale resolution on the fly to the best output.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_graphicsMgr_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // Xbox 360 and most PCs support FourSamples/0 
            // (4x) and TwoSamples/0 (2x) antialiasing.
            PresentationParameters pp =
                e.GraphicsDeviceInformation.PresentationParameters;
            //pp.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            if (e.GraphicsDeviceInformation.Adapter.IsWideScreen)
            {
                m_debugManager.WriteLogEntry("Widescreen detected");
                m_graphicsMgr.PreferredBackBufferWidth = 1280;
                m_graphicsMgr.PreferredBackBufferHeight = 720;
            }
            else
            {
                m_debugManager.WriteLogEntry("4/3 display ratio detected");
                m_graphicsMgr.PreferredBackBufferWidth = 1024;
                m_graphicsMgr.PreferredBackBufferHeight = 768;
            }

            InitDisplay();
        }

        protected abstract void InitDisplay();

        #endregion

        #region field_accessors

        /// <summary>
        /// Reference to the debug manager.
        /// </summary>
        public DebugManager DebugMgr
        {
            get { return m_debugManager; }
        }

        /// <summary>
        /// Reference to the scene manager.
        /// </summary>
        public Objects.Scenes.SceneManager SceneMgr
        {
            get { return m_sceneManager; }
        }

        /// <summary>
        /// Reference to the resource manager.
        /// </summary>
        public Resources.ResourceManager ResourceMgr
        {
            get { return m_resourceManager; }
        }

        /// <summary>
        /// Reference to the asynchronous task manager.
        /// </summary>
        public AsyncTasks.AsyncTaskManager AsyncTaskMgr
        {
            get { return m_asyncTaskManager; }
        }

        /// <summary>
        /// Reference to the XNA graphics device manager.
        /// </summary>
        public Microsoft.Xna.Framework.GraphicsDeviceManager GraphicsMgr
        {
            get { return m_graphicsMgr; }
        }

        /// <summary>
        /// Designates the 'leader' player index, which is an asignment to
        /// the game pad that controls the game menu system.
        /// </summary>
        public PlayerIndex LeadPlayerIndex
        {
            get { return m_leadPlayerIndex; }
            set { m_leadPlayerIndex = value; }
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Allows the game to perform any initialization it needs to before
        /// starting to run. This is where it can query for any required
        /// services and load any non-graphic related content.  
        /// Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        sealed protected override void Initialize()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //--- LNA ---//
            //first lower the number of threads in thread pool to  numCPUs
            //  This decreases switching overhead due to the magnitude of game
            //  objects to be processed.
#if XBOX
            //the xbox only provides 4 hardware threads and not 6 (the number
            //  prossesor count would return. So adjust manually.
            ThreadPool.SetMaxThreads(4, 4);
#else
            ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);
#endif

            //invoke API method
            GameMain(Environment.ProcessorCount);

            m_debugManager.WriteLogEntry("++++ Init ++++");
            //check there is a stage set
            m_debugManager.Assert(m_sceneManager.CurrentStage != null, 
                "LnaCore:Initialize - no stage set.");
            m_debugManager.Assert(m_sceneManager.CurrentStage.CurrentScene != null,
                "LnaCore:Initialize - no scene set to first stage.");

            //invoke first initialise
            m_debugManager.Assert(m_sceneManager.CurrentStage.Init(),
                "LnaCore:Initialize - first init failed.");

            m_debugManager.WriteLogEntry("++++ //// ++++");
            //--- XNA ---//
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to
        /// load all of your content.
        /// </summary>
        sealed protected override void LoadContent()
        {
            //--- LNA ---//
            m_debugManager.WriteLogEntry("++++ Load ++++");
            m_debugManager.Assert(m_sceneManager.CurrentStage.Load(),
                "LnaCore:LoadContent - first load failed.");
            m_debugManager.WriteLogEntry("++++ //// ++++");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to
        /// unload all content.
        /// </summary>
        sealed protected override void UnloadContent()
        {
            //--- LNA ---//
            m_debugManager.WriteLogEntry("++++ Unload ++++");

            m_sceneManager.CurrentStage.Unload();
            m_resourceManager.ResourceLoader.Unload();

            //deinit here - XNA.Game does not provide one directly
            m_debugManager.Assert(m_sceneManager.CurrentStage.Deinit(),
                "LnaCore:UnloadContent - deinit scene manager failed.");

            m_debugManager.WriteLogEntry("++++ ////// ++++");
        }

        #endregion

        #region game_loop

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values.
        /// </param>
        sealed protected override void Update(GameTime gameTime)
        {
            //--- LNA ---//
            //m_debugManager.WriteLogEntry("---- UPDATE ----");

            m_sceneManager.CurrentStage.Update(gameTime);
            if (m_sceneManager.ContinueLoop == false)
                Exit();

            //m_debugManager.WriteLogEntry("---- ////// ----");
            //--- XNA ---//
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values.
        /// </param>
        sealed protected override void Draw(GameTime gameTime)
        {
            //--- LNA ---//
            //m_debugManager.WriteLogEntry("---- RENDER ----");
            m_sceneManager.CurrentStage.Draw(gameTime, GraphicsDevice);
            //m_debugManager.WriteLogEntry("---- ////// ----");
            //--- XNA ---//
            base.Draw(gameTime);
        }

        #endregion

        #endregion
    }
}
