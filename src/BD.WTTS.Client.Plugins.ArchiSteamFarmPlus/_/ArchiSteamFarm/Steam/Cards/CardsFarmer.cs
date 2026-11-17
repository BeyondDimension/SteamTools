using STJ = System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiSteamFarm.Steam.Cards;

public sealed class CardsFarmer
{
    [STJ.JsonInclude]
    public IReadOnlyCollection<Game>? CurrentGamesFarming { get; private set; }

    [STJ.JsonInclude]
    public IReadOnlyCollection<Game>? GamesToFarm { get; private set; }

    [STJ.JsonInclude]
    public bool Paused { get; private set; }

    [STJ.JsonInclude]
    public TimeSpan TimeRemaining { get; private set; }

    [STJ.JsonConstructor]
    public CardsFarmer()
    {
    }
}
