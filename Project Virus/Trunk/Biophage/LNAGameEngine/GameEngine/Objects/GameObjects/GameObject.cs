/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;

namespace LNA.GameEngine.Objects.GameObjects
{
    /// <summary>
    /// Represents the base for all game objects.
    /// </summary>
    public abstract class GameObject : IGameObject
    {
        #region fields

        protected uint m_id;
        protected DebugManager m_debugMgr;

        protected bool m_isInit;
        protected bool m_isLoaded;
        protected bool m_isVisible;
        protected bool m_isActive;

        private object m_gameObjLock;
        private SynchroniseWithThreadPool m_syncThreadPool;

        // Summary:
        //  The order in which to draw this object relative to other
        //  objects. Objects with a lower value are drawn first.
        //
        // Returns:
        //  Order in which to draw this object relative to other
        //  objects.
        protected int m_drawOrder;

        //Child game objects for implementing groups. The parent game
        //  object is responsible for the calls to Update, Animate, and
        //  Draw.
        protected Dictionary<uint, GameObject> m_childGameObjs;

        #endregion

        #region event_handlers

        ///// <summary>
        ///// Fire event delegate when draw order is changed.
        ///// </summary>
        //public event EventHandler DrawOrderChanged;

        ///// <summary>
        ///// Fire event delegate when visibility is changed.
        ///// </summary>
        //public event EventHandler VisibleChanged;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Game object Id.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the game object scene.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        public GameObject(  uint id, 
                            DebugManager debugMgr, ResourceManager resourceMgr,  
                            Scene scene, bool addToScene)
        {
            //set fields
            m_id = id;
            m_debugMgr = debugMgr;
            m_isInit = false;
            m_isLoaded = false;
            m_isVisible = true;
            m_isActive = true;
            m_drawOrder = 0;
            m_childGameObjs = new Dictionary<uint, GameObject>();
            m_gameObjLock = new object();

            //misc
            if (addToScene)
            {
                m_debugMgr.Assert(scene != null,
                    "GameObject:Constructor - 'scene' is null.");
                scene.AddGameObj(this);
            }

            //logs
            //string sLog = "GameObject:Constructor - done for game obj id=";
            //sLog += m_id; sLog +=".";
            //m_debugMgr.WriteLogEntry(sLog);
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
        /// True if the game object can be drawn.
        /// </summary>
        public virtual bool Visible
        {
            get { return m_isVisible; }
            set { m_isVisible = value; }
        }

        /// <summary>
        /// True if the game object is active (updateable).
        /// </summary>
        public virtual bool Active
        {
            get { return m_isActive; }
            set { m_isActive = value; }
        }

        /// <summary>
        /// The draw order of the game object.
        /// </summary>
        /// <remarks>
        /// Objects with a lower value are drawn first.
        /// </remarks>
        public virtual int DrawOrder
        {
            get { return m_drawOrder; }
            set { m_drawOrder = value; }
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Custom initialisation method. Will be called before load by the
        /// scene's 'Init' method.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public abstract bool Init();

        /// <summary>
        /// Queries if the game object has been initialised.
        /// </summary>
        public bool IsInit
        {
            get { return m_isInit; }
        }

        /// <summary>
        /// Re-initialises the object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public abstract bool Reinit();

        #region loading

        /// <summary>
        /// Loads the game object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public abstract bool Load();

        /// <summary>
        /// Queries if the game object has been loaded.
        /// </summary>
        public bool IsLoaded
        {
            get { return m_isLoaded; }
        }

        /// <summary>
        /// Unloads the object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public abstract bool Unload();

        #endregion

        /// <summary>
        /// Deinitialises the game object. Called by the scene's 'Deinit'
        /// method after unloading.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public abstract bool Deinit();

        #endregion

        #region child_tree

        /// <summary>
        /// Adds a child game object.
        /// </summary>
        /// <param name="gameObj">
        /// Reference to the game object to add as a child.
        /// </param>
        public void AddChildObj(GameObject gameObj)
        {
            //m_debugMgr.WriteLogEntry("GameObject:AddChildObj - doing.");

            m_debugMgr.Assert(gameObj != null, 
                "GameObject:AddChildObj - 'gameObj' is null.");
            m_childGameObjs.Add(gameObj.Id, gameObj);
        }

        /// <summary>
        /// Gets a child game object.
        /// </summary>
        /// <param name="gameObjId">
        /// Id of the child game object to return reference to.
        /// </param>
        /// <returns>
        /// Reference to the child game object. Returns null if no matching
        /// child game object.
        /// </returns>
        public virtual GameObject GetChildObj(uint gameObjId)
        {
            //m_debugMgr.WriteLogEntry("GameObject:GetChildObj - doing.");

            GameObject gObj;
            m_childGameObjs.TryGetValue(gameObjId, out gObj);
            return gObj;
        }

        /// <summary>
        /// Removes a child game object.
        /// </summary>
        /// <param name="gameObjId">
        /// Id of the child game object to remove.
        /// </param>
        /// <returns>
        /// True if child object removed, otherwise false.
        /// </returns>
        public bool RemoveChildObj(uint gameObjId)
        {
            //m_debugMgr.WriteLogEntry("GameObject:RemoveChildObj - doing.");

            bool retVal = false;
            if (m_childGameObjs.ContainsKey(gameObjId))
            {
                retVal = m_childGameObjs.Remove(gameObjId);
            }
            return retVal;
        }

        /// <summary>
        /// Reference to the game object's child game objects.
        /// </summary>
        Dictionary<uint, GameObject> ChildObjCollection
        {
            get { return m_childGameObjs; }
        }

        #endregion

        #region game_loop

        /// <summary>
        /// Must be called directly before 'DoUpdate' to assign the thread
        /// completed flag.
        /// </summary>
        /// <param name="syncThreadPool">
        /// Reference to the flag to signal once update is done.
        /// </param>
        public void SetUpdate(SynchroniseWithThreadPool syncThreadPool)
        {
            m_syncThreadPool = syncThreadPool;
        }

        /// <summary>
        /// Worker thread entry function. It makes sure that the 'Update'
        /// and 'Animate' methods are called in the correct order and that
        /// only one thread can do an update at a time (enforced by a lock).
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public void DoUpdate(object gameTime)
        {
            //call order
            lock (m_gameObjLock)
            {
                //m_debugMgr.WriteLogEntry("GameObject:DoUpdate - doing.");
                if (m_isActive)
                {
                    Update((Microsoft.Xna.Framework.GameTime)gameTime);
                    Animate((Microsoft.Xna.Framework.GameTime)gameTime);
                }
            }

            //signal complete
            //m_syncThreadPool.TaskComplete();
            m_syncThreadPool.TaskCompleted();
//#if XBOX

//#else
//            m_doneEvent.Set();
//#endif
        }

        /// <summary>
        /// Render entry function. Makes sure draw cannot be processed until
        /// update methods have completed.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// XNA graphics device.
        /// </param>
        /// <param name="camera">
        /// Camera for the scene, provides the projection & view matricies.
        /// </param>
        public void DoDraw( Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                                    CameraGObj camera)
        {
            lock (m_gameObjLock)
            {
                //m_debugMgr.WriteLogEntry("GameObject:DoDraw - doing.");
                if (m_isVisible)
                    Draw(gameTime, graphicsDevice, camera);
            }
        }

        /// <summary>
        /// Custom update routine for this game object.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public abstract void Update(Microsoft.Xna.Framework.GameTime gameTime);

        /// <summary>
        /// Custom animation routine for this game object. This will be
        /// called directly after the 'Update' method.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public abstract void Animate(Microsoft.Xna.Framework.GameTime gameTime);

        /// <summary>
        /// Custom draw routine for this game object.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// XNA graphics device.
        /// </param>
        /// <param name="camera">
        /// Camera for the scene, provides the projection & view matricies.
        /// </param>
        public abstract void Draw(  Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                                    CameraGObj camera);

        #endregion

        #endregion
    }
}
