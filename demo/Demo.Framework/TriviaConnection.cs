using System;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Console;

namespace Demo.Framework;

public sealed class TriviaConnection : ITriviaConnection
{
    private readonly HttpClient _client;
    private readonly Queue<TriviaQuestion> _queue;
    private string? _token;

    public TriviaConnection()
    {
        _client = new HttpClient();
        _queue = new Queue<TriviaQuestion>();
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
        var json = await Get("https://opentdb.com/api_category.php");
        var data = JsonConvert.DeserializeObject<TriviaCategoryResult>(json);
        if (data == null)
        {
            throw new InvalidOperationException("Could not get categories");
        }

        return data.Categories;
    }

    public async Task<TriviaQuestion> GetQuestion(StatusContext ctx, TriviaDifficulty difficulty, TriviaCategory category)
    {
        if (_queue.Count > 0)
        {
            return _queue.Dequeue();
        }

        var fetch = 10;

        while (true)
        {
            await Task.Delay(500);
            ctx.Spinner(Spinner.Known.Earth);
            ctx.Status("Sending web request...");

            var url = $"https://opentdb.com/api.php?amount={fetch}&category={category.Id}&difficulty={difficulty.ToString().ToLowerInvariant()}&type=multiple";
            var json = await Get(url);

            var data = JsonConvert.DeserializeObject<TriviaQuestionResult>(json);
            if (data == null)
            {
                throw new InvalidOperationException("Could not get question");
            }

            if (data.ResponseCode == 0)
            {
#if RECORD
                var guid = Guid.NewGuid().ToString("N");
                File.WriteAllText($"question_{guid}.json", json);
#endif

                foreach (var question in data.Results)
                {
                    _queue.Enqueue(question.Normalize());
                }

                return _queue.Dequeue();
            }
            else if (data.ResponseCode is 1)
            {
                fetch /= 2;

                // We don't want to spam them.
                ctx.Status("Taking a bit longer than expected...");
                await Task.Delay(1000);
            }
            else if (data.ResponseCode is 3)
            {
                fetch /= 2;

                // We don't want to spam them.
                ctx.Status("Taking a bit longer than expected...");
                ResetSessionKey();
                await Task.Delay(1000);
            }
            else if (data.ResponseCode is 4)
            {
                fetch /= 2;

                // We don't want to spam them.
                ctx.Status("Taking a bit longer than expected...");
                ResetSessionKey();
                await Task.Delay(1000);
            }
            else
            {
                throw new InvalidOperationException("Could not get question due to a server response");
            }
        }
    }

    private async Task<string> Get(string url, bool includeToken = true)
    {
        if (includeToken)
        {
            await EnsureSessionKey(false);

            url = url.Contains("?")
                ? $"{url}&token={_token}"
                : $"{url}?token={_token}";

            return await _client.GetStringAsync(url);
        }

        return await _client.GetStringAsync(url);
    }

    private void ResetSessionKey()
    {
        _token = null;
    }

    private async Task EnsureSessionKey(bool update = false)
    {
        if (_token != null)
        {
            return;
        }

        var json = await _client.GetStringAsync("https://opentdb.com/api_token.php?command=request");
        var response = JsonConvert.DeserializeObject<TriviaTokenResponse>(json);
        if (response == null)
        {
            throw new InvalidOperationException("Could not retrieve session key");
        }

        _token = response.Token;
    }
}
