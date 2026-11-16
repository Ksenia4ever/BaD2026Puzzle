namespace BaD2026Puzzle.Models;

public record Fragment(int Id, string Value)
{
    public string Start => Value[..2];

    public string End => Value[^2..];
}
