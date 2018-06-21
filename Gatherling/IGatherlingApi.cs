using System;
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
        [Obsolete]
        Task<Round> GetCurrentPairings(string eventName);
        Task<Round> GetCurrentPairings(Event tournament);
        Task<Deck> GetDeckAsync(int deckID);
        Task<string> GetVerificationCodeAsync(string playerName);
        Task<string> ResetPasswordAsync(string playerName);
    }
}
