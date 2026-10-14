using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SpotifyPlaylistManager;

public record PlaylistInfo(string Id, string Name, string Owner, bool Collaborative, bool Owned)
{
    public string Url => "https://open.spotify.com/playlist/" + Id;
}

public class SpotifyClient(HttpClient http, Func<bool, CancellationToken, Task<string>> token) : ISpotify
{
    private const string Base = "https://api.spotify.com/v1/";

    public async Task<List<PlaylistInfo>> Playlists(CancellationToken ct)
    {
        using var profile = await Request(HttpMethod.Get, "me", null, ct);
        var userId = profile.RootElement.Str("id");
        var items = await Pages("me/playlists?limit=50", ct);
        return items.Where(p => p.Child("owner").Str("id") == userId || p.Bool("collaborative") == true)
            .Select(p => new PlaylistInfo(p.Str("id"), p.Str("name"), p.Child("owner").Str("display_name"),
                p.Bool("collaborative") == true, p.Child("owner").Str("id") == userId)).ToList();
    }

    public async Task<List<Entry>> Entries(string playlist, CancellationToken ct)
    {
        var items = await Pages($"playlists/{playlist}/items?limit=50&additional_types=track,episode", ct);
        return items.Select((wrapper, index) => Entry.Parse(wrapper.Child("item").ValueKind != JsonValueKind.Undefined
            ? wrapper.Child("item") : wrapper.Child("track"), index + 1, wrapper.Bool("is_local") == true)).ToList();
    }

    public async Task<List<Entry>> Search(string title, string artist, CancellationToken ct)
    {
        // Strip syntax characters so a CSV field cannot introduce extra search filters.
        static string Term(string value) => value.Replace('"', ' ').Replace(':', ' ').Trim();
        var query = Uri.EscapeDataString($"track:{Term(title)} artist:{Term(artist)}");
        using var json = await Request(HttpMethod.Get, $"search?type=track&limit=5&q={query}", null, ct);
        return json.RootElement.GetProperty("tracks").GetProperty("items").EnumerateArray()
            .Select(e => Entry.Parse(e, 0)).ToList();
    }

    public async Task Append(string playlist, string[] ids, CancellationToken ct)
    {
        if (ids.Length is < 1 or > 100 || ids.Any(id => !SpotifyIds.Valid(id))) throw new ArgumentException("Append requires 1–100 valid track IDs.");
        using var result = await Request(HttpMethod.Post, $"playlists/{playlist}/items", new { uris = ids.Select(id => "spotify:track:" + id).ToArray() }, ct);
    }

    private async Task<List<JsonElement>> Pages(string path, CancellationToken ct)
    {
        var result = new List<JsonElement>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (path != "")
        {
            if (!visited.Add(path)) throw new SpotifyException("Spotify returned a pagination loop.");
            using var json = await Request(HttpMethod.Get, path, null, ct);
            result.AddRange(json.RootElement.GetProperty("items").EnumerateArray().Select(e => e.Clone()));
            path = json.RootElement.Str("next");
        }
        return result;
    }

    private async Task<JsonDocument> Request(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        var uri = new Uri(new Uri(Base), path);
        if (uri.Scheme != "https" || uri.Host != "api.spotify.com" || uri.Port != 443 || uri.UserInfo != "" || !uri.AbsolutePath.StartsWith("/v1/", StringComparison.Ordinal))
            throw new SpotifyException("Spotify returned an unexpected pagination URL.");
        var refreshed = false;
        var hasRefreshed = false;
        for (var attempt = 0; ; attempt++)
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await token(refreshed, ct));
            refreshed = false;
            if (body != null) request.Content = JsonContent.Create(body);
            using var response = await http.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized && !hasRefreshed) { refreshed = true; hasRefreshed = true; continue; }
            if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < 3)
            {
                var delay = response.Headers.RetryAfter?.Delta ?? (response.Headers.RetryAfter?.Date - DateTimeOffset.UtcNow) ?? TimeSpan.FromSeconds(2);
                // Long quota exhaustion should surface to the user instead of appearing to hang.
                if (delay > TimeSpan.FromSeconds(30)) throw new SpotifyException($"Spotify rate limit: retry after {Math.Ceiling(delay.TotalSeconds)} seconds.");
                await Task.Delay(delay < TimeSpan.Zero ? TimeSpan.Zero : delay, ct);
                continue;
            }
            if (!response.IsSuccessStatusCode)
            {
                var hint = response.StatusCode switch
                {
                    HttpStatusCode.Forbidden => "Check app allowlisting and playlist ownership/collaboration/write access.",
                    HttpStatusCode.Unauthorized => "Sign in again.",
                    HttpStatusCode.TooManyRequests => "Spotify quota exceeded; retry later.",
                    HttpStatusCode.NotFound => "Check the playlist or track ID.",
                    _ => "Try again later."
                };
                throw new SpotifyException($"Spotify API request failed ({(int)response.StatusCode}). {hint}");
            }
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonDocument.Parse(content);
        }
    }
}
