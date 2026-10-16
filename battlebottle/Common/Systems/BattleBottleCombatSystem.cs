
using Terraria;
using Terraria.ModLoader;
using BattleBottle.Common.Players;

namespace BattleBottle.Common.Systems
{
    // Detects encounter starts and ends.
    public class BattleBottleCombatSystem : ModSystem
    {
        // Remembers whether a boss or supported event was active
        // during the previous world update.
        private static bool wasEncounterActive;

        public override void OnWorldLoad()
        {
            // Initialize from the current state so loading into an
            // already-active encounter doesn't look like a new start.
            wasEncounterActive = IsEncounterActive();
        }

        public override void OnWorldUnload()
        {
            // Remove Battle Bottle-owned buffs before the old world state is discarded.
            foreach (Player player in Main.player)
            {
                if (player.active)
                {
                    player.GetModPlayer<BattleBottlePlayer>().ClearBottleBuffs();
                }
            }
            wasEncounterActive = false;
        }

        public override void PostUpdateWorld()
        {
            bool encounterActive = IsEncounterActive();

            // Detect the moment an encounter ends.
            if (wasEncounterActive && !encounterActive)
            {
                ClearAllBottleBuffs();
            }

            // Remember the current state for the next update.
            wasEncounterActive = encounterActive;
        }

        // Returns true while a boss or supported event is active. 
        // Works for select vanilla events and any modded event considered an invasion
        public static bool IsEncounterActive()
        {
            // Boss fights remain independent of event detection.
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.boss)
                    return true;
            }

            // Explicitly supported vanilla events with progress bars.
            bool vanillaEventActive =
                Main.invasionType != 0 || // Goblin Army, Frost Legion,
                                        // Pirate Invasion, Martian Madness,
                                        // and compatible modded invasions.
                Main.slimeRain ||         // Slime Rain
                Terraria.GameContent.Events.DD2Event.Ongoing || // Old One's Army
                Main.pumpkinMoon ||       // Pumpkin Moon
                Main.snowMoon;            // Frost Moon

            return vanillaEventActive;
        }

        // Remove only the buffs tracked by each player's bottle component.
        private static void ClearAllBottleBuffs()
        {
            foreach (Player player in Main.player)
            {
                if (!player.active)
                    continue;

                player.GetModPlayer<BattleBottlePlayer>()
                    .ClearBottleBuffs();
            }
        }
    }
}
