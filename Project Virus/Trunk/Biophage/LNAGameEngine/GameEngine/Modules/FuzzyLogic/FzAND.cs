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
    /// Represents the fuzzy binary operator AND.
    /// </summary>
    public class FzAND : IFuzzyTerm
    {
        #region fields

        private List<IFuzzyTerm> m_terms = new List<IFuzzyTerm>();

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="rhs">
        /// Reference to the right side object to copy from.
        /// </param>
        public FzAND(FzAND rhs)
        {
            foreach (IFuzzyTerm fTerm in rhs.m_terms)
            {
                m_terms.Add(fTerm.Clone());
            }
        }

        /// <summary>
        /// Argument constructor, taking two terms.
        /// </summary>
        /// <param name="op1">
        /// Fuzzy term 1.
        /// </param>
        /// <param name="op2">
        /// Fuzzy term 2.
        /// </param>
        public FzAND(IFuzzyTerm op1, IFuzzyTerm op2)
        {
            m_terms.Add(op1.Clone());
            m_terms.Add(op2.Clone());
        }

        /// <summary>
        /// Argument constructor, taking three terms.
        /// </summary>
        /// <param name="op1">
        /// Fuzzy term 1.
        /// </param>
        /// <param name="op2">
        /// Fuzzy term 2.
        /// </param>
        /// <param name="op3">
        /// Fuzzy term 3.
        /// </param>
        public FzAND(IFuzzyTerm op1, IFuzzyTerm op2, IFuzzyTerm op3)
        {
            m_terms.Add(op1.Clone());
            m_terms.Add(op2.Clone());
            m_terms.Add(op3.Clone());
        }

        /// <summary>
        /// Argument constructor, taking four terms.
        /// </summary>
        /// <param name="op1">
        /// Fuzzy term 1.
        /// </param>
        /// <param name="op2">
        /// Fuzzy term 2.
        /// </param>
        /// <param name="op3">
        /// Fuzzy term 3.
        /// </param>
        /// <param name="op4">
        /// Fuzzy term 4.
        /// </param>
        public FzAND(IFuzzyTerm op1, IFuzzyTerm op2, IFuzzyTerm op3, IFuzzyTerm op4)
        {
            m_terms.Add(op1.Clone());
            m_terms.Add(op2.Clone());
            m_terms.Add(op3.Clone());
            m_terms.Add(op4.Clone());
        } 

        #endregion

        #region IFuzzyTerm Members

        public IFuzzyTerm Clone()
        {
            return new FzAND(this);
        }

        public double DegreeOfMembership
        {
            get
            {
                double smallest = double.MaxValue;

                foreach (IFuzzyTerm fTerm in m_terms)
                {
                    if (fTerm.DegreeOfMembership < smallest)
                        smallest = fTerm.DegreeOfMembership;
                }

                return smallest;
            }
        }

        public void ClearDOM()
        {
            foreach (IFuzzyTerm fTerm in m_terms)
            {
                fTerm.ClearDOM();
            }
        }

        public void InterpValsDOM(double val)
        {
            foreach (IFuzzyTerm fTerm in m_terms)
            {
                fTerm.InterpValsDOM(val);
            }
        }

        #endregion

        #endregion
    }
}
