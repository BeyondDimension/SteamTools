using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiSteamFarm.Steam.Cards;

public sealed class CardsFarmer
{
    [JsonProperty]
    public IReadOnlyCollection<Game>? CurrentGamesFarming { get; private set; }

    [JsonProperty]
    public IReadOnlyCollection<Game>? GamesToFarm { get; private set; }

    [JsonProperty]
    public bool Paused { get; private set; }

    [JsonProperty]
    public TimeSpan TimeRemaining { get; private set; }

    [JsonConstructor]
    public CardsFarmer()
    {
    }
}
