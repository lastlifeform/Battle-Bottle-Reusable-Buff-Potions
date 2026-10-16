
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using BattleBottle.Content.Items;

using BattleBottle.Common.Systems;

namespace BattleBottle.Common.Players
{
    public class BattleBottlePlayer : ModPlayer
    {
        public const int SlotCount = 8;

        // Persistent potion storage. These items are never consumed
        // when their buffs are applied.
        public Item[] StoredPotions = new Item[SlotCount];

        // Buffs that Battle Bottle added when they were not already active.
        // These are runtime-only and are not saved.
        private readonly HashSet<int> bottleBuffTypes = new HashSet<int>();

        // Refresh buffs twice per second instead of every game tick.
        private const int RefreshInterval = 30;
        private int refreshTimer;

        // Tracks whether this player was in an encounter last update.
        private bool wasInEncounter;
        

        /// Returns true only if the Battle Bottle is in the player's
        /// regular inventory or hotbar, not in a chest or bank.
        /// Inventory slots 0-49 are the hotbar and main inventory.
        private bool HasBottleInInventory()
        {
            int bottleType = ModContent.ItemType<
                BattleBottle.Content.Items.BattleBottleItem>();

            for (int i = 0; i < 50; i++)
            {
                Item item = Player.inventory[i];

                if (item != null && !item.IsAir && item.type == bottleType)
                    return true;
            }

            return false;
        }


        /// ------animation stuff
        /// Remaining ticks in the Battle Bottle drinking animation.
        /// A value of zero means the animation is inactive.
        public int DrinkAnimationTimer { get; private set; }
        private const int DrinkAnimationDuration = 36;
        /// Starts the visual drinking animation.
        public void PlayBottleDrinkAnimation()
        {
            DrinkAnimationTimer = DrinkAnimationDuration;
        }


        public override void Initialize()
        {
            InitializeSlots();

            bottleBuffTypes.Clear();
            refreshTimer = 0;
            wasInEncounter = false;
        }

        private void InitializeSlots()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (StoredPotions[i] == null)
                    StoredPotions[i] = new Item();
            }
        }

        // Save all eight stored items, including empty slots.
        public override void SaveData(TagCompound tag)
        {
            InitializeSlots();

            var items = new List<TagCompound>();

            foreach (Item item in StoredPotions)
                items.Add(ItemIO.Save(item));

            tag["StoredPotions"] = items;
        }

        // Restore stored items when loading a player.
        public override void LoadData(TagCompound tag)
        {
            StoredPotions = new Item[SlotCount];

            if (tag.ContainsKey("StoredPotions"))
            {
                var items = tag.GetList<TagCompound>("StoredPotions");

                for (int i = 0; i < SlotCount; i++)
                {
                    StoredPotions[i] = i < items.Count
                        ? ItemIO.Load(items[i])
                        : new Item();
                }
            }

            InitializeSlots();

            bottleBuffTypes.Clear();
            refreshTimer = 0;
            wasInEncounter = false;
        }

        // Check encounter state and refresh stored buffs.
        public override void PostUpdate()
        {

            // ---animation stuff
            if (DrinkAnimationTimer > 0)
                DrinkAnimationTimer--;


            if (Main.netMode == Terraria.ID.NetmodeID.Server &&
                Player.whoAmI < 0)
                return;
            bool hasBottle = HasBottleInInventory();
            bool inEncounter = BattleBottleCombatSystem.IsEncounterActive() && hasBottle;
            /* TEMPORARY DEBUG: report whether the encounter detector sees a boss/event.
            if (inEncounter && !wasInEncounter && Player.whoAmI == Main.myPlayer)
            {
                Main.NewText("[Battle Bottle] Encounter detected.");
            }
            */

            // If the encounter has just ended, remove buffs owned by
            // Battle Bottle immediately.
            if (wasInEncounter && !inEncounter)
            {
                ClearBottleBuffs();
                refreshTimer = 0;
            }

            // Apply buffs immediately when the encounter begins.
            if (inEncounter && !wasInEncounter)
            {
                ApplyStoredBuffs();
                PlayBottleDrinkAnimation(); //animation play
                refreshTimer = 0;
            }

            // Keep buffs refreshed while the encounter remains active.
            if (inEncounter)
            {
                refreshTimer++;

                if (refreshTimer >= RefreshInterval)
                {
                    ApplyStoredBuffs();
                    refreshTimer = 0;
                }
            }

            wasInEncounter = inEncounter;
        }

        // Apply the primary buff from each eligible stored potion.
        // The stored item itself is never used or consumed.
        
        // Apply buffs only from slots that are currently unlocked.
        private void ApplyStoredBuffs()
        {
            if (!Player.active || Player.dead)
                return;

            InitializeSlots();

            for (int i = 0; i < SlotCount; i++)
            {
                // Slot indexes are zero-based: 0 is the first UI slot.
                if (!BattleBottleWorld.IsSlotUnlocked(i))
                    continue;

                Item item = StoredPotions[i];

                if (!BattleBottlePotionRules.IsEligibleBuffPotion(item))
                    continue;

                ApplyBottleBuff(item.buffType, item.buffTime);
            }
        }
        /*
        // TEMPORARY DIAGNOSTICS:
        // Reports whether stored items are found and considered eligible.
        private void ApplyStoredBuffs()
        {
            if (!Player.active || Player.dead)
                return;

            InitializeSlots();

            // Only print diagnostics for this local player.
            bool showDebug = Player.whoAmI == Main.myPlayer;

            foreach (Item item in StoredPotions)
            {
                if (item == null || item.IsAir)
                    continue;

                bool eligible = BattleBottlePotionRules.IsEligibleBuffPotion(item);

                if (showDebug)
                {
                    Main.NewText(
                        $"Battle Bottle: {item.Name} | " +
                        $"potion={item.potion}, " +
                        $"buffType={item.buffType}, " +
                        $"buffTime={item.buffTime}, " +
                        $"eligible={eligible}"
                    );
                }

                if (!eligible)
                    continue;

                ApplyBottleBuff(item.buffType, item.buffTime);
            }
        }*/

        // Apply a buff and record it only if it was not active beforehand.
        // Apply a buff without extending a pre-existing buff from another source.
        // Buffs owned by the Battle Bottle can still be refreshed during combat.
        public void ApplyBottleBuff(int buffType, int duration)
        {
            if (buffType <= 0 || duration <= 0)
                return;

            bool alreadyOwned = bottleBuffTypes.Contains(buffType);
            bool alreadyActive = Player.HasBuff(buffType);

            // If another source already provided this buff, leave its duration alone.
            // If that buff expires during combat, a later refresh can apply it.
            if (alreadyActive && !alreadyOwned)
                return;

            Player.AddBuff(buffType, duration);

            // Track buffs the bottle applies so they can be removed at encounter end.
            if (!alreadyActive)
                bottleBuffTypes.Add(buffType);
        }

        // Remove buffs owned by Battle Bottle when the encounter ends.
        public void ClearBottleBuffs()
        {
            foreach (int buffType in bottleBuffTypes)
            {
                Player.ClearBuff(buffType);
            }

            bottleBuffTypes.Clear();
        }

        // Reapply buffs after respawning in an active encounter.
        public override void OnRespawn()
        {
            refreshTimer = 0;

            if (BattleBottleCombatSystem.IsEncounterActive() && HasBottleInInventory())
            {
                ApplyStoredBuffs();
                PlayBottleDrinkAnimation(); //animation
                wasInEncounter = true;
            }
        }
    }
}
