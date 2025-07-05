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
    /// Represents the Fuzzy Set base.
    /// </summary>
    public abstract class FuzzySet
    {
        #region fields

        protected DebugManager m_debugMgr;

        protected double m_degreeOfMembership;
        protected double m_representitiveVal;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="representativeVal">
        /// Value to represent the overall FuzzySet. IE: The peak
        /// of a triangle set or mid-point of a plateau.
        /// </param>
        public FuzzySet(DebugManager debugMgr, double representativeVal)
        {
            m_debugMgr = debugMgr;
            m_representitiveVal = representativeVal;
        }

        #endregion

        #region field accessors

        /// <summary>
        /// FuzzySet's degree of membership.
        /// </summary>
        public double DegreeOfMembership
        {
            get { return m_degreeOfMembership; }
            set
            {
                m_debugMgr.Assert((value <= 1) && (value >= 0),
                    "FuzzySet:DegreeOfMembership - value must be between (or equal to ) 0 and 1.");
                m_degreeOfMembership = value;
            }
        }

        /// <summary>
        /// Value to represent the overall FuzzySet. IE: The peak
        /// of a triangle set or mid-point of a plateau.
        /// </summary>
        public double RepresentativeVal
        {
            get { return m_representitiveVal; }
        }

        #endregion

        #region fuzzy

        /// <summary>
        /// Reset degree of membership to zero.
        /// </summary>
        public void ClearDOM()
        {
            m_degreeOfMembership = 0.0;
        }

        /// <summary>
        /// If the value passed is greater than the current degree of membership,
        /// it is set as the new value. Used for FLV consequents.
        /// </summary>
        /// <param name="val"></param>
        public void InterpValsDOM(double val)
        {
            if (val > m_degreeOfMembership) 
                m_degreeOfMembership = val;
        }

        /// <summary>
        /// Calculates the degree of membership of this set from a given value.
        /// </summary>
        /// <param name="val">
        /// Test value.
        /// </param>
        /// <returns>
        /// Degree of membership.
        /// </returns>
        public abstract double CalculateDOM(double val);

        #endregion

        #endregion
    }
}
