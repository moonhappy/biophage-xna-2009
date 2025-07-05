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
    /// Represents a fuzzy variable.
    /// </summary>
    public class FuzzyVariable
    {
        #region fields

        private DebugManager m_debugMgr;

        private Dictionary<string, FuzzySet> m_memberSets;

        private double m_minRange;
        private double m_maxRange;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        public FuzzyVariable(DebugManager debugMgr)
        {
            m_debugMgr = debugMgr;

            m_memberSets = new Dictionary<string, FuzzySet>();
            m_minRange = 0.0;
            m_maxRange = 0.0;
        }

        #endregion

        #region fuzzy

        public FuzzySetProxy AddLeftShoulderSet(string name, double minBound, double peak, double maxBound)
        {
            FzSetLeftShoulder fSetLeftShoulder = new FzSetLeftShoulder(m_debugMgr, peak, peak - minBound, maxBound - peak);
            m_memberSets[name] = fSetLeftShoulder;
            AdjustRangeToFit(minBound, maxBound);

            return new FuzzySetProxy(m_debugMgr, fSetLeftShoulder);
        }

        public FuzzySetProxy AddRightShoulderSet(string name, double minBound, double peak, double maxBound)
        {
            FzSetRightShoulder fSetRightShoulder = new FzSetRightShoulder(m_debugMgr, peak, peak - minBound, maxBound - peak);
            m_memberSets[name] = fSetRightShoulder;
            AdjustRangeToFit(minBound, maxBound);

            return new FuzzySetProxy(m_debugMgr, fSetRightShoulder);
        }

        public FuzzySetProxy AddTriangularSet(  string name,
                                                double minBound,
                                                double peak,
                                                double maxBound)
        {
            FzSetTriangle fSetTriangle = new FzSetTriangle(m_debugMgr, peak, peak - minBound, maxBound - peak);
            m_memberSets[name] = fSetTriangle;
            AdjustRangeToFit(minBound, maxBound);

            return new FuzzySetProxy(m_debugMgr, fSetTriangle);
        }

        public FuzzySetProxy AddSingletonSet(string name,
                                                double minBound,
                                                double peak,
                                                double maxBound)
        {
            FzSetSingleton fSetSingleton = new FzSetSingleton(m_debugMgr,
                                              peak,
                                              peak - minBound,
                                              maxBound - peak);
            m_memberSets[name] = fSetSingleton;
            AdjustRangeToFit(minBound, maxBound);

            return new FuzzySetProxy(m_debugMgr, fSetSingleton);
        }

        /// <summary>
        /// Fuzzify a value by calculating it's degree of membership against
        /// each of the subsets.
        /// </summary>
        /// <param name="val">
        /// Value to fuzzify.
        /// </param>
        public void Fuzzify(double val)
        {
            //make sure the value is within the bounds of this variable
            if (val < m_minRange)
                val = m_minRange;
            else if (val > m_maxRange)
                val = m_maxRange;

            m_debugMgr.Assert((val >= m_minRange) && (val <= m_maxRange),
                     "FuzzyVariable:Fuzzify - value out of range");

            foreach (KeyValuePair<string, FuzzySet> fSetKVP in m_memberSets)
            {
                fSetKVP.Value.DegreeOfMembership = fSetKVP.Value.CalculateDOM(val);
            }
        }

        /// <summary>
        /// Defuzzify this fuzzy variable.
        /// </summary>
        /// <remarks>
        /// The defuzzification method uses the averaging the maxima method.
        /// Refer to p436 of Buckland's 'Programming Game AI by Example'.
        /// </remarks>
        /// <returns>
        /// Translated 'crisp' value of the fuzzy variable.
        /// </returns>
        public double Defuzzify()
        {
            double bottom = 0.0;
            double top = 0.0;

            foreach (KeyValuePair<string, FuzzySet> fSetKVP in m_memberSets)
            {
                bottom += fSetKVP.Value.DegreeOfMembership;

                top += fSetKVP.Value.RepresentativeVal * fSetKVP.Value.DegreeOfMembership;
            }

            //make sure bottom is not equal to zero
            if (0.0 == bottom) 
                return 0.0;

            return top / bottom;
        }

        /// <summary>
        /// Clips fuzzy value into the passed range.
        /// </summary>
        /// <param name="min">
        /// The minimal value the fuzzy variable can be.
        /// </param>
        /// <param name="max">
        /// The maximum value the fuzzy variable can be.
        /// </param>
        private void AdjustRangeToFit(double min, double max)
        {
            if (min < m_minRange) 
                m_minRange = min;
            if (max > m_maxRange) 
                m_maxRange = max;
        }

        #endregion

        #endregion
    }
}
