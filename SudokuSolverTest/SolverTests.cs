using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SudokuSolver;
using System.ComponentModel;

namespace SudokuSolverTest
{
    public class SolverTests
    {
        [Theory]
        [InlineData("Regular/4109.txt", "198725463735964182624831597471659238986312754352478916549283671817546329263197845")]
        [InlineData("Regular/4112.txt", "356147928472968351891352746125694837743815269689273415918426573237581694564739182")]
        [InlineData("Regular/4319.txt", "356914728471823596829567413164375289287149365593682174648251937915736842732498651")]
        [InlineData("Regular/4322.txt", "651938274239475618748612593917263485382549167564781932195327846876154329423896751")]
        [InlineData("Regular/Unnamed 1.txt", "314659728295781463876324591123596874489173256567842319751938642932465187648217935")]
        [InlineData("Regular/Unnamed 2.txt", "624379185951248736387156924769435812532981647148627359215763498873594261496812573")]
        [InlineData("Regular/Unnamed 3.txt", "728564319961273584453981726316842975549716832872395641297458163135627498684139257")]
        [InlineData("Regular/Unnamed 4.txt", "892634715153792846674185329549361287726948531318257694937426158465813972281579463")]
        [InlineData("Regular/Unnamed 5.txt", "367928145829415673541637892154783269692541387783269451936852714278194536415376928")]
        [InlineData("Regular/Unnamed 6.txt", "139675248782314659546982317923841576458267193617539482895123764371496825264758931")]
        [InlineData("Tough/116.txt", "631748592547692831892351746718469253259813467364527918973184625125976384486235179")]
        [InlineData("Tough/117.txt", "953481267461927358287356941572634819349518672618279435724193586896745123135862794")]
        [InlineData("Training/Avoidable Rectangle 1.txt", "954382167761594283238671459417953628625748931893126745346819572589267314172435896")]
        public void TestSolverSolutions(string filename, string solution)
        {
            var puzzlesRoot = "../../../../Puzzles/";
            var puzzle = Kermalis.SudokuSolver.Core.Puzzle.LoadFile(puzzlesRoot+filename);
            var solver = new Kermalis.SudokuSolver.Core.Solver(puzzle);
            var args = new DoWorkEventArgs(null);
            solver.DoWork(this, args);
            Assert.True((bool)args.Result);
            Assert.Equal(solution, puzzle.Solution());
        }

        [Theory]
        [InlineData("Training/Naked single.txt",        "198725463735964182624831597471659238986312754352478916549283671817546329263197845", 1, "Naked single        R2C6: 4")]
        [InlineData("Training/Hidden single.txt",       "356147928472968351891352746125694837743815269689273415918426573237581694564739182", 1, "Hidden single       R2C9: 1")]
        [InlineData("Training/Naked pair.txt",          "139675248782314659546982317923841576458267193617539482895123764371496825264758931", 1, "Naked pair          ( R4C3, R4C7 ): ( 3, 5 )")]
        [InlineData("Training/Hidden pair.txt",         "631748592547692831892351746718469253259813467364527918973184625125976384486235179", 2, "Hidden pair         ( R2C4, R1C6 ): ( 6, 8 )")]
        [InlineData("Training/Naked Triple.txt",        "891235764537694218462187935243519876156728493978346521319452687684973152725861349", 2, "Naked triple        ( R1C4, R6C4, R7C4 ): ( 2, 3, 4 )")]
        [InlineData("Training/Locked candidate.txt",    "953481267461927358287356941572634819349518672618279435724193586896745123135862794", 2, "Locked candidate    Column C7 locks within block 9: ( R7C7, R9C7 ): 7")]
        [InlineData("Training/Hidden Triple.txt",       "467318295895246713231975648659182374342597861178463529724851936986734152513629487", 4, "Hidden triple       ( R4C8, R5C8, R9C8 ): ( 6, 7, 8 )")]
        [InlineData("Training/Hidden Quad.txt",         "639247815528391764417658239182576943764983521953124687276435198841769352395812476", 4, "Hidden quadruple    ( R1C9, R2C9, R3C9, R8C9 ): ( 2, 4, 5, 9 )")]
        [InlineData("Training/Hidden Rectangle.txt",    "691453287352781694748296513935128746874639125216547839129375468563814972487962351", 1, "Hidden rectangle    ( R2C2, R3C2, R2C9, R3C9 ): ( 4, 5 )")]
        [InlineData("Training/Jellyfish.txt",           "256819743184637259973452168831265497645791832729348615312576984598124376467983521", 1, "Jellyfish           ( R3C1, R3C2, R3C5, R3C9, R4C1, R4C2, R4C5, R4C9, R6C1, R6C2, R6C5, R6C9, R7C1, R7C2, R7C5 ): 7")]
        [InlineData("Training/Pointing Tuples.txt",     "732456198419283756685971423528197634964835271371624985296518347843762519157349862", 6, "Pointing tuple      Starting in blockrow 2's 2nd block, 2nd row: ( 8, 5 )")]
        [InlineData("Training/Unique Rectangle 1.txt",  "598764123346821597217395846479532681682917354153486972961248735825673419734159268", 3, "Unique rectangle    ( R6C2, R8C2, R6C3, R8C3 ): ( 2, 5 )")]
        [InlineData("Training/Unique Rectangle 2.txt",  "482735196175962348936418275319827564624591783857346912798653421263174859541289637", 1, "Unique rectangle    ( R4C2, R6C2, R4C8, R6C8 ): ( 1, 5 )")]
        [InlineData("Training/XY-Chain 1.txt",          "923861457846375921751429368187536294534792186269148573675913842392684715418257639", 1, "XY-Chain            ( R6C2, R2C6 )-( R6C3, R2C3, R5C3, R5C1, R8C1, R7C3, R9C3, R9C9, R3C9, R3C5, R4C5, R4C6 ): 6")]
        [InlineData("Training/XY-Chain 2.txt",          "154983726978264153362751489217348695693527841845196237589672314721435968436819572", 2, "XY-Chain            ( R4C1, R7C9 )-( R6C2, R8C2, R8C4, R8C3, R7C3, R7C4 ): 2")]
        [InlineData("Training/X-Wing.txt",              "237481659694527138851369247419873562785612493326954871143295786578136924962748315", 4, "X-Wing              ( R2C3, R2C7, R5C3, R5C7 ): 4")]
        [InlineData("Training/Y-Wing.txt",              "931247586754698231628153794195764328482935617376812945869521473513479862247386159", 6, "Y-Wing              ( R1C2, R9C2, R2C3 ): 4")]
        public void TestSolverStrategies(string filename, string solution, int actionIndex = -1, string action = null)
        {
            var puzzlesRoot = "../../../../Puzzles/";
            var puzzle = Kermalis.SudokuSolver.Core.Puzzle.LoadFile(puzzlesRoot + filename);
            var solver = new Kermalis.SudokuSolver.Core.Solver(puzzle);
            var args = new DoWorkEventArgs(null);
            solver.DoWork(this, args);
            Assert.True((bool)args.Result);
            Assert.Equal(solution, puzzle.Solution());
            if (actionIndex >= 0)
            {
                Assert.Equal(action, puzzle.Actions[actionIndex]);
            }
        }

    }
}
