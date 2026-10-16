using Terraria;

namespace BattleBottle.Common.Players
{
    public static class BattleBottlePotionRules
    {
        // Determines whether a stored item provides a valid positive buff.
        // Do not rely on Item.potion: it does not reliably identify every
        // potion that should be accepted by the Battle Bottle.
        public static bool IsEligibleBuffPotion(Item item)
        {
            if (item == null || item.IsAir)
                return false;

            // The item must define a buff and a positive duration.
            if (item.buffType <= 0 || item.buffTime <= 0)
                return false;

            
            return true;
        }
    }
}