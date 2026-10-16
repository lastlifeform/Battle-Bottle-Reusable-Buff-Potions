
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace BattleBottle.Content.Items
{
    public class BattleBottleItem : ModItem
    {
        // Temporary vanilla texture for testing.
        // You can replace this with your own sprite later.
        //public override string Texture =>
            //$"Terraria/Images/Item_{ItemID.Bottle}";
        //DisplayName.SetDefault("Battle Bottle");
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(silver: 72);
            Item.rare = ItemRarityID.Blue;

            // The bottle is not equipment or a consumable.
            Item.accessory = false;
            Item.consumable = false;

            // Disable normal item-use behavior.
            Item.useStyle = ItemUseStyleID.None;
            Item.useAnimation = 0;
            Item.useTime = 0;
        }

        // Allow the bottle to respond to inventory right-clicks.
        public override bool CanRightClick()
        {
            return true;
        }

        // Open the bottle UI without removing the bottle.
        public override void RightClick(Player player)
        {
            BattleBottle.Common.Systems
                .BattleBottleUISystem.OpenBottle(player);
        }

        // IMPORTANT:
        // Terraria normally consumes one item after RightClick().
        // Returning false prevents that behavior.
        public override bool ConsumeItem(Player player)
        {
            return false;
        }

        // Crafting recipes for Crimson and Corruption worlds.
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bottle)
                .AddIngredient(ItemID.ObsidianSkull)
                .AddIngredient(ItemID.FallenStar, 3)
                .AddIngredient(ItemID.DemoniteBar)
                .AddTile(TileID.DemonAltar)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.Bottle)
                .AddIngredient(ItemID.ObsidianSkull)
                .AddIngredient(ItemID.FallenStar, 3)
                .AddIngredient(ItemID.CrimtaneBar)
                .AddTile(TileID.DemonAltar)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(
                Mod,
                "RightClickToOpen",
                "Right Click To Open"
            ));

            tooltips.Add(new TooltipLine(
                Mod,
                "StoredPotionDescription",
                "Stored buff potions will be activated for free during boss fights and some events"
            ));
        }
    }
}
