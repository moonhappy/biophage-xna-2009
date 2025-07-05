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
    /// Exttends menu button to encase a session selection
    /// </summary>
    public class MenuButtonAvSession : MenuButton
    {
        #region fields

        protected Microsoft.Xna.Framework.Net.AvailableNetworkSession m_avaliableSession;

        #endregion

        #region methods

        public MenuButtonAvSession(uint id, string buttonLabel, string buttonDesc, SpriteFontResHandle buttonFont,
                                    DebugManager debugMgr, ResourceManager resourceMgr,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                    Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                    MenuWindow menuWnd, bool addToMenuWindow,
                                    Microsoft.Xna.Framework.Net.AvailableNetworkSession avSession)
            : base(id, buttonLabel, buttonDesc, buttonFont, debugMgr, resourceMgr, graphicsMgr, spriteBatch, menuWnd, addToMenuWindow)
        {
            m_avaliableSession = avSession;
        }

        public Microsoft.Xna.Framework.Net.AvailableNetworkSession GetSession
        {
            get { return m_avaliableSession; }
        }

        #endregion
    }
}
