/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using LNA.GameEngine;

namespace LNA.GameEngine.Core.AsyncTasks
{
    /// <summary>
    /// Used to dispatch and manage asynchronous tasks. This class also
    /// provides a way to communicate asynchronous task results to other
    /// threads, these results can either be intermediate or complete
    /// results of the task.
    /// </summary>
    public sealed class AsyncTaskManager
    {
        #region fields

        private DebugManager m_dbgMgr;

        private volatile uint m_idCount;
        private Dictionary<uint, Thread> m_threads;
        private Dictionary<uint, LinkedList<AsyncReturnPackage>> m_returnPackages;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        public AsyncTaskManager(DebugManager debugMgr)
        {
            //set fields
            m_dbgMgr = debugMgr;
            m_idCount = 0;
            m_threads = new Dictionary<uint, Thread>();
            m_returnPackages = new Dictionary<uint, LinkedList<AsyncReturnPackage>>();

            //log
            //m_dbgMgr.WriteLogEntry("AsyncTaskManager:Constructor - done.");
        }

        #endregion

        #region threading

        #region thread_creation

        /// <summary>
        /// Increments the task Id counter safely and returns the next
        /// number. Thread safe method.
        /// </summary>
        /// <returns>
        /// The next task Id.
        /// </returns>
        private uint NextTaskId()
        {
            //increment the id count - check for overflow
            m_idCount++;
            if (m_idCount == uint.MaxValue)
                m_idCount = 1;

            return m_idCount;
        }

        /// <summary>
        /// Used to dispatch a new asynchronous task. Thread safe method.
        /// </summary>
        /// <remarks>
        /// A new thread will be created with a below normal priority to
        /// process this task. The priority is lowered to allow the urgent
        /// synchronous tasks to complete more often so the game loop does
        /// not stall due to many asynchronous tasks. Thread safe method.
        /// </remarks>
        /// <typeparam name="T">
        /// The asynchronous task to dispatch. It must implement the
        /// ILnaAsyncTask interface and provide a default constructor.
        /// </typeparam>
        /// <param name="taskParam">
        /// The task parameters/work data boxed as an object. The unboxing
        /// and use of the data must be handled in the 'DoWork' method.
        /// </param>
        /// <returns>
        /// The task's unique identifier. This allows the caller to check
        /// for any return packages completed by the task. Also, the Id
        /// allows the task's thread to be interfered.
        /// </returns>
        public uint SubmitTask<T>(object taskParam)
            where T : AsyncTask, new()
        {
            //m_dbgMgr.WriteLogEntry("AsynTaskManager:SubmitTask - doing.");

            //next task id
            uint localId = NextTaskId();

            //create task and thread
            T task = new T();
            task.Create(this, localId, taskParam);
            Thread thread = new Thread(task.ThreadEntry);

            //lower priority, synchronous threads should always
            //  have a higher priority.
            thread.Priority = ThreadPriority.BelowNormal;

            //add to collection
            uint numAttempts = 0;
            lock (m_threads)
            {
                //check for highly unlikely problem
                while (m_threads.ContainsKey(localId))
                {
                    if (numAttempts < 10)
                    {
                        m_dbgMgr.WriteLogEntry("AsyncTaskManager:SubmitTask - really old tasks detected.");
                        localId = NextTaskId();
                    }
                    else
                        m_dbgMgr.Assert(false,
                            "AsyncTaskManager:SubmitTask - too many old tasks detected.");

                    numAttempts++;
                }
                //certain that there is a spot for the new task - so add
                m_threads.Add(localId, thread);
            }

            //start and return Id
            thread.Start();
            return localId;
        }

        #endregion

        #region thread_return

        /// <summary>
        /// To be called by an asychronous task to return information.
        /// Typical producer / consumer policy. Thread safe method.
        /// </summary>
        /// <remarks>
        /// Calling this method will not override any pre-existing return
        /// packages. Instead the return package will be added to a linked
        /// list that will be added to the return package dictionary/map.
        /// </remarks>
        /// <param name="returnPackage">
        /// Reference to the package to return.
        /// </param>
        public void ProduceReturn(AsyncReturnPackage returnPackage)
        {
            //m_dbgMgr.WriteLogEntry("AsyncTaskManager:ConsumeReturn - doing package.");

            //gain lock to package collection
            LinkedList<AsyncReturnPackage> pkgList;
            lock (m_returnPackages)
            {
                //check if Id already exists
                if (m_returnPackages.ContainsKey(returnPackage.TaskId))
                    m_returnPackages[returnPackage.TaskId].AddLast(returnPackage);
                else
                {
                    pkgList = new LinkedList<AsyncReturnPackage>();
                    pkgList.AddLast(returnPackage);
                    m_returnPackages.Add(returnPackage.TaskId, pkgList);
                }
            }
        }

        /// <summary>
        /// Adds a group of return packages to the return package
        /// dictionary/map. Typical producer / consumer policy. Thread safe
        /// method.
        /// </summary>
        /// <remarks>
        /// Calling this method will not override any pre-existing return
        /// packages. Instead the linked list parameter will be appended to
        /// the existing linked list on the return packages dictionary/map.
        /// </remarks>
        /// <param name="listReturns">
        /// Reference to the list of return packages.
        /// </param>
        public void ProduceReturn(LinkedList<AsyncReturnPackage> listReturns)
        {
            //m_dbgMgr.WriteLogEntry("AsyncTaskManager:ConsumeReturn - doing list.");

            //check the list is not empty
            if (listReturns.First == null)
            {
                m_dbgMgr.WriteLogEntry("AsyncTaskManager:ConsumeReturn - 'listReturns' is empty.");
                return;
            }

            uint tskId = listReturns.First.Value.TaskId;
            //gain lock to package collection
            lock (m_returnPackages)
            {
                //check for match Id
                if (m_returnPackages.ContainsKey(tskId))
                    m_returnPackages[tskId].AddLast(listReturns.First);
                else
                    m_returnPackages.Add(tskId, listReturns);
            }
        }

        /// <summary>
        /// Returns a reference to a linked list containing all return
        /// packages returned from the same asynchonous task. Typical 
        /// producer / consumer policy. Thread safe method.
        /// </summary>
        /// <remarks>
        /// This method will also remove the linked list from the return
        /// packages dictionary/map. If the caller is only concerned with
        /// some of the return packages, these should be removed from the
        /// list and then the list should be put back to the dictionary/map
        /// using the 'AddReturnPackage' method.
        /// </remarks>
        /// <param name="taskId">
        /// The asynchronous task Id to check for results.
        /// </param>
        /// <returns>
        /// Reference to a linked list of return packages by the
        /// asynchronous task. Returns null if no return packages found.
        /// </returns>
        public LinkedList<AsyncReturnPackage> ConsumeReturn(uint taskId)
        {
            //m_dbgMgr.WriteLogEntry("AsyncTaskManager:ConsumeReturn - doing.");

            LinkedList<AsyncReturnPackage> returnList = null;
            //gain lock for package collection
            lock (m_returnPackages)
            {
                if (m_returnPackages.ContainsKey(taskId))
                {
                    returnList = m_returnPackages[taskId];
                    m_returnPackages.Remove(taskId);
                }
            }
            return returnList;
        }

        #endregion

        #region thread_control

        /// <summary>
        /// Aborts a task.
        /// </summary>
        /// <param name="taskId">
        /// Task Id to abort.
        /// </param>
        /// <returns>
        /// True if task aborted ok; otherwise if the task does not exist
        /// , has completed, or the task thread is in a state that can't be
        /// aborted, false will be returned.
        /// </returns>
        public bool AbortTask(uint taskId)
        {
            //m_dbgMgr.WriteLogEntry("AsyncTaskManager:StopTask - doing.");

            //find and abort the task thread
            bool retCond = true;
            Thread threadRef;
            lock (m_threads)
            {
                if (m_threads.ContainsKey(taskId))
                {
                    threadRef = m_threads[taskId];
                    try
                    {
                        threadRef.Abort();
                    }
                    catch (ThreadStateException)
                    {
                        retCond = false;
                        m_dbgMgr.WriteLogEntry(
                            "AsyncTaskManager:StopTask - task can't be aborted.");
                    }
                }
                else
                {
                    retCond = false;
                }
            }
            return retCond;
        }

        /// <summary>
        /// Removes the thread reference from the thread collection. But
        /// does not affect the thread's operation.
        /// </summary>
        /// <param name="taskId">
        /// Task Id to remove from thread collection.
        /// </param>
        /// <returns>
        /// True if task thread removed ok; otherwise if the task thread
        /// does not exist false will be returned.
        /// </returns>
        public bool RemoveTask(uint taskId)
        {
            //m_dbgMgr.WriteLogEntry("AsyncTaskManager:RemoveTask - doing.");

            //find and remove from dicitionary/map
            bool retCond = true;
            lock (m_threads)
            {
                retCond = m_threads.Remove(taskId);
            }
            return retCond;
        }

        #endregion

        #endregion

        #endregion
    }
}
