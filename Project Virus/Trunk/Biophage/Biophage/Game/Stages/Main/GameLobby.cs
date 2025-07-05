using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA;
using LNA.GameEngine;
using LNA.GameEngine.Core;
using LNA.GameEngine.Core.AsyncTasks;
using LNA.GameEngine.Objects;
using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.GameObjects.Assets;
using LNA.GameEngine.Objects.GameObjects.Sprites;
using LNA.GameEngine.Objects.UI;
using LNA.GameEngine.Objects.UI.Menu;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;
using LNA.GameEngine.Resources.Drawable;
using LNA.GameEngine.Resources.Playable;

namespace Biophage.Game.Stages.Main
{
    /// <summary>
    /// This scene allows players to wait for the game session to start.
    /// In the mean time, other players may join the session, only the host
    /// has active control of this scene. This scene will transfer to the loading
    /// scene once all delagations have completed.
    /// </summary>
    public class GameLobbyScn : Scene
    {
        #region fields

        protected uint m_assetCount;

        protected TextureResHandle m_isReadyTextureRes;
        protected TextureResHandle m_hasVoiceTextureRes;
        protected TextureResHandle m_isTalkingTextureRes;
        protected TextureResHandle m_voiceMutedTextureRes;
        protected SpriteFontResHandle m_fontRes;

        protected Stages.Game.SessionDetails m_sessionDetails;

        #endregion

        #region methods

        #region construction

        public GameLobbyScn(    uint id,
                                DebugManager debugMgr, ResourceManager resourceMgr,
                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                SpriteFontResHandle sceneFont,
                                Scene parentScn)
            : base( id, 
                    debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                    Microsoft.Xna.Framework.Graphics.Color.Black,
                    sceneFont,
                    parentScn.Stage, parentScn, null)
        {
            m_camera = new CameraGObj(
                uint.MaxValue, debugMgr, resourceMgr, this,
                false, graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000.0f);

            m_assetCount = 1;

            TextureAsset backgroundWall = new TextureAsset(
              m_assetCount++, Microsoft.Xna.Framework.Vector3.Zero,
              1.92f, 1.08f, "Content\\MainStage\\", "Background",
              debugMgr, resourceMgr, this, true,
              graphicsMgr);

            TextureAsset lobbyTitle = new TextureAsset(
                m_assetCount++, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage\\", "LobbyTitle",
                debugMgr, resourceMgr, this, true,
                graphicsMgr);

            m_isReadyTextureRes = new TextureResHandle(
                m_debugMgr, m_resMgr,
                "Content\\Common\\LobbyChatImages\\", "ready");
            m_hasVoiceTextureRes = new TextureResHandle(
                m_debugMgr, m_resMgr,
                "Content\\Common\\LobbyChatImages\\", "chatAble");
            m_isTalkingTextureRes = new TextureResHandle(
                m_debugMgr, m_resMgr,
                "Content\\Common\\LobbyChatImages\\", "chatTalking");
            m_voiceMutedTextureRes = new TextureResHandle(
                m_debugMgr, m_resMgr,
                "Content\\Common\\LobbyChatImages\\", "chatMute");

            m_fontRes = new SpriteFontResHandle(
                m_debugMgr, m_resMgr,
                "Content\\Fonts\\", "PromptFont");

            RoundCornerQuadAsset lobbyWnd = new RoundCornerQuadAsset(
                m_assetCount++, new Microsoft.Xna.Framework.Vector3(0f,-0.09f,0f),
                1.25f, 0.75f, 0.025f, Microsoft.Xna.Framework.Graphics.Color.DarkSlateGray,
                m_debugMgr, m_resMgr, this, true, graphicsMgr);
        }

        #endregion

        #region creation

        public static GameLobbyScn Create(  DebugManager debugMgr, ResourceManager resourceMgr,
                                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                            Scene parent)
        {
            GameLobbyScn scn = new GameLobbyScn(
                GlobalConstants.LOBBY_SCN_ID, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr,resourceMgr,"Content\\Fonts\\","PromptFont"),
                parent);

            return scn;
        }

        #endregion

        #region loading

        public override bool Load()
        {
            bool retVal = base.Load();

            if ((!m_isReadyTextureRes.Load()) ||
                    (!m_hasVoiceTextureRes.Load()) ||
                    (!m_isTalkingTextureRes.Load()) ||
                    (!m_voiceMutedTextureRes.Load()) ||
                    (!m_fontRes.Load()))
                retVal = false;

            if (retVal)
                m_isLoaded = true;
            else
                m_isLoaded = false;

            return retVal;
        }

        public override bool Unload()
        {
            bool retVal = base.Unload();

            if (    (!m_isReadyTextureRes.Unload()) ||
                    (!m_hasVoiceTextureRes.Unload()) ||
                    (!m_isTalkingTextureRes.Unload()) ||
                    (!m_voiceMutedTextureRes.Unload()) ||
                    (!m_fontRes.Unload()))
                retVal = false;

            if (retVal)
                m_isLoaded = false;
            else
                m_isLoaded = true;

            return retVal;
        }

        #endregion

        #region game loop

        public override void Input(Microsoft.Xna.Framework.GameTime gameTime,
                                    ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
                                    , ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
                                    )
        {
            GameplaySettingsScn gpScn = (GameplaySettingsScn)Stage.GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);
            m_sessionDetails = gpScn.sessionDetails;

            m_sessionDetails.gamerMe = null;
            m_debugMgr.Assert(m_sessionDetails.netSessionComponent.GetNetworkSession.LocalGamers.Count == 1,
                "GameLobby:Input - more or less than one local player.");
            foreach (Microsoft.Xna.Framework.Net.LocalNetworkGamer gmr in m_sessionDetails.netSessionComponent.GetNetworkSession.LocalGamers)
            {
                m_sessionDetails.gamerMe = gmr;
            }

            if (    (newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) &&
                    m_prevGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.A))
#if !XBOX
                || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter) &&
                    m_prevKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Enter))
#endif
                )
            {
                if (!m_sessionDetails.gamerMe.IsReady)
                    m_sessionDetails.gamerMe.IsReady = true;
                else if (m_sessionDetails.gamerMe.IsHost)
                {
                    // The host has an option to force starting the game, even if not
                    // everyone has marked themselves ready. If they press select twice
                    // in a row, the first time marks the host ready, then the second
                    // time we ask if they want to force start.
                    if (m_sessionDetails.netSessionComponent.GetNetworkSession.AllGamers.Count >= 2)
                    {
                        ShowPrompt("Force start?");
                        GetPromptYESButton.UIAction += ConfirmForceStart;
                    }
                }
            }
            else if (   (newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                        m_prevGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B))
#if !XBOX
                    || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) &&
                        m_prevKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Escape))
#endif
                    )
            {
                if (m_sessionDetails.gamerMe.IsReady)
                    m_sessionDetails.gamerMe.IsReady = false;
                else if (m_sessionDetails.gamerMe.IsHost)
                    //go back to game play settings - quickly now
                    Stage.SetCurrentScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);
                else
                    m_sessionDetails.netSessionComponent.RequestLeaveSession();
            }
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            GameplaySettingsScn gpScn = (GameplaySettingsScn)Stage.GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);
            m_sessionDetails = gpScn.sessionDetails;

            //Ready to load game?
            if (m_sessionDetails.netSessionComponent.GetNetworkSession.SessionState == 
                Microsoft.Xna.Framework.Net.NetworkSessionState.Playing)
            {
                //make sure we get and wait for start data before loading begins
                if (!m_sessionDetails.isHost)
                    ClientGetStartInfo();

                // Check if we should leave the lobby and begin gameplay.
                // We pass null as the controlling player, because the networked
                // gameplay screen accepts input from any local players who
                // are in the session, not just a single controlling player.
                //start the game
                Stage.SetCurrentScene(GlobalConstants.LOADING_SCN_ID);

                LoadingScn lScn = (LoadingScn)Stage.CurrentScene;
                lScn.SceneLoadDetails(m_sessionDetails);
            }
            else if (m_sessionDetails.netSessionComponent.GetNetworkSession.IsHost && 
                m_sessionDetails.netSessionComponent.GetNetworkSession.IsEveryoneReady)
            {
                // The host checks whether everyone has marked themselves
                // as ready, and starts the game in response.

                //check there is a minimal of atleast two
                if (m_sessionDetails.netSessionComponent.GetNetworkSession.AllGamers.Count >= 2)
                {
                    HostSendStartInfo();
                    m_sessionDetails.netSessionComponent.GetNetworkSession.StartGame();
                }
            }
        }

        public void HostSendStartInfo()
        {
            m_debugMgr.Assert(m_sessionDetails.isHost, 
                "GameLobby:SendStartInfo - only host can dispatch game start data.");

            //write data packet - ID, Type, NumBots, GameLevelID-offset, typesettings (if type accepts)
            m_sessionDetails.netSessionComponent.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_NEWGAME_ID);
            m_sessionDetails.netSessionComponent.packetWriter.Write((byte)m_sessionDetails.type);
            m_sessionDetails.netSessionComponent.packetWriter.Write(m_sessionDetails.numBots);
            m_sessionDetails.netSessionComponent.packetWriter.Write((byte)m_sessionDetails.gameLevel);
            if ((m_sessionDetails.type == GlobalConstants.GameplayType.TIMED_MATCH)||
                (m_sessionDetails.type == GlobalConstants.GameplayType.ILLNESS))
                m_sessionDetails.netSessionComponent.packetWriter.Write(m_sessionDetails.typeSettings);

            //send packet
            m_sessionDetails.gamerMe.SendData(m_sessionDetails.netSessionComponent.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.ReliableInOrder);
        }

        public void ClientGetStartInfo()
        {
            m_debugMgr.Assert(!m_sessionDetails.isHost,
                "GameLobby:ClientGetStartInfo - only clients should respond to start data.");

            bool notSet = true;
            Microsoft.Xna.Framework.Net.NetworkGamer sender;
            while (notSet)
            {
                if (m_sessionDetails.gamerMe.IsDataAvailable)
                {
                    m_sessionDetails.gamerMe.ReceiveData(m_sessionDetails.netSessionComponent.packetReader, out sender);
                    //hope we got the correct packet data
                    if (m_sessionDetails.netSessionComponent.packetReader.ReadByte() != (byte)GlobalConstants.NETPACKET_IDS.NETSERVER_NEWGAME_ID)
                    {
                        m_debugMgr.Assert(false, "GameLobby:ClientGetStartInfo - recieved incorrect packet somehow.");
                    }
                    else
                    {
                        m_sessionDetails.type = (GlobalConstants.GameplayType)m_sessionDetails.netSessionComponent.packetReader.ReadByte();
                        m_sessionDetails.numBots = m_sessionDetails.netSessionComponent.packetReader.ReadByte();
                        m_sessionDetails.gameLevel = (GlobalConstants.GameLevel)m_sessionDetails.netSessionComponent.packetReader.ReadByte();
                        if ((m_sessionDetails.type == GlobalConstants.GameplayType.TIMED_MATCH) ||
                            (m_sessionDetails.type == GlobalConstants.GameplayType.ILLNESS))
                            m_sessionDetails.typeSettings = m_sessionDetails.netSessionComponent.packetReader.ReadByte();
                    }
                    notSet = false;
                }
            }
        }

        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        void ConfirmForceStart(object sender, EventArgs e)
        {
            if (m_sessionDetails.netSessionComponent.GetNetworkSession.SessionState == 
                Microsoft.Xna.Framework.Net.NetworkSessionState.Lobby)
            {
                HostSendStartInfo();
                m_sessionDetails.netSessionComponent.GetNetworkSession.StartGame();
            }
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            base.Draw(gameTime, graphicsDevice);


            Stage.SceneMgr.Game.spriteBatch.Begin();

            // Draw all the gamers in the session.
            Microsoft.Xna.Framework.Vector2 position = new Microsoft.Xna.Framework.Vector2(110, 170);
            int gamerCount = 0;
            GameplaySettingsScn gpScn = (GameplaySettingsScn)Stage.GetChildScene(GlobalConstants.GAMEPLAY_SETTINGS_SCN_ID);
            m_sessionDetails = gpScn.sessionDetails;
            foreach (Microsoft.Xna.Framework.Net.NetworkGamer gamer in m_sessionDetails.netSessionComponent.GetNetworkSession.AllGamers)
            {
                DrawGamer(gamer, position);

                // Advance to the next screen position, wrapping into two
                // columns if there are more than 8 gamers in the session.
                if (++gamerCount == 6)
                {
                    position.X += 433;
                    position.Y = 170;
                }
                else
                    position.Y += ((Microsoft.Xna.Framework.Graphics.SpriteFont)m_fontRes.GetResource).LineSpacing+20;
            }

            Stage.SceneMgr.Game.spriteBatch.End();
        }

        /// <summary>
        /// Helper draws the gamertag and status icons for a single NetworkGamer.
        /// </summary>
        void DrawGamer(Microsoft.Xna.Framework.Net.NetworkGamer gamer, Microsoft.Xna.Framework.Vector2 position)
        {
            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch = Stage.SceneMgr.Game.spriteBatch;

            Microsoft.Xna.Framework.Vector2 iconWidth = new Microsoft.Xna.Framework.Vector2(70, 0);
            Microsoft.Xna.Framework.Vector2 iconOffset = new Microsoft.Xna.Framework.Vector2(0, 0);

            Microsoft.Xna.Framework.Vector2 iconPosition = position + iconOffset;

            // Draw the "is ready" icon.
            if (gamer.IsReady)
            {
                spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_isReadyTextureRes.GetResource,
                    iconPosition, Microsoft.Xna.Framework.Graphics.Color.Lime);
            }

            iconPosition += iconWidth;

            // Draw the "is muted", "is talking", or "has voice" icon.
            if (gamer.IsMutedByLocalUser)
            {
                spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_voiceMutedTextureRes.GetResource,
                    iconPosition, Microsoft.Xna.Framework.Graphics.Color.Red);
            }
            else if (gamer.IsTalking)
            {
                spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_isTalkingTextureRes.GetResource,
                    iconPosition, Microsoft.Xna.Framework.Graphics.Color.Yellow);
            }
            else if (gamer.HasVoice)
            {
                spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hasVoiceTextureRes.GetResource,
                    iconPosition, Microsoft.Xna.Framework.Graphics.Color.White);
            }

            // Draw the gamertag, normally in white, but yellow for local players.
            string text = gamer.Gamertag;

            if (gamer.IsHost)
                text += " (HOST)";

            spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_fontRes.GetResource,
                text, position + iconWidth * 2f,
                Stages.Game.BiophageGameBaseScn.GamerColour(gamer.Id));
        }
        
        #endregion

        #endregion
    }
}
