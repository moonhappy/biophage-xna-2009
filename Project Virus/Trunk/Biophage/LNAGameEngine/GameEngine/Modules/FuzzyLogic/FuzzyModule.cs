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
    public class FuzzyModule
    {
        #region fields

        private DebugManager m_debugMgr;

        private Dictionary<string, FuzzyVariable> m_variables;
        private List<FuzzyRule> m_rules;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        public FuzzyModule(DebugManager debugMgr)
        {
            m_debugMgr = debugMgr;
            m_variables = new Dictionary<string, FuzzyVariable>();
            m_rules = new List<FuzzyRule>();
        }

        #endregion

        #region fuzzy

        private void SetConfidencesOfConsequentsToZero()
        {
            foreach (FuzzyRule fRule in m_rules)
            {
                fRule.SetConfidenceOfConsequentToZero();
            }
        }

        /// <summary>
        /// Creates a new fuzzy variable to this fuzzy module.
        /// </summary>
        /// <param name="VarName">
        /// The fuzzy variable name it is refered to as.
        /// </param>
        /// <returns>
        /// Reference to the newly created fuzzy variable object.
        /// </returns>
        public FuzzyVariable CreateFuzzyVar(string varName)
        {
            FuzzyVariable fVar = new FuzzyVariable(m_debugMgr);
            m_variables[varName] = fVar;

            return fVar;
        }
  
        /// <summary>
        /// Add rule to fuzzy module.
        /// </summary>
        /// <param name="antecedent">
        /// The antecedent term of the fuzzy rule.
        /// </param>
        /// <param name="consequent">
        /// The consequent term of the fuzzy rule.
        /// </param>
        public void AddRule(IFuzzyTerm antecedent, IFuzzyTerm consequent)
        {
            m_rules.Add(new FuzzyRule(antecedent, consequent));
        }

        /// <summary>
        /// Sets the fuzzy liguistic variable specified to 
        /// the fuzzified passed 'crisp' value.
        /// </summary>
        /// <param name="NameOfFLV">
        /// The name of the fuzzy linguist variable to use
        /// </param>
        /// <param name="val">
        /// The value to fuzzify.
        /// </param>
        public void Fuzzify(string nameOfFLV, double val)
        {
            //check the key exists
            m_debugMgr.Assert(m_variables.ContainsKey(nameOfFLV),
                    "FuzzyModule:Fuzzify - key not found");

            m_variables[nameOfFLV].Fuzzify(val);
        }

        /// <summary>
        /// Returns the 'crisp' value of a fuzzy variable.
        /// </summary>
        /// <param name="nameOfFLV">
        /// Name of the fuzzy variable to look-up.
        /// </param>
        /// <returns>
        /// 'Crisp' value equivalent of the fuzzy variable.
        /// </returns>
        public double DeFuzzify(string nameOfFLV)
        {
            //first make sure the key exists
            m_debugMgr.Assert(m_variables.ContainsKey(nameOfFLV),
                    "FuzzyModule:DeFuzzifyMaxAv - key not found");

            //clear the DOMs
            SetConfidencesOfConsequentsToZero();

            //process rules
            foreach (FuzzyRule fRule in m_rules)
            {
                fRule.Calculate();
            }

            //defuzzify the resultant
            return m_variables[nameOfFLV].Defuzzify();
        }

        #endregion

        #endregion
    }
}
