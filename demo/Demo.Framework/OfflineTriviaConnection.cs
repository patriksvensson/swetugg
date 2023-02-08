using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace Demo.Framework;

public sealed class OfflineTriviaConnection : ITriviaConnection
{
    private readonly List<TriviaQuestion> _questions;
    private readonly HashSet<string> _asked;

    public OfflineTriviaConnection()
    {
        _questions = new List<TriviaQuestion>();
        _asked = new HashSet<string>();

        var directory = Path.GetDirectoryName(typeof(TriviaClient).Assembly.Location);
        directory = Path.Combine(directory, "Data");

        var files = Directory.GetFiles(directory, "*.json");
        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            var result = JsonConvert.DeserializeObject<TriviaQuestionResult>(json);
            if (result != null)
            {
                foreach (var item in result.Results)
                {
                    _questions.Add(item.Normalize());
                }
            }
        }
    }

    public List<TriviaDifficulty> GetDifficulties()
    {
        return new List<TriviaDifficulty>
        {
            TriviaDifficulty.Easy,
            TriviaDifficulty.Medium,
            TriviaDifficulty.Hard,
        };
    }

    public async Task<List<TriviaCategory>> GetCategories()
    {
        await Task.Delay(1200);

        var categories = new HashSet<string>(_questions.Select(x => x.Category));
        return categories.Select((c, i) => new TriviaCategory
        {
            Id = i,
            Name = c,
        }).ToList();
    }

    public async Task<TriviaQuestion> GetQuestion(StatusContext ctx, TriviaDifficulty difficulty, TriviaCategory category)
    {
        await Task.Delay(800);

        var exactMatches = _questions.Where(x => x.Category == category.Name &&
            x.Difficulty == difficulty.ToString().ToLowerInvariant() &&
            !_asked.Contains(x.Question))
            .ToList();

        if (exactMatches.Count > 0)
        {
            var q = exactMatches.GetRandom();
            _asked.Add(q.Question);
            return q;
        }

        var almostExactMatch = _questions.Where(x => x.Category == category.Name &&
            x.Difficulty == difficulty.ToString().ToLowerInvariant()).ToList();
        if (almostExactMatch.Count > 0)
        {
            _asked.Clear();

            var q = almostExactMatch.GetRandom();
            _asked.Add(q.Question);
            return q;
        }

        var bestGuess = _questions.Where(x => x.Category == category.Name).ToList();
        if (bestGuess.Count > 0)
        {
            _asked.Clear();

            var q = bestGuess.GetRandom();
            _asked.Add(q.Question);
            return q;
        }

        throw new InvalidOperationException("Could not get question.");
    }
}
