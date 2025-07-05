/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Resources;
using LNA.GameEngine.Objects.Scenes;

namespace LNA.GameEngine.Objects.GameObjects.Assets
{
    /// <summary>
    /// A special kind of ellipse.
    /// </summary>
    public class CircleAsset : EllipseAsset
    {
        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Asset Id.
        /// </param>
        /// <param name="initialPosition">
        /// Initial 'top-left' position.
        /// </param>
        /// <param name="initialRadius">
        /// Initial circle radius.
        /// </param>
        /// <param name="initialColour">
        /// Initial colour.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the scene asset belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics manager.
        /// </param>
        public CircleAsset( uint id, Microsoft.Xna.Framework.Vector3 initialPosition,
                            float initialRadius,
                            Microsoft.Xna.Framework.Graphics.Color initialColour,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Scene scene, bool addToScene,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr)
            : base(id, initialPosition, (initialRadius * 2.0f), (initialRadius * 2.0f), 
            initialColour, debugMgr, resourceMgr, scene, addToScene, graphicsMgr)
        { }

        #endregion

        /// <summary>
        /// Changes the circle width and height linearly.
        /// </summary>
        public override float Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
                base.Height = value;
            }
        }

        /// <summary>
        /// Changes the circle width and height linearly.
        /// </summary>
        public override float Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
                base.Width = value;
            }
        }

        /// <summary>
        /// Changes the circle's radius.
        /// </summary>
        public float Radius
        {
            get { return (m_width / 2f); }
            set
            {
                Width = value * 2.0f;
                //Height = value * 2.0f;
            }
        }

        #endregion
    }
}
