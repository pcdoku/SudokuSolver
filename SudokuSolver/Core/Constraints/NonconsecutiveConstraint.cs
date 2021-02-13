using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Core.Constraints
{
    public class NonconsecutiveConstraint: BaseConstraint
    {
        public override string Name()
        {
            return "nonconsecutive";
        }

        public override List<SolverTechnique> GetSolverTechniques()
        {
            var techniques = new List<SolverTechnique>
            {
                new SolverTechnique(ConsecutiveCandidates, "Consecutive Candidates"), // TODO: do this in bulk like other classic candidate removal?
                new SolverTechnique(TupleWithAdjacentConsecutiveCandidate, "Tuple with adjacent consecutive candidate")
            };
            // TODO: advanced nonconsecutive
            // 1) https://discord.com/channels/709370620642852885/721090566481510732/803030352439672883
            // 2) https://discord.com/channels/709370620642852885/721090566481510732/803028814682783774
            return techniques;
        }

        private static bool ConsecutiveCandidates(Puzzle puzzle)
        {
            var offsets = new List<List<int>>() { new List<int> { 0, 1 }, new List<int> { 1, 0 } };

            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    foreach (var offset in offsets)
                    {
                        if ((x + offset[0] >= 9) || (y + offset[1] >= 9))
                            continue;
                        var cell1 = puzzle[x, y];
                        var cell2 = puzzle[x + offset[0], y + offset[1]];
                        if (TestAdjacentCellsForConsecutiveDigits(puzzle, cell1, cell2))
                            return true;
                    }
                }
            }
            if (TestAdjacentCellsForConsecutiveDigits(puzzle, puzzle[8, 7], puzzle[8, 8]))
                return true;
            if (TestAdjacentCellsForConsecutiveDigits(puzzle, puzzle[7, 8], puzzle[8, 8]))
                return true;
            return false;
        }

        private static bool TestAdjacentCellsForConsecutiveDigits(Puzzle puzzle, Cell cell1, Cell cell2)
        {
            if (cell1.Value == 0 && cell2.Value == 0)
            {
                // see if we have a consecutive pair
                if (cell1.Candidates.Count == 2 && cell1.Candidates == cell2.Candidates
                    && Math.Abs(cell1.Candidates.First() - cell1.Candidates.Skip(1).First()) == 1)
                {
                    // exceptions are not handled, so we have to make the solver fail by clearing all candidates
                    // TODO: throw new Exception("invalid puzzle, consecutive pair"); 
                    puzzle.LogAction(Puzzle.TechniqueFormat("Consecutive Digits ", "{0}: {1}", cell2.ToString(), cell2.Candidates.Print(), cell2, (Cell)null));
                    cell2.Candidates.Clear();
                    return true;
                }
                return false;
            }
            if (cell1.Value != 0 && cell2.Value != 0)
            {
                // see if we have a valid puzzle
                if (Math.Abs(cell1.Value - cell2.Value) == 1)
                {
                    // TODO: throw new Exception("invalid puzzle, nonconsecutive constraint"); 
                    puzzle.LogAction(Puzzle.TechniqueFormat("Consecutive Digits ", "{0}: {1}", cell2.ToString(), cell2.Candidates.Print(), cell2, (Cell)null));
                    var cell = cell1.OriginalValue == 0 ? cell1 : cell2;
                    puzzle.Set(cell, 0);
                    cell.Candidates.Clear();
                    return true;
                }
                return false;
            }
            // if we get here it is because once cell has a value and the other does not
            var knownCell = cell1.Value != 0 ? cell1 : cell2;
            var candidateCell = cell1.Value == 0 ? cell1 : cell2;

            var consecutiveValues = GetConsecutiveValues(knownCell.Value);
            // remove candidates which are 1 away from the known value
            var originalCandidates = new HashSet<int>(candidateCell.Candidates);
            if (puzzle.ChangeCandidates(candidateCell, consecutiveValues, remove: true))
            {
                puzzle.LogAction(Puzzle.TechniqueFormat("Consecutive Digits ", "{0}: {1}", candidateCell.ToString(), originalCandidates.Intersect(consecutiveValues).Print(), candidateCell, (Cell)null));
                return true;
            }
            return false;
        }

        /// <summary>
        /// All cells in the region with a particular candidate are orthogonally adjacent to the same cell.
        /// </summary>
        /// <param name="puzzle"></param>
        /// <returns></returns>
        private static bool TupleWithAdjacentConsecutiveCandidate(Puzzle puzzle)
        {
            var regions = new List<Region>(puzzle.Rows);
            regions.AddRange(puzzle.Columns);
            regions.AddRange(puzzle.Blocks);
            foreach (var region in regions)
            {
                for (var n = 1; n <= 9; n++)
                {
                    var cells = region.GetCellsWithCandidate(n).ToList();
                    var adjacentToAll = cells.Select(cell => CellsAdjacentToOrSelf(puzzle, cell)).IntersectAll().Where(c => c.Value == 0).ToList();
                    if (adjacentToAll.Count > 0)
                    {
                        var consecutiveCandidates = GetConsecutiveValues(n);
                        var originalCandidates = adjacentToAll.Select(x => new HashSet<int>(x.Candidates)).UniteAll();
                        if (puzzle.ChangeCandidates(adjacentToAll, consecutiveCandidates, remove: true))
                        {
                            puzzle.LogAction(Puzzle.TechniqueFormat("Tuple of " + n + "s with adjacent consecutive candidate ", "{0}: {1}", adjacentToAll.Print(), originalCandidates.Intersect(consecutiveCandidates).Print()), cells, (Cell)null);
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        private static HashSet<Cell> CellsAdjacentToOrSelf(Puzzle puzzle, Cell cell)
        {
            var adjacent = new HashSet<Cell>() { cell };
            if (cell.Point.X > 0)
                adjacent.Add(puzzle[cell.Point.X - 1, cell.Point.Y]);
            if (cell.Point.X < 8)
                adjacent.Add(puzzle[cell.Point.X + 1, cell.Point.Y]);
            if (cell.Point.Y > 0)
                adjacent.Add(puzzle[cell.Point.X, cell.Point.Y - 1]);
            if (cell.Point.Y < 8)
                adjacent.Add(puzzle[cell.Point.X, cell.Point.Y + 1]);
            return adjacent;
        }

        private static List<int> GetConsecutiveValues(int value)
        {
            var consecutiveValues = new List<int>();
            if (value < 9)
                consecutiveValues.Add(value + 1);
            if (value > 1)
                consecutiveValues.Add(value - 1);
            return consecutiveValues;
        }

    }
}
