using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SudokuSolver.Core
{
    public sealed class CellSnapshot
    {
        public int Value { get; }
        public ReadOnlyCollection<int> Candidates { get; }
        public bool IsCulprit { get; }
        public bool IsSemiCulprit { get; }

        public CellSnapshot(int value, HashSet<int> candidates, bool isCulprit, bool isSemiCulprit)
        {
            Value = value;
            Candidates = new ReadOnlyCollection<int>(candidates.ToArray());
            IsCulprit = isCulprit;
            IsSemiCulprit = isSemiCulprit;
        }
    }

    [DebuggerDisplay("{DebugString()}", Name = "{ToString()}")]
    public sealed class Cell
    {
        public int Value { get; private set; }
        public HashSet<int> Candidates { get; } = new HashSet<int>(Utils.OneToNine);

        public int OriginalValue { get; set; }
        public SPoint Point { get; }

        public List<CellSnapshot> Snapshots { get; } = new List<CellSnapshot>();
        
        public Cell(int value, SPoint point)
        {
            OriginalValue = Value = value;
            Point = point;
        }

        public void Set(int newValue)
        {
            Value = newValue;
        }

        public void CreateSnapshot(bool isCulprit, bool isSemiCulprit)
        {
            Snapshots.Add(new CellSnapshot(Value, Candidates, isCulprit, isSemiCulprit));
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Cell other)
            {
                return other.Point.Equals(Point);
            }
            return false;
        }
        public override string ToString()
        {
            return Point.ToString();
        }
        public string DebugString()
        {
            string s = Point.ToString() + " ";
            if (Value == 0)
            {
                s += "has candidates: " + Candidates.Print();
            }
            else
            {
                s += "- " + Value.ToString();
            }
            return s;
        }
    }
}
