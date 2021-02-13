using System.Collections.Generic;

namespace SudokuSolver.Core.Constraints
{
    public abstract class ChessConstraint: BaseConstraint
    {
        protected static bool AntiChessMoveConstraint(Puzzle puzzle, List<List<int>> offsets, string constraintName)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    foreach (var offset in offsets)
                    {
                        var cell2_x = x + offset[0];
                        var cell2_y = y + offset[1];
                        if (cell2_x >= 0 && cell2_x < 9 && cell2_y >= 0 && cell2_y < 9)
                        {
                            var cell1 = puzzle[x, y];
                            var cell2 = puzzle[cell2_x, cell2_y];
                            if ((cell1.Value == 0) != (cell2.Value == 0))
                            {
                                var knownCell = cell1.Value != 0 ? cell1 : cell2;
                                var candidateCell = cell1.Value == 0 ? cell1 : cell2;
                                if (puzzle.ChangeCandidates(candidateCell, knownCell.Value, remove: true))
                                {
                                    puzzle.LogAction(Puzzle.TechniqueFormat(constraintName, "{0}: {1}", candidateCell.ToString(), knownCell.Value, candidateCell, (Cell)null));
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

    }

    public class AntiKingConstraint : ChessConstraint
    {
        public override string Name()
        {
            return "antiking";
        }

        public override List<SolverTechnique> GetSolverTechniques()
        {
            var techniques = new List<SolverTechnique>
            {
                new SolverTechnique(AntiKing, Name())
            };

            return techniques;
        }

        private static bool AntiKing(Puzzle puzzle)
        {
            var offsets = new List<List<int>>() {
                new List<int> { 1, -1 },
                new List<int> { 1, 1 }
            };
            return AntiChessMoveConstraint(puzzle, offsets, "AntiKing Constraint ");
        }
    }

    public class AntiKnightConstraint: ChessConstraint
    {
        public override string Name()
        {
            return "antiknight";
        }

        public override List<SolverTechnique> GetSolverTechniques()
        {
            var techniques = new List<SolverTechnique>
            {
                new SolverTechnique(AntiKnight, Name())
                // TODO: Remove candidates which can be "seen" by every candidate of a neighboring box, similar to TupleWithAdjacentConsecutiveCandidate, except not adjacent
                //   a better name might be ChessPointingTuple or PointingChessTuple
            };

            return techniques;
        }

        private static bool AntiKnight(Puzzle puzzle)
        {
            var offsets = new List<List<int>>() {
                new List<int> { -2, 1 },
                new List<int> { 2, 1 },
                new List<int> { -1, 2 },
                new List<int> { 1, 2 },
            };
            return AntiChessMoveConstraint(puzzle, offsets, "AntiKnight Constraint ");
        }



    }
}
