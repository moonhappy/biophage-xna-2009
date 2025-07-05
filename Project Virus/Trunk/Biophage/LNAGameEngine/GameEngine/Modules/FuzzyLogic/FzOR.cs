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
    /// Represents the fuzzy binary operator OR.
    /// </summary>
    public class FzOR : IFuzzyTerm
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
        public FzOR(FzOR rhs)
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
        public FzOR(IFuzzyTerm op1, IFuzzyTerm op2)
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
        public FzOR(IFuzzyTerm op1, IFuzzyTerm op2, IFuzzyTerm op3)
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
        public FzOR(IFuzzyTerm op1, IFuzzyTerm op2, IFuzzyTerm op3, IFuzzyTerm op4)
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
            return new FzOR(this);
        }

        public double DegreeOfMembership
        {
            get
            {
                double largest = double.MinValue;

                foreach (IFuzzyTerm fTerm in m_terms)
                {
                    if (fTerm.DegreeOfMembership > largest)
                        largest = fTerm.DegreeOfMembership;
                }

                return largest;
            }
        }

        /// <summary>
        /// DO NOT CALL.
        /// </summary>
        public void ClearDOM()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// DO NOT CALL.
        /// </summary>
        public void InterpValsDOM(double val)
        {
            throw new NotSupportedException();
        }

        #endregion

        #endregion
    }
}
