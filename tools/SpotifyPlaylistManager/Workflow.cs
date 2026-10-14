using System.Globalization;

namespace SpotifyPlaylistManager;

public interface ISpotify
{
    Task<List<Entry>> Search(string title, string artist, CancellationToken ct);
    Task<List<Entry>> Entries(string playlist, CancellationToken ct);
    Task Append(string playlist, string[] ids, CancellationToken ct);
}

public record Selection(int Request, string TrackId);
public record ImportResult(int Added, int Skipped, int Unselected);

public static class Workflow
{
    public static readonly string[] ReviewHeaders = ["request_id", "input_title", "input_artist", "title", "artists", "album", "duration_ms", "track_id", "spotify_url", "selected", "status", "error"];

    public static async Task Resolve(ISpotify api, string input, string output, CancellationToken ct)
    {
        var requests = Csv.Read(input, "title", "artist");
        var result = new List<string[]>();
        foreach (var row in requests)
        {
            var title = row.Get("title").Trim();
            var artist = row.Get("artist").Trim();
            var error = row.Error != "" ? row.Error : title == "" || artist == "" ? "Title and artist must be nonempty." : "";
            var candidates = error == "" ? (await api.Search(title, artist, ct)).Where(e => e.Searchable).DistinctBy(e => e.Id).Take(5).ToArray() : [];
            if (candidates.Length == 0)
                result.Add([Csv.Number(row.Request), title, artist, "", "", "", "", "", "", "false", error == "" ? "unmatched" : "error", error]);
            foreach (var e in candidates)
                result.Add([Csv.Number(row.Request), title, artist, e.Title, e.ArtistNames, e.Album,
                    e.DurationMs?.ToString(CultureInfo.InvariantCulture) ?? "", e.Id, e.Url, "false", "candidate", ""]);
        }
        Csv.Write(output, ReviewHeaders, result);
    }

    public static (List<Selection> Selected, int Unselected) ReadSelections(string path)
    {
        var rows = Csv.Read(path, "request_id", "track_id", "selected");
        var selections = new List<Selection>();
        var requests = new HashSet<int>();
        var selectedRequests = new HashSet<int>();
        foreach (var row in rows)
        {
            if (row.Error != "") throw new ArgumentException($"Review row {row.Request}: {row.Error}");
            if (!int.TryParse(row.Get("request_id"), NumberStyles.None, CultureInfo.InvariantCulture, out var request) || request < 1)
                throw new ArgumentException($"Review row {row.Request}: invalid request_id.");
            requests.Add(request);
            var marker = row.Get("selected").Trim().ToLowerInvariant();
            if (marker is not ("" or "false" or "0" or "no" or "true" or "1" or "yes"))
                throw new ArgumentException($"Review row {row.Request}: selected must be true/false, yes/no, 1/0, or blank.");
            if (marker is not ("true" or "1" or "yes")) continue;
            var id = row.Get("track_id").Trim();
            if (!SpotifyIds.Valid(id)) throw new ArgumentException($"Review row {row.Request}: invalid selected Spotify track ID.");
            if (!selectedRequests.Add(request)) throw new ArgumentException($"Request {request} has multiple selected candidates.");
            selections.Add(new(request, id));
        }
        return (selections.OrderBy(s => s.Request).ToList(), requests.Count - selections.Count);
    }

    public static async Task<ImportResult> Add(ISpotify api, string playlist, string review, CancellationToken ct)
    {
        var (selected, unselected) = ReadSelections(review); // Validate everything before even reading Spotify.
        var existing = KnownIds(await api.Entries(playlist, ct));
        var pending = selected.Select(s => s.TrackId).Where(existing.Add).ToArray();
        var skipped = selected.Count - pending.Length;
        var added = 0;
        foreach (var batch in pending.Chunk(100))
        {
            try { await api.Append(playlist, batch, ct); added += batch.Length; }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or SpotifyException or System.Text.Json.JsonException or IOException)
            {
                // An append may have committed even when its response was lost. Never replay it here.
                var reconciliation = "Could not reconcile; re-read the playlist before retrying.";
                if (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var now = KnownIds(await api.Entries(playlist, ct));
                        reconciliation = $"{batch.Count(now.Contains)} of {batch.Length} IDs in the failed batch are now present.";
                    }
                    catch (Exception readError) when (readError is HttpRequestException or OperationCanceledException or SpotifyException or System.Text.Json.JsonException or IOException)
                    { /* Preserve the original write failure; the next run always re-reads. */ }
                }
                throw new SpotifyException($"Import stopped after {added} confirmed additions; {skipped} exact-ID skips; {unselected} unselected requests. {reconciliation} Rerun the same add command to add missing IDs. Cause: {ex.Message}", ex);
            }
        }
        return new(added, skipped, unselected);
    }

    // Spotify can relink an unavailable recording to a playable ID. Keep both identities for exact-ID skipping.
    private static HashSet<string> KnownIds(IEnumerable<Entry> entries) => entries.Where(e => e.Type == "track" && e.Status != "local")
        .SelectMany(e => new[] { e.Id, e.OriginalId }).Where(id => id != "").ToHashSet(StringComparer.Ordinal);
}

public class SpotifyException(string message, Exception? inner = null) : Exception(message, inner);
