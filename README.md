# KenKen Solver

A constraint-satisfaction KenKen puzzle solver written in C# (.NET 6.0). Solves any arbitrary N×N KenKen puzzle using backtracking search with constraint propagation.

## Building & Running

```bash
dotnet build KenKen/KenKen.csproj
dotnet run --project KenKen/KenKen.csproj
```

## Puzzle Input Format

Puzzles are defined in text files with two sections:

**1. Grid layout** — an N×N grid where each character is a cage identifier:

```
abbccc
adeefg
ddhhfg
iihhjk
limmjk
lnnooo
```

**2. Cage constraints** — one line per cage: `<id> <target> <operation>`

```
a 3 /
b 11 +
c 24 *
d 36 *
e 11 +
f 3 -
g 2 /
h 12 +
i 24 *
j 3 -
k 1 -
l 3 -
m 6 *
n 3 -
o 90 *
```

Operations: `+` (addition), `-` (subtraction), `*` (multiplication), `/` (division), `.` (single-cell, value given).

## How It Works

The solver uses a three-phase approach:

1. **Solve single-cell cages** — cells with the `.` operation are filled immediately and their values are propagated as constraints to neighbors.
2. **Identify dependent cells** — cells sharing possible values along rows/columns are linked as dependencies.
3. **Backtracking search** — iterates through dependent cells, tries candidate values, and backtracks on conflicts. Valid cage combinations are precomputed to prune the search space.

Each row and column must contain the numbers 1 through N exactly once (Latin square constraint), and each cage's values must combine to its target using the specified operation.

## Project Structure

```
KenKen/
├── Program.cs          # Entry point
├── KenKen.cs           # Solver orchestration and puzzle parsing
├── Cage.cs             # Cage constraint logic and permutation generation
├── Cell.cs             # Cell state, neighbor tracking, and backtracking
├── CageOperation.cs    # Operation enum (+, -, *, /)
└── boards/             # Example puzzle files
```
