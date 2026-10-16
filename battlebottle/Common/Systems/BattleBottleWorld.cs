
using Terraria;
using Terraria.ModLoader;

namespace BattleBottle.Common.Systems
{
    public class BattleBottleWorld : ModSystem
    {
        // The first three slots are always available.
        // Each remaining slot belongs to one specific boss.
        public static bool IsSlotUnlocked(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0:
                case 1:
                case 2:
                    return true;

                case 3:
                    return NPC.downedDeerclops;

                case 4:
                    return NPC.downedQueenBee;

                case 5:
                    return NPC.downedQueenSlime;

                case 6:
                    return NPC.downedFishron;

                case 7:
                    return NPC.downedEmpressOfLight;

                default:
                    return false;
            }
        }

        // Retained for any existing code that needs the total capacity.
        public static int UnlockedSlots
        {
            get
            {
                int count = 0;

                for (int i = 0; i < 8; i++)
                {
                    if (IsSlotUnlocked(i))
                        count++;
                }

                return count;
            }
        }
    }
}
