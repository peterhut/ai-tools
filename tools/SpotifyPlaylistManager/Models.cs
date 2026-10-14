using System.Text.Json;
using System.Text.RegularExpressions;

namespace SpotifyPlaylistManager;

public record Artist(string Id, string Name);
public record Entry(int Position, string Id, string OriginalId, string Title, Artist[] Artists,
    string Album, int? DurationMs, string Url, string Type, string Status)
{
    public bool Searchable => Type == "track" && Status == "available" && Id != "";
    public string ArtistNames => string.Join("; ", Artists.Select(a => a.Name));

    public static Entry Parse(JsonElement item, int position, bool local = false)
    {
        if (item.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return new(position, "", "", "", [], "", null, "", "unknown", "unavailable");
        local |= item.Bool("is_local") == true;
        var artists = item.TryGetProperty("artists", out var array)
            ? array.EnumerateArray().Select(a => new Artist(a.Str("id"), a.Str("name"))).ToArray() : [];
        var status = local ? "local" : item.Bool("is_playable") == false || item.Str("id") == "" ||
            (item.TryGetProperty("restrictions", out var restrictions) && restrictions.Str("reason") != "")
            ? "unavailable" : "available";
        return new(position, item.Str("id"), item.Child("linked_from").Str("id"), item.Str("name"), artists,
            item.Child("album").Str("name"), item.TryGetProperty("duration_ms", out var duration) && duration.TryGetInt32(out var ms) ? ms : null,
            item.Child("external_urls").Str("spotify"), item.Str("type"), status);
    }
}

public static class JsonFields
{
    public static JsonElement Child(this JsonElement value, string key) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty(key, out var child) ? child : default;
    public static string Str(this JsonElement value, string key) =>
        value.Child(key) is var child && child.ValueKind == JsonValueKind.String ? child.GetString()! : "";
    public static bool? Bool(this JsonElement value, string key) => value.Child(key).ValueKind switch
    {
        JsonValueKind.True => true, JsonValueKind.False => false, _ => null
    };
}

public static partial class TrackGrouping
{
    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex Spaces();
    // Strip only delimited suffixes that contain a recognized version word.
    // Never remove arbitrary trailing words (Home Again must remain distinct).
    [GeneratedRegex(@"\s*(?:\((?<label>[^()]*)\)|\[(?<label>[^\[\]]*)\]|\s[-–—]\s(?<label>.+))\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex Suffix();
    [GeneratedRegex(@"\b(?:live|remaster(?:ed)?|remix(?:ed)?|mix|edit|version|acoustic|instrumental|demo|radio|extended|mono|stereo)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VersionWord();
    public static string Normalize(string value) => Spaces().Replace(value.Trim(), " ").ToUpperInvariant();
    public static string BaseTitle(string title)
    {
        title = title.Trim();
        while (Suffix().Match(title) is { Success: true } match && VersionWord().IsMatch(match.Groups["label"].Value))
            title = title[..match.Index].TrimEnd();
        return Normalize(title);
    }

    public static List<Entry[]> Groups(IEnumerable<Entry> entries) => entries
        .Where(e => e.Searchable && e.Artists.Length > 0 && (e.Artists[0].Id != "" || Normalize(e.Artists[0].Name) != "") && BaseTitle(e.Title) != "")
        .GroupBy(e => (Title: BaseTitle(e.Title), Artist: e.Artists[0].Id != ""
            ? "id:" + e.Artists[0].Id : "name:" + Normalize(e.Artists[0].Name)))
        .Select(g => g.OrderBy(e => e.Position).ToArray()).Where(g => g.Length > 1)
        .OrderBy(g => g[0].Position).ToList();
}

public static partial class SpotifyIds
{
    [GeneratedRegex(@"^[A-Za-z0-9]{22}$", RegexOptions.CultureInvariant)]
    private static partial Regex IdPattern();
    public static bool Valid(string id) => IdPattern().IsMatch(id);
    public static string Playlist(string value)
    {
        if (value.StartsWith("spotify:playlist:", StringComparison.Ordinal)) value = value[17..];
        else if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == "https" && uri.Host == "open.spotify.com")
        {
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var index = Array.IndexOf(segments, "playlist");
            value = index >= 0 && index + 2 == segments.Length ? segments[index + 1] : "";
        }
        if (!Valid(value)) throw new ArgumentException("Expected a Spotify playlist ID, URI, or https://open.spotify.com/playlist/... URL.");
        return value;
    }
}
