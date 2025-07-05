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
    /// Represents the fuzzy hedge operator VERY.
    /// </summary>
    public class FzVERY : IFuzzyTerm
    {
        #region feilds

        private FuzzySet m_set;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="fSetProxy">
        /// Fuzzy set proxy delgation class.
        /// </param>
        public FzVERY(FuzzySetProxy fSetProxy)
        {
            m_set = fSetProxy.GetSet;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="rhs">
        /// Reference to the object to clone from.
        /// </param>
        private FzVERY(FzVERY rhs)
        {
            m_set = rhs.m_set;
        }

        #endregion

        #region IFuzzyTerm Members

        public IFuzzyTerm Clone()
        {
            return new FzVERY(this);
        }

        public double DegreeOfMembership
        {
            get
            {
                return (m_set.DegreeOfMembership * m_set.DegreeOfMembership);
            }
        }

        public void ClearDOM()
        {
            m_set.ClearDOM();
        }

        public void InterpValsDOM(double val)
        {
            m_set.InterpValsDOM(val * val);
        }

        #endregion

        #endregion
    }
}
