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
    /// Proxy class for FuzzySets.
    /// </summary>
    public class FuzzySetProxy : IFuzzyTerm
    {
        #region fields

        private DebugManager m_debugMgr;
        private FuzzySet m_set;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument Constructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="fuzzySet">
        /// FuzzySet object this Proxy belongs to.
        /// </param>
        public FuzzySetProxy(DebugManager debugMgr, FuzzySet fuzzySet)
        {
            m_debugMgr = debugMgr;
            m_set = fuzzySet;

            //asserts
            m_debugMgr.Assert(m_set != null,
                "FuzzySetProxy:Constructor - 'fuzzySet' is null.");
        }

        #endregion

        #region feild_accessors

        /// <summary>
        /// Reference to the set this proxy refers to.
        /// </summary>
        public FuzzySet GetSet
        {
            get { return m_set; }
        }
        
        #endregion

        #region IFuzzyTerm Members

        /// <summary>
        /// Creates a clone of this Fuzzy Object.
        /// </summary>
        /// <returns>
        /// Reference to the new Fuzzy Object (as IFuzzyTerm).
        /// </returns>
        public IFuzzyTerm Clone()
        {
            return new FuzzySetProxy(m_debugMgr, m_set);
        }

        /// <summary>
        /// FuzzySet's degree of membership.
        /// </summary>
        public double DegreeOfMembership
        {
            get { return m_set.DegreeOfMembership; }
        }

        /// <summary>
        /// Clears the FuzzySet's degree of membership to zero.
        /// </summary>
        public void ClearDOM()
        {
            m_set.ClearDOM();
        }

        /// <summary>
        /// Sets a new max value to FuzzySet's degree of membership.
        /// </summary>
        /// <param name="val">
        /// If greater than the FuzzySet's current degree of membership value,
        /// it will be set as the new value.
        /// </param>
        public void InterpValsDOM(double val)
        {
            m_set.InterpValsDOM(val);
        }

        #endregion

        #endregion
    }
}
