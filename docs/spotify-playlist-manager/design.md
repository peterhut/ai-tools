# Spotify playlist manager

The user confirmed the shared understanding and authorized implementation. This document records the agreed scope; see [usage.md](usage.md) for setup, commands, and verification.

## Confirmed scope

- Personal C# CLI tool for Linux and Windows, targeting .NET 10. Initially require the SDK and run through `dotnet run`; standalone binaries can follow later. The user has Spotify Premium.
- Add tracks identified by CSV title and artist to an existing playlist.
- Export playlist contents to CSV.
- Report duplicate groups by base title and first listed artist, showing variations rather than only repeated Spotify track IDs. Normalize capitalization and extra whitespace and remove recognized version suffixes, including live, remaster, and remix labels. Do not use unrestricted prefix matching: `Home` and `Home Again` stay separate, while `Home`, `Home (Live)`, and `Home - Artist B Remix` group together when the first listed artist matches. Additional artist credits do not prevent grouping.
- Do not create playlists or delete entries.
- Resolve input to a review CSV with up to five candidate rows per input request, initially unselected. Include original title/artist, exact Spotify title/artists, album, duration, track ID, and a selected column. The user picks one candidate per request; unmatched requests remain visible with a status.
- Skip exact Spotify track IDs already present in the target playlist or selected repeatedly within an import. Allow different variations, including remasters.
- Accept playlist URLs or IDs and provide a command to find accessible playlists. Adding requires an existing writable playlist.
- Exports include version information and preserve playlist ordering, including repeated entries, local files, unavailable tracks, and episodes. Include entry type and status; matching and duplicate grouping operate on searchable music tracks only.

## CSV workflow

1. Read a UTF-8, comma-separated input CSV with required `title` and `artist` headers. Allow additional columns. Preserve input order and show malformed rows as errors in the review CSV alongside unmatched requests.
2. Search Spotify and generate the review CSV described above. Each candidate identifies its original input request; requests without candidates still have a row and status. Nothing is selected automatically.
3. The user edits `selected` to pick at most one candidate per request.
4. Validate the complete review file before any playlist additions. Multiple selections for a request or invalid selected track IDs reject the import. Unselected and unmatched requests are skipped. The selected Spotify ID determines the added track; title and other metadata are descriptive.
5. Append selected tracks in original input order, skipping exact IDs already present or selected repeatedly in the import. Different versions are allowed even when they belong to the same duplicate group.
6. Report additions, skips, and failures. After a partial failure, rerunning adds only missing IDs. Re-read playlist contents before retrying an uncertain write rather than blindly repeating it.

## Export and duplicate reports

- Export one playlist with position, full title, all artists, album, duration, Spotify URL, track ID, entry type, and status. Preserve the returned version labels and playlist ordering; no separate catalog version field is assumed.
- Preserve repeated entries and non-searchable entries in exports, retaining whatever metadata Spotify returns and their original positions.
- Inspect duplicate groups within one existing playlist per command. Produce a terminal summary and CSV with every entry in each group, including position, full title, all artists, album, and ID. Groups contain at least two entries and can include repeated identical IDs as well as distinct variations.
- Local files, unavailable tracks, and episodes do not participate in catalog matching or duplicate grouping.

## Authentication and access

- The user registers a personal Spotify developer app and authenticates through browser-based Authorization Code with PKCE.
- Persist a refresh token in the user's local application configuration outside the repository. Provide logout to clear saved credentials.
- List accessible owned and collaborative playlists; accept a playlist URL or ID for individual operations. Adding requires write access.
- Live Spotify access has not been tested during this design interview.

## Implementation outline

The command surface covers login/logout, playlist listing, export, CSV resolution, reviewed additions, and duplicate reporting. Command spelling and code organization are implementation details; the operations and their contracts above are agreed.

Implement the tool independently of the existing visualisation plugin. This design changes no plugin behavior or manifests.

## API constraints to account for

- Development Mode requires a Premium app owner and allowlisted users: [quota modes](https://developer.spotify.com/documentation/web-api/concepts/quota-modes).
- Playlist item retrieval is restricted to owned or collaborative playlists: [Get Playlist Items](https://developer.spotify.com/documentation/web-api/reference/get-playlists-items).
- Catalog search supports track and artist filters but still requires a selection policy: [Search](https://developer.spotify.com/documentation/web-api/reference/search).
- Local user-authorized access can use Authorization Code with PKCE: [authorization](https://developer.spotify.com/documentation/web-api/concepts/authorization).
- Spotify describes artists as performers and does not guarantee a primary artist role for the first element. First listed artist is this tool's grouping convention: [Get Track](https://developer.spotify.com/documentation/web-api/reference/get-track).
- Playlist additions preserve request order and support at most 100 items per request. Multi-request recovery must account for previously committed batches and uncertain writes: [Add Items to Playlist](https://developer.spotify.com/documentation/web-api/reference/add-items-to-playlist).
