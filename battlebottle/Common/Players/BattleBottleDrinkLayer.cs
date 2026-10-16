
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BattleBottle.Common.Players
{
    public class BattleBottleDrinkLayer : PlayerDrawLayer
    {
        // Draw immediately after the player's head layer.
        public override Position GetDefaultPosition()
            => new AfterParent(PlayerDrawLayers.Head);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            BattleBottlePlayer bottlePlayer =
                drawInfo.drawPlayer.GetModPlayer<BattleBottlePlayer>();

            return bottlePlayer.DrinkAnimationTimer > 0;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;

            BattleBottlePlayer bottlePlayer =
                player.GetModPlayer<BattleBottlePlayer>();

            int timer = bottlePlayer.DrinkAnimationTimer;

            if (timer <= 0)
                return;

            // Reuse the vanilla Bottle item sprite.
            Texture2D texture = ModContent.Request<Texture2D>(
                "BattleBottle/Content/Items/BattleBottleItem"
            ).Value;

            // Convert the remaining timer into elapsed animation time.
            float elapsed = 36f - timer;

            // Lift the bottle toward the player's mouth over the first
            // 8 ticks, hold it there, then lower it over the final 12.
            float lift;

            if (elapsed < 8f)
            {
                lift = elapsed / 8f;
            }
            else if (elapsed < 24f)
            {
                lift = 1f;
            }
            else
            {
                lift = 1f - (elapsed - 24f) / 12f;
            }

            lift = MathHelper.Clamp(lift, 0f, 1f);

            int direction = player.direction;

            // Position the bottle beside the face, moving it upward
            // and slightly forward as the animation progresses.
            Vector2 worldPosition = player.MountedCenter + new Vector2(
                direction * (5f + 4f * lift),
                -3f - 12f * lift
            );

            Vector2 screenPosition = worldPosition - Main.screenPosition;

            // Tilt the bottle while it is held at the mouth.
            float tilt = MathHelper.Clamp((elapsed - 6f) / 4f, 0f, 1f);

            if (elapsed > 24f)
                tilt *= MathHelper.Clamp(1f - (elapsed - 24f) / 12f, 0f, 1f);

            float rotation = direction * -0.9f * tilt;

            Color color = Lighting.GetColor(
                (int)(worldPosition.X / 16f),
                (int)(worldPosition.Y / 16f)
            );

            SpriteEffects effects = direction == -1
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            drawInfo.DrawDataCache.Add(new DrawData(
                texture,
                screenPosition,
                null,
                color,
                rotation,
                texture.Size() * 0.5f,
                1f,
                effects,
                0
            ));
        }
    }
}
