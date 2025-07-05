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
using System.Globalization;

namespace Biophage.Game.Stages.Game.Common
{
    public class FinishedRanksScreen : GameObject
    {
        #region fields

        Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;
        Microsoft.Xna.Framework.Graphics.SpriteBatch m_spriteBatch;

        BiophageGameBaseScn m_biophageScn;

        public Stack<byte> finishVirusRanks = new Stack<byte>();
        public bool isGameOver = false;

        uint m_assetCount = 1;

        TextureResHandle m_hasVoiceTextureRes;
        TextureResHandle m_isTalkingTextureRes;
        TextureResHandle m_voiceMutedTextureRes;
        SpriteFontResHandle m_fontRes;

        RoundCornerQuadAsset m_finishWnd;

        CameraGObj m_camera;

        #endregion

        #region methods

        #region construction

        public FinishedRanksScreen( DebugManager dbgMgr, ResourceManager resMgr,
                                    BiophageGameBaseScn scn)
            : base(uint.MaxValue, dbgMgr, resMgr, (Scene)scn, false)
        {
            m_graphicsMgr = scn.Stage.SceneMgr.Game.GraphicsMgr;
            m_spriteBatch = scn.Stage.SceneMgr.Game.spriteBatch;
            m_biophageScn = scn;

            m_camera = new CameraGObj(
                uint.MaxValue, dbgMgr, resMgr, null,
                false, m_graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000.0f);

            Active = false;
            Visible = true;

            m_camera = new CameraGObj(
                uint.MaxValue, dbgMgr, resMgr, null,
                false, m_graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000.0f);

            m_hasVoiceTextureRes = new TextureResHandle(
                m_debugMgr, resMgr,
                "Content\\Common\\LobbyChatImages\\", "chatAble");
            m_isTalkingTextureRes = new TextureResHandle(
                m_debugMgr, resMgr,
                "Content\\Common\\LobbyChatImages\\", "chatTalking");
            m_voiceMutedTextureRes = new TextureResHandle(
                m_debugMgr, resMgr,
                "Content\\Common\\LobbyChatImages\\", "chatMute");

            m_fontRes = new SpriteFontResHandle(
                m_debugMgr, resMgr,
                "Content\\Fonts\\", "PromptFont");

            m_finishWnd = new RoundCornerQuadAsset(
                m_assetCount++, new Microsoft.Xna.Framework.Vector3(0f, -0.09f, 0f),
                1.25f, 0.75f, 0.025f, Microsoft.Xna.Framework.Graphics.Color.DarkGray,
                dbgMgr, resMgr, scn, false, m_graphicsMgr);

            Init();
            Load();
        }

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                if ((!m_finishWnd.Init()) ||
                    (!m_camera.Init()))
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }
            return retVal;
        }

        public override bool Reinit()
        {
            bool retVal = true;

                if ((!m_finishWnd.Reinit())||
                    (!m_camera.Reinit()))
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;

            return retVal;
        }

        #region loading

        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if (    (!m_hasVoiceTextureRes.Load()) ||
                        (!m_isTalkingTextureRes.Load()) ||
                        (!m_voiceMutedTextureRes.Load()) ||
                        (!m_fontRes.Load()) ||
                        (!m_finishWnd.Load()) ||
                        (!m_camera.Load()))
                    retVal = false;

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }
            return retVal;
        }

        public override bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                if ((!m_hasVoiceTextureRes.Unload()) ||
                        (!m_isTalkingTextureRes.Unload()) ||
                        (!m_voiceMutedTextureRes.Unload()) ||
                        (!m_fontRes.Unload()) ||
                        (!m_finishWnd.Unload()) ||
                        (!m_camera.Unload()))
                    retVal = false;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }
            return retVal;
        }

        #endregion

        public override bool Deinit()
        {
            bool retVal = true;
            if (m_isInit)
            {
                if ((!m_finishWnd.Deinit()) ||
                    (!m_camera.Deinit()))
                    retVal = false;

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }
            return retVal;
        }

        #endregion

        #region game loop

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_debugMgr.Assert(false,
                "FinishedRanksScreen:Update - shouldn't be updated.");
        }

        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_debugMgr.Assert(false,
                "FinishedRanksScreen:Animate - shouldn't be animated.");
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            if (isGameOver)
            {
                m_finishWnd.Draw(gameTime, graphicsDevice, m_camera);

                m_spriteBatch.Begin();

                //draw title
                Microsoft.Xna.Framework.Vector2 position = new Microsoft.Xna.Framework.Vector2(100, 110);
                m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_fontRes.GetResource,
                "GAME OVER", position, Microsoft.Xna.Framework.Graphics.Color.Black);
                position.X--; position.Y--;
                m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_fontRes.GetResource,
                "GAME OVER", position, Microsoft.Xna.Framework.Graphics.Color.White);

                // Draw all the gamers in the session.
                position = new Microsoft.Xna.Framework.Vector2(100, 170);
                int gamerCount = 0;
                foreach (byte virusId in finishVirusRanks)
                {
                    DrawVirus((Virus)m_biophageScn.GameObjects[BiophageGameBaseScn.VirusGobjID(virusId)],
                        gamerCount + 1,
                        position);

                    // Advance to the next screen position, wrapping into two
                    // columns if there are more than 8 gamers in the session.
                    if (++gamerCount == 6)
                    {
                        position.X += 433;
                        position.Y = 170;
                    }
                    else
                        position.Y += ((Microsoft.Xna.Framework.Graphics.SpriteFont)m_fontRes.GetResource).LineSpacing + 20;
                }

                m_spriteBatch.End();
            }
        }

        /// <summary>
        /// Helper draws the gamertag (or bot id) and status icons for a single virus.
        /// </summary>
        void DrawVirus(Virus virus, int rank, Microsoft.Xna.Framework.Vector2 position)
        {
            //retrieve gamer info if not bot
            Microsoft.Xna.Framework.Net.NetworkGamer gamer = null;
            Microsoft.Xna.Framework.Net.NetworkSession netSession = null;
            string text = "";

            if (m_biophageScn.m_sessionDetails.isMultiplayer)
                netSession = m_biophageScn.m_sessionDetails.netSessionComponent.GetNetworkSession;

            if (!virus.virusStateData.isBot)
            {
                if (netSession != null)
                {
                    gamer = netSession.FindGamerById(virus.virusStateData.netPlayerId);
                    if (gamer != null)
                        text = rank.ToString() + ". " + gamer.Gamertag;
                    else
                        text = rank.ToString() + ". PLAYER SIGNED OUT";
                }
                else
                    text = rank.ToString() + ". PLAYER";
            }
            else
                text = rank.ToString() + ". BOT";

            Microsoft.Xna.Framework.Vector2 iconWidth = new Microsoft.Xna.Framework.Vector2(70, 0);
            Microsoft.Xna.Framework.Vector2 iconOffset = new Microsoft.Xna.Framework.Vector2(0, 0);

            Microsoft.Xna.Framework.Vector2 iconPosition = position + iconOffset;

            iconPosition += iconWidth;

            // Draw the "is muted", "is talking", or "has voice" icon.
            if ((netSession != null) && (gamer != null))
            {
                if (gamer.IsMutedByLocalUser)
                {
                    m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_voiceMutedTextureRes.GetResource,
                        iconPosition, Microsoft.Xna.Framework.Graphics.Color.Red);
                }
                else if (gamer.IsTalking)
                {
                    m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_isTalkingTextureRes.GetResource,
                        iconPosition, Microsoft.Xna.Framework.Graphics.Color.Yellow);
                }
                else if (gamer.HasVoice)
                {
                    m_spriteBatch.Draw((Microsoft.Xna.Framework.Graphics.Texture2D)m_hasVoiceTextureRes.GetResource,
                        iconPosition, Microsoft.Xna.Framework.Graphics.Color.White);
                }
            }

            m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_fontRes.GetResource,
                text, position + iconWidth * 2f,
                virus.virusStateData.colour);
        }

        #endregion

        #endregion
    }
}
