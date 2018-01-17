using System.Threading.Tasks;
using Gatherling.Models;

namespace Gatherling
{
    public interface IGatherlingApi
    {
        int ApiVersion { get; }
        ServerSettings Settings { get; }

        Task AuthenticateAsync();
        Task<Event[]> GetActiveEventsAsync();
        Task<Round> GetCurrentPairings(string eventName);
        Task<Deck> GetDeckAsync(int deckID);
        Task<string> GetVerificationCodeAsync(string playerName);
        Event LoadEvent(string name);
    }
}