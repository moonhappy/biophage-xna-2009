/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNA.GameEngine.Core.AsyncTasks
{
    /// <summary>
    /// Return package from asynchronous task. Used to communicate
    /// results from the asynchronous task to another thread via the
    /// AsyncTaskManager class object.
    /// </summary>
    public sealed class AsyncReturnPackage
    {
        #region fields

        private uint m_taskId;
        private uint m_returnId;

        private object m_returnData;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="taskId">
        /// Id of the asynchronous task that this return package was created
        /// from.
        /// </param>
        /// <param name="returnId">
        /// Optional Id for this return package. Usefull for some purposes.
        /// </param>
        /// <param name="returnData">
        /// Boxed reference to the return data object.
        /// </param>
        public AsyncReturnPackage(  uint taskId, uint returnId,
                                    object returnData)
        {
            //set fields
            m_taskId = taskId;
            m_returnId = returnId;
            m_returnData = returnData;
        }

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="taskId">
        /// Id of the asynchronous task that this return package was created
        /// from.
        /// </param>
        /// <param name="returnData">
        /// Boxed reference to the return data object.
        /// </param>
        public AsyncReturnPackage(uint taskId, object returnData)
        {
            //set fields
            m_taskId = taskId;
            m_returnId = 0;
            m_returnData = returnData;
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Id of the asynchronous task that this return package was created
        /// from.
        /// </summary>
        public uint TaskId
        {
            get { return m_taskId; }
        }

        /// <summary>
        /// Optional Id for this return package. Usefull for some purposes.
        /// </summary>
        public uint ReturnId
        {
            get { return m_returnId; }
        }

        /// <summary>
        /// Boxed reference to the return data object.
        /// </summary>
        public object ReturnData
        {
            get { return m_returnData; }
        }

        #endregion

        #endregion
    }
}
