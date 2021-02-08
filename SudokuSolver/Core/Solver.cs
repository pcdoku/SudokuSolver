using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SudokuSolver.Core
{
    public sealed class Solver
    {
        private sealed class SolverTechnique
        {
            public Func<Puzzle, bool> Function { get; }
            /// <summary>Currently unused.</summary>
            public string Url { get; }

            public SolverTechnique(Func<Puzzle, bool> function, string url)
            {
                Function = function;
                Url = url;
            }
        }

        private static readonly SolverTechnique[] _classicSudokuTechniquesBasic = new SolverTechnique[]
        {
            // see https://www.sudokuwiki.org/sudoku.htm for suggested order 
            new SolverTechnique(HiddenSingle, "Hidden single"),
            new SolverTechnique(NakedPair, "https://www.sudokuwiki.org/Naked_Candidates"),
            new SolverTechnique(NakedTriple, "https://www.sudokuwiki.org/Naked_Candidates"),
            new SolverTechnique(HiddenPair, "https://www.sudokuwiki.org/Hidden_Candidates"),
            new SolverTechnique(HiddenTriple, "https://www.sudokuwiki.org/Hidden_Candidates"),
            new SolverTechnique(NakedQuadruple, "https://www.sudokuwiki.org/Naked_Candidates"),
            new SolverTechnique(HiddenQuadruple, "https://www.sudokuwiki.org/Hidden_Candidates"),
            new SolverTechnique(PointingTuple, "https://hodoku.sourceforge.net/en/tech_intersections.php#lc1"),
        };

        private static readonly SolverTechnique[] _classicSudokuTechniquesAdvanced = new SolverTechnique[]
        {
            new SolverTechnique(LockedCandidate, "https://hodoku.sourceforge.net/en/tech_intersections.php#lc1"),
            new SolverTechnique(XWing, "https://www.sudokuwiki.org/X_Wing_Strategy"),
            new SolverTechnique(YWing, "https://www.sudokuwiki.org/Y_Wing_Strategy"),
            new SolverTechnique(Swordfish, "https://www.sudokuwiki.org/Sword_Fish_Strategy"),
            new SolverTechnique(XYZWing, "https://www.sudokuwiki.org/XYZ_Wing"),
            new SolverTechnique(XYChain, "https://www.sudokuwiki.org/XY_Chains"),
            new SolverTechnique(Jellyfish, "https://www.sudokuwiki.org/Jelly_Fish_Strategy"),
            new SolverTechnique(UniqueRectangle, "https://www.sudokuwiki.org/Unique_Rectangles"),
            new SolverTechnique(HiddenRectangle, "https://hodoku.sourceforge.net/en/tech_ur.php#hr"),
            new SolverTechnique(AvoidableRectangle, "https://www.sudokuwiki.org/Avoidable_Rectangles")
        };

        public Puzzle Puzzle { get; }
        private List<SolverTechnique> Techniques { get; }

        public Solver(Puzzle puzzle)
        {
            Puzzle = puzzle;
            Techniques = new List<SolverTechnique>(_classicSudokuTechniquesBasic);
            if (puzzle.Constraints.Contains("nonconsecutive"))
            {
                Techniques.Add(new SolverTechnique(ConsecutiveCandidates, "Consecutive Candidates")); // TODO: do this in bulk like other classic candidate removal?
                Techniques.Add(new SolverTechnique(TupleWithAdjacentConsecutiveCandidate, "Tuple with adjacent consecutive candidate"));
                // TODO: advanced nonconsecutive
                // 1) https://discord.com/channels/709370620642852885/721090566481510732/803030352439672883
                // 2) https://discord.com/channels/709370620642852885/721090566481510732/803028814682783774
            }
            if (puzzle.Constraints.Contains("antiknight"))
            {
                Techniques.Add(new SolverTechnique(AntiKnight, "antiknight"));
                // TODO: Remove candidates which can be "seen" by every candidate of a neighboring box, similar to TupleWithAdjacentConsecutiveCandidate, except not adjacent
            }
            if (puzzle.Constraints.Contains("antiking"))
            {
                Techniques.Add(new SolverTechnique(AntiKing, "antiking"));
            }
            Techniques.AddRange(_classicSudokuTechniquesAdvanced);
        }

        public void DoWork(object sender, DoWorkEventArgs e)
        {
            Puzzle.RefreshCandidates();
            Puzzle.LogAction("Begin");

            bool solved; // If this is true after a segment, the puzzle is solved and we can break
            do
            {
                solved = true;

                bool changed = false;
                // Check for naked singles or a completed puzzle
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        Cell cell = Puzzle[x, y];
                        if (cell.Value == 0)
                        {
                            solved = false;
                            // Check for naked singles
                            int[] a = cell.Candidates.ToArray(); // Copy
                            if (a.Length == 1)
                            {
                                Puzzle.Set(cell, a[0]);
                                Puzzle.LogAction(Puzzle.TechniqueFormat("Naked single", "{0}: {1}", cell, a[0]), cell, (Cell)null);
                                changed = true;
                            }
                        }
                    }
                }
                // Solved or failed to solve
                if (solved || (!changed && !RunTechnique()))
                {
                    break;
                }
            } while (true);

            e.Result = solved;
        }

        private bool RunTechnique()
        {
            foreach (SolverTechnique t in Techniques)
            {
                // Skip the previous technique unless it is good to repeat it
                if (t.Function.Invoke(Puzzle))
                {
                    return true;
                }
            }
            return false;
        }

        #region Methods

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

        private static bool AntiKing(Puzzle puzzle)
        {
            var offsets = new List<List<int>>() {
                new List<int> { 1, -1 },
                new List<int> { 1, 1 }
            };
            return AntiChessMoveConstraint(puzzle, offsets, "AntiKing Constraint ");
        }

        private static bool AntiChessMoveConstraint(Puzzle puzzle, List<List<int>> offsets, string constraintName)
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
                            puzzle.LogAction(Puzzle.TechniqueFormat("Tuple of "+n+"s with adjacent consecutive candidate ", "{0}: {1}", adjacentToAll.Print(), originalCandidates.Intersect(consecutiveCandidates).Print()), cells, (Cell)null);
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

        private static List<int> GetConsecutiveValues(int value)
        {
            var consecutiveValues = new List<int>();
            if (value < 9)
                consecutiveValues.Add(value + 1);
            if (value > 1)
                consecutiveValues.Add(value - 1);
            return consecutiveValues;
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

        private static bool NonConsecutivePath(Puzzle puzzle)
        {
            // Adjacent digits along the marked grey lines may not be consecutive
            // using this fractal pattern: https://f-puzzles.com/?id=yxsenx7s
            var adjacentPairs = PeanoAdjacentPairs(puzzle);
            foreach(var pair in adjacentPairs)
            {
                if (pair[0].Value == 0 && pair[1].Value == 0)
                    continue;

                if (pair[0].Value != 0 && pair[1].Value!= 0)
                {
                    if (Math.Abs(pair[0].Value- pair[1].Value) == 1)
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
                for (int boxCol = 0; boxCol < 3; boxCol++ )
                {
                    if ((boxCol+boxRow)%2 == 0)
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
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3, boxRow * 3 + 1], puzzle[boxCol * 3, boxRow * 3 +2] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3, boxRow * 3 +2], puzzle[boxCol * 3 + 1, boxRow * 3+2] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 1, boxRow * 3+2], puzzle[boxCol * 3 + 1, boxRow * 3 + 1] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 1, boxRow * 3 + 1], puzzle[boxCol * 3 + 1, boxRow * 3] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 1, boxRow * 3], puzzle[boxCol * 3 + 2, boxRow * 3] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 2, boxRow * 3], puzzle[boxCol * 3 + 2, boxRow * 3 + 1] });
                        adjacentPairs.Add(new[] { puzzle[boxCol * 3 + 2, boxRow * 3 + 1], puzzle[boxCol * 3 + 2, boxRow * 3+2 ] });

                    }
                }
            }
            // connect path between boxes
            adjacentPairs.Add(new[] { puzzle[2, 6], puzzle[2,5] });
            adjacentPairs.Add(new[] { puzzle[0, 3], puzzle[0, 2] });
            adjacentPairs.Add(new[] { puzzle[2, 0], puzzle[3, 0] });
            adjacentPairs.Add(new[] { puzzle[5, 2], puzzle[5, 3] });
            adjacentPairs.Add(new[] { puzzle[3, 5], puzzle[3, 6] });
            adjacentPairs.Add(new[] { puzzle[5, 8], puzzle[6, 8] });
            adjacentPairs.Add(new[] { puzzle[8, 6], puzzle[8, 5] });
            adjacentPairs.Add(new[] { puzzle[6, 3], puzzle[6, 2] });

            return adjacentPairs;
        }

        private static bool AvoidableRectangle(Puzzle puzzle)
        {
            for (int type = 1; type <= 2; type++)
            {
                for (int x1 = 0; x1 < 9; x1++)
                {
                    Region c1 = puzzle.Columns[x1];
                    for (int x2 = x1 + 1; x2 < 9; x2++)
                    {
                        Region c2 = puzzle.Columns[x2];
                        for (int y1 = 0; y1 < 9; y1++)
                        {
                            for (int y2 = y1 + 1; y2 < 9; y2++)
                            {
                                for (int value1 = 1; value1 <= 9; value1++)
                                {
                                    for (int value2 = value1 + 1; value2 <= 9; value2++)
                                    {
                                        int[] candidates = new int[] { value1, value2 };
                                        var cells = new Cell[] { c1[y1], c1[y2], c2[y1], c2[y2] };
                                        if (cells.Any(c => c.OriginalValue != 0))
                                        {
                                            continue;
                                        }

                                        IEnumerable<Cell> alreadySet = cells.Where(c => c.Value != 0),
                                            notSet = cells.Where(c => c.Value == 0);

                                        switch (type)
                                        {
                                            case 1:
                                            {
                                                if (alreadySet.Count() != 3)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            case 2:
                                            {
                                                if (alreadySet.Count() != 2)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                        }
                                        var pairs = new Cell[][]
                                        {
                                            new Cell[] { cells[0], cells[3] },
                                            new Cell[] { cells[1], cells[2] }
                                        };
                                        foreach (Cell[] pair in pairs)
                                        {
                                            Cell[] otherPair = pair == pairs[0] ? pairs[1] : pairs[0];
                                            foreach (int i in candidates)
                                            {
                                                int otherVal = candidates.Single(ca => ca != i);
                                                if (((pair[0].Value == i && pair[1].Value == 0 && pair[1].Candidates.Count == 2 && pair[1].Candidates.Contains(i))
                                                    || (pair[1].Value == i && pair[0].Value == 0 && pair[0].Candidates.Count == 2 && pair[0].Candidates.Contains(i)))
                                                    && otherPair.All(c => c.Value == otherVal || (c.Candidates.Count == 2 && c.Candidates.Contains(otherVal))))
                                                {
                                                    goto breakpairs;
                                                }
                                            }
                                        }
                                        continue; // Did not find
                                    breakpairs:
                                        bool changed = false;
                                        switch (type)
                                        {
                                            case 1:
                                            {
                                                Cell cell = notSet.ElementAt(0);
                                                if (cell.Candidates.Count == 2)
                                                {
                                                        puzzle.Set(cell, cell.Candidates.Except(candidates).ElementAt(0));
                                                }
                                                else
                                                {
                                                    puzzle.ChangeCandidates(cell, cell.Candidates.Intersect(candidates));
                                                }
                                                changed = true;
                                                break;
                                            }
                                            case 2:
                                            {
                                                IEnumerable<int> commonCandidates = notSet.Select(c => c.Candidates.Except(candidates)).IntersectAll();
                                                if (commonCandidates.Any()
                                                    && puzzle.ChangeCandidates(notSet.Select(c => puzzle.GetCellsVisibleClassicSudoku(c)).IntersectAll(), commonCandidates))
                                                {
                                                    changed = true;
                                                }
                                                break;
                                            }
                                        }

                                        if (changed)
                                        {
                                            puzzle.LogAction(Puzzle.TechniqueFormat("Avoidable rectangle", "{0}: {1}", cells.Print(), candidates.Print()), cells, (Cell)null);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool HiddenRectangle(Puzzle puzzle)
        {
            for (int x1 = 0; x1 < 9; x1++)
            {
                Region c1 = puzzle.Columns[x1];
                for (int x2 = x1 + 1; x2 < 9; x2++)
                {
                    Region c2 = puzzle.Columns[x2];
                    for (int y1 = 0; y1 < 9; y1++)
                    {
                        for (int y2 = y1 + 1; y2 < 9; y2++)
                        {
                            for (int value1 = 1; value1 <= 9; value1++)
                            {
                                for (int value2 = value1 + 1; value2 <= 9; value2++)
                                {
                                    int[] candidates = new int[] { value1, value2 };
                                    var cells = new Cell[] { c1[y1], c1[y2], c2[y1], c2[y2] };
                                    if (cells.Any(c => !c.Candidates.ContainsAll(candidates)))
                                    {
                                        continue;
                                    }
                                    ILookup<int, Cell> l = cells.ToLookup(c => c.Candidates.Count);
                                    IEnumerable<Cell> gtTwo = l.Where(g => g.Key > 2).SelectMany(g => g);
                                    int gtTwoCount = gtTwo.Count();
                                    if (gtTwoCount < 2 || gtTwoCount > 3)
                                    {
                                        continue;
                                    }

                                    bool changed = false;
                                    foreach (Cell c in l[2])
                                    {
                                        int eks = c.Point.X == x1 ? x2 : x1,
                                            why = c.Point.Y == y1 ? y2 : y1;
                                        foreach (int i in candidates)
                                        {
                                            if (!puzzle.Rows[why].GetCellsWithCandidate(i).Except(cells).Any() // "i" only appears in our UR
                                                && !puzzle.Columns[eks].GetCellsWithCandidate(i).Except(cells).Any())
                                            {
                                                Cell diag = puzzle[eks, why];
                                                if (diag.Candidates.Count == 2)
                                                {
                                                    puzzle.Set(diag, i);
                                                }
                                                else
                                                {
                                                    puzzle.ChangeCandidates(diag, i == value1 ? value2 : value1);
                                                }
                                                changed = true;
                                            }
                                        }
                                    }
                                    if (changed)
                                    {
                                        puzzle.LogAction(Puzzle.TechniqueFormat("Hidden rectangle", "{0}: {1}", cells.Print(), candidates.Print()), cells, (Cell)null);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool UniqueRectangle(Puzzle puzzle)
        {
            for (int type = 1; type <= 6; type++) // Type
            {
                for (int x1 = 0; x1 < 9; x1++)
                {
                    Region c1 = puzzle.Columns[x1];
                    for (int x2 = x1 + 1; x2 < 9; x2++)
                    {
                        Region c2 = puzzle.Columns[x2];
                        for (int y1 = 0; y1 < 9; y1++)
                        {
                            for (int y2 = y1 + 1; y2 < 9; y2++)
                            {
                                for (int value1 = 1; value1 <= 9; value1++)
                                {
                                    for (int value2 = value1 + 1; value2 <= 9; value2++)
                                    {
                                        int[] candidates = new int[] { value1, value2 };
                                        var cells = new Cell[] { c1[y1], c1[y2], c2[y1], c2[y2] };
                                        if (cells.Any(c => !c.Candidates.ContainsAll(candidates)))
                                        {
                                            continue;
                                        }

                                        ILookup<int, Cell> l = cells.ToLookup(c => c.Candidates.Count);
                                        Cell[] gtTwo = l.Where(g => g.Key > 2).SelectMany(g => g).ToArray(),
                                            two = l[2].ToArray(), three = l[3].ToArray(), four = l[4].ToArray();

                                        switch (type) // Check for candidate counts
                                        {
                                            case 1:
                                            {
                                                if (two.Length != 3 || gtTwo.Length != 1)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            case 2:
                                            case 6:
                                            {
                                                if (two.Length != 2 || three.Length != 2)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            case 3:
                                            {
                                                if (two.Length != 2 || gtTwo.Length != 2)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            case 4:
                                            {
                                                if (two.Length != 2 || three.Length != 1 || four.Length != 1)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            case 5:
                                            {
                                                if (two.Length != 1 || three.Length != 3)
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                        }

                                        switch (type) // Check for extra rules
                                        {
                                            case 1:
                                            {
                                                if (gtTwo[0].Candidates.Count == 3)
                                                {
                                                    puzzle.Set(gtTwo[0], gtTwo[0].Candidates.Single(c => !candidates.Contains(c)));
                                                }
                                                else
                                                {
                                                    puzzle.ChangeCandidates(gtTwo, candidates);
                                                }
                                                break;
                                            }
                                            case 2:
                                            {
                                                if (!three[0].Candidates.SetEquals(three[1].Candidates))
                                                {
                                                    continue;
                                                }
                                                if (!puzzle.ChangeCandidates(puzzle.GetCellsVisibleClassicSudoku(three[0]).Intersect(puzzle.GetCellsVisibleClassicSudoku(three[1])), three[0].Candidates.Except(candidates)))
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            case 3:
                                            {
                                                if (gtTwo[0].Point.X != gtTwo[1].Point.X && gtTwo[0].Point.Y != gtTwo[1].Point.Y)
                                                {
                                                    continue; // Must be non-diagonal
                                                }
                                                IEnumerable<int> others = gtTwo[0].Candidates.Except(candidates).Union(gtTwo[1].Candidates.Except(candidates));
                                                if (others.Count() > 4 || others.Count() < 2)
                                                {
                                                    continue;
                                                }
                                                IEnumerable<Cell> nSubset = ((gtTwo[0].Point.Y == gtTwo[1].Point.Y) ? // Same row
                                                        puzzle.Rows[gtTwo[0].Point.Y] : puzzle.Columns[gtTwo[0].Point.X])
                                                        .Where(c => c.Candidates.ContainsAny(others) && !c.Candidates.ContainsAny(Utils.OneToNine.Except(others)));
                                                if (nSubset.Count() != others.Count() - 1)
                                                {
                                                    continue;
                                                }
                                                if (!puzzle.ChangeCandidates(nSubset.Union(gtTwo).Select(c => puzzle.GetCellsVisibleClassicSudoku(c)).IntersectAll(), others))
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            case 4:
                                            {
                                                int[] remove = new int[1];
                                                if (four[0].Point.BlockIndex == three[0].Point.BlockIndex)
                                                {
                                                    if (puzzle.Blocks[four[0].Point.BlockIndex].GetCellsWithCandidate(value1).Count() == 2)
                                                    {
                                                        remove[0] = value2;
                                                    }
                                                    else if (puzzle.Blocks[four[0].Point.BlockIndex].GetCellsWithCandidate(value2).Count() == 2)
                                                    {
                                                        remove[0] = value1;
                                                    }
                                                }
                                                if (remove[0] != 0) // They share the same row/column but not the same block
                                                {
                                                    if (three[0].Point.X == three[0].Point.X)
                                                    {
                                                        if (puzzle.Columns[four[0].Point.X].GetCellsWithCandidate(value1).Count() == 2)
                                                        {
                                                            remove[0] = value2;
                                                        }
                                                        else if (puzzle.Columns[four[0].Point.X].GetCellsWithCandidate(value2).Count() == 2)
                                                        {
                                                            remove[0] = value1;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (puzzle.Rows[four[0].Point.Y].GetCellsWithCandidate(value1).Count() == 2)
                                                        {
                                                            remove[0] = value2;
                                                        }
                                                        else if (puzzle.Rows[four[0].Point.Y].GetCellsWithCandidate(value2).Count() == 2)
                                                        {
                                                            remove[0] = value1;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                puzzle.ChangeCandidates(cells.Except(l[2]), remove);
                                                break;
                                            }
                                            case 5:
                                            {
                                                if (!three[0].Candidates.SetEquals(three[1].Candidates) || !three[1].Candidates.SetEquals(three[2].Candidates))
                                                {
                                                    continue;
                                                }
                                                if (!puzzle.ChangeCandidates(three.Select(c => puzzle.GetCellsVisibleClassicSudoku(c)).IntersectAll(), three[0].Candidates.Except(candidates)))
                                                {
                                                    continue;
                                                }
                                                break;
                                            }
                                            case 6:
                                            {
                                                if (three[0].Point.X == three[1].Point.X)
                                                {
                                                    continue;
                                                }
                                                int set;
                                                if (c1.GetCellsWithCandidate(value1).Count() == 2 && c2.GetCellsWithCandidate(value1).Count() == 2 // Check if "v" only appears in the UR
                                                    && puzzle.Rows[two[0].Point.Y].GetCellsWithCandidate(value1).Count() == 2
                                                        && puzzle.Rows[two[1].Point.Y].GetCellsWithCandidate(value1).Count() == 2)
                                                {
                                                    set = value1;
                                                }
                                                else if (c1.GetCellsWithCandidate(value2).Count() == 2 && c2.GetCellsWithCandidate(value2).Count() == 2
                                                    && puzzle.Rows[two[0].Point.Y].GetCellsWithCandidate(value2).Count() == 2
                                                        && puzzle.Rows[two[1].Point.Y].GetCellsWithCandidate(value2).Count() == 2)
                                                {
                                                    set = value2;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                puzzle.Set(two[0], set);
                                                puzzle.Set(two[1], set);
                                                break;
                                            }
                                        }

                                        puzzle.LogAction(Puzzle.TechniqueFormat("Unique rectangle", "{0}: {1}", cells.Print(), candidates.Print()), cells, (Cell)null);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool XYChain(Puzzle puzzle)
        {
            bool Recursion(Cell startCell, List<Cell> ignore, Cell currentCell, int theOneThatWillEndItAllBaybee, int mustFind)
            {
                ignore.Add(currentCell);
                IEnumerable<Cell> visible = puzzle.GetCellsVisibleClassicSudoku(currentCell).Except(ignore);
                foreach (Cell cell in visible)
                {
                    if (cell.Candidates.Count != 2)
                    {
                        continue; // Must have two candidates
                    }
                    if (!cell.Candidates.Contains(mustFind))
                    {
                        continue; // Must have "mustFind"
                    }
                    int otherCandidate = cell.Candidates.Except(new int[] { mustFind }).Single();
                    // Check end condition
                    if (otherCandidate == theOneThatWillEndItAllBaybee && startCell != currentCell)
                    {
                        Cell[] commonVisibleWithStartCell = puzzle.GetCellsVisibleClassicSudoku(cell).Intersect(puzzle.GetCellsVisibleClassicSudoku(startCell)).ToArray();
                        if (commonVisibleWithStartCell.Length > 0)
                        {
                            IEnumerable<Cell> commonWithEndingCandidate = commonVisibleWithStartCell.Where(c => c.Candidates.Contains(theOneThatWillEndItAllBaybee));
                            if (puzzle.ChangeCandidates(commonWithEndingCandidate, theOneThatWillEndItAllBaybee))
                            {
                                ignore.Remove(startCell); // Remove here because we're now using "ignore" as "semiCulprits" and exiting
                                var culprits = new Cell[] { startCell, cell };
                                puzzle.LogAction(Puzzle.TechniqueFormat("XY-Chain", "{0}-{1}: {2}", culprits.Print(), ignore.SingleOrMultiToString(), theOneThatWillEndItAllBaybee), culprits, ignore);
                                return true;
                            }
                        }
                    }
                    // Loop again
                    if (Recursion(startCell, ignore, cell, theOneThatWillEndItAllBaybee, otherCandidate))
                    {
                        return true;
                    }
                }
                ignore.Remove(currentCell);
                return false;
            }

            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = puzzle[x, y];
                    if (cell.Candidates.Count != 2)
                    {
                        continue; // Must have two candidates
                    }
                    var ignore = new List<Cell>();
                    int start1 = cell.Candidates.ElementAt(0);
                    int start2 = cell.Candidates.ElementAt(1);
                    if (Recursion(cell, ignore, cell, start1, start2) || Recursion(cell, ignore, cell, start2, start1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool XYZWing(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                bool FindXYZWings(Region region)
                {
                    bool changed = false;
                    Cell[] cells2 = region.Where(c => c.Candidates.Count == 2).ToArray();
                    Cell[] cells3 = region.Where(c => c.Candidates.Count == 3).ToArray();
                    if (cells2.Length > 0 && cells3.Length > 0)
                    {
                        for (int j = 0; j < cells2.Length; j++)
                        {
                            Cell c2 = cells2[j];
                            for (int k = 0; k < cells3.Length; k++)
                            {
                                Cell c3 = cells3[k];
                                if (c2.Candidates.Intersect(c3.Candidates).Count() != 2)
                                {
                                    continue;
                                }

                                IEnumerable<Cell> c3Sees = puzzle.GetCellsVisibleClassicSudoku(c3).Except(region)
                                    .Where(c => c.Candidates.Count == 2 // If it has 2 candidates
                                    && c.Candidates.Intersect(c3.Candidates).Count() == 2 // Shares them both with p3
                                    && c.Candidates.Intersect(c3.Candidates).Count() == 1); // And shares one with p2
                                foreach (Cell c2_2 in c3Sees)
                                {
                                    IEnumerable<Cell> allSee = puzzle.GetCellsVisibleClassicSudoku(c2).Intersect(puzzle.GetCellsVisibleClassicSudoku(c3)).Intersect(puzzle.GetCellsVisibleClassicSudoku(c2_2));
                                    int allHave = c2.Candidates.Intersect(c3.Candidates).Intersect(c2_2.Candidates).Single(); // Will be 1 Length
                                    if (puzzle.ChangeCandidates(allSee, allHave))
                                    {
                                        var culprits = new Cell[] { c2, c3, c2_2 };
                                        puzzle.LogAction(Puzzle.TechniqueFormat("XYZ-Wing", "{0}: {1}", culprits.Print(), allHave), culprits, (Cell)null);
                                        changed = true;
                                    }
                                }
                            }
                        }
                    }
                    return changed;
                }

                if (FindXYZWings(puzzle.Rows[i]) || FindXYZWings(puzzle.Columns[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool YWing(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                bool FindYWings(Region region)
                {
                    Cell[] cells = region.Where(c => c.Candidates.Count == 2).ToArray();
                    if (cells.Length > 1)
                    {
                        for (int j = 0; j < cells.Length; j++)
                        {
                            Cell c1 = cells[j];
                            for (int k = j + 1; k < cells.Length; k++)
                            {
                                Cell c2 = cells[k];
                                IEnumerable<int> inter = c1.Candidates.Intersect(c2.Candidates);
                                if (inter.Count() != 1)
                                {
                                    continue;
                                }

                                int other1 = c1.Candidates.Except(inter).ElementAt(0),
                                    other2 = c2.Candidates.Except(inter).ElementAt(0);

                                var a = new Cell[] { c1, c2 };
                                foreach (Cell cell in a)
                                {
                                    IEnumerable<Cell> c3a = puzzle.GetCellsVisibleClassicSudoku(cell).Except(cells).Where(c => c.Candidates.Count == 2 && c.Candidates.Intersect(new int[] { other1, other2 }).Count() == 2);
                                    if (c3a.Count() == 1) // Example: p1 and p3 see each other, so remove similarities from p2 and p3
                                    {
                                        Cell c3 = c3a.ElementAt(0);
                                        Cell cOther = a.Single(c => c != cell);
                                        IEnumerable<Cell> commonCells = puzzle.GetCellsVisibleClassicSudoku(cOther).Intersect(puzzle.GetCellsVisibleClassicSudoku(c3));
                                        int candidate = cOther.Candidates.Intersect(c3.Candidates).Single(); // Will just be 1 candidate
                                        if (puzzle.ChangeCandidates(commonCells, candidate))
                                        {
                                            var culprits = new Cell[] { c1, c2, c3 };
                                            puzzle.LogAction(Puzzle.TechniqueFormat("Y-Wing", "{0}: {1}", culprits.Print(), candidate), culprits, (Cell)null);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }

                if (FindYWings(puzzle.Rows[i]) || FindYWings(puzzle.Columns[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool Jellyfish(Puzzle puzzle)
        {
            return FindFish(puzzle, 4);
        }

        private static bool Swordfish(Puzzle puzzle)
        {
            return FindFish(puzzle, 3);
        }

        private static bool XWing(Puzzle puzzle)
        {
            return FindFish(puzzle, 2);
        }

        private static bool PointingTuple(Puzzle puzzle)
        {
            for (int i = 0; i < 3; i++)
            {
                Cell[][] blockrow = new Cell[3][], blockcol = new Cell[3][];
                for (int r = 0; r < 3; r++)
                {
                    blockrow[r] = puzzle.Blocks[r + (i * 3)].ToArray();
                    blockcol[r] = puzzle.Blocks[i + (r * 3)].ToArray();
                }
                for (int r = 0; r < 3; r++) // 3 blocks in a blockrow/blockcolumn
                {
                    int[][] rowCandidates = new int[3][], colCand = new int[3][];
                    for (int j = 0; j < 3; j++) // 3 rows/columns in block
                    {
                        // The 3 cells' candidates in a block's row/column
                        rowCandidates[j] = blockrow[r].GetRowInBlock(j).Select(c => c.Candidates).UniteAll().ToArray();
                        colCand[j] = blockcol[r].GetColumnInBlock(j).Select(c => c.Candidates).UniteAll().ToArray();
                    }

                    bool RemovePointingTuple(bool doRows, int rcIndex, IEnumerable<int> candidates)
                    {
                        bool changed = false;
                        for (int j = 0; j < 3; j++)
                        {
                            if (j == r)
                            {
                                continue;
                            }
                            Cell[] rcs = doRows ? blockrow[j].GetRowInBlock(rcIndex) : blockcol[j].GetColumnInBlock(rcIndex);
                            if (puzzle.ChangeCandidates(rcs, candidates))
                            {
                                changed = true;
                            }
                        }
                        if (changed)
                        {
                            Cell[] culprits = doRows ? blockrow[r].GetRowInBlock(rcIndex) : blockcol[r].GetColumnInBlock(rcIndex);
                            puzzle.LogAction(Puzzle.TechniqueFormat("Pointing tuple",
                                "Starting in block{0} {1}'s {2} block, {3} {0}: {4}",
                                doRows ? "row" : "column", i + 1, _ordinalStr[r + 1], _ordinalStr[rcIndex + 1], candidates.SingleOrMultiToString()), culprits, (Cell)null);
                        }
                        return changed;
                    }
                    // Now check if a row has a distinct candidate
                    IEnumerable<int> zero_distinct = rowCandidates[0].Except(rowCandidates[1]).Except(rowCandidates[2]);
                    if (zero_distinct.Any())
                    {
                        if (RemovePointingTuple(true, 0, zero_distinct))
                        {
                            return true;
                        }
                    }
                    IEnumerable<int> one_distinct = rowCandidates[1].Except(rowCandidates[0]).Except(rowCandidates[2]);
                    if (one_distinct.Any())
                    {
                        if (RemovePointingTuple(true, 1, one_distinct))
                        {
                            return true;
                        }
                    }
                    IEnumerable<int> two_distinct = rowCandidates[2].Except(rowCandidates[0]).Except(rowCandidates[1]);
                    if (two_distinct.Any())
                    {
                        if (RemovePointingTuple(true, 2, two_distinct))
                        {
                            return true;
                        }
                    }
                    // Now check if a column has a distinct candidate
                    zero_distinct = colCand[0].Except(colCand[1]).Except(colCand[2]);
                    if (zero_distinct.Any())
                    {
                        if (RemovePointingTuple(false, 0, zero_distinct))
                        {
                            return true;
                        }
                    }
                    one_distinct = colCand[1].Except(colCand[0]).Except(colCand[2]);
                    if (one_distinct.Any())
                    {
                        if (RemovePointingTuple(false, 1, one_distinct))
                        {
                            return true;
                        }
                    }
                    two_distinct = colCand[2].Except(colCand[0]).Except(colCand[1]);
                    if (two_distinct.Any())
                    {
                        if (RemovePointingTuple(false, 2, two_distinct))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool LockedCandidate(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int candidate = 1; candidate <= 9; candidate++)
                {
                    bool FindLockedCandidates(bool doRows)
                    {
                        IEnumerable<Cell> cellsWithCandidates = (doRows ? puzzle.Rows : puzzle.Columns)[i].GetCellsWithCandidate(candidate);

                        // Even if a block only has these candidates for this "k" value, it'd be slower to check that before cancelling "BlacklistCandidates"
                        if (cellsWithCandidates.Count() == 3 || cellsWithCandidates.Count() == 2)
                        {
                            int[] blocks = cellsWithCandidates.Select(c => c.Point.BlockIndex).Distinct().ToArray();
                            if (blocks.Length == 1)
                            {
                                if (puzzle.ChangeCandidates(puzzle.Blocks[blocks[0]].Except(cellsWithCandidates), candidate))
                                {
                                    puzzle.LogAction(Puzzle.TechniqueFormat("Locked candidate",
                                        "{4} {0} locks within block {1}: {2}: {3}",
                                        doRows ? SPoint.RowLetter(i) : SPoint.ColumnLetter(i), blocks[0] + 1, cellsWithCandidates.Print(), candidate, doRows ? "Row" : "Column"), cellsWithCandidates, (Cell)null);
                                    return true;
                                }
                            }
                        }
                        return false;
                    }
                    if (FindLockedCandidates(true) || FindLockedCandidates(false))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool HiddenQuadruple(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                if (FindHiddenTuples(puzzle, puzzle.Blocks[i], 4)
                    || FindHiddenTuples(puzzle, puzzle.Rows[i], 4)
                    || FindHiddenTuples(puzzle, puzzle.Columns[i], 4))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool NakedQuadruple(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                if (FindNakedTuples(puzzle, puzzle.Blocks[i], 4)
                    || FindNakedTuples(puzzle, puzzle.Rows[i], 4)
                    || FindNakedTuples(puzzle, puzzle.Columns[i], 4))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HiddenTriple(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                if (FindHiddenTuples(puzzle, puzzle.Blocks[i], 3)
                    || FindHiddenTuples(puzzle, puzzle.Rows[i], 3)
                    || FindHiddenTuples(puzzle, puzzle.Columns[i], 3))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool NakedTriple(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                if (FindNakedTuples(puzzle, puzzle.Blocks[i], 3)
                    || FindNakedTuples(puzzle, puzzle.Rows[i], 3)
                    || FindNakedTuples(puzzle, puzzle.Columns[i], 3))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HiddenPair(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                if (FindHiddenTuples(puzzle, puzzle.Blocks[i], 2)
                    || FindHiddenTuples(puzzle, puzzle.Rows[i], 2)
                    || FindHiddenTuples(puzzle, puzzle.Columns[i], 2))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool NakedPair(Puzzle puzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                if (FindNakedTuples(puzzle, puzzle.Blocks[i], 2)
                    || FindNakedTuples(puzzle, puzzle.Rows[i], 2)
                    || FindNakedTuples(puzzle, puzzle.Columns[i], 2))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HiddenSingle(Puzzle puzzle)
        {
            bool changed = false;
            foreach (ReadOnlyCollection<Region> regionCollection in puzzle.Regions)
            {
                foreach (var region in regionCollection)
                {
                    for (int candidate = 1; candidate <= 9; candidate++)
                    {
                        Cell[] c = region.GetCellsWithCandidate(candidate).ToArray();
                        if (c.Length == 1)
                        {
                            puzzle.Set(c[0], candidate);
                            puzzle.LogAction(Puzzle.TechniqueFormat("Hidden single", "{0}: {1}", c[0], candidate), c[0], (Cell)null);
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }

        #endregion

        #region Method Helpers

        private static readonly string[] _fishStr = new string[] { string.Empty, string.Empty, "X-Wing", "Swordfish", "Jellyfish" };
        private static readonly string[] _tupleStr = new string[] { string.Empty, "single", "pair", "triple", "quadruple" };
        private static readonly string[] _ordinalStr = new string[] { string.Empty, "1st", "2nd", "3rd" };

        // Find X-Wing, Swordfish & Jellyfish
        private static bool FindFish(Puzzle puzzle, int amount)
        {
            for (int candidate = 1; candidate <= 9; candidate++)
            {
                bool DoFish(int loop, int[] indexes)
                {
                    if (loop == amount)
                    {
                        IEnumerable<IEnumerable<Cell>> rowCells = indexes.Select(i => puzzle.Rows[i].GetCellsWithCandidate(candidate)),
                            colCells = indexes.Select(i => puzzle.Columns[i].GetCellsWithCandidate(candidate));

                        IEnumerable<int> rowLengths = rowCells.Select(cells => cells.Count()),
                            colLengths = colCells.Select(parr => parr.Count());

                        if (rowLengths.Max() == amount && rowLengths.Min() > 0 && rowCells.Select(cells => cells.Select(c => c.Point.X)).UniteAll().Count() <= amount)
                        {
                            IEnumerable<Cell> row2D = rowCells.UniteAll();
                            if (puzzle.ChangeCandidates(row2D.Select(c => puzzle.Columns[c.Point.X]).UniteAll().Except(row2D), candidate))
                            {
                                puzzle.LogAction(Puzzle.TechniqueFormat(_fishStr[amount], "{0}: {1}", row2D.Print(), candidate), row2D, (Cell)null);
                                return true;
                            }
                        }
                        if (colLengths.Max() == amount && colLengths.Min() > 0 && colCells.Select(cells => cells.Select(c => c.Point.Y)).UniteAll().Count() <= amount)
                        {
                            IEnumerable<Cell> col2D = colCells.UniteAll();
                            if (puzzle.ChangeCandidates(col2D.Select(c => puzzle.Rows[c.Point.Y]).UniteAll().Except(col2D), candidate))
                            {
                                puzzle.LogAction(Puzzle.TechniqueFormat(_fishStr[amount], "{0}: {1}", col2D.Print(), candidate), col2D, (Cell)null);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        for (int i = loop == 0 ? 0 : indexes[loop - 1] + 1; i < 9; i++)
                        {
                            indexes[loop] = i;
                            if (DoFish(loop + 1, indexes))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                if (DoFish(0, new int[amount]))
                {
                    return true;
                }
            }
            return false;
        }

        // Find hidden pairs/triples/quadruples
        private static bool FindHiddenTuples(Puzzle puzzle, Region region, int amount)
        {
            // If there are only "amount" cells with candidates, we don't have to waste our time
            if (region.Count(c => c.Candidates.Count > 0) == amount)
            {
                return false;
            }

            bool DoHiddenTuples(int loop, int[] candidates)
            {
                if (loop == amount)
                {
                    IEnumerable<Cell> cells = candidates.Select(c => region.GetCellsWithCandidate(c)).UniteAll();
                    IEnumerable<int> cands = cells.Select(c => c.Candidates).UniteAll();
                    if (cells.Count() != amount // There aren't "amount" cells for our tuple to be in
                        || cands.Count() == amount // We already know it's a tuple (might be faster to skip this check, idk)
                        || !cands.ContainsAll(candidates))
                    {
                        return false; // If a number in our combo doesn't actually show up in any of our cells
                    }
                    if (puzzle.ChangeCandidates(cells, Utils.OneToNine.Except(candidates)))
                    {
                        puzzle.LogAction(Puzzle.TechniqueFormat("Hidden " + _tupleStr[amount], "{0}: {1}", cells.Print(), candidates.Print()), cells, (Cell)null);
                        return true;
                    }
                }
                else
                {
                    for (int i = candidates[loop == 0 ? loop : loop - 1] + 1; i <= 9; i++)
                    {
                        candidates[loop] = i;
                        if (DoHiddenTuples(loop + 1, candidates))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            return DoHiddenTuples(0, new int[amount]);
        }

        // Find naked pairs/triples/quadruples
        private static bool FindNakedTuples(Puzzle puzzle, Region region, int amount)
        {
            bool DoNakedTuples(int loop, Cell[] cells, int[] indexes)
            {
                if (loop == amount)
                {
                    IEnumerable<int> combo = cells.Select(c => c.Candidates).UniteAll();
                    if (combo.Count() == amount)
                    {
                        if (puzzle.ChangeCandidates(indexes.Select(i => puzzle.GetCellsVisibleClassicSudoku(region[i])).IntersectAll(), combo))
                        {
                            puzzle.LogAction(Puzzle.TechniqueFormat("Naked " + _tupleStr[amount], "{0}: {1}", cells.Print(), combo.Print()), cells, (Cell)null);
                            return true;
                        }
                    }
                }
                else
                {
                    for (int i = loop == 0 ? 0 : indexes[loop - 1] + 1; i < 9; i++)
                    {
                        Cell c = region[i];
                        if (c.Candidates.Count == 0)
                        {
                            continue;
                        }
                        cells[loop] = c;
                        indexes[loop] = i;
                        if (DoNakedTuples(loop + 1, cells, indexes))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            return DoNakedTuples(0, new Cell[amount], new int[amount]);
        }

        #endregion
    }
}
