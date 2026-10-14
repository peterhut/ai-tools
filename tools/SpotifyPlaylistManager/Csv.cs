using System.Globalization;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace SpotifyPlaylistManager;

public record CsvRow(int Request, Dictionary<string, string> Fields, string Error)
{
    public string Get(string key) => Fields.GetValueOrDefault(key, "");
}

public static class Csv
{
    public static List<CsvRow> Read(string path, params string[] required)
    {
        using var parser = new TextFieldParser(path, Encoding.UTF8, detectEncoding: true)
        {
            TextFieldType = FieldType.Delimited, HasFieldsEnclosedInQuotes = true, TrimWhiteSpace = false
        };
        parser.SetDelimiters(",");
        var headers = parser.ReadFields()?.Select(h => h.Trim()).ToArray() ?? throw new ArgumentException("CSV is empty.");
        if (headers.Any(h => h == "") || headers.Distinct(StringComparer.OrdinalIgnoreCase).Count() != headers.Length)
            throw new ArgumentException("CSV headers must be nonempty and unique.");
        foreach (var name in required)
            if (!headers.Contains(name, StringComparer.OrdinalIgnoreCase)) throw new ArgumentException($"CSV requires a '{name}' header.");
        var rows = new List<CsvRow>();
        while (!parser.EndOfData)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var error = "";
            try
            {
                var values = parser.ReadFields() ?? [];
                if (values.Length != headers.Length) error = $"Expected {headers.Length} fields, found {values.Length}.";
                for (var i = 0; i < Math.Min(values.Length, headers.Length); i++) fields[headers[i]] = values[i];
            }
            catch (MalformedLineException) { error = $"Malformed CSV near line {parser.ErrorLineNumber}."; }
            rows.Add(new(rows.Count + 1, fields, error));
        }
        return rows;
    }

    public static void Write(string path, string[] headers, IEnumerable<IEnumerable<string>> rows)
    {
        // Replace only after the complete output has been written successfully.
        var full = Path.GetFullPath(path);
        var temp = full + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var writer = new StreamWriter(temp, false, new UTF8Encoding(true)))
            {
                writer.WriteLine(string.Join(',', headers.Select(Escape)));
                foreach (var row in rows) writer.WriteLine(string.Join(',', row.Select(Escape)));
            }
            File.Move(temp, full, overwrite: true);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    private static string Escape(string value) => value.IndexOfAny([',', '"', '\r', '\n']) >= 0
        ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    public static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
    public static readonly string[] ExportHeaders = ["position", "title", "artists", "album", "duration_ms", "spotify_url", "track_id", "original_track_id", "entry_type", "status"];
    public static string[] ExportRow(Entry e) => [Number(e.Position), e.Title, e.ArtistNames, e.Album,
        e.DurationMs?.ToString(CultureInfo.InvariantCulture) ?? "", e.Url, e.Id, e.OriginalId, e.Type, e.Status];
}
