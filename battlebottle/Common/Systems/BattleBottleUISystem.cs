
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using BattleBottle.Common.UI;

namespace BattleBottle.Common.Systems
{
    // Manages the Battle Bottle window.
    public class BattleBottleUISystem : ModSystem
    {
        private UserInterface bottleInterface;
        private BattleBottleUI bottleUI;

        // Prevent Escape from closing the window repeatedly
        // while the key is held down.
        private bool escapeWasPressed;

        public override void Load()
        {
            // Dedicated servers do not draw user interfaces.
            if (Main.dedServ)
                return;

            //load specific locked sprites
            BattleBottleUI.LoadLockedSlotTextures();

            bottleInterface = new UserInterface();
            bottleUI = new BattleBottleUI();
            bottleUI.Activate();
        }

        // Called by BattleBottleItem when the bottle is right-clicked.
        public static void OpenBottle(Player player)
        {
            // Only open the UI for the local player.
            if (Main.dedServ || player.whoAmI != Main.myPlayer)
                return;

            ModContent.GetInstance<BattleBottleUISystem>()
                .ShowBottle();
        }

        private void ShowBottle()
        {
            if (bottleInterface == null || bottleUI == null)
                return;

            // Display the bottle window.
            bottleInterface.SetState(bottleUI);
        }

        // Close the window from other code when needed.
        public static void CloseBottle()
        {
            if (Main.dedServ)
                return;

            ModContent.GetInstance<BattleBottleUISystem>()
                .HideBottle();
        }

        private void HideBottle()
        {
            // A null UI state means no bottle window is displayed.
            bottleInterface?.SetState(null);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (bottleInterface?.CurrentState == null)
                return;

            // Close the window when Escape is pressed.
            bool escapePressed = Main.keyState.IsKeyDown(Keys.Escape);

            if (escapePressed && !escapeWasPressed)
            {
                HideBottle();
            }

            escapeWasPressed = escapePressed;

            // Update UI elements while the window is open.
            bottleInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(
            System.Collections.Generic.List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(
                layer => layer.Name == "Vanilla: Mouse Text");

            if (mouseTextIndex == -1)
                return;

            // Draw the bottle window above the world but below mouse text.
            layers.Insert(
                mouseTextIndex,
                new LegacyGameInterfaceLayer(
                    "BattleBottle: UI",
                    delegate
                    {
                        bottleInterface?.Draw(
                            Main.spriteBatch,
                            Main.gameTimeCache);

                        return true;
                    },
                    InterfaceScaleType.UI));
        }

        public override void Unload()
        {
            // Release the cached locked-slot textures.
            BattleBottleUI.UnloadLockedSlotTextures();

            // Release UI references when the mod unloads.
            bottleInterface = null;
            bottleUI = null;
        }
    }
}
