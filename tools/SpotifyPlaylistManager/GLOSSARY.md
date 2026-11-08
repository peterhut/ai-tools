# Spotify Playlist Management

Personal playlist management: exporting playlist contents, adding tracks from a CSV, and inspecting track variations.

## Language

**Playlist**:
An ordered collection of Spotify entries that can contain repeated tracks.

**CSV track request**:
A row identifying a desired track by title and artist for addition to an existing playlist.
_Avoid_: Import track, matched track

**Duplicate group**:
Two or more playlist entries sharing a base title and first listed artist, so their variations can be inspected together. Membership does not imply that an entry should be removed.
_Avoid_: Redundant tracks

**Base title**:
A song title without recognized version labels such as live, remaster, or remix suffixes. Distinct titles such as `Home` and `Home Again` remain distinct.
_Avoid_: Title prefix

**First listed artist**:
The artist listed first among a track's Spotify artist credits. This is a grouping convention rather than a claim about the artist's primary role.
_Avoid_: Primary artist

**Track variation**:
A different release or version of a song, potentially with additional artist credits or a version suffix in its title.

**Candidate**:
A Spotify catalog track returned as a possible match for a CSV track request.

**Review CSV**:
A file presenting candidates with their exact Spotify titles and track identifiers for the user to choose one match per CSV track request.

**Selected track**:
The single candidate the user chooses for a CSV track request, identified by its Spotify track ID.
