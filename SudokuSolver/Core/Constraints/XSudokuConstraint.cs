using System.Collections.Generic;

namespace SudokuSolver.Core.Constraints
{
    public class XSudokuConstraint : BaseConstraint
    {
        public override string Name()
        {
            return "x-sudoku";
        }

        public override List<Region> GetRegions(Puzzle puzzle)
        {
            var diagonal1 = new List<Cell>();
            var diagonal2 = new List<Cell>();
            for (int n = 0; n < 9; n++)
            {
                diagonal1.Add(puzzle[n,n]);
                diagonal2.Add(puzzle[8 - n,n]);
            }
            return new List<Region>(new List<Region>() { new Region(diagonal1.ToArray()), new Region(diagonal2.ToArray()) });
        }
    }
}
