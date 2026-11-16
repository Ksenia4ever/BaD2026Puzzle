# BaD-2026: Digital Puzzle

## Description

A C# application that solves the BaD-2026 Digital Puzzle challenge.

The goal is to construct the longest possible digit sequence from 142 fragments. Each fragment can be used at most once, and adjacent fragments must share two matching digits.

## Technologies

- C#
- .NET 10
- Google OR-Tools (SCIP)

## Algorithm

The problem is represented as a directed multigraph:

- Vertices represent two-digit prefixes and suffixes.
- Each fragment is a directed edge.
- Mixed-Integer Programming (MIP) is used to maximize the number of selected fragments.
- Flow constraints ensure connectivity.
- Hierholzer's algorithm constructs the final Euler trail.

The SCIP solver is used to establish optimality.

## Results

| Metric | Value |
|---|---|
| Input fragments | 142 |
| Used fragments | 67 |
| Final sequence length | 270 digits |
| Validation | PASSED |
| Solver status | OPTIMAL |

## How to Run

1. Open the project in Visual Studio.
2. Restore NuGet packages.
3. Place `fragments.txt` in the application's output directory.
4. Build and run the project.
5. The result will be saved to `result.txt` in the output directory.

## Project Structure

- `Program.cs` — application entry point.
- `Models/Fragment.cs` — fragment representation.
- `Services/PuzzleSolver.cs` — optimization and Euler trail construction.
- `Services/ChainValidator.cs` — solution validation.
- `fragments.txt` — input fragments.
- `result.txt` — generated solution.
