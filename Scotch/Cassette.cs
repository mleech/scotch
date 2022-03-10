using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Scotch;

public static class Cassette
{
    public static object locker = new();

    public static IEnumerable<HttpInteraction> ReadCassette(string cassettePath)
    {
        if (!File.Exists(cassettePath)) return new List<HttpInteraction>();

        var jsonString = File.ReadAllText(cassettePath);
        var cassetteParseResult = JsonConvert.DeserializeObject<List<HttpInteraction>>(jsonString, new VersionConverter());
        return cassetteParseResult;
    }

    public static void UpdateInteraction(string cassettePath, HttpInteraction httpInteraction)
    {
        lock (locker)
        {
            var existingInteractions = ReadCassette(cassettePath).ToList();
            var matchingIndex = existingInteractions.FindIndex(i => Helpers.RequestsMatch(httpInteraction.Request, i.Request));
            List<HttpInteraction> newInteractions;
            if (matchingIndex < 0)
            {
                newInteractions = existingInteractions.Append(httpInteraction).ToList();
            }
            else
            {
                newInteractions = existingInteractions.ToList();
                newInteractions[matchingIndex] = httpInteraction;
            }

            WriteCassette(cassettePath, newInteractions);
        }
    }

    public static void WriteCassette(string cassettePath, IEnumerable<HttpInteraction> httpInteraction)
    {
        var serializedInteraction = JsonConvert.SerializeObject(httpInteraction.ToList(), Formatting.Indented, new VersionConverter());
        File.WriteAllText(cassettePath, serializedInteraction.ToString());
    }
}
