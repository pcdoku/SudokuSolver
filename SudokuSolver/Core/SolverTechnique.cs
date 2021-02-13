using System;

namespace SudokuSolver.Core
{
    public sealed class SolverTechnique
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
}
