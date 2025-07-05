/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LNA.GameEngine.Core.AsyncTasks
{
    /// <summary>
    /// Base definition of an asychronous task. Simply override the
    /// 'DoWork' method with you own code. You must also provide a
    /// default constructor for all derived classes.
    /// </summary>
    /// <remarks>
    /// It is recommended that all derived classes call the base default
    /// constructor.
    /// </remarks>
    public abstract class AsyncTask
    {
        #region fields

        protected uint m_id;
        protected object m_param;

        protected AsyncTaskManager m_asyncMgr;
        private int[] m_tAffAssign = { 5 };

        #endregion

        #region methods

        /// <summary>
        /// Default initialisation/creation method.
        /// </summary>
        /// <param name="asyncMgr">
        /// Reference to the aysnchronous task manager.
        /// </param>
        /// <param name="taskId">
        /// The task's Id.
        /// </param>
        /// <param name="taskParam">
        /// The task's parameter/work data.
        /// </param>
        public void Create(AsyncTaskManager asyncMgr, uint taskId, object taskParam)
        {
            //set fields
            m_asyncMgr = asyncMgr;
            m_id = taskId;
            m_param = taskParam;
        }

        /// <summary>
        /// Called by the asynchronous task manager in 'SubmitTask' method.
        /// </summary>
        public void ThreadEntry()
        {
#if XBOX
            //must change thread's processor affinity before running on xbox
            //  hardware thread 5 is choosen as it is away from interfering
            //  with others (to a degree).
            Thread.CurrentThread.SetProcessorAffinity(m_tAffAssign);
#endif
            //call the DoWork method
            DoWork();

            //clean up after self
            m_asyncMgr.RemoveTask(m_id);
        }

        public abstract void DoWork();

        #endregion
    }
}
