using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using MoonShared;
using System.Reflection;
using System.Reflection.Emit;
using StardewValley.Tools;
using StardewValley.Menus;
using StardewValley.Objects;

namespace RanchingToolUpgrades
{
    [HarmonyPatch(typeof(Utility), nameof(Utility.getBlacksmithUpgradeStock))]
    class Utility_GetBlacksmithUpgradeStock
    {
        public static void Postfix(
            Dictionary<ISalable, int[]> __result,
            Farmer who)
        {
            try
            {
                UpgradeablePail.AddToShopStock(itemPriceAndStock: __result, who: who);
                UpgradeableShears.AddToShopStock(itemPriceAndStock: __result, who: who);
                UpgradeablePan.AddToShopStock(itemPriceAndStock: __result, who: who);
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(EfficientToolEnchantment), nameof(EfficientToolEnchantment.CanApplyTo))]
    class EnchantmentFixer
    {
        private static bool Prefix(Item item, ref bool __result)
        {
            if (item is Tool && ((item is MilkPail) || (item is Shears) || (item is Pan))) { 
                __result = true;
                return false; //do not run original
            }
            return true; //run original
        }
    }

    [HarmonyPatch(typeof(Tool), "get_" + nameof(Tool.Name))]
    public static class ToolNamePatch
    {
        public static void Postfix(Tool __instance, ref string __result)
        {
            if (__instance.UpgradeLevel >= 5)
            {
                string tier = __instance.UpgradeLevel == 5 ? "radioactive" : "mythicite";
                string tool = "";
                switch (__instance.BaseName)
                {
                    case "Axe": tool = "axe"; break;
                    case "Watering Can": tool = "wcan"; break;
                    case "Pickaxe": tool = "pick"; break;
                    case "Hoe": tool = "hoe"; break;
                    case "Shovel": tool = "shovel"; break;
                    case "Shears": tool = "shears"; break;
                    case "Ore Pan": tool = "pan"; break;
                    case "Milk Pail": tool = "pail"; break;
                }
                __result = ModEntry.Instance.I18n.Get($"tool." + tool + "." + tier);
            }
        }
    }

    [HarmonyPatch(typeof(SpaceCore.Interface.NewForgeMenu), nameof(SpaceCore.Interface.NewForgeMenu.CraftItem))]
    class NewForgeMenuCraftItemPatcher
    {
        public static void Postfix(ref Item __result, Item left_item, Item right_item)
        {
            ///Make sure margo is loaded to do forge upgrading
            if (ModEntry.MargoLoaded)
            {

                ///If players have the Margo Compact turned off, do not do anything
                if (ModEntry.Config.MargoCompact == false)
                {
                    return;
                }

                ///Check to see if the item is a shovel or upgradeable shovel. If it is not, skip this code
                if (left_item is not (Tool tool and (UpgradeableShears or Shears or MilkPail or UpgradeablePail or Pan or UpgradeablePan)))
                {
                    return;
                }

                /// Check to see if Moon Misadventures is loaded, if it is, set upgrade level to 6, if not, set it to 5
                int maxToolUpgrade = ModEntry.MoonLoaded == true ? 6 : 5;
                if (tool.UpgradeLevel >= maxToolUpgrade)
                {
                    return;
                }
                ///Get the right item to upgrade the tool
                int upgradeItemIndex = tool.UpgradeLevel switch
                {
                    0 => ObjectIds.CopperBar,
                    1 => ObjectIds.IronBar,
                    2 => ObjectIds.GoldBar,
                    3 => ObjectIds.IridiumBar,
                    4 => ObjectIds.RadioactiveBar,
                    5 => "spacechase0.MoonMisadventures/Mythicite Bar".GetDeterministicHashCode(),
                    _ => ObjectIds.PrismaticShard,
                };
                ///If the right item is the ... right item, allow for tool upgrade
                if (right_item.ParentSheetIndex == upgradeItemIndex && right_item.Stack >= 5)
                {
                    ((Tool)left_item).UpgradeLevel++;
                }
                __result = left_item;
            }
        }
    }

    [HarmonyPatch(typeof(SpaceCore.Interface.NewForgeMenu), nameof(SpaceCore.Interface.NewForgeMenu.IsValidCraftIngredient))]
    class NewForgeMenuIsValidCraftIngredientPatcher
    {
        public static void Postfix(ref bool __result, Item item)
        {
            ///Make sure margo is loaded to do forge upgrading
            if (ModEntry.MargoLoaded)
            {
                ///If players have the Margo Compact turned off, do not do anything
                if (ModEntry.Config.MargoCompact == false)
                {
                    return;
                }
                ///Check to see if the item is a shovel or upgradeable shovel. If it is not, skip this code
                if (item is not (Tool tool and (UpgradeableShears or Shears or MilkPail or UpgradeablePail or Pan or UpgradeablePan)))
                {
                    return;
                }
                int maxToolUpgrade = ModEntry.MoonLoaded == true ? 7 : 6;
                if (tool.UpgradeLevel < maxToolUpgrade)
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SpaceCore.Interface.NewForgeMenu), nameof(SpaceCore.Interface.NewForgeMenu.IsValidCraft))]
    class NewForgeMenuIsValidCraftPatcher
    {
        public static void Postfix(ref bool __result, Item left_item, Item right_item)
        {
            ///If players have the Margo Compact turned off, do not do anything
            if (ModEntry.Config.MargoCompact == false)
            {
                return;
            }

            ///Check to see if the item is a shovel or upgradeable shovel. If it is not, skip this code
            if (left_item is not (Tool tool and (UpgradeableShears or Shears or MilkPail or UpgradeablePail or Pan or UpgradeablePan)))
            {
                return;
            }
            int maxToolUpgrade = ModEntry.MoonLoaded == true ? 6 : 5;
            if (tool.UpgradeLevel >= maxToolUpgrade)
            {
                return;
            }
            int upgradeItemIndex = tool.UpgradeLevel switch
            {
                0 => ObjectIds.CopperBar,
                1 => ObjectIds.IronBar,
                2 => ObjectIds.GoldBar,
                3 => ObjectIds.IridiumBar,
                4 => ObjectIds.RadioactiveBar,
                5 => "spacechase0.MoonMisadventures/Mythicite Bar".GetDeterministicHashCode(),
                _ => ObjectIds.PrismaticShard,
            };

            if (right_item.ParentSheetIndex == upgradeItemIndex && right_item.Stack >= 5)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.showHoldingItem))]
    class Farmer_ShowHoldingItem
    {
        public static bool Prefix(
            Farmer who)
        {
            try
            {
                Item mrg = who.mostRecentlyGrabbedItem;
                if (mrg is UpgradeablePail || mrg is UpgradeableShears || mrg is UpgradeablePan)
                {
                    Rectangle r = UpgradeablePail.IconSourceRectangle((who.mostRecentlyGrabbedItem as Tool).UpgradeLevel);
                    switch (mrg)
                    {
                        case UpgradeablePail:
                            r = UpgradeablePail.IconSourceRectangle((who.mostRecentlyGrabbedItem as Tool).UpgradeLevel);
                            break;
                        case UpgradeableShears:
                            r = UpgradeableShears.IconSourceRectangle((who.mostRecentlyGrabbedItem as Tool).UpgradeLevel);
                            break;
                        case UpgradeablePan:
                            r = UpgradeablePan.IconSourceRectangle((who.mostRecentlyGrabbedItem as Tool).UpgradeLevel);
                            break;
                    }
                    Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
                        textureName: ModEntry.Assets.SpritesPath,
                        sourceRect: r,
                        animationInterval: 2500f,
                        animationLength: 1,
                        numberOfLoops: 0,
                        position: who.Position + new Vector2(0f, -124f),
                        flicker: false,
                        flipped: false,
                        layerDepth: 1f,
                        alphaFade: 0f,
                        color: Color.White,
                        scale: 4f,
                        scaleChange: 0f,
                        rotation: 0f,
                        rotationChange: 0f)
                    {
                        motion = new Vector2(0f, -0.1f)
                    });
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Utility), nameof(Utility.getFishShopStock))]
    class Utility_GetFishShopStock
    {

        /// <summary>
        /// Removes the old Copper Pan tool from the fishing shop.
        /// </summary>
        public static void Postfix(Dictionary<ISalable, int[]> __result)
        {
            try
            {
                // Keying off of `new Pan()` doesn't work.
                // Iterate over items for sale, and remove any by the name "Copper Pan".
                foreach (ISalable key in __result.Keys)
                {
                    if (key.Name.Equals("Copper Pan"))
                    {
                        __result.Remove(key);
                    }
                }
                if (ModEntry.Config.BuyablePan)
                {
                    __result.Add(new UpgradeablePan(0), new int[2] { ModEntry.Config.BuyCost, 2147483647 });
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(Utility), nameof(Utility.getAnimalShopStock))]
    class Utility_GetAnimalShopStock
    {
        public static void Postfix(
            Dictionary<ISalable, int[]> __result
            )
        {
            try
            {
                // Keying off of `new MilkPail()` or `new Shears()` doesn't work.
                // Iterate over items for sale, and remove any by the name "Milk Pail" or "Shears"
                foreach (ISalable key in __result.Keys)
                {
                    if (key.Name.Equals("Milk Pail") || key.Name.Equals("Shears"))
                    {
                        __result.Remove(key);
                    }
                }
                if (Game1.player.hasItemWithNameThatContains("Pail") == null)
                {
                    __result.Add(new UpgradeablePail(0), new int[2] { ModEntry.Config.PailBuyCost, 1 });
                }
                if (Game1.player.hasItemWithNameThatContains("Shears") == null)
                {
                    __result.Add(new UpgradeableShears(0), new int[2] { ModEntry.Config.ShearsBuyCost, 1 });
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(Utility), nameof(Utility.PerformSpecialItemPlaceReplacement))]
    class Utility_PerformSpecialItemPlaceReplacement
    {
        /// <summary>
        /// Handles using pan as a hat in certain menus.
        /// </summary>
        public static bool Prefix(
            ref Item __result,
            Item placedItem)
        {
            try
            {
                if (placedItem != null && placedItem is UpgradeablePan upgradeablePan)
                {
                    __result = UpgradeablePan.PanToHat(upgradeablePan);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Utility), nameof(Utility.PerformSpecialItemGrabReplacement))]
    class Utility_PerformSpecialItemGrabReplacement
    {

        /// <summary>
        /// Handles using pan as a hat in certain menus.
        /// </summary>
        public static bool Prefix(
            ref Item __result,
            Item heldItem)
        {
            try
            {
                if (heldItem != null && heldItem is Hat)
                {
                    int hatId = (int)(heldItem as Hat).which.Value;
                    if (hatId == ModEntry.JsonAssets.GetHatId("Pan"))
                    {
                        __result = new UpgradeablePan(0);
                    }
                    else if (hatId == 71) // Using original copper pan hat.
                    {
                        __result = new UpgradeablePan(1);
                    }
                    else if (hatId == ModEntry.JsonAssets.GetHatId("Steel Pan"))
                    {
                        __result = new UpgradeablePan(2);
                    }
                    else if (hatId == ModEntry.JsonAssets.GetHatId("Gold Pan"))
                    {
                        __result = new UpgradeablePan(3);
                    }
                    else if (hatId == ModEntry.JsonAssets.GetHatId("Iridium Pan"))
                    {
                        __result = new UpgradeablePan(4);
                    }
                    
                    else if (hatId == ModEntry.JsonAssets.GetHatId("Radioactive Pan"))
                    {
                        __result = new UpgradeablePan(5);
                    }
                    else if (hatId == ModEntry.JsonAssets.GetHatId("Mythicite Pan"))
                    {
                        __result = new UpgradeablePan(6);
                    }
                    else
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
    class InventoryPage_ReceiveLeftClick
    {
        /// <summary>
        /// Handles pan to hat conversion in Inventory page.  Since there's no good entry point for patching,
        /// detects changes to player.hat.Value and player.CursorSlotItem using __state.
        /// </summary>
        public static void Prefix(ref Item[] __state)
        {
            if (Game1.player.CursorSlotItem is UpgradeablePan)
            {
                __state = new Item[] {
                    Game1.player.CursorSlotItem,
                    Game1.player.hat.Value,
                };
            }
        }

        /// <summary>
        /// Handles pan to hat conversion in Inventory page.  Since there's no good entry point for patching,
        /// detects changes to player.hat.Value and player.CursorSlotItem using __state.
        /// </summary>
        public static void Postfix(Item[] __state)
        {
            try
            {
                if (__state is not null && __state[0] is UpgradeablePan upgradeablePan && __state[1] != Game1.player.hat.Value)
                {
                    Game1.player.hat.Value = UpgradeablePan.PanToHat(upgradeablePan);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.toolPowerIncrease))]
    class Farmer_ToolPowerIncrease
    {
        static void Postfix(Farmer __instance)
        {
            if (__instance.CurrentTool is UpgradeablePan)
            {
                __instance.FarmerSprite.CurrentFrame = 123;
            }
        }
    }

    [HarmonyPatch(typeof(FarmerSprite), nameof(FarmerSprite.getAnimationFromIndex))]
    class FarmerSprite_GetAnimationFromIndex
    {
        /// <summary>
        /// Use a TemporaryAnimatedSprite to make the panning animation reflect upgrade level.
        /// </summary>
        public static void Postfix(int index, FarmerSprite requester)
        {
            try
            {
                var owner = Traverse.Create(requester).Field("owner").GetValue<Farmer>();
                if (owner is null && owner == Game1.player)
                    return;

                if (index == 303)
                {
                    int upgradeLevel = owner.CurrentTool.UpgradeLevel;
                    int genderOffset = owner.IsMale ? -1 : 0;

                    Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
                        textureName: ModEntry.Assets.SpritesPath,
                        sourceRect: UpgradeablePan.AnimationSourceRectangle(upgradeLevel),
                        animationInterval: ModEntry.Config.AnimationFrameDuration,
                        animationLength: 4,
                        numberOfLoops: 3,
                        position: owner.Position + new Vector2(0f, (ModEntry.Config.AnimationYOffset + genderOffset) * 4),
                        flicker: false,
                        flipped: false,
                        layerDepth: 1f,
                        alphaFade: 0f,
                        color: Color.White,
                        scale: 4f,
                        scaleChange: 0f,
                        rotation: 0f,
                        rotationChange: 0f)
                    {
                        endFunction = extraInfo =>
                        {
                            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
                                textureName: ModEntry.Assets.SpritesPath,
                                sourceRect: UpgradeablePan.AnimationSourceRectangle(upgradeLevel),
                                animationInterval: ModEntry.Config.AnimationFrameDuration,
                                animationLength: 3,
                                numberOfLoops: 0,
                                position: owner.position + new Vector2(0f, (ModEntry.Config.AnimationYOffset + genderOffset) * 4),
                                flicker: false,
                                flipped: false,
                                layerDepth: 1f,
                                alphaFade: 0f,
                                color: Color.White,
                                scale: 4f,
                                scaleChange: 0f,
                                rotation: 0f,
                                rotationChange: 0f)
                            {
                                endFunction = extraInfo =>
                                {
                                    Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
                                        textureName: ModEntry.Assets.SpritesPath,
                                        sourceRect: UpgradeablePan.AnimationSourceRectangle(upgradeLevel),
                                        animationInterval: ModEntry.Config.AnimationFrameDuration * 2.5f,
                                        animationLength: 1,
                                        numberOfLoops: 0,
                                        position: owner.position + new Vector2(0f, (ModEntry.Config.AnimationYOffset + genderOffset) * 4),
                                        flicker: false,
                                        flipped: false,
                                        layerDepth: 1f,
                                        alphaFade: 0f,
                                        color: Color.White,
                                        scale: 4f,
                                        scaleChange: 0f,
                                        rotation: 0f,
                                        rotationChange: 0f));
                                }
                            });
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
        }
    }

    [HarmonyPatch(typeof(Event), nameof(Event.command_awardFestivalPrize))]
    class Event_Command_AwardFestivalPrize
    {
        /// <summary>
        /// Changes which pan tool is rewarded during events.
        /// </summary>
        public static bool Prefix(Event __instance, string[] split)
        {
            try
            {
                if (split.Length > 1 && split[1].ToLower() == "pan")
                {
                    Game1.player.addItemByMenuIfNecessary(new UpgradeablePan());
                    if (Game1.activeClickableMenu == null)
                    {
                        __instance.CurrentCommand++;
                    }
                    __instance.CurrentCommand++;
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Event), nameof(Event.command_itemAboveHead))]
    class Event_Command_ItemAboveHead
    {
        /// <summary>
        /// Changes which pan tool is shown being held during events.
        /// </summary>
        public static bool Prefix(Event __instance, string[] split)
        {
            try
            {
                if (split.Length > 1 && split[1].Equals("pan"))
                {
                    __instance.farmer.holdUpItemThenMessage(new UpgradeablePan());
                    __instance.CurrentCommand++;
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Event), nameof(Event.skipEvent))]
    class Event_SkipEvent
    {
        /// <summary>
        /// Rewards modded pan tool if event is skipped.
        /// </summary>
        public static bool Prefix(
            Event __instance,
            Dictionary<string, Vector3> ___actorPositionsAfterMove)
        {
            try
            {
                if (__instance.id == 404798)
                {
                    // Generic skip logic copied from skipEvent.
                    // If other mods patch skipEvent to change this logic, things might break.
                    if (__instance.playerControlSequence)
                    {
                        __instance.EndPlayerControlSequence();
                    }
                    Game1.playSound("drumkit6");
                    ___actorPositionsAfterMove.Clear();
                    foreach (NPC i in __instance.actors)
                    {
                        bool ignore_stop_animation = i.Sprite.ignoreStopAnimation;
                        i.Sprite.ignoreStopAnimation = true;
                        i.Halt();
                        i.Sprite.ignoreStopAnimation = ignore_stop_animation;
                        __instance.resetDialogueIfNecessary(i);
                    }
                    __instance.farmer.Halt();
                    __instance.farmer.ignoreCollisions = false;
                    Game1.exitActiveMenu();
                    Game1.dialogueUp = false;
                    Game1.dialogueTyping = false;
                    Game1.pauseTime = 0f;

                    // Event specific skip logic.
                    if (Game1.player.getToolFromName("Pan") is null)
                    {
                        Game1.player.addItemByMenuIfNecessary(new UpgradeablePan());
                    }
                    __instance.endBehaviors(new string[1] { "end" }, Game1.currentLocation);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
            return true;
        }
    }

    // 3rd party
    // Fix Wear More Rings incompatibility
    [HarmonyPatch("StardewHack.WearMoreRings.ModEntry", "EquipmentClick")]
    class WearMoreRings_ModEntry_EquipmentClick
    {
        public static bool Prepare()
        {
            return ModEntry.Instance.Helper.ModRegistry.IsLoaded("bcmpinc.WearMoreRings");
        }

        public static void Prefix(
            StardewValley.Menus.ClickableComponent icon
            )
        {
            try
            {
                if (icon.name == "Hat" && Game1.player.CursorSlotItem is UpgradeablePan pan)
                {
                    Game1.player.CursorSlotItem = UpgradeablePan.PanToHat(pan);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
        }
    }

    // Allow sending pan to upgrade in the mail with Mail Services
    [HarmonyPatch("MailServicesMod.ToolUpgradeOverrides", "mailbox")]
    class MailServicesMod_ToolUpgradeOverrides_Mailbox_Pan
    {
        public static bool Prepare()
        {
            return ModEntry.Instance.Helper.ModRegistry.IsLoaded("Digus.MailServicesMod");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Is(OpCodes.Isinst, typeof(Axe)))
                {
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(UpgradeablePan));
                    yield return code[i + 1];
                    yield return code[i + 2];
                    // ILCode of newer versions is shorter for whatever reason
                    if (ModEntry.Instance.Helper.ModRegistry.Get("Digus.MailServicesMod").Manifest.Version.IsOlderThan("1.5"))
                    {
                        yield return code[1 + 3];
                    }
                    yield return code[i];
                }
                else
                {
                    yield return code[i];
                }
            }
        }
    }

    // 3rd party
    // Allow sending tools to upgrade in the mail with Mail Services
    [HarmonyPatch("MailServicesMod.ToolUpgradeOverrides", "mailbox")]
    class MailServicesMod_ToolUpgradeOverrides_Mailbox
    {
        public static bool Prepare()
        {
            return ModEntry.Instance.Helper.ModRegistry.IsLoaded("Digus.MailServicesMod");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Is(OpCodes.Isinst, typeof(Axe)))
                {
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(UpgradeablePail));
                    yield return code[i + 1];
                    yield return code[i + 2];
                    yield return code[i + 3];
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(UpgradeableShears));
                    yield return code[i + 1];
                    yield return code[i + 2];
                    yield return code[i + 3];
                    yield return code[i];
                }
                else
                {
                    yield return code[i];
                }
            }
        }
    }



}
