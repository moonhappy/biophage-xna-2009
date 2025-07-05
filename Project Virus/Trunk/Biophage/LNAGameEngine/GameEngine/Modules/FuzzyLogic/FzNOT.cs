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
    /// Represents the fuzzy unary operator NOT (Negate or Complement).
    /// </summary>
    public class FzNOT : IFuzzyTerm
    {
        #region fields

        private IFuzzyTerm m_fzTerm;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="rhs">
        /// Reference to the right-side/src term.
        /// </param>
        public FzNOT(FzNOT rhs)
        {
            m_fzTerm = rhs.m_fzTerm.Clone();
        }

        /// <summary>
        /// Arguement constructor.
        /// </summary>
        /// <param name="fzTerm">
        /// Term to negate.
        /// </param>
        public FzNOT(IFuzzyTerm fzTerm)
        {
            m_fzTerm = fzTerm.Clone();
        }

        #endregion

        #region IFuzzyTerm Members

        public IFuzzyTerm Clone()
        {
            return new FzNOT(this);
        }

        public double DegreeOfMembership
        {
            get 
            { 
                return (1.0 - m_fzTerm.DegreeOfMembership); 
            }
        }

        public void ClearDOM()
        {
            m_fzTerm.ClearDOM();
        }

        public void InterpValsDOM(double val)
        {
            m_fzTerm.InterpValsDOM(val);
        }

        #endregion

        #endregion
    }
}
