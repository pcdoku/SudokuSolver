using System;
using Xunit;
using SudokuSolver;

namespace SudokuSolverTest
{
    public class PenpaEditImportTests
    {
        [Fact(Skip="the import doesn't work")]
        public void TestPenpaEditImport()
        {
            var penpaSolvingUrl = "https://swaroopg92.github.io/penpa-edit/?m=solve&p=1ZJRa7swEMDf/RQjz/dgTOJs3sb279PY4N+OMURK6tJVSGsXIwWl/ew9rx0jH2Gc9/P8nSTRpPvujbfA0+kSBeAdQ/KCMityyvQWyyY4q+/goQ/b1mMB8DqfJ+WtXyUl4wxYhslZdR4W55Ix4FUyDv/1OKx0WZ1gePstF3pEvhA58YM4J2bEpS5LgYvjIDIQAoSsKhgEDgDDE/akBKlA5iDvQRZTL6WeIj7rkakC1IzpApiaQZ5SlaeQ82vFIc+wwqn+0YTvxEeiRJ6SZFrA9GPiUH/P4RbhbrCudauu9xtTW6Y3xnUWyO373dr6SLm2PbhmH783Sfv5FcujcS4S18MVqbrxtYtV8E30bLxvj5HZmbCNxNoEPIjdtjnEI9l9iBcQzM/HnC4=";
            var puzzlejson = SudokuSolver.Core.PenpaEditImport.Import(penpaSolvingUrl);
            Assert.Equal("foo", puzzlejson);
        }
    }
}
