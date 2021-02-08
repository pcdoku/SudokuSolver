// import code ported from https://github.com/swaroopg92/penpa-edit/blob/master/docs/js/general.js
// which was forked from https://github.com/rjrudman/penpa-edit/blob/master/docs/js/general.js#L474
// and in turn was forked from https://github.com/opt-pan/penpa-edit/blob/master/docs/js/general.js#L621 (original creator)
using LZStringCSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SudokuSolver.Core
{
    public class FPuzzleImport
    {
        public static Puzzle Import(string urlstring)
        {
            var urlParts = urlstring.Split('?');
            var queryParts = urlParts[1].Split('&');

            if (!urlstring.Contains("f-puzzles.com") || !queryParts.Any(x => x.StartsWith("load=")))
                throw new InvalidDataException("URL is not a supported f-puzzles url. Do not used compressed links.");

            var encodedPuzzle = queryParts.First(x => x.StartsWith("load=")).Substring(5);
            var jsonString = LZString.DecompressFromBase64(encodedPuzzle);
            dynamic fpuzzle = JsonConvert.DeserializeObject<dynamic>(jsonString);
            if (fpuzzle.size != 9)
                throw new InvalidDataException("only 9x9 grids supported at this time");

            int[][] board = Utils.CreateJaggedArray<int[][]>(9, 9);
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    var fpuzzleCell = fpuzzle.grid[row][col];
                    board[col][row] = fpuzzleCell.value ?? 0;
                    // TODO: save "given" as Cell OriginalValue
                }
            }
            var constraints = new List<string>();
            if (fpuzzle["diagonal+"]?.Value ?? false && fpuzzle["diagonal-"]?.Value ?? false)
                constraints.Add("x-sudoku"); // TODO: in future remove x-sudoku in favor of individual diagonals
            if (fpuzzle["diagonal+"]?.Value ?? false)
                constraints.Add("diagonalup");
            if (fpuzzle["diagonal-"]?.Value ?? false)
                constraints.Add("diagonaldown");
            if (fpuzzle.antiknight?.Value ?? false)
                constraints.Add("antiknight");
            if (fpuzzle.antiking?.Value ?? false)
                constraints.Add("antiking");
            if (fpuzzle.nonconsecutive?.Value ?? false)
                constraints.Add("nonconsecutive");
            if (fpuzzle.disjointgroups?.Value ?? false)
                constraints.Add("disjointgroups");

            var puzzle = new Puzzle(board, false, constraints);
            return puzzle;

        }
    }
}
