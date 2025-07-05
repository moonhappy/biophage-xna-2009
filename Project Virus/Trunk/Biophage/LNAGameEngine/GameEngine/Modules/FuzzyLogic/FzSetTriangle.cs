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
    public class FzSetTriangle : FuzzySet
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
        /// The 'peak' value of the shoulder shape.
        /// </param>
        /// <param name="leftOffset">
        /// The offset of the left plateau from the peak.
        /// </param>
        /// <param name="rightOffset">
        /// The offset of the right slope from the peak.
        /// </param>
        public FzSetTriangle(DebugManager debugMgr,
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
            //test to prevent divide by zero errors
            if (    ((m_rightOffset == 0.0) && ((m_peak == val)))   ||
                    ((m_leftOffset == 0.0) && ((m_peak == val)))    )
                return 1.0;

            //find DOM
            else if ((val <= m_peak) && (val >= (m_peak - m_leftOffset)))
            {
                double grad = 1.0 / m_leftOffset;
                return (grad * (val - (m_peak - m_leftOffset)));
            }
            else if ((val > m_peak) && (val < (m_peak + m_rightOffset)))
            {
                double grad = 1.0 / -m_rightOffset;
                return (grad * (val - m_peak) + 1.0);
            }
            else
                return 0.0;
        }

        #endregion

        #endregion
    }
}
