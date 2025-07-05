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
    /// Interface of a FuzzyTerm.
    /// </summary>
    public interface IFuzzyTerm
    {
        IFuzzyTerm Clone();

        double DegreeOfMembership { get; }
        void ClearDOM();
        void InterpValsDOM(double val);
    }
}
