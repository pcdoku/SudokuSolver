using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Core.Constraints
{
    /// <summary>
    /// TODO: This is incomplete
    /// </summary>
    public class NonconsecutivePathConstraint: BaseConstraint
    {
        public override string Name()
        {
            return "nonconsecutivepath";
        }

        public override List<SolverTechnique> GetSolverTechniques()
        {
            var techniques = new List<SolverTechnique>
            {
                new SolverTechnique(NonConsecutivePath, "Nonconsecutive Path"), 
            };
            return techniques;
        }

        private static bool NonConsecutivePath(Puzzle puzzle)
        {
            // Adjacent digits along the marked grey lines may not be consecutive
            // using this fractal pattern: https://f-puzzles.com/?id=yxsenx7s
            var adjacentPairs = PeanoAdjacentPairs(puzzle);
            foreach (var pair in adjacentPairs)
            {
                if (pair[0].Value == 0 && pair[1].Value == 0)
                    continue;

                if (pair[0].Value != 0 && pair[1].Value != 0)
                {
                    if (Math.Abs(pair[0].Value - pair[1].Value) == 1)
                    {
                        throw new Exception("invalid puzzle, nonconsecutive constraint");
                    }
                    continue;
                }
                var knownIndex = pair[0].Value != 0 ? 0 : 1;
                var knownValue = pair[knownIndex].Value;
                var otherIndex = 1 - knownIndex;
                var forbiddenValues = new List<int>();
                if (knownValue > 1)
                    forbiddenValues.Add(knownValue - 1);
                if (knownValue < 9)
                    forbiddenValues.Add(knownValue + 1);

                if (pair[otherIndex].Candidates.Overlaps(forbiddenValues))
                {
                    var overlappingValues = pair[otherIndex].Candidates.Intersect(forbiddenValues).ToList();
                    foreach (int value in overlappingValues)
                        pair[otherIndex].Candidates.Remove(value);
                    puzzle.LogAction(Puzzle.TechniqueFormat("Consecutive digits on path", "{0}: {1}", pair.Print(), overlappingValues.Print()), pair, pair[otherIndex]);
                    return true;
                }

            }
            return false;
        }

        private static List<Cell[]> PeanoAdjacentPairs(Puzzle puzzle)
        {
            var adjacentPairs = new List<Cell[]>();
            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    if ((boxCol + boxRow) % 2 == 0)
                    {
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3, boxRow * 3 + 2], puzzle[boxCol * 3, boxRow * 3 + 1] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3, boxRow * 3 + 1], puzzle[boxCol * 3, boxRow * 3] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3, boxRow * 3], puzzle[boxCol * 3 + 1, boxRow * 3] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 1, boxRow * 3], puzzle[boxCol * 3 + 1, boxRow * 3 + 1] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 1, boxRow * 3 + 1], puzzle[boxCol * 3 + 1, boxRow * 3 + 2] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 1, boxRow * 3 + 2], puzzle[boxCol * 3 + 2, boxRow * 3 + 2] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 2, boxRow * 3 + 2], puzzle[boxCol * 3 + 2, boxRow * 3 + 1] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 2, boxRow * 3 + 1], puzzle[boxCol * 3 + 2, boxRow * 3] });
                    }
                    else
                    {
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3, boxRow * 3], puzzle[boxCol * 3, boxRow * 3 + 1] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3, boxRow * 3 + 1], puzzle[boxCol * 3, boxRow * 3 + 2] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3, boxRow * 3 + 2], puzzle[boxCol * 3 + 1, boxRow * 3 + 2] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 1, boxRow * 3 + 2], puzzle[boxCol * 3 + 1, boxRow * 3 + 1] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 1, boxRow * 3 + 1], puzzle[boxCol * 3 + 1, boxRow * 3] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 1, boxRow * 3], puzzle[boxCol * 3 + 2, boxRow * 3] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 2, boxRow * 3], puzzle[boxCol * 3 + 2, boxRow * 3 + 1] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 2, boxRow * 3 + 1], puzzle[boxCol * 3 + 2, boxRow * 3 + 2] });

                    }
                }
            }
            // connect path between boxes
            adjacentPairs.Add(new[] { puzzle[2, 6], puzzle[2, 5] });
            adjacentPairs.Add(new[] { puzzle[0, 3], puzzle[0, 2] });
            adjacentPairs.Add(new[] { puzzle[2, 0], puzzle[3, 0] });
            adjacentPairs.Add(new[] { puzzle[5, 2], puzzle[5, 3] });
            adjacentPairs.Add(new[] { puzzle[3, 5], puzzle[3, 6] });
            adjacentPairs.Add(new[] { puzzle[5, 8], puzzle[6, 8] });
            adjacentPairs.Add(new[] { puzzle[8, 6], puzzle[8, 5] });
            adjacentPairs.Add(new[] { puzzle[6, 3], puzzle[6, 2] });

            return adjacentPairs;
        }

    }
}
