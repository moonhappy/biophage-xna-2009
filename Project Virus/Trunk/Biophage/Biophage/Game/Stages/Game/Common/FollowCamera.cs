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

namespace Biophage.Game.Stages.Game.Common
{
    /// <summary>
    /// Follows a position and orientation.
    /// </summary>
    public class FollowCamera : CameraGObj
    {
        #region fields

        Microsoft.Xna.Framework.Vector3 m_followForwardDir = Microsoft.Xna.Framework.Vector3.Forward;

        float m_followDist = 1f;

        #endregion

        #region methods

        #region construction

        public FollowCamera(uint id,
                                DebugManager debugMgr, ResourceManager resMgr,
                                Scene scn,
                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                Microsoft.Xna.Framework.Vector3 followPos,
                                Microsoft.Xna.Framework.Vector3 followUpDir,
                                Microsoft.Xna.Framework.Vector3 followForwardDir,
                                float nearClip, float farClip, float initFollowDist)
            : base( id, debugMgr, resMgr, scn, true,
                    graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                    followPos, followPos, followUpDir, 45f,
                    nearClip, farClip)
        {
            m_followForwardDir = followForwardDir;
            FollowDistance = initFollowDist;
        }

        #endregion

        #region properties

        public float FollowDistance
        {
            get { return m_followDist; }
            set
            {
                //set field
                m_followDist = value;
                if (m_followDist <= NearClip)
                    m_followDist = NearClip + JigLibX.Math.JiggleMath.Epsilon;
            }
        }

        public Microsoft.Xna.Framework.Vector3 ForwardDir
        {
            get { return m_followForwardDir; }
            set
            {
                //set field
                m_followForwardDir = value;

                FollowDistance = FollowDistance;
            }
        }

        #endregion

        #region game loop

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //set position - reverse of forward by length from follow position
            Microsoft.Xna.Framework.Vector3 tempVec = -m_followForwardDir;
            if (tempVec.Length() != 0)
                tempVec.Normalize();
            Position = (tempVec * m_followDist) + LookingAt;
        }

        #endregion

        #endregion
    }
}
