using System;
namespace Gatherling.Models
{

    public enum EventStructure
    {
        Unknown,
        Swiss,
        SingleElimination,
        League,
        RoundRobin,
    }
    public struct SubEvent
    {
        public int Rounds { get; internal set; }
        public EventStructure Mode { get; internal set; }

        public string ModeRaw { get; internal set; }

        public SubEvent(string mode, int rounds)
        {
            ModeRaw = mode;
            Rounds = rounds;
            var found = Enum.TryParse<EventStructure>(mode, out var parsed);
            if (!found)
            {
                if (mode == "Swiss (Blossom)")
                {
                    parsed = EventStructure.Swiss;
                    ModeRaw = "Swiss";
                }
            }
            Mode = parsed;
        }
    }
}
