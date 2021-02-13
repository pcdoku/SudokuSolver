using System.Collections.Generic;
using Xunit;
using System.Linq;

namespace SudokuSolverTest
{
    public class FPuzzlesImportTests
    {
        [Fact]
        public void TestFPuzzlesImportXSudoku()
        {
            var url = "https://www.f-puzzles.com/?load=N4IgzglgXgpiBcBOANCA5gJwgEwQbT2AF9ljSQA3AQwBsBXOeAdlTQgpgDsEAXDBkmSGDBlWgwQBmVuy69+MUdXqMArDI7d4fAQF1khMSoQAODXO0KRpa8PLKJSc1p2KbN/YSXjGAJmfyAu6gDowAbAGWQSE+CCzosi5WwbZEnnYZ3sbw0gmagW4x2ep5Fq5ZjhGlSdFpBpnBRo4AjJHlKY2hUm1W6bZNfj219rHwZtUFqSPZACxDhXVeHUUt8xWMKBNRC52jJWz52yJ9y+sI+4mTywNxax71Uw0ZN/BzW+W6+iDYEFRoAPacWgAagKqB+f0BtAAtJMgA===";
            var puzzle = SudokuSolver.Core.FPuzzleImport.Import(url);
            Assert.Equal("--7----35\n8-----9--\n-2-67----\n-----356-\n----1---3\n--2-8--4-\n---19--5-\n----5--7-\n--------4\n", puzzle.ToString());
            Assert.Equal(new HashSet<string>() { "x-sudoku" }, new HashSet<string>(puzzle.Constraints.Select(x => x.Name())));
            //Assert.Equal("{\"size\":9,\"grid\":[[{},{},{\"value\":7,\"given\":true},{},{},{},{},{\"value\":3,\"given\":true},{\"value\":5,\"given\":true}],[{\"value\":8,\"given\":true},{},{},{},{},{},{\"value\":9,\"given\":true},{},{}],[{},{\"value\":2,\"given\":true},{},{\"value\":6,\"given\":true},{\"value\":7,\"given\":true},{},{},{},{}],[{},{},{},{},{},{\"value\":3,\"given\":true},{\"value\":5,\"given\":true},{\"value\":6,\"given\":true},{}],[{},{},{},{},{\"value\":1,\"given\":true},{},{},{},{\"value\":3,\"given\":true}],[{},{},{\"value\":2,\"given\":true},{},{\"value\":8,\"given\":true},{},{},{\"value\":4,\"given\":true},{}],[{},{},{},{\"value\":1,\"given\":true},{\"value\":9,\"given\":true},{},{},{\"value\":5,\"given\":true},{}],[{},{},{},{},{\"value\":5,\"given\":true},{},{},{\"value\":7,\"given\":true},{}],[{},{},{},{},{},{},{},{},{\"value\":4,\"given\":true}]],\"diagonal+\":true,\"diagonal-\":true}", puzzlejson);
        }

        [Fact]
        public void TestFPuzzlesImportAllGridWideConstraints()
        {
            var url = "https://www.f-puzzles.com/?load=N4IgzglgXgpiBcBOANCA5gJwgEwQbT2AF9ljSSzKLryBdZQmq8l54+x1p7rjtn/nQaCR3PgIm9hk0UM6zR4rssX0Q2CAEM0AewB2mgDYBqBABcMAVxioN2/UYC05qzZCa9ZiAGs9ENAAWZi7WqB5e3hB6aCFuGmAAVjpRZpg6lgAOYLGoevoAxvpgMPmWXgBucPAW1kRAA==";
            var puzzle = SudokuSolver.Core.FPuzzleImport.Import(url);
            Assert.Equal("---------\n---------\n---------\n---------\n---------\n---------\n---------\n---------\n---------\n", puzzle.ToString());
            Assert.Equal(new HashSet<string>() { "x-sudoku", "antiknight", "antiking", "nonconsecutive"}, puzzle.Constraints.Select(x => x.Name()));
            //Assert.Equal("{\"size\":9,\"grid\":[[{},{},{},{},{},{},{},{},{}],[{},{},{},{},{},{},{},{},{}],[{},{},{},{},{},{},{},{},{}],[{},{},{},{},{},{},{},{},{}],[{},{},{},{},{},{},{},{},{}],[{},{},{},{},{},{},{},{},{}],[{},{},{},{},{},{},{},{},{}],[{},{},{},{},{},{},{},{},{}],[{},{},{},{},{},{},{},{},{}]],\"diagonal+\":true,\"diagonal-\":true}", puzzlejson);
        }
    }
}
