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
    /// Represents a Fuzzy Rule in the form:
    /// IF antecedent THEN consequent - where 
    /// antecedent:= fzVar1 AND fzVar2 AND ... fzVarN, and
    /// consequent:= fzVar.
    /// </summary>
    public class FuzzyRule
    {
        #region fields

        private IFuzzyTerm m_antecedent;
        private IFuzzyTerm m_consequent;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Arguement constructor.
        /// </summary>
        /// <param name="ancedent">
        /// The antecedent fuzzy term of the fuzzy rule.
        /// </param>
        /// <param name="consequent">
        /// The consequent fuzzy term of the fuzzy rule.
        /// </param>
        public FuzzyRule(IFuzzyTerm antecedent, IFuzzyTerm consequent)
        {
            m_antecedent = antecedent.Clone();
            m_consequent = consequent.Clone();
        }

        #endregion

        #region fuzzy

        /// <summary>
        /// Clear the consequent term to zero.
        /// </summary>
        public void SetConfidenceOfConsequentToZero() 
        {
            m_consequent.ClearDOM();
        }

        /// <summary>
        /// Updates the consequent degree of membership with the antcedent's
        /// degree of confidence.
        /// </summary>
        public void Calculate()
        {
            m_consequent.InterpValsDOM(m_antecedent.DegreeOfMembership);
        }

        #endregion

        #endregion
    }
}
