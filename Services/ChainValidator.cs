using BaD2026Puzzle.Models;

namespace BaD2026Puzzle.Services;

public class ChainValidator
{
    public bool IsValid(IReadOnlyList<Fragment> chain)
    {
        if (chain.Count == 0)
            return false;

        var usedIds = new HashSet<int>();

        for (int i = 0; i < chain.Count; i++)
        {
            Fragment current = chain[i];

            if (!usedIds.Add(current.Id))
                return false;

            if (i > 0 && chain[i - 1].End != current.Start)
                return false;
        }

        return true;
    }

    public string BuildSequence(IReadOnlyList<Fragment> chain)
    {
        if (!IsValid(chain))
            throw new InvalidOperationException(
                "Invalid fragment chain.");

        return chain[0].Value +
               string.Concat(chain.Skip(1)
                   .Select(fragment => fragment.Value[2..]));
    }
}
