using System.Collections.Generic;
using System.Linq;
using PDBot.Core.API;
using PDBot.Data;

namespace PDBot.Core.GameObservers;

public static class FlavorNameChecker
{
    static readonly Dictionary<string, string> flavourNameToName = new();

    public static bool IsFlavorName(string name, out string realName)
    {
        if (CardName.RealCards.Contains(name))
        {
            realName = name;
            return false;
        }
            
        if (flavourNameToName.TryGetValue(name, out realName))
        {
            return true;
        }
            
        var card = Scryfall.GetCardFromSearch(name);

        if (card == null)
        {
            flavourNameToName[name] = null;
            return false;
        }
            
        realName = card.FullName;
        flavourNameToName[name] = card.FullName;
        return true;
    }
}