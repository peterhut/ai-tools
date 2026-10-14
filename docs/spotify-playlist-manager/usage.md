# Spotify playlist manager

A personal C# CLI for Linux and Windows with export, reviewed CSV additions, and duplicate reports. It requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0); there are no third-party runtime packages.

## Spotify setup

1. Open the [Spotify developer dashboard](https://developer.spotify.com/dashboard) and create an app with Web API access. Development Mode requires the app owner to have Spotify Premium; add your Spotify account under the app's user-access settings if needed. See [quota modes](https://developer.spotify.com/documentation/web-api/concepts/quota-modes).
2. In the app settings, register this exact redirect URI: `http://127.0.0.1:43821/callback`. Spotify permits HTTP for explicit loopback addresses; `localhost` is not accepted. See [redirect URI requirements](https://developer.spotify.com/documentation/web-api/concepts/redirect_uri).
3. Copy the app's **client ID**. This tool uses PKCE and does not need a client secret.
4. Run from the repository root, replacing the placeholder:

```sh
dotnet run --project tools/SpotifyPlaylistManager -- login --client-id YOUR_CLIENT_ID
```

Authorize in the browser on the same machine as the CLI. The tool listens only on `127.0.0.1:43821` and waits up to five minutes. If the browser does not open automatically, open the printed URL manually. Use `--no-browser` to suppress automatic opening. On a remote machine, forward that loopback port to your local browser or run login locally; copying the URL alone does not forward the callback.

Authorization requests playlist read and modification scopes, not playback access. Tokens are saved in `SpotifyPlaylistManager/credentials.json` under your user's local application-data directory (normally `~/.local/share` on Linux and `%LOCALAPPDATA%` on Windows). This file contains unencrypted access and refresh tokens; Linux directory/file permissions are restricted to the user, and Windows uses inherited permissions from the user profile. Keep it private and outside source control. Tokens refresh automatically; `logout` deletes local credentials. To revoke access at Spotify, remove the app from your Spotify account's connected apps.

## Find a playlist

```sh
dotnet run --project tools/SpotifyPlaylistManager -- playlists
dotnet run --project tools/SpotifyPlaylistManager -- playlists --filter road
```

The list includes owned and collaborative playlists with IDs and URLs. Individual commands accept the ID, `spotify:playlist:ID`, or a Spotify playlist URL, including URLs with sharing query parameters. Current Spotify access rules restrict playlist contents to owned or collaborative playlists; adding also requires write access. See [playlist access](https://developer.spotify.com/documentation/web-api/reference/get-playlists-items).

## Resolve a CSV and add your selections

Create a UTF-8, comma-separated CSV with a header:

```csv
title,artist
Home,Artist A
"A Title, With a Comma",Artist B
```

Additional columns are accepted. Quote fields containing commas, quotes, or newlines using standard CSV escaping. Header names are case-insensitive. Blank physical lines are ignored. Invalid data rows are represented as errors in the review; missing or repeated headers reject the input.

```sh
dotnet run --project tools/SpotifyPlaylistManager -- resolve --input tracks.csv --output review.csv
```

Each input request receives up to five distinct, available Spotify candidates in API result order. No candidate is selected automatically. The review includes `request_id`, input title/artist, exact title/artists, album, duration in milliseconds, track ID, Spotify URL, `selected`, status, and error. Requests without candidates remain as `unmatched` or `error` rows. Spotify search results are suggestions, so inspect version titles and albums before choosing.

Open the review in a CSV editor and set `selected` to `true` for at most one candidate per `request_id`. Keep IDs as text, preserve request IDs, and save as comma-separated UTF-8 CSV. `yes` and `1` also select; blank, `false`, `no`, and `0` leave a row unselected. Unknown selection values, malformed rows, invalid request IDs, multiple choices per request, and syntactically invalid selected track IDs reject the whole review before additions. The selected Spotify ID determines the track; edited title/album text does not change the selection. A syntactically valid ID absent from Spotify can still fail at the API.

```sh
dotnet run --project tools/SpotifyPlaylistManager -- add PLAYLIST_ID --review review.csv
```

This command writes immediately from your explicit selections. It appends in original request order, even if you reordered the review CSV. Unselected requests are skipped. Existing exact IDs and repeated selected IDs are skipped; different catalog IDs, including remasters and remixes, remain eligible. Spotify relinking exposes an original ID and playable replacement ID; both count as already present when returned for an existing entry.

Additions use batches of up to 100 tracks. A failed or uncertain batch stops the import, re-reads the playlist when possible, and reports confirmed additions and skips. Rerun the same command to add missing IDs. The tool never blindly replays a timed-out append. Local concurrent imports into the same playlist are blocked. Spotify does not provide an atomic transaction across the read and append operations: avoid modifying that playlist from another client during an import. If a batch partially applies, rerunning preserves the already-appended entries rather than rearranging them; the tool does not reorder or delete entries.

## Export and inspect variations

```sh
dotnet run --project tools/SpotifyPlaylistManager -- export PLAYLIST_ID --output playlist.csv
dotnet run --project tools/SpotifyPlaylistManager -- duplicates PLAYLIST_ID --output duplicates.csv
```

Export preserves one-based positions, repeated entries, full version titles, all artists (separated by `; `), album, duration, Spotify URL, track ID, relinked original track ID, entry type, and status. Local files, unavailable entries, and episodes retain their positions and whatever metadata Spotify supplies. If Spotify returns a null item, metadata is blank and status is unavailable. The export contains readable metadata, not an automatically selected review file; use title/artist input and resolve to choose catalog entries for additions.

Duplicate reporting prints a terminal summary and writes every member of each group to CSV, including group ID, base title, first artist, and the export columns. A group must have at least two entries. Only available catalog music tracks with artist metadata participate.

Grouping normalizes case and whitespace and strips delimited suffixes containing recognized version words: live, remaster, remix, mix, edit, version, acoustic, instrumental, demo, radio, extended, mono, or stereo. Suffix delimiters are parentheses, brackets, or a dash surrounded by spaces. For example, `Home`, `Home (Live)`, and `Home - Artist B Remix` group together when their first listed artist matches; `Home Again` stays separate. First-artist Spotify IDs are compared when available, with normalized names as a fallback. Additional artists do not prevent grouping. This is a heuristic for inspection, not proof that tracks are the same recording; unexpected naming or reordered artist credits can produce missing or surprising groups. All original metadata remains visible.

Output CSV files replace existing files only after a complete new file has been written. Parent directories must already exist. Playlist export reads paginated contents; avoid editing the playlist during export to keep positions consistent.

## Errors and verification

Commands return `0` on success, `1` on invalid input/API/file errors, and `130` on cancellation or timeouts outside import recovery. Authentication and API errors omit tokens. A 403 usually requires checking app allowlisting, playlist access, or granted scopes. Rate limits are retried a bounded number of times for short waits; longer waits and quota exhaustion are reported. Server/network write failures stop the import.

```sh
dotnet build tools/SpotifyPlaylistManager --configuration Release
dotnet run --project tests/SpotifyPlaylistManager.Tests
dotnet run --project tools/SpotifyPlaylistManager -- help
```

The dependency-free tests exercise CSV parsing and review validation, grouping, pagination, API payloads, token refresh, a real loopback PKCE callback with a fake token endpoint, rate limits, local import locking, and recovery from partial writes or lost responses. They do not contact Spotify or modify a real playlist. Live account access needs verification with your app and a playlist you own; Windows execution needs verification on Windows.
