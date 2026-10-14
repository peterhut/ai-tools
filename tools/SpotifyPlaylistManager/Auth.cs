using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SpotifyPlaylistManager;

public record Credentials(string ClientId, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public class CredentialStore
{
    public string DirectoryPath { get; }
    private string TokenPath => Path.Combine(DirectoryPath, "credentials.json");
    public CredentialStore(string? directory = null)
    {
        DirectoryPath = directory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SpotifyPlaylistManager");
    }
    public Credentials? Load() => File.Exists(TokenPath)
        ? JsonSerializer.Deserialize<Credentials>(File.ReadAllText(TokenPath)) : null;
    public void Save(Credentials credentials)
    {
        EnsureDirectory();
        var temp = Path.Combine(DirectoryPath, Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write, Share = FileShare.None };
            if (!OperatingSystem.IsWindows()) options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
            using (var stream = new FileStream(temp, options)) JsonSerializer.Serialize(stream, credentials);
            File.Move(temp, TokenPath, overwrite: true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    public void Logout() { if (File.Exists(TokenPath)) File.Delete(TokenPath); }
    public void EnsureDirectory()
    {
        Directory.CreateDirectory(DirectoryPath);
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(DirectoryPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    }
    public FileStream LockPlaylist(string playlist)
    {
        EnsureDirectory();
        try { return new FileStream(Path.Combine(DirectoryPath, $"import-{playlist}.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException ex) { throw new SpotifyException("Another local import is already using this playlist. Try again after it finishes.", ex); }
    }
}

public class SpotifyAuth(HttpClient http, CredentialStore store)
{
    public const string RedirectUri = "http://127.0.0.1:43821/callback";
    private const string Scopes = "playlist-read-private playlist-read-collaborative playlist-modify-private playlist-modify-public";
    private Credentials? credentials;

    public async Task<string> AccessToken(bool forceRefresh, CancellationToken ct)
    {
        credentials ??= store.Load() ?? throw new SpotifyException("Sign in first: login --client-id YOUR_SPOTIFY_CLIENT_ID");
        if (forceRefresh || credentials.ExpiresAt <= DateTimeOffset.UtcNow.AddSeconds(60))
        {
            credentials = await Exchange(new() { ["grant_type"] = "refresh_token", ["refresh_token"] = credentials.RefreshToken,
                ["client_id"] = credentials.ClientId }, credentials.ClientId, credentials.RefreshToken, ct);
            store.Save(credentials);
        }
        return credentials.AccessToken;
    }

    public async Task Login(string clientId, bool openBrowser, CancellationToken ct)
    {
        if (clientId.Length != 32 || clientId.Any(c => !Uri.IsHexDigit(c))) throw new ArgumentException("Spotify client ID must contain 32 hexadecimal characters.");
        var verifier = Base64Url(RandomNumberGenerator.GetBytes(48));
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        var state = Base64Url(RandomNumberGenerator.GetBytes(32));
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = clientId, ["response_type"] = "code", ["redirect_uri"] = RedirectUri,
            ["scope"] = Scopes, ["state"] = state, ["code_challenge_method"] = "S256", ["code_challenge"] = challenge
        };
        var url = "https://accounts.spotify.com/authorize?" + string.Join('&', parameters.Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value)));
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromMinutes(5));
        using var listener = new TcpListener(IPAddress.Loopback, 43821);
        listener.Start();
        Console.WriteLine($"Open this URL to sign in (waiting up to five minutes):\n{url}");
        if (openBrowser)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
            { Console.WriteLine("Could not open a browser automatically. Open the URL above manually."); }
        }
        while (true)
        {
            using var connection = await listener.AcceptTcpClientAsync(timeout.Token);
            using var stream = connection.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, leaveOpen: true);
            using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
            requestTimeout.CancelAfter(TimeSpan.FromSeconds(5));
            string? line;
            try { line = await reader.ReadLineAsync(requestTimeout.Token); }
            catch (OperationCanceledException) when (!timeout.IsCancellationRequested) { continue; }
            if (line == null || line.Length > 8192) continue;
            var parts = line.Split(' ');
            if (parts.Length != 3 || parts[0] != "GET" || !Uri.TryCreate("http://127.0.0.1" + parts[1], UriKind.Absolute, out var callback)) continue;
            var query = ParseQuery(callback.Query);
            if (callback.AbsolutePath != "/callback" || query.GetValueOrDefault("state") != state)
            {
                await Reply(stream, "400 Bad Request", "Invalid login callback. Return to the Spotify authorization page.", timeout.Token);
                continue;
            }
            if (query.ContainsKey("error"))
            {
                await Reply(stream, "400 Bad Request", "Authorization was declined. Return to the terminal.", timeout.Token);
                throw new SpotifyException("Spotify authorization was declined.");
            }
            if (!query.TryGetValue("code", out var code) || code == "")
            {
                await Reply(stream, "400 Bad Request", "Missing authorization code.", timeout.Token);
                continue;
            }
            try
            {
                credentials = await Exchange(new() { ["grant_type"] = "authorization_code", ["code"] = code,
                    ["redirect_uri"] = RedirectUri, ["client_id"] = clientId, ["code_verifier"] = verifier }, clientId, "", timeout.Token);
                store.Save(credentials);
            }
            catch
            {
                await Reply(stream, "500 Internal Server Error", "Login failed. See the terminal for details.", timeout.Token);
                throw;
            }
            await Reply(stream, "200 OK", "Signed in. You can close this tab and return to the terminal.", timeout.Token);
            Console.WriteLine("Signed in. Credentials saved in your local application data directory.");
            return;
        }
    }

    private async Task<Credentials> Exchange(Dictionary<string, string> fields, string clientId, string previousRefresh, CancellationToken ct)
    {
        using var response = await http.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(fields), ct);
        if (!response.IsSuccessStatusCode) throw new SpotifyException($"Spotify token request failed ({(int)response.StatusCode}). Check your app settings or log in again.");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = json.RootElement;
        var refresh = root.Str("refresh_token");
        var access = root.Str("access_token");
        if (access == "" || (!root.TryGetProperty("expires_in", out var expires)) || !expires.TryGetInt32(out var seconds))
            throw new SpotifyException("Spotify returned an incomplete token response.");
        if (refresh == "") refresh = previousRefresh;
        if (refresh == "") throw new SpotifyException("Spotify returned no refresh token. Sign in again.");
        return new(clientId, access, refresh, DateTimeOffset.UtcNow.AddSeconds(seconds));
    }
    public static string Base64Url(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static Dictionary<string, string> ParseQuery(string query) => query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(p => p.Split('=', 2)).Where(p => p.Length == 2).GroupBy(p => Uri.UnescapeDataString(p[0]))
        .Where(g => g.Count() == 1).ToDictionary(g => g.Key, g => Uri.UnescapeDataString(g.First()[1].Replace('+', ' ')));
    private static async Task Reply(Stream stream, string status, string body, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        var header = Encoding.ASCII.GetBytes($"HTTP/1.1 {status}\r\nContent-Type: text/plain; charset=utf-8\r\nContent-Length: {bytes.Length}\r\nConnection: close\r\n\r\n");
        await stream.WriteAsync(header, ct);
        await stream.WriteAsync(bytes, ct);
    }
}
