/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.GameObjects.Assets;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;

namespace LNA.GameEngine.Objects.UI
{
    /// <summary>
    /// Method prototype delegate for action routines.
    /// </summary>
    /// <param name="sender">
    /// Reference to the sender object.
    /// </param>
    /// <param name="e">
    /// Event arguments.
    /// </param>
    public delegate void UIEventAction(object sender, EventArgs e);

    /// <summary>
    /// Defines the functionality that ui "items" must provide. A menu
    /// item can be anything located on a menu window, like a button or
    /// label.
    /// </summary>
    public abstract class UIObject : Asset
    {
        #region fields

        protected bool m_isSelected;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// UI item Id.
        /// </param>
        /// <param name="initialPosition">
        /// Initial position of the UI item.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the scene the UI item belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the UI item will be automatically added to the
        /// scene.
        /// </param>
        public UIObject(uint id, Microsoft.Xna.Framework.Vector3 initialPosition,
                        DebugManager debugMgr, ResourceManager resourceMgr,
                        Scene scene, bool addToScene)
            : base(id, initialPosition, debugMgr, resourceMgr, scene, addToScene)
        {
            //set feilds
            m_isSelected = false;

            //log
            //m_debugMgr.WriteLogEntry("UIObject:Constructor - done.");
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Whether the UI item is selected.
        /// </summary>
        public abstract bool IsSelected { get; set; }

        #endregion

        #region event_handling

        public event UIEventAction UIAction;

        /// <summary>
        /// This method will be called when an event occurs.
        /// </summary>
        /// <remarks>
        /// This method will be called only when: (1) the UI item is
        /// active, (2) the UI item is selected, and (3) the user has
        /// 'pressed/entered' the item.
        /// </remarks>
        public virtual void ActionEvent()
        {
            //check active
            if (Active && IsSelected)
                if (UIAction != null)
                    UIAction(this, EventArgs.Empty);
        }

        public void ClearEvents()
        {
            UIAction = null;
        }

        #endregion

        #endregion
    }
}
