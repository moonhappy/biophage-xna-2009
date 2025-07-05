/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LNA.GameEngine.Objects
{
    /// <summary>
    /// This acts as a replacement to the ManualResetEvent / WaitHandle
    /// for use with the ThreadPool on the Xbox 360 system.
    /// </summary>
    public sealed class SynchroniseWithThreadPool
    {
        #region fields

        private volatile int m_numTasksToDo;
        private object m_taskCounterLock;

        /// <summary>
        /// Use 'WaitOne' method to block a thread until the set/continue
        /// signal condition is met.
        /// </summary>
        public ManualResetEvent stallCondition;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="numTasksToDo">
        /// Number of tasks that must complete before the block condition is
        /// released.
        /// </param>
        public SynchroniseWithThreadPool(int numTasksToDo)
        {
            m_numTasksToDo = numTasksToDo;
            m_taskCounterLock = new object();
            stallCondition = new ManualResetEvent(false);
        }

        #endregion

        /// <summary>
        /// When a task completes, it should signal the finished state by calling
        /// this method. This will decrement the 'tasks todo' counter. When the
        /// counter reaches 0 (IE: all tasks have completed), the block condition
        /// will be released.
        /// </summary>
        public void TaskCompleted()
        {
            lock (m_taskCounterLock)
            {
                m_numTasksToDo--;
                if (m_numTasksToDo == 0)
                    stallCondition.Set();
            }
        }

        #endregion
    }
}