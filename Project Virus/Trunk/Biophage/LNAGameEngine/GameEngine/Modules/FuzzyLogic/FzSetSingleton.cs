/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

/* NOTE:
 
    This is based on Mat Buckland's FuzzyLogic example.   
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LNA.GameEngine.Modules.FuzzyLogic
{
    /// <summary>
    /// Represents a fuzzy set with a 'synaptic / binary' fire condition.
    /// </summary>
    public class FzSetSingleton : FuzzySet
    {
        #region feilds

        private double m_peak;
        private double m_rightOffset;
        private double m_leftOffset;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="peak">
        /// The 'peak / fire' value of the singleton.
        /// </param>
        /// <param name="leftOffset">
        /// Range of values below peak that will produce a zero
        /// degree of membership.
        /// </param>
        /// <param name="rightOffset">
        /// Range of values above peak that will produce a zero
        /// degree of membership.
        /// </param>
        public FzSetSingleton(DebugManager debugMgr,
                                    double peak,
                                    double leftOffset,
                                    double rightOffset)
            : base(debugMgr, peak)
        {
            m_peak = peak;
            m_rightOffset = rightOffset;
            m_leftOffset = leftOffset;
        }

        #endregion

        #region fuzzy

        public override double CalculateDOM(double val)
        {
            if (val == m_peak)
                return 1.0;
            else
                return 0.0;
        }

        #endregion

        #endregion
    }
}
