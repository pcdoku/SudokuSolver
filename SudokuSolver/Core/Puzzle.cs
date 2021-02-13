using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using SudokuSolver.Core.Constraints;

namespace SudokuSolver.Core
{
    public class Constraint
    {
        public string type;
    }
    public class PuzzleDefinition
    {
        public List<Constraint> constraints;
        public List<List<int>> givenDigits;
    }
    public sealed class Puzzle
    {
        public readonly ReadOnlyCollection<Region> Rows;
        public readonly ReadOnlyCollection<Region> Columns;
        public readonly ReadOnlyCollection<Region> Blocks;
        public readonly ReadOnlyCollection<ReadOnlyCollection<Region>> Regions;

        public readonly BindingList<string> Actions = new BindingList<string>();
        public readonly bool IsCustom;
        private readonly Cell[][] _board; // col, row
        public readonly List<BaseConstraint> Constraints;

        public Cell this[int x, int y] => _board[x][y];

        public Puzzle(int[][] board, bool isCustom, List<BaseConstraint> constraints = null)
        {
            IsCustom = isCustom;
            Constraints = constraints ?? new List<BaseConstraint>();
            _board = Utils.CreateJaggedArray<Cell[][]>(9, 9);
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    _board[x][y] = new Cell(board[x][y], new SPoint(x, y));
                }
            }
            Region[] rows = new Region[9],
                columns = new Region[9],
                blocks = new Region[9];
            for (int i = 0; i < 9; i++)
            {
                Cell[] cells;
                int c;

                cells = new Cell[9];
                for (c = 0; c < 9; c++)
                {
                    cells[c] = _board[c][i];
                }
                rows[i] = new Region(cells);

                for (c = 0; c < 9; c++)
                {
                    cells[c] = _board[i][c];
                }
                columns[i] = new Region(cells);

                c = 0;
                int ix = i % 3 * 3,
                    iy = i / 3 * 3;
                for (int x = ix; x < ix + 3; x++)
                {
                    for (int y = iy; y < iy + 3; y++)
                    {
                        cells[c++] = _board[x][y];
                    }
                }
                blocks[i] = new Region(cells);
            }
            Rows = new ReadOnlyCollection<Region>(rows);
            Columns = new ReadOnlyCollection<Region>(columns);
            Blocks = new ReadOnlyCollection<Region>(blocks);
            var regionList = new List<ReadOnlyCollection<Region>>();
            regionList.Add(Rows);
            regionList.Add(Columns);
            regionList.Add(Blocks);
            foreach( var constraint in Constraints)
            {
                regionList.Add(new ReadOnlyCollection<Region>(constraint.GetRegions(this)));
            }
            Regions = new ReadOnlyCollection<ReadOnlyCollection<Region>>(regionList);
        }

        public bool ChangeCandidates(Cell cell, int candidate, bool remove = true)
        {
            return remove ? cell.Candidates.Remove(candidate) : cell.Candidates.Add(candidate);
        }
        public bool ChangeCandidates(Cell cell, IEnumerable<int> candidates, bool remove = true)
        {
            bool changed = false;
            foreach (int value in candidates)
            {
                if (remove ? cell.Candidates.Remove(value) : cell.Candidates.Add(value))
                {
                    changed = true;
                }
            }
            return changed;
        }
        public bool ChangeCandidates(IEnumerable<Cell> cells, int candidate, bool remove = true)
        {
            bool changed = false;
            foreach (Cell cell in cells)
            {
                if (remove ? cell.Candidates.Remove(candidate) : cell.Candidates.Add(candidate))
                {
                    changed = true;
                }
            }
            return changed;
        }
        public bool ChangeCandidates(IEnumerable<Cell> cells, IEnumerable<int> candidates, bool remove = true)
        {
            bool changed = false;
            foreach (Cell cell in cells)
            {
                foreach (int value in candidates)
                {
                    if (remove ? cell.Candidates.Remove(value) : cell.Candidates.Add(value))
                    {
                        changed = true;
                    }
                }
            }
            return changed;
        }
        public void RefreshCandidates()
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    foreach (int i in Utils.OneToNine)
                    {
                        cell.Candidates.Add(i);
                    }
                }
            }
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    if (cell.Value != 0)
                    {
                        Set(cell, cell.Value);
                    }
                }
            }
        }

        public void Set(Cell cell, int newValue, bool refreshOtherCellCandidates = false)
        {
            int oldValue = cell.Value;
            cell.Set(newValue);
            if (newValue == 0)
            {
                foreach (int i in Utils.OneToNine)
                {
                    cell.Candidates.Add(i);
                }
                ChangeCandidates(GetCellsVisibleForAllRegions(cell), oldValue, remove: false);
            }
            else
            {
                cell.Candidates.Clear();
                ChangeCandidates(GetCellsVisibleForAllRegions(cell), newValue);
            }
            if (refreshOtherCellCandidates)
            {
                RefreshCandidates();
            }

        }

        public void ChangeOriginalValue(Cell cell, int value)
        {
            cell.OriginalValue = value;
            Set(cell, value, refreshOtherCellCandidates: true);
        }

        public IEnumerable<Cell> GetCellsVisibleForAllRegions(Cell cell)
        {
            var regionsContainingCell = Regions.SelectMany(x => x).Where(r => r.Contains(cell));
            var visibleCells = regionsContainingCell.SelectMany(x => x).Distinct().Where(c => c != cell);
            return visibleCells;
        }

        public IEnumerable<Cell> GetCellsVisibleClassicSudoku(Cell cell)
        {
            Region block = Blocks[cell.Point.BlockIndex];
            Region col = Columns[cell.Point.X];
            Region row = Rows[cell.Point.Y];
            return block.Union(col).Union(row).Except(new Cell[] { cell });
        }

        /// <summary>
        /// Load a puzzle from a file, automatically detecting one of the following formats:
        /// a) 1 row of 81 values
        /// b) 9 rows of 9 values
        /// The positions without a 1-9 digit value can be represented by 0 or any non-digit symbol.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Puzzle LoadFile(string fileName)
        {
            var fileLines = File.ReadAllText(fileName);
            if (fileLines[0] == '{')
            {
                return LoadJson(fileLines);
            }
            return Load(fileLines);
        }

        /// <summary>
        /// This loads a simplified form of the ipuz.org format since
        /// we don't expect interoperability due to our custom variations.
        /// This omits the namespaces used in ipuz.org.
        /// </summary>
        /// <param name="puzzlejson"></param>
        /// <returns></returns>
        public static Puzzle LoadJson(string puzzlejson)
        {
            var puzzledata = JsonConvert.DeserializeObject<PuzzleDefinition>(puzzlejson);
            int[][] board = Utils.CreateJaggedArray<int[][]>(9, 9);
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    board[col][row] = puzzledata.givenDigits[row][col];  // some day fix this so board indices are row,col                  
                }
            }
            return new Puzzle(board, false, puzzledata.constraints.Select(x => CreateConstraint(x)).ToList());
        }

        private static BaseConstraint CreateConstraint(Constraint constraint)
        {
            if (constraint.type == "x-sudoku")
                return new XSudokuConstraint();
            if (constraint.type == "nonconsecutive")
                return new NonconsecutiveConstraint();
            if (constraint.type == "antiknight")
                return new AntiKnightConstraint();
            if (constraint.type == "antiking")
                return new AntiKingConstraint();
            return null;
        }


        public static Puzzle Load(string puzzledata, List<BaseConstraint> constraints = null)
        {
            var fileLines = puzzledata.Split(new[] {"\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            int[][] board = Utils.CreateJaggedArray<int[][]>(9, 9);
            if (fileLines.Length == 1)
            {
                string line = fileLines[0];
                // the whole puzzle in 1 line
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (int.TryParse(line[j+i*9].ToString(), out int value)) // Anything can represent 0
                        {
                            board[j][i] = value;
                        }
                    }                    
                }
                return new Puzzle(board, false, constraints);
            }
            if (fileLines.Length != 9)
            {
                throw new InvalidDataException("Puzzle must have 1 row of 81 values or 9 rows of 9 values.");
            }
            for (int i = 0; i < 9; i++)
            {
                string line = fileLines[i];
                if (line.Length != 9)
                {
                    throw new InvalidDataException($"Row {i} must have 9 values.");
                }
                for (int j = 0; j < 9; j++)
                {
                    if (int.TryParse(line[j].ToString(), out int value)) // Anything can represent 0
                    {
                        board[j][i] = value;
                    }
                }
            }

            return new Puzzle(board, false, constraints);
        }

        public void Save(string fileName)
        {
            using (var file = new StreamWriter(fileName))
            {
                file.Write(this.ToString());
            }
        }

        public override string ToString()
        {
            string result = "";
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[y, x];
                    if (cell.OriginalValue == 0)
                    {
                        result += '-';
                    }
                    else
                    {
                        result += cell.OriginalValue.ToString();
                    }
                }
                result += '\n';
            }
            return result;
        }

        public string Solution()
        {
            string result = "";
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[y, x];
                    if (cell.Value == 0)
                    {
                        result += '-';
                    }
                    else
                    {
                        result += cell.Value.ToString();
                    }
                }
            }
            return result;
        }

        public static string TechniqueFormat(string technique, string format, params object[] args)
        {
            return string.Format(string.Format("{0,-20}", technique) + format, args);
        }

        public void LogAction(string action)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(false, false);
                }
            }
            Actions.Add(action);
        }
        public void LogAction(string action, Cell culprit, Cell semiCulprit)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(culprit == cell, semiCulprit == cell);
                }
            }
            Actions.Add(action);
        }
        public void LogAction(string action, IEnumerable<Cell> culprits, Cell semiCulprit)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(culprits != null && culprits.Contains(cell), semiCulprit == cell);
                }
            }
            Actions.Add(action);
        }
        public void LogAction(string action, Cell culprit, IEnumerable<Cell> semiCulprits)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(culprit == cell, semiCulprits != null && semiCulprits.Contains(cell));
                }
            }
            Actions.Add(action);
        }
        public void LogAction(string action, IEnumerable<Cell> culprits, IEnumerable<Cell> semiCulprits)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(culprits != null && culprits.Contains(cell), semiCulprits != null && semiCulprits.Contains(cell));
                }
            }
            Actions.Add(action);
        }
    }
}
