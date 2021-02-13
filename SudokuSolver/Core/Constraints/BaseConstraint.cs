using System.Collections.Generic;

namespace SudokuSolver.Core.Constraints
{
    public abstract class BaseConstraint
    {
        public abstract string Name();

        public virtual List<SolverTechnique> GetSolverTechniques() {
            return new List<SolverTechnique>();
        }

        public virtual List<Region> GetRegions(Puzzle puzzle)
        {
            return new List<Region>();
        }

    }
}
