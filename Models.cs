using System.Text.Json.Serialization;
namespace football_bingo
{
    public class Root
    {
        [JsonPropertyName("gameData")]
        public GameData GameData { get; set; }
    }

    public class GameData
    {
        // remit is an array of arrays of remit objects
        [JsonPropertyName("remit")]
        public List<List<RemitItem>> Remit { get; set; }

        [JsonPropertyName("players")]
        public List<Player> Players { get; set; }
    }

    public class RemitItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        // optional
        [JsonPropertyName("helperText")]
        public string HelperText { get; set; }

        // optional
        [JsonPropertyName("prefix")]
        public string Prefix { get; set; }
    }

    public class Player
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        // "f" – seems to be surname / main name
        [JsonPropertyName("f")]
        public string FamilyName { get; set; }

        // "g" – given name / first name (can be empty)
        [JsonPropertyName("g")]
        public string GivenName { get; set; }

        // "v" – array of remit IDs
        [JsonPropertyName("v")]
        public List<int> RemitIds { get; set; }

        // "p" – optional position (RW, CB, etc.)
        [JsonPropertyName("p")]
        public string Position { get; set; }
    }
}
