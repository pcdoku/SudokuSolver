using SudokuSolver.Core;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SudokuSolver.Core.Constraints;

namespace SudokuSolverTest
{
    public class PuzzleTests
    {
        [Fact]
        public void TestGetCellsVisibleForAllRegionsClassicSudoku()
        {
            int[][] board = Utils.CreateJaggedArray<int[][]>(9, 9);
            var puzzle = new Puzzle(board, false);
            var targetCell = puzzle[5, 5];
            var expectedVisible = new HashSet<Cell>(puzzle.Rows[5].Union(puzzle.Columns[5]).Union(puzzle.Blocks[4]).Except(new Cell[] { targetCell }));
            var visibleCells = new HashSet<Cell>(puzzle.GetCellsVisibleForAllRegions(targetCell));
            Assert.Equal(expectedVisible, visibleCells);
        }

        [Fact]
        public void TestGetCellsVisibleForAllRegionsXSudoku()
        {
            int[][] board = Utils.CreateJaggedArray<int[][]>(9, 9);
            var xSudokuConstraint = new XSudokuConstraint();
            var puzzle = new Puzzle(board, false, new List<BaseConstraint>() { xSudokuConstraint });
            var targetCell = puzzle[4, 4]; // in the middle of the X
            var expectedVisible = new HashSet<Cell>(puzzle.Rows[4].Union(puzzle.Columns[4]).Union(puzzle.Blocks[4]).Union(xSudokuConstraint.GetRegions(puzzle).SelectMany(x => x)).Distinct().Except(new Cell[] { targetCell }));
            var visibleCells = new HashSet<Cell>(puzzle.GetCellsVisibleForAllRegions(targetCell));
            Assert.Equal(expectedVisible, visibleCells);
        }
    }
}
