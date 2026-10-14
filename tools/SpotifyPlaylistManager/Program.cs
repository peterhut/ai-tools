using System.Text;

namespace SpotifyPlaylistManager;

public static class Program
{
    private const string Help = """
        Spotify playlist manager (.NET 10)

        dotnet run --project tools/SpotifyPlaylistManager -- <command> [arguments]

        login --client-id CLIENT_ID [--no-browser]
        logout
        playlists [--filter TEXT]
        export PLAYLIST --output playlist.csv
        resolve --input tracks.csv --output review.csv
        add PLAYLIST --review review.csv
        duplicates PLAYLIST --output duplicates.csv

        PLAYLIST accepts an ID, Spotify URI, or Spotify playlist URL.
        Input CSV requires title,artist headers. Review CSV: mark selected=true
        for at most one candidate per request_id; then run add.
        Add appends in input order and skips only exact IDs already present.
        Export positions are one-based. Output files are replaced atomically.

        Spotify app redirect URI: http://127.0.0.1:43821/callback
        """;

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
        try
        {
            if (args.Length == 0 || args is ["help"] or ["--help"] or ["-h"]) { Console.WriteLine(Help); return 0; }
            var options = Arguments.Parse(args);
            using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(30) };
            var store = new CredentialStore();
            var auth = new SpotifyAuth(http, store);
            var api = new SpotifyClient(http, auth.AccessToken);
            var ct = cancellation.Token;
            switch (options.Command)
            {
                case "login": await auth.Login(options.Require("client-id"), options.Get("no-browser") == null, ct); break;
                case "logout": store.Logout(); Console.WriteLine("Local credentials cleared. To revoke Spotify authorization, remove the app in your Spotify account settings."); break;
                case "playlists":
                    foreach (var p in (await api.Playlists(ct)).Where(p => options.Get("filter") == null || p.Name.Contains(options.Get("filter")!, StringComparison.OrdinalIgnoreCase)))
                        Console.WriteLine($"{p.Id}\t{p.Name}\t{(p.Owned ? "owned" : "collaborative")}\t{p.Url}");
                    break;
                case "export":
                    var entries = await api.Entries(SpotifyIds.Playlist(options.Playlist!), ct);
                    Csv.Write(options.Require("output"), Csv.ExportHeaders, entries.Select(Csv.ExportRow));
                    Console.WriteLine($"Exported {entries.Count} ordered entries to {options.Require("output")}."); break;
                case "resolve":
                    var input = options.Require("input");
                    var output = options.Require("output");
                    if (Path.GetFullPath(input).Equals(Path.GetFullPath(output), OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                        throw new ArgumentException("Input and review output must be different files.");
                    await Workflow.Resolve(api, input, output, ct);
                    Console.WriteLine($"Wrote {output}. Mark selected=true for one candidate per request_id, then run add."); break;
                case "add":
                    var playlist = SpotifyIds.Playlist(options.Playlist!);
                    using (store.LockPlaylist(playlist))
                    {
                        var result = await Workflow.Add(api, playlist, options.Require("review"), ct);
                        Console.WriteLine($"Added {result.Added}; skipped {result.Skipped} exact IDs; {result.Unselected} unselected requests.");
                    }
                    break;
                case "duplicates":
                    var groups = TrackGrouping.Groups(await api.Entries(SpotifyIds.Playlist(options.Playlist!), ct));
                    Csv.Write(options.Require("output"), ["group_id", "base_title", "first_artist", .. Csv.ExportHeaders],
                        groups.SelectMany((group, index) => group.Select(e => new[] { Csv.Number(index + 1), TrackGrouping.BaseTitle(e.Title), e.Artists[0].Name }.Concat(Csv.ExportRow(e)))));
                    foreach (var group in groups)
                    {
                        Console.WriteLine($"{group.Length} entries: {TrackGrouping.BaseTitle(group[0].Title)} / {group[0].Artists[0].Name}");
                        foreach (var e in group) Console.WriteLine($"  {e.Position}: {e.Title} — {e.ArtistNames} [{e.Album}] {e.Id}");
                    }
                    Console.WriteLine($"{groups.Count} groups. Report: {options.Require("output")}"); break;
            }
            return 0;
        }
        catch (OperationCanceledException) { Console.Error.WriteLine("Canceled or timed out. If adding, rerun to reconcile the playlist and add missing IDs."); return 130; }
        catch (Exception ex) when (ex is ArgumentException or IOException or SpotifyException or HttpRequestException or System.Text.Json.JsonException or System.Net.Sockets.SocketException or UnauthorizedAccessException)
        { Console.Error.WriteLine($"Error: {ex.Message}"); return 1; }
    }
}

public record Arguments(string Command, string? Playlist, Dictionary<string, string> Options)
{
    public string? Get(string key) => Options.GetValueOrDefault(key);
    public string Require(string key) => Get(key) is { Length: > 0 } value ? value : throw new ArgumentException($"Missing --{key}.");
    public static Arguments Parse(string[] args)
    {
        var command = args[0];
        var allowed = command switch
        {
            "login" => new[] { "client-id", "no-browser" }, "logout" => [], "playlists" => ["filter"],
            "export" or "duplicates" => ["output"], "resolve" => ["input", "output"], "add" => ["review"],
            _ => throw new ArgumentException($"Unknown command '{command}'. Run help.")
        };
        var hasPlaylist = command is "export" or "duplicates" or "add";
        var index = hasPlaylist ? 2 : 1;
        if (hasPlaylist && (args.Length < 2 || args[1].StartsWith("--", StringComparison.Ordinal))) throw new ArgumentException("Missing playlist URL or ID.");
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = index; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException($"Unexpected argument '{args[i]}'.");
            var key = args[i][2..];
            if (!allowed.Contains(key) || options.ContainsKey(key)) throw new ArgumentException($"Unknown or repeated option --{key}.");
            var value = "true";
            if (key != "no-browser")
            {
                if (++i >= args.Length || args[i].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException($"Missing value for --{key}.");
                value = args[i];
            }
            options[key] = value;
        }
        var result = new Arguments(command, hasPlaylist ? args[1] : null, options);
        foreach (var key in allowed.Where(k => k is not ("no-browser" or "filter"))) result.Require(key);
        return result;
    }
}
