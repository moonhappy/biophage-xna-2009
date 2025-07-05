/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Resources;

namespace LNA.GameEngine.Objects.UI.Menu
{
    /// <summary>
    /// Represents a menu object.
    /// </summary>
    public abstract class MenuObject : UIObject
    {
        #region constants

        public const float ConstCornerRadius = 0.025f;

        public const float HeightDelta = 0.125f;

        protected bool m_isSelectable;

        #endregion

        #region fields

        protected MenuWindow m_menuWnd;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Menu object Id.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="menuWnd">
        /// Reference to the menu window
        /// </param>
        /// <param name="addToMenuWnd">
        /// If true, the menu object will be automatically added to
        /// the menu window.
        /// </param>
        public MenuObject(  uint id, bool isSelectable,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            MenuWindow menuWnd, bool addToMenuWnd)
            : base(id, Microsoft.Xna.Framework.Vector3.Zero, debugMgr, resourceMgr, null, false)
        {
            m_isSelectable = isSelectable;
            m_menuWnd = menuWnd;
            if (addToMenuWnd)
            {
                m_debugMgr.Assert(m_menuWnd != null,
                    "MenuObject:Constructor - 'menuWnd' is null.");
                m_menuWnd.AddMenuObj(this);
            }
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Changes to the position are permanent with menu objects, unlike 
        /// assets which can be reset.
        /// </summary>
        public override Microsoft.Xna.Framework.Vector3 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                m_initPosition = value;

                Reinit();
            }
        }

        /// <summary>
        /// Reference to this object's menu window.
        /// </summary>
        public MenuWindow GetMenuWindow
        {
            get { return m_menuWnd; }
            set { m_menuWnd = value; }
        }

        public virtual bool IsSelectable
        {
            get { return m_isSelectable; }
            set { m_isSelectable = value; }
        }

        public abstract string Description { get; }

        #endregion

        #endregion
    }
}
