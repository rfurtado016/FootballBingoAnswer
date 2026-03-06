using football_bingo;
using System.Net;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add a single HttpClient for the app
builder.Services.AddHttpClient();

var app = builder.Build();

const string SharedCss = """
    *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

    :root {
      --bg: #0d1117;
      --surface: #161b22;
      --border: #30363d;
      --green: #238636;
      --green-hover: #2ea043;
      --text: #e6edf3;
      --text-muted: #8b949e;
      --radius: 8px;
    }

    body {
      background: var(--bg);
      color: var(--text);
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
      min-height: 100vh;
      padding: 2rem 1rem;
    }

    /* Home page hero */
    .hero {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem 1rem;
    }

    .hero-card {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      padding: 2.5rem 2rem;
      width: 100%;
      max-width: 420px;
      text-align: center;
    }

    .hero-icon { font-size: 3rem; margin-bottom: 1rem; }
    .hero-title { font-size: 1.75rem; font-weight: 700; margin-bottom: 0.5rem; }
    .hero-subtitle { color: var(--text-muted); margin-bottom: 2rem; }

    .form-group { display: flex; flex-direction: column; gap: 0.75rem; }

    input[type="text"] {
      background: var(--bg);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      color: var(--text);
      font-size: 1rem;
      padding: 0.625rem 0.875rem;
      width: 100%;
      outline: none;
      transition: border-color 0.15s;
    }

    input[type="text"]:focus { border-color: var(--green); }

    button[type="submit"] {
      background: var(--green);
      border: none;
      border-radius: var(--radius);
      color: #fff;
      cursor: pointer;
      font-size: 1rem;
      font-weight: 600;
      padding: 0.625rem 1.25rem;
      transition: background 0.15s;
    }

    button[type="submit"]:hover { background: var(--green-hover); }

    /* Dual-button row on home page */
    .form-actions { display: flex; gap: 0.75rem; }
    .form-actions button { flex: 1; }

    .btn-secondary {
      flex: 1;
      background: transparent;
      border: 1px solid var(--border);
      border-radius: var(--radius);
      color: var(--text);
      cursor: pointer;
      font-size: 1rem;
      font-weight: 600;
      padding: 0.625rem 1.25rem;
      transition: border-color 0.15s, color 0.15s;
    }

    .btn-secondary:hover { border-color: var(--green); color: var(--green); }

    /* Results page */
    .results-header {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 2rem;
      flex-wrap: wrap;
    }

    .back-link {
      color: var(--green);
      text-decoration: none;
      font-size: 0.9rem;
    }

    .back-link:hover { text-decoration: underline; }

    .results-title { font-size: 1.5rem; font-weight: 700; }

    .card-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1rem;
    }

    .card {
      background: var(--surface);
      border: 1px solid var(--border);
      border-left: 4px solid var(--green);
      border-radius: var(--radius);
      padding: 1rem 1.25rem;
      transition: box-shadow 0.15s;
    }

    .card:hover { box-shadow: 0 0 0 1px var(--green); }

    .card-empty {
      border-left-color: var(--border);
      opacity: 0.6;
    }

    .card-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 0.5rem;
      margin-bottom: 0.75rem;
    }

    .card-title {
      font-weight: 600;
      font-size: 0.95rem;
      line-height: 1.4;
    }

    .player-badge {
      background: var(--green);
      border-radius: 999px;
      color: #fff;
      font-size: 0.75rem;
      font-weight: 700;
      min-width: 1.5rem;
      padding: 0.1rem 0.45rem;
      text-align: center;
      white-space: nowrap;
      flex-shrink: 0;
    }

    .badge-empty { background: var(--border); color: var(--text-muted); }

    .player-list {
      list-style: none;
      display: flex;
      flex-direction: column;
      gap: 0.3rem;
    }

    .player-item { font-size: 0.875rem; }
    .player-name { font-weight: 500; }
    .player-pos { color: var(--text-muted); font-size: 0.8rem; margin-left: 0.3rem; }

    .no-players { color: var(--text-muted); font-size: 0.875rem; font-style: italic; }

    /* Remit tags used in the players view */
    .remit-tags {
      display: flex;
      flex-wrap: wrap;
      gap: 0.35rem;
    }

    .remit-tag {
      background: #1f2d1f;
      border: 1px solid var(--green);
      border-radius: 4px;
      color: var(--text);
      font-size: 0.75rem;
      padding: 0.15rem 0.5rem;
    }
    """;

// Serve a simple HTML form at the root
app.MapGet("/", () =>
{
    var body = """
        <div class="hero">
          <div class="hero-card">
            <div class="hero-icon">&#x26BD;</div>
            <h1 class="hero-title">Football Bingo</h1>
            <p class="hero-subtitle">Enter a Bingo ID to explore the game.</p>
            <div class="form-group">
              <input type="text" id="bingoId" name="bingoId" placeholder="Bingo ID" required
                     oninput="sync(this.value)">
              <div class="form-actions">
                <form id="fBingo" method="get" action="/bingo" style="flex:1;display:contents">
                  <input type="hidden" name="bingoId" id="hBingo">
                  <button type="submit">Bingo Groups</button>
                </form>
                <form id="fPlayers" method="get" action="/players" style="flex:1;display:contents">
                  <input type="hidden" name="bingoId" id="hPlayers">
                  <button type="submit" class="btn-secondary">Players</button>
                </form>
              </div>
            </div>
            <script>
              function sync(v) {
                document.getElementById('hBingo').value = v;
                document.getElementById('hPlayers').value = v;
              }
              document.querySelectorAll('form').forEach(f => f.addEventListener('submit', function(e) {
                var v = document.getElementById('bingoId').value.trim();
                if (!v) { e.preventDefault(); document.getElementById('bingoId').focus(); }
              }));
            </script>
          </div>
        </div>
        """;
    return Results.Content(PageShell("Football Bingo Viewer", body), "text/html");
});

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

    var cards = new StringBuilder();

    for (int groupIndex = 0; groupIndex < remitGroups.Count; groupIndex++)
    {
        var group = remitGroups[groupIndex];

        var groupRemitIds = group.Select(r => r.Id).ToHashSet();

        string groupTitle = string.Join(" + ", group.Select(r => WebUtility.HtmlEncode(r.DisplayName)));

        var matchingPlayers = players
            .Where(p => p.RemitIds != null && groupRemitIds.All(id => p.RemitIds.Contains(id)))
            .ToList();

        string emptyClass = matchingPlayers.Count == 0 ? " card-empty" : "";
        string badgeClass = matchingPlayers.Count == 0 ? " badge-empty" : "";

        cards.Append($"""
            <div class="card{emptyClass}">
              <div class="card-header">
                <span class="card-title">{groupTitle}</span>
                <span class="player-badge{badgeClass}">{matchingPlayers.Count}</span>
              </div>
            """);

        if (matchingPlayers.Count == 0)
        {
            cards.AppendLine("""  <p class="no-players">No players match this group.</p>""");
        }
        else
        {
            cards.AppendLine("""  <ul class="player-list">""");
            foreach (var p in matchingPlayers)
            {
                string fullName = string.IsNullOrWhiteSpace(p.GivenName)
                    ? p.FamilyName
                    : $"{p.GivenName} {p.FamilyName}";

                string posSpan = string.IsNullOrWhiteSpace(p.Position)
                    ? ""
                    : $"""<span class="player-pos">{WebUtility.HtmlEncode(p.Position)}</span>""";

                cards.AppendLine($"""    <li class="player-item"><span class="player-name">{WebUtility.HtmlEncode(fullName)}</span>{posSpan}</li>""");
            }
            cards.AppendLine("  </ul>");
        }

        cards.AppendLine("</div>");
    }

    string encodedId = WebUtility.HtmlEncode(bingoId);
    var resultsBody = $"""
        <div class="results-header">
          <a class="back-link" href="/">&larr; Back</a>
          <h1 class="results-title">Bingo Result &mdash; {encodedId}</h1>
        </div>
        <div class="card-grid">
        {cards}
        </div>
        """;

    return Results.Content(PageShell($"Football Bingo Result {encodedId}", resultsBody), "text/html");
});

app.MapGet("/players", async (string bingoId, IHttpClientFactory httpClientFactory) =>
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

    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    Root root = JsonSerializer.Deserialize<Root>(json, options);
    GameData gameData = root.GameData;

    // Build a flat id → DisplayName lookup from all remit groups
    var remitById = gameData.Remit
        .SelectMany(group => group)
        .GroupBy(r => r.Id)
        .ToDictionary(g => g.Key, g => g.First().DisplayName);

    var cards = new StringBuilder();

    foreach (var p in gameData.Players)
    {
        string fullName = string.IsNullOrWhiteSpace(p.GivenName)
            ? p.FamilyName
            : $"{p.GivenName} {p.FamilyName}";

        string posSpan = string.IsNullOrWhiteSpace(p.Position)
            ? ""
            : $"""<span class="player-pos">{WebUtility.HtmlEncode(p.Position)}</span>""";

        bool hasRemits = p.RemitIds != null && p.RemitIds.Count > 0;
        string emptyClass = hasRemits ? "" : " card-empty";
        string badgeClass = hasRemits ? "" : " badge-empty";
        int remitCount = hasRemits ? p.RemitIds.Count : 0;

        cards.Append($"""
            <div class="card{emptyClass}">
              <div class="card-header">
                <span class="card-title">{WebUtility.HtmlEncode(fullName)}{posSpan}</span>
                <span class="player-badge{badgeClass}">{remitCount}</span>
              </div>
            """);

        if (!hasRemits)
        {
            cards.AppendLine("""  <p class="no-players">No remits.</p>""");
        }
        else
        {
            cards.AppendLine("""  <div class="remit-tags">""");
            foreach (var id in p.RemitIds)
            {
                string label = remitById.TryGetValue(id, out var name)
                    ? WebUtility.HtmlEncode(name)
                    : id.ToString();
                cards.AppendLine($"""    <span class="remit-tag">{label}</span>""");
            }
            cards.AppendLine("  </div>");
        }

        cards.AppendLine("</div>");
    }

    string encodedId = WebUtility.HtmlEncode(bingoId);
    var resultsBody = $"""
        <div class="results-header">
          <a class="back-link" href="/">&larr; Back</a>
          <h1 class="results-title">Players &mdash; {encodedId}</h1>
        </div>
        <div class="card-grid">
        {cards}
        </div>
        """;

    return Results.Content(PageShell($"Football Bingo Players {encodedId}", resultsBody), "text/html");
});

app.Run();

string PageShell(string title, string body) => $$"""
    <!doctype html><html lang="en">
    <head>
      <meta charset="utf-8">
      <meta name="viewport" content="width=device-width, initial-scale=1">
      <title>{{title}}</title>
      <style>{{SharedCss}}</style>
    </head>
    <body>{{body}}</body>
    </html>
    """;
