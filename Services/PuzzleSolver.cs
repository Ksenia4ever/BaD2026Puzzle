using BaD2026Puzzle.Models;
using Google.OrTools.LinearSolver;

namespace BaD2026Puzzle.Services;

public class PuzzleSolver
{
    public List<Fragment> Solve(List<Fragment> fragments)
    {
        int n = fragments.Count;

        if (n == 0)
            return new List<Fragment>();

        string[] vertices = fragments
            .SelectMany(f => new[] { f.Start, f.End })
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        Solver solver = Solver.CreateSolver("SCIP")
            ?? throw new InvalidOperationException(
                "SCIP solver is unavailable.");

        // Select each fragment at most once.
        Variable[] selected = Enumerable.Range(0, n)
            .Select(i => solver.MakeBoolVar($"selected_{i}"))
            .ToArray();

        var used = vertices.ToDictionary(
            v => v,
            v => solver.MakeBoolVar($"used_{v}"));

        var start = vertices.ToDictionary(
            v => v,
            v => solver.MakeBoolVar($"start_{v}"));

        var end = vertices.ToDictionary(
            v => v,
            v => solver.MakeBoolVar($"end_{v}"));

        // Flow variables ensure connectivity.
        Variable[] flow = Enumerable.Range(0, n)
            .Select(i => solver.MakeNumVar(0, n, $"flow_{i}"))
            .ToArray();

        Constraint startCount = solver.MakeConstraint(1, 1);
        Constraint endCount = solver.MakeConstraint(1, 1);

        foreach (string v in vertices)
        {
            startCount.SetCoefficient(start[v], 1);
            endCount.SetCoefficient(end[v], 1);
        }

        foreach (Fragment f in fragments)
        {
            int i = f.Id;

            Constraint fromUsed = solver.MakeConstraint(
                double.NegativeInfinity, 0);

            fromUsed.SetCoefficient(selected[i], 1);
            fromUsed.SetCoefficient(used[f.Start], -1);

            Constraint toUsed = solver.MakeConstraint(
                double.NegativeInfinity, 0);

            toUsed.SetCoefficient(selected[i], 1);
            toUsed.SetCoefficient(used[f.End], -1);

            Constraint capacity = solver.MakeConstraint(
                double.NegativeInfinity, 0);

            capacity.SetCoefficient(flow[i], 1);
            capacity.SetCoefficient(selected[i], -n);
        }

        foreach (string v in vertices)
        {
            // Euler trail degree balance.
            Constraint degree = solver.MakeConstraint(0, 0);

            degree.SetCoefficient(start[v], -1);
            degree.SetCoefficient(end[v], 1);

            Constraint incident = solver.MakeConstraint(
                double.NegativeInfinity, 0);

            incident.SetCoefficient(used[v], 1);

            // Reachability from the chosen start.
            Constraint connectivity = solver.MakeConstraint(
                double.NegativeInfinity, 0);

            connectivity.SetCoefficient(used[v], 1);
            connectivity.SetCoefficient(start[v], -n);

            foreach (Fragment f in fragments)
            {
                int direction =
                    (f.Start == v ? 1 : 0) -
                    (f.End == v ? 1 : 0);

                if (direction != 0)
                {
                    degree.SetCoefficient(
                        selected[f.Id], direction);

                    connectivity.SetCoefficient(
                        flow[f.Id], direction);
                }

                if (f.Start == v || f.End == v)
                {
                    incident.SetCoefficient(
                        selected[f.Id], -1);
                }
            }
        }

        // Optimization objective.
        Objective objective = solver.Objective();

        foreach (Variable variable in selected)
            objective.SetCoefficient(variable, 1);

        objective.SetMaximization();

        Console.WriteLine("Searching for optimal solution...");

        Solver.ResultStatus status = solver.Solve();

        if (status != Solver.ResultStatus.OPTIMAL)
        {
            throw new InvalidOperationException(
                $"Optimal solution not proven. Status: {status}");
        }

        List<Fragment> chosen = fragments
            .Where(f => selected[f.Id].SolutionValue() > 0.5)
            .ToList();

        string firstVertex = vertices.First(
            v => start[v].SolutionValue() > 0.5);

        return BuildEulerTrail(chosen, firstVertex);
    }

    private static List<Fragment> BuildEulerTrail(
        List<Fragment> fragments,
        string firstVertex)
    {
        var adjacency = fragments
            .GroupBy(f => f.Start)
            .ToDictionary(
                g => g.Key,
                g => new Stack<Fragment>(g));

        var stack =
            new Stack<(string Vertex, Fragment? Edge)>();

        var result = new List<Fragment>();

        stack.Push((firstVertex, null));

        while (stack.Count > 0)
        {
            var current = stack.Peek();

            if (adjacency.TryGetValue(
                    current.Vertex, out var edges)
                && edges.Count > 0)
            {
                Fragment next = edges.Pop();

                stack.Push((next.End, next));
            }
            else
            {
                var item = stack.Pop();

                if (item.Edge != null)
                    result.Add(item.Edge);
            }
        }

        result.Reverse();

        if (result.Count != fragments.Count)
        {
            throw new InvalidOperationException(
                "Failed to construct the complete trail.");
        }

        return result;
    }
}
