using System.Net;
using System.Text;
using System.Text.Json;
using SpotifyPlaylistManager;

// Dependency-free executable tests: fake Spotify at the HTTP/workflow boundaries.
var root = Path.Combine(Path.GetTempPath(), "spotify-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
var tests = new List<(string Name, Func<Task> Run)>();
var ct = CancellationToken.None;
string FileAt(string name) => Path.Combine(root, name);
string Id(int n) => n.ToString().PadLeft(22, '0');
Entry Track(int n, string title = "Home", int position = 1, params Artist[] artists) =>
    new(position, Id(n), "", title, artists.Length > 0 ? artists : [new("artist-a", "Artist A")], "Album", 123000, "https://open.spotify.com/track/" + Id(n), "track", "available");
void Check(bool value, string message = "Assertion failed") { if (!value) throw new Exception(message); }
void Equal<T>(T expected, T actual) { Check(EqualityComparer<T>.Default.Equals(expected, actual), $"Expected {expected}, got {actual}"); }
async Task Throws<T>(Func<Task> action) where T : Exception
{
    try { await action(); } catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}");
}
string Review(string name, params string[][] rows)
{
    var path = FileAt(name);
    Csv.Write(path, ["request_id", "track_id", "selected"], rows);
    return path;
}

tests.Add(("CSV roundtrip preserves UTF-8, commas, quotes, multiline fields, and ordering", () =>
{
    var path = FileAt("roundtrip.csv");
    Csv.Write(path, ["title", "artist"], [new[] { "Hôme, \"Live\"\nAgain", "Artist A; Artist B" }, new[] { "Second", "B" }]);
    var rows = Csv.Read(path, "title", "artist");
    Equal(2, rows.Count); Equal("Hôme, \"Live\"\nAgain", rows[0].Get("title")); Equal(2, rows[1].Request);
    Check(rows.All(r => r.Error == ""));
    return Task.CompletedTask;
}));

tests.Add(("CSV rejects missing/duplicate headers and records malformed rows", async () =>
{
    var path = FileAt("invalid.csv");
    File.WriteAllText(path, "title,title\na,b\n");
    await Throws<ArgumentException>(() => Task.FromResult(Csv.Read(path, "title", "artist")));
    File.WriteAllText(path, "title\na\n");
    await Throws<ArgumentException>(() => Task.FromResult(Csv.Read(path, "title", "artist")));
    File.WriteAllText(path, "title,artist\na,b,extra\n\"bad\"tail,b\ngood,artist\n");
    var rows = Csv.Read(path, "title", "artist");
    Check(rows[0].Error != ""); Check(rows[1].Error != ""); Equal("good", rows[2].Get("title"));
}));

tests.Add(("Resolve produces unselected candidates and preserves invalid/unmatched requests", async () =>
{
    var input = FileAt("input.csv"); var output = FileAt("resolved.csv");
    File.WriteAllText(input, "title,artist,extra\nHome,Artist A,x\nMissing,Artist A,x\n,Artist A,x\nBroken,Artist A,x,extra\n");
    var fake = new FakeSpotify { SearchResults = (title, _) => title == "Home" ? Enumerable.Range(1, 8).Select(n => Track(n)).ToList() : [] };
    await Workflow.Resolve(fake, input, output, ct);
    var rows = Csv.Read(output, "request_id", "track_id", "selected", "status");
    Equal(8, rows.Count); Equal(5, rows.Count(r => r.Get("request_id") == "1"));
    Check(rows.All(r => r.Get("selected") == "false"));
    Equal("unmatched", rows[5].Get("status")); Equal("error", rows[6].Get("status")); Equal("error", rows[7].Get("status"));
    Equal(2, fake.SearchCalls);
}));

tests.Add(("Duplicate groups include remixes and featured artists without merging unrelated titles", () =>
{
    var a = new Artist("artist-a", "Artist A"); var b = new Artist("artist-b", "Artist B");
    var entries = new[] { Track(1), Track(2, " home (Live) ", 2), Track(3, "Home - Artist B Remix", 3, a, b),
        Track(4, "Home Again", 4), Track(5, "Home", 5, b), Track(6, "Home - Remastered 2011", 6),
        Track(1, "Home", 7), Track(7, "Home", 8) with { Status = "local" }, Track(8, "Home", 9) with { Status = "unavailable" },
        Track(9, "Home", 10) with { Type = "episode" } };
    var groups = TrackGrouping.Groups(entries);
    Equal(1, groups.Count); Equal(5, groups[0].Length); Equal(7, groups[0][4].Position);
    Equal("HOME", TrackGrouping.BaseTitle("Home (Live) - Remastered 2011"));
    Equal("HOME (PART TWO)", TrackGrouping.BaseTitle("Home (Part Two)"));
    Equal("HOME AGAIN", TrackGrouping.BaseTitle("Home Again"));
    return Task.CompletedTask;
}));

tests.Add(("Review validation happens before any Spotify access", async () =>
{
    var fake = new FakeSpotify();
    foreach (var rows in new[] {
        new[] { new[] { "1", Id(1), "true" }, new[] { "1", Id(2), "yes" } },
        new[] { new[] { "1", Id(1), "true" }, new[] { "2", "bad-id", "true" } },
        new[] { new[] { "1", Id(1), "tru" } },
        new[] { new[] { "0", Id(1), "true" } } })
        await Throws<ArgumentException>(() => Workflow.Add(fake, Id(100), Review("bad-review.csv", rows), ct));
    Equal(0, fake.EntryCalls); Equal(0, fake.Batches.Count);
}));

tests.Add(("Add sorts by request order, skips exact IDs and repeated selections, allows remaster", async () =>
{
    var fake = new FakeSpotify { Current = [Track(1)] };
    var review = Review("ordered-review.csv", ["5", Id(3), "true"], ["1", Id(1), "true"], ["3", Id(2), "yes"],
        ["2", "", "false"], ["4", Id(2), "1"]);
    var result = await Workflow.Add(fake, Id(100), review, ct);
    Equal(2, result.Added); Equal(2, result.Skipped); Equal(1, result.Unselected);
    Check(fake.Batches.Single().SequenceEqual(new[] { Id(2), Id(3) }));
    var rerun = await Workflow.Add(fake, Id(100), review, ct);
    Equal(0, rerun.Added); Equal(4, rerun.Skipped); Equal(1, fake.Batches.Count);
}));

tests.Add(("Relinked original IDs are recognized without excluding unavailable existing IDs", async () =>
{
    var fake = new FakeSpotify { Current = [Track(2) with { OriginalId = Id(1) }, Track(3) with { Status = "unavailable" }] };
    var result = await Workflow.Add(fake, Id(100), Review("linked.csv", ["1", Id(1), "true"], ["2", Id(3), "true"]), ct);
    Equal(0, result.Added); Equal(2, result.Skipped);
}));

tests.Add(("Partial batch failure stops writes; rerun appends only missing IDs", async () =>
{
    var fake = new FakeSpotify { FailBatch = 2 };
    var review = Review("batches.csv", Enumerable.Range(1, 205).Select(n => new[] { n.ToString(), Id(n), "true" }).ToArray());
    await Throws<SpotifyException>(() => Workflow.Add(fake, Id(100), review, ct));
    Equal(100, fake.Current.Count); Equal(2, fake.EntryCalls); Equal(2, fake.Batches.Count);
    fake.FailBatch = null;
    var result = await Workflow.Add(fake, Id(100), review, ct);
    Equal(105, result.Added); Equal(100, result.Skipped); Equal(205, fake.Current.Count);
    Check(fake.Current.Select(e => e.Id).SequenceEqual(Enumerable.Range(1, 205).Select(Id)));
}));

tests.Add(("Lost append response is reconciled and never blindly replayed", async () =>
{
    var fake = new FakeSpotify { FailBatch = 1, CommitBeforeFailure = true };
    var review = Review("lost.csv", ["1", Id(1), "true"], ["2", Id(2), "true"]);
    await Throws<SpotifyException>(() => Workflow.Add(fake, Id(100), review, ct));
    Equal(2, fake.Current.Count); Equal(1, fake.Batches.Count); Equal(2, fake.EntryCalls);
    fake.FailBatch = null;
    var result = await Workflow.Add(fake, Id(100), review, ct);
    Equal(0, result.Added); Equal(1, fake.Batches.Count);
}));

tests.Add(("API pagination reads current item schema and preserves all entry positions", async () =>
{
    var handler = new FakeHandler(async request =>
    {
        Equal("Bearer", request.Headers.Authorization!.Scheme);
        var path = request.RequestUri!.PathAndQuery;
        if (path.Contains("offset=50")) return Json("""{"items":[{"item":{"id":"0000000000000000000004","name":"Episode","type":"episode","duration_ms":300}}],"next":null}""");
        Check(path.Contains("/items?"));
        await Task.CompletedTask;
        return Json("""{"items":[{"item":{"id":"0000000000000000000001","name":"Home","type":"track","artists":[{"id":"a","name":"A"}],"album":{"name":"Album"},"duration_ms":123,"external_urls":{"spotify":"url"}}},{"item":null},{"is_local":true,"item":{"name":"Local","type":"track","artists":[{"name":"A"}]}}],"next":"https://api.spotify.com/v1/playlists/0000000000000000000100/items?offset=50"}""");
    });
    using var http = new HttpClient(handler);
    var api = new SpotifyClient(http, (_, _) => Task.FromResult("token"));
    var entries = await api.Entries(Id(100), ct);
    Equal(4, entries.Count); Equal("available", entries[0].Status); Equal("unavailable", entries[1].Status);
    Equal("local", entries[2].Status); Equal("episode", entries[3].Type); Equal(4, entries[3].Position);
    var path = FileAt("export.csv"); Csv.Write(path, Csv.ExportHeaders, entries.Select(Csv.ExportRow));
    Equal(4, Csv.Read(path, "position", "status").Count);
}));

tests.Add(("API search escapes terms, filters unavailable tracks, and sends append IDs in order", async () =>
{
    var handler = new FakeHandler(async request =>
    {
        if (request.Method == HttpMethod.Get)
        {
            var query = Uri.UnescapeDataString(request.RequestUri!.Query);
            Check(query.Contains("limit=5")); Check(query.Contains("track:Home artist:Artist A"));
            return Json("""{"tracks":{"items":[{"id":"0000000000000000000001","name":"Home","type":"track","artists":[]},{"id":"0000000000000000000002","name":"Home","type":"track","is_playable":false}]}}""");
        }
        Equal("/v1/playlists/" + Id(100) + "/items", request.RequestUri!.AbsolutePath);
        using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
        var ids = body.RootElement.GetProperty("uris").EnumerateArray().Select(e => e.GetString()).ToArray();
        Check(ids.SequenceEqual(new[] { "spotify:track:" + Id(2), "spotify:track:" + Id(1) }));
        return Json("""{"snapshot_id":"updated"}""", HttpStatusCode.Created);
    });
    using var http = new HttpClient(handler); var api = new SpotifyClient(http, (_, _) => Task.FromResult("token"));
    var results = await api.Search("Home", "Artist A", ct); Equal(1, results.Count(e => e.Searchable));
    await api.Append(Id(100), [Id(2), Id(1)], ct);
}));

tests.Add(("A truncated append response is reconciled through the HTTP client without retrying", async () =>
{
    var committed = false; var posts = 0; var reads = 0;
    using var http = new HttpClient(new FakeHandler(request =>
    {
        if (request.Method == HttpMethod.Post)
        {
            committed = true; posts++;
            return Task.FromResult(Json("{", HttpStatusCode.Created));
        }
        reads++;
        return Task.FromResult(committed
            ? Json("""{"items":[{"item":{"id":"0000000000000000000001","type":"track","name":"Home","artists":[]}}],"next":null}""")
            : Json("""{"items":[],"next":null}"""));
    }));
    var api = new SpotifyClient(http, (_, _) => Task.FromResult("token"));
    await Throws<SpotifyException>(() => Workflow.Add(api, Id(100), Review("truncated.csv", ["1", Id(1), "true"]), ct));
    Equal(1, posts); Equal(2, reads);
    var rerun = await Workflow.Add(api, Id(100), FileAt("truncated.csv"), ct);
    Equal(0, rerun.Added); Equal(1, rerun.Skipped); Equal(1, posts);
}));

tests.Add(("Long rate limits surface immediately rather than waiting or replaying writes", async () =>
{
    var calls = 0;
    using var http = new HttpClient(new FakeHandler(_ =>
    {
        calls++;
        var response = Json("{}", HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromMinutes(10));
        return Task.FromResult(response);
    }));
    var api = new SpotifyClient(http, (_, _) => Task.FromResult("token"));
    await Throws<SpotifyException>(() => api.Append(Id(100), [Id(1)], ct)); Equal(1, calls);
}));

tests.Add(("API refreshes 401 and honors bounded 429 retries", async () =>
{
    var calls = 0; var refreshes = 0;
    var handler = new FakeHandler(_ =>
    {
        calls++;
        if (calls == 1) return Task.FromResult(Json("{}", HttpStatusCode.Unauthorized));
        if (calls == 2)
        {
            var response = Json("{}", HttpStatusCode.TooManyRequests);
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.Zero);
            return Task.FromResult(response);
        }
        return Task.FromResult(Json("""{"tracks":{"items":[]}}"""));
    });
    using var http = new HttpClient(handler);
    var api = new SpotifyClient(http, (force, _) => { if (force) refreshes++; return Task.FromResult("token"); });
    await api.Search("Home", "A", ct); Equal(3, calls); Equal(1, refreshes);
}));

tests.Add(("API never retries a 500 append and rejects foreign pagination URLs", async () =>
{
    var calls = 0;
    using var http = new HttpClient(new FakeHandler(_ => { calls++; return Task.FromResult(Json("{}", HttpStatusCode.InternalServerError)); }));
    var api = new SpotifyClient(http, (_, _) => Task.FromResult("token"));
    await Throws<SpotifyException>(() => api.Append(Id(100), [Id(1)], ct)); Equal(1, calls);
    calls = 0;
    using var foreignHttp = new HttpClient(new FakeHandler(_ => { calls++; return Task.FromResult(Json("""{"items":[],"next":"https://example.com/steal-token"}""")); }));
    var foreignApi = new SpotifyClient(foreignHttp, (_, _) => Task.FromResult("token"));
    await Throws<SpotifyException>(() => foreignApi.Entries(Id(100), ct)); Equal(1, calls);
}));

tests.Add(("Playlist discovery filters to owned/collaborative and supports name search data", async () =>
{
    using var http = new HttpClient(new FakeHandler(request => Task.FromResult(request.RequestUri!.AbsolutePath.EndsWith("/me", StringComparison.Ordinal)
        ? Json("""{"id":"me"}""")
        : Json("""{"items":[{"id":"one","name":"Mine","owner":{"id":"me","display_name":"Me"}},{"id":"two","name":"Shared","collaborative":true,"owner":{"id":"other"}},{"id":"three","name":"Editorial","owner":{"id":"spotify"}}],"next":null}"""))));
    var api = new SpotifyClient(http, (_, _) => Task.FromResult("token"));
    var playlists = await api.Playlists(ct); Equal(2, playlists.Count); Check(playlists[0].Owned); Check(playlists[1].Collaborative);
}));

tests.Add(("Credentials persist outside repo, preserve refresh token, clear on logout", async () =>
{
    var directory = FileAt("credentials"); var store = new CredentialStore(directory);
    store.Save(new("client", "old", "refresh", DateTimeOffset.UtcNow.AddMinutes(-1)));
    if (!OperatingSystem.IsWindows())
        Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(Path.Combine(directory, "credentials.json")));
    using var http = new HttpClient(new FakeHandler(async request =>
    {
        Equal("https://accounts.spotify.com/api/token", request.RequestUri!.ToString());
        var body = await request.Content!.ReadAsStringAsync(); Check(body.Contains("refresh_token=refresh"));
        return Json("""{"access_token":"new","expires_in":3600}""");
    }));
    var auth = new SpotifyAuth(http, store); Equal("new", await auth.AccessToken(false, ct));
    Equal("refresh", store.Load()!.RefreshToken); Equal("new", store.Load()!.AccessToken);
    store.Logout(); Check(store.Load() == null);
    await Throws<SpotifyException>(() => new SpotifyAuth(http, store).AccessToken(false, ct));
}));

tests.Add(("PKCE login checks state, exchanges code, and persists credentials over real loopback HTTP", async () =>
{
    var store = new CredentialStore(FileAt("login"));
    string? codeVerifier = null;
    using var tokenHttp = new HttpClient(new FakeHandler(async request =>
    {
        var body = await request.Content!.ReadAsStringAsync();
        var fields = body.Split('&').Select(p => p.Split('=', 2)).ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));
        Equal("authorization_code", fields["grant_type"]); Equal("test-code", fields["code"]);
        Equal(SpotifyAuth.RedirectUri, fields["redirect_uri"]); codeVerifier = fields["code_verifier"];
        return Json("""{"access_token":"access","refresh_token":"refresh","expires_in":3600}""");
    }));
    var auth = new SpotifyAuth(tokenHttp, store);
    var output = new StringWriter(); var originalOutput = Console.Out;
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    try
    {
        Console.SetOut(output);
        var login = auth.Login(new string('a', 32), false, deadline.Token);
        var url = output.ToString().Split('\n').Single(line => line.StartsWith("https://accounts.spotify.com/authorize?", StringComparison.Ordinal)).Trim();
        var query = new Uri(url).Query.TrimStart('?').Split('&').Select(p => p.Split('=', 2)).ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));
        Equal("S256", query["code_challenge_method"]);
        using var browser = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        using var invalid = await browser.GetAsync(SpotifyAuth.RedirectUri + "?state=wrong&code=test-code", deadline.Token);
        Equal(HttpStatusCode.BadRequest, invalid.StatusCode); Check(store.Load() == null);
        using var success = await browser.GetAsync(SpotifyAuth.RedirectUri + "?state=" + query["state"] + "&code=test-code", deadline.Token);
        Equal(HttpStatusCode.OK, success.StatusCode); await login;
        Equal(query["code_challenge"], SpotifyAuth.Base64Url(System.Security.Cryptography.SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier!))));
        Equal("access", store.Load()!.AccessToken); Equal("refresh", store.Load()!.RefreshToken);
    }
    finally { Console.SetOut(originalOutput); deadline.Cancel(); }
}));

tests.Add(("Concurrent local imports are blocked and locks release on disposal", async () =>
{
    var store = new CredentialStore(FileAt("locks"));
    using (store.LockPlaylist(Id(100)))
        await Throws<SpotifyException>(() => Task.FromResult(store.LockPlaylist(Id(100))));
    using var unlocked = store.LockPlaylist(Id(100));
}));

tests.Add(("CLI rejects missing/unknown options and parses playlist URLs", async () =>
{
    Equal(Id(100), SpotifyIds.Playlist("https://open.spotify.com/playlist/" + Id(100) + "?si=abc"));
    Equal(Id(100), SpotifyIds.Playlist("spotify:playlist:" + Id(100)));
    await Throws<ArgumentException>(() => Task.FromResult(SpotifyIds.Playlist("https://example.com/playlist/" + Id(100))));
    await Throws<ArgumentException>(() => Task.FromResult(Arguments.Parse(["add", Id(100), "--review"])));
    await Throws<ArgumentException>(() => Task.FromResult(Arguments.Parse(["export", Id(100), "--output", "x.csv", "--typo", "x"])));
    await Throws<ArgumentException>(() => Task.FromResult(Arguments.Parse(["create"])));
    Equal("x.csv", Arguments.Parse(["add", Id(100), "--review", "x.csv"]).Require("review"));
}));

var failed = 0;
try
{
    foreach (var (name, run) in tests)
    {
        try { await run(); Console.WriteLine($"PASS {name}"); }
        catch (Exception ex) { failed++; Console.Error.WriteLine($"FAIL {name}\n{ex}"); }
    }
}
finally { Directory.Delete(root, recursive: true); }
Console.WriteLine($"{tests.Count - failed}/{tests.Count} tests passed.");
return failed == 0 ? 0 : 1;

static HttpResponseMessage Json(string body, HttpStatusCode status = HttpStatusCode.OK) =>
    new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handle) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => handle(request);
}
sealed class FakeSpotify : ISpotify
{
    public List<Entry> Current = [];
    public List<string[]> Batches = [];
    public int EntryCalls;
    public int SearchCalls;
    public int? FailBatch;
    public bool CommitBeforeFailure;
    public Func<string, string, List<Entry>> SearchResults = (_, _) => [];
    public Task<List<Entry>> Search(string title, string artist, CancellationToken ct) { SearchCalls++; return Task.FromResult(SearchResults(title, artist)); }
    public Task<List<Entry>> Entries(string playlist, CancellationToken ct) { EntryCalls++; return Task.FromResult(Current.ToList()); }
    public Task Append(string playlist, string[] ids, CancellationToken ct)
    {
        Batches.Add(ids);
        var fail = FailBatch == Batches.Count;
        if (!fail || CommitBeforeFailure)
            Current.AddRange(ids.Select(id => new Entry(Current.Count + 1, id, "", "Home", [new("a", "A")], "Album", 1000, "", "track", "available")));
        if (fail) throw new HttpRequestException("Lost response");
        return Task.CompletedTask;
    }
}
