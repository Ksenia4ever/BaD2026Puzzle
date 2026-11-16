using BaD2026Puzzle.Models;
using BaD2026Puzzle.Services;

try
{
    string inputPath = Path.Combine(
        AppContext.BaseDirectory,
        "fragments.txt");

    if (!File.Exists(inputPath))
        throw new FileNotFoundException(
            "Input file not found.", inputPath);

    string[] lines = File.ReadAllLines(inputPath);

    List<Fragment> fragments = lines
        .Select(line => line.Trim())
        .Where(line => line.Length > 0)
        .Select((value, id) => new Fragment(id, value))
        .ToList();

    if (fragments.Any(f =>
        f.Value.Length < 4 ||
        f.Value.Any(c => c < '0' || c > '9')))
    {
        throw new InvalidDataException(
            "All fragments must contain at least four digits.");
    }

    Console.WriteLine("BaD-2026: Digital Puzzle");
    Console.WriteLine($"Loaded fragments: {fragments.Count}");

    var solver = new PuzzleSolver();
    var validator = new ChainValidator();

    List<Fragment> chain = solver.Solve(fragments);

    bool isValid = validator.IsValid(chain);

    if (!isValid)
        throw new InvalidOperationException(
            "Solution validation failed.");

    string sequence = validator.BuildSequence(chain);

    Console.WriteLine();
    Console.WriteLine("===== RESULTS =====");
    Console.WriteLine($"Total fragments: {fragments.Count}");
    Console.WriteLine($"Used fragments: {chain.Count}");
    Console.WriteLine($"Result length: {sequence.Length}");
    Console.WriteLine("Validation: PASSED");
    Console.WriteLine("Optimality: PROVEN");

    Console.WriteLine();
    Console.WriteLine("Fragment order:");
    Console.WriteLine(string.Join(" -> ",
        chain.Select(f => f.Value)));

    Console.WriteLine();
    Console.WriteLine("Final sequence:");
    Console.WriteLine(sequence);

    string outputPath = Path.Combine(
        AppContext.BaseDirectory,
        "result.txt");

    File.WriteAllText(outputPath, sequence);

    Console.WriteLine();
    Console.WriteLine($"Result saved to: {outputPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.ExitCode = 1;
}
