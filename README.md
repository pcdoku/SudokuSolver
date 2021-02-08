# SudokuSolver

A program that works out the solution to a Sudoku puzzle by using human techniques and proofs.
Because it logs the human techniques it uses, you can learn how to get past obstacles you found yourself in.

![Preview](https://i.imgur.com/ERLFyc6.gif)

The form draws the puzzle and its changes. If it gets stuck, the candidates for each cell will be shown for debugging:

![Failure Preview](https://i.imgur.com/WE7UMun.png)

Once design is done, of course, there will be no candidates showing, as every cell will be filled (assuming the input puzzle is valid and has human-solvable steps).

Big thanks to http://hodoku.sourceforge.net and http://www.sudokuwiki.org for providing a lot of information on tough Sudoku techniques.

## Changes from the original program from Kermalis

* switched to RnCn coodinate notation
* added unit tests
* support loading puzzles from a string of 81 digits (like used by sudokuwiki.org, and others)
* support loading puzzles from json file to allow other variants to be described
* added strategies for solving nonconsecutive, antiknight, antiking, and x-sudoku
* much refactoring, updating to .NET 5.0
* added ability to import from f-puzzles URL