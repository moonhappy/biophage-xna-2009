/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;

using LNA.GameEngine;
using LNA.GameEngine.Core;
using LNA.GameEngine.Objects;
using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Objects.UI.Menu;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;

namespace LNA.GameEngine.Core
{
    /// <summary>
    /// This class has been adapted from the XNA tutorial 'NetworkStateManagement'.
    /// </summary>
    public class LnaNetworkSessionComponent : GameComponent
    {
        #region constants

        public const uint PROMPT_MENU_ID = 100;

        public const uint YESNO_WND_ID = 10;
        public const uint YESNO_LABEL_ID = 1;
        public const uint YESNO_BUT_YES_ID = 2;
        public const uint YESNO_BUT_NO_ID = 3;

        public const uint INFO_WND_ID = 20;
        public const uint INFO_LABEL_ID = 1;
        public const uint INFO_BUT_OK_ID = 2;

        #endregion

        #region fields

        protected bool isCreated = false;

        protected DebugManager m_debugMgr;
        protected ResourceManager m_resourceMgr;
        protected SceneManager m_sceneMgr;

        protected NetworkSession m_networkSession;

        protected bool m_notifyWhenPlayersJoinOrLeave;

        protected string sessionEndMessage = null;

        public PacketWriter packetWriter = new PacketWriter();
        public PacketReader packetReader = new PacketReader();

        #endregion

        #region events

        public event EventHandler LeavingSession;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Called, but not nessarily used unless created.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="sceneMgr">
        /// Reference to the scene manager.
        /// </param>
        /// <param name="game">
        /// Reference to the LnaGame.
        /// </param>
        public LnaNetworkSessionComponent(DebugManager debugMgr, ResourceManager resourceMgr, SceneManager sceneMgr,
                                            LnaGame game, SpriteFontResHandle promptFont)
            : base((Game)game)
        {
            //set fields
            m_debugMgr = debugMgr;
            m_resourceMgr = resourceMgr;
            m_sceneMgr = sceneMgr;
            m_networkSession = null;

            //asserts
            m_debugMgr.Assert(m_resourceMgr != null,
                "LnaNetworkSessionComponent:Constructor - resource manager is null.");
            m_debugMgr.Assert(m_sceneMgr != null,
                "LnaNetworkSessionComponent:Constructor - scene manager is null.");

            //log
            m_debugMgr.WriteLogEntry("LnaNetworkSessionComponent:Constructor - done.");
        }

        #endregion

        #region creation

        public void Create(NetworkSession networkSession)
        {
            //set
            m_networkSession = networkSession;

            //assert
            m_debugMgr.Assert(m_networkSession != null,
                "LnaNetworkSessionComponent:Create - network session param is null.");

            //setup
            Game.Services.AddService(typeof(NetworkSession), m_networkSession);

            //hook-up event handlers
            m_networkSession.GamerJoined += GamerJoined;
            m_networkSession.GamerLeft += GamerLeft;
            m_networkSession.SessionEnded += SessionEnded;

            isCreated = true;
            m_notifyWhenPlayersJoinOrLeave = true;
        }

        #endregion

        #region field_accessors

        public NetworkSession GetNetworkSession
        {
            get { return m_networkSession; }
        }

        public bool NotifySessionEvents
        {
            get { return m_notifyWhenPlayersJoinOrLeave; }
            set { m_notifyWhenPlayersJoinOrLeave = value; }
        }

        #endregion

        #region event_handlers

        /// <summary>
        /// Event handler called when a gamer joins the session.
        /// Displays a notification message.
        /// </summary>
        void GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            if (m_notifyWhenPlayersJoinOrLeave)
            {
                m_sceneMgr.CurrentStage.CurrentScene
                    .ShowMessage("Gamer '" + e.Gamer.Gamertag + "' has joined");
            }
        }

        void SessionEnded(object sender, NetworkSessionEndedEventArgs e)
        {
            switch (e.EndReason)
            {
                case NetworkSessionEndReason.ClientSignedOut:
                    sessionEndMessage = null;
                    break;

                case NetworkSessionEndReason.HostEndedSession:
                    sessionEndMessage = "Host ended session";
                    break;

                case NetworkSessionEndReason.RemovedByHost:
                    sessionEndMessage = "Host removed you";
                    break;

                case NetworkSessionEndReason.Disconnected:
                default:
                    sessionEndMessage = "Network disconnected";
                    break;
            }

            m_notifyWhenPlayersJoinOrLeave = false;
        }

        void GamerLeft(object sender, GamerLeftEventArgs e)
        {
            if (m_notifyWhenPlayersJoinOrLeave)
            {
                m_sceneMgr.CurrentStage.CurrentScene
                    .ShowMessage("Gamer '" + e.Gamer.Gamertag + "' has left");
            }
        }

        void LeaveSessionYesAction(object sender, EventArgs e)
        {
            LeaveSession();
        }

        #endregion

        #region initialisation

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Remove the NetworkSessionComponent.
                Game.Components.Remove(this);

                // Remove the NetworkSession service.
                Game.Services.RemoveService(typeof(NetworkSession));

                // Dispose the NetworkSession.
                if (m_networkSession != null)
                {
                    m_networkSession.Dispose();
                    m_networkSession = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region game_loop

        public override void Update(GameTime gameTime)
        {
            if (m_networkSession == null)
                return;

            try
            {
                m_networkSession.Update();

                // Has the session ended?
                if (m_networkSession.SessionState == NetworkSessionState.Ended)
                {
                    sessionEndMessage = "Host Ended Session";
                    LeaveSession();
                }
            }
            catch (Exception exception)
            {
                // Handle any errors from the network session update.
                m_debugMgr.WriteLogEntry(
                    "LnaNetworkSessionComponent:Update - NetworkSession.Update threw " + exception);

                sessionEndMessage = "Network disconnected";

                LeaveSession();
            }
        }

        #endregion

        #region session

        /// <summary>
        /// Checks whether the specified session type is online.
        /// Online sessions cannot be used by local profiles, or if
        /// parental controls are enabled, or when running in trial mode.
        /// </summary>
        /// <remarks>
        /// Straight copy from tutorial.
        /// </remarks>
        public static bool IsOnlineSessionType(NetworkSessionType sessionType)
        {
            switch (sessionType)
            {
                case NetworkSessionType.Local:
                case NetworkSessionType.SystemLink:
                    return false;

                case NetworkSessionType.PlayerMatch:
                case NetworkSessionType.Ranked:
                    return true;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Public method called when the user wants to leave the network session.
        /// Displays a confirmation message box, then disposes the session, removes
        /// the NetworkSessionComponent, and returns them to the main menu screen.
        /// </summary>
        public void RequestLeaveSession()
        {
            string label;
            if (GetNetworkSession.IsHost)
                label = "End session?";
            else
                label = "Leave session?";

            m_sceneMgr.CurrentStage.CurrentScene.ShowPrompt(label);
            m_sceneMgr.CurrentStage.CurrentScene.GetPromptYESButton.UIAction += LeaveSessionYesAction;
        }

        /// <summary>
        /// Internal method for leaving the network session. This disposes the 
        /// session, removes the NetworkSessionComponent, and returns the user
        /// to the main menu screen.
        /// </summary>
        void LeaveSession()
        {
            // Destroy this NetworkSessionComponent.
            Dispose();

            // If we have a sessionEndMessage string explaining why the session has
            // ended (maybe this was a network disconnect, or perhaps the host kicked
            // us out?) create a message box to display this reason to the user.

            if (!string.IsNullOrEmpty(sessionEndMessage))
            {
                m_sceneMgr.CurrentStage.CurrentScene.ShowMessage(sessionEndMessage);
                m_sceneMgr.CurrentStage.CurrentScene.GetMsgBoxButton.UIAction += GetMsgBoxButton_UIAction;
                m_sceneMgr.CurrentStage.CurrentScene.IsPaused = true;
                m_sceneMgr.CurrentStage.CurrentScene.IsVisible = false;
            }
            else
            {
                //let programmer define what they want to do when session ends
                if (LeavingSession != null)
                    LeavingSession(this, null);
            }
        }

        void GetMsgBoxButton_UIAction(object sender, EventArgs e)
        {
            //let programmer define what they want to do when session ends
            m_sceneMgr.CurrentStage.CurrentScene.IsPaused = false;
            m_sceneMgr.CurrentStage.CurrentScene.IsVisible = true;
            if (LeavingSession != null)
                LeavingSession(this, null);
        }

        /// <summary>
        /// Searches through the Game.Components collection to
        /// find the NetworkSessionComponent (if any exists).
        /// </summary>
        public static LnaNetworkSessionComponent FindSessionComponent(LnaGame game)
        {
            return game.Components.OfType<LnaNetworkSessionComponent>().FirstOrDefault();
        }

        #endregion

        #endregion
    }
}
