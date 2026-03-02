using System.Net.Http;
using System.Text;
using System.Text.Json;
using football_bingo;

var builder = WebApplication.CreateBuilder(args);

// Add a single HttpClient for the app
builder.Services.AddHttpClient();

var app = builder.Build();

// Serve a simple HTML form at the root
app.MapGet("/", () => Results.Content(@"
<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <title>Football Bingo Viewer</title>
</head>
<body>
  <h1>Football Bingo Viewer</h1>
  <form method=""get"" action=""/bingo"">
    <label for=""bingoId"">Bingo ID:</label>
    <input type=""text"" id=""bingoId"" name=""bingoId"" required>
    <button type=""submit"">Show result</button>
  </form>
</body>
</html>
", "text/html"));

app.MapGet("/bingo", async (string bingoId, IHttpClientFactory httpClientFactory) =>
{
    if (string.IsNullOrWhiteSpace(bingoId))
    {
        return Results.BadRequest("Missing bingoId.");
    }

    string path = $"https://playfootball.games/api/football-bingo/{bingoId}.json";
    var client = httpClientFactory.CreateClient();

    HttpResponseMessage response;
    try
    {
        response = await client.GetAsync(path);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error calling API: {ex.Message}");
    }

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem($"API returned status {response.StatusCode}");
    }

    var json = await response.Content.ReadAsStringAsync();

    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    Root root = JsonSerializer.Deserialize<Root>(json, options);
    GameData gameData = root.GameData;

    var remitGroups = gameData.Remit;
    var players = gameData.Players;

    var sb = new StringBuilder();

    sb.AppendLine("# Remit groups and matching players");
    sb.AppendLine();

    for (int groupIndex = 0; groupIndex < remitGroups.Count; groupIndex++)
    {
        var group = remitGroups[groupIndex];

        // IDs that must be matched by the player (AND logic)
        var groupRemitIds = group.Select(r => r.Id).ToHashSet();

        // Group title: e.g. "Africa", or "Croatia + Real Madrid"
        string groupTitle = string.Join(" + ", group.Select(r => r.DisplayName));

        // Markdown heading for this group
        sb.AppendLine($"## {groupTitle}");
        sb.AppendLine();

        var matchingPlayers = players
            .Where(p => p.RemitIds != null && groupRemitIds.All(id => p.RemitIds.Contains(id)))
            .ToList();

        if (matchingPlayers.Count == 0)
        {
            sb.AppendLine("- _No players match this remit group._");
            sb.AppendLine();
            continue;
        }

        foreach (var p in matchingPlayers)
        {
            string fullName = string.IsNullOrWhiteSpace(p.GivenName)
                ? p.FamilyName
                : $"{p.GivenName} {p.FamilyName}";

            string positionPart = string.IsNullOrWhiteSpace(p.Position)
                ? ""
                : $" ({p.Position})";

            // Markdown bullet
            sb.AppendLine($"- **{fullName}**{positionPart}");
        }

        sb.AppendLine(); // blank line between groups
    }

    // Wrap the Markdown in simple HTML with <pre> so formatting is preserved
    var html = $@"
<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <title>Football Bingo Result {bingoId}</title>
</head>
<body>
  <a href=""/"">&larr; Back</a>
  <h1>Football Bingo Result for {bingoId}</h1>
  <pre>{System.Net.WebUtility.HtmlEncode(sb.ToString())}</pre>
</body>
</html>
";

    return Results.Content(html, "text/html");
});

app.Run();