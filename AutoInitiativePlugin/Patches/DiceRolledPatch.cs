using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoInitiative.Patches
{
    // Utility class to store the initiatives of the creatures
    internal static class InitUtils
    {
        internal static bool EditIsOpen = false;
        internal static Dictionary<CreatureGuid, (int, int)> Initiatives = new Dictionary<CreatureGuid, (int, int)>();

        // Clear all tracked initiatives
        internal static void ClearInitiatives()
        {
            Initiatives.Clear();
            AutoInitiativePlugin.logger.LogInfo("Cleared Tracked Initiatives");
        }

        // Set the initiatives in the InitiativeManager
        internal static void SetInitiatives()
        {
            // Sort the initiatives by initiative and modifier
            CreatureGuid[] array = Initiatives
                .OrderByDescending(entry => entry.Value.Item1)
                .ThenByDescending(entry => entry.Value.Item2)
                .Select(entry => { AutoInitiativePlugin.logger.LogInfo($"{entry.Value}: {entry.Key}"); return entry.Key; }).ToArray();

            InitiativeManager.SetEditQueue(array);
        }
    }

    // Patch to remove a creature from the initiative list when it is removed from the edit queue
    [HarmonyPatch(typeof(InitiativeManager), "RemoveCreatureFromEditQueue")]
    public class InitiativeManagerRemoveCreatureFromEditQueuePatch
    {
        static void Postfix(CreatureGuid creatureGuid)
        {
            // Remove the creature from the initiative list
            if (InitUtils.Initiatives.ContainsKey(creatureGuid))
            {
                InitUtils.Initiatives.Remove(creatureGuid);
                AutoInitiativePlugin.logger.LogInfo($"Removed {creatureGuid} from Initiative");
            }
        }
    }

    // Patch to remove a creature from the initiative list when it is released
    [HarmonyPatch(typeof(InitiativeManager), "ReleaseCreature")]
    public class InitiativeManagerReleaseCreaturePatch
    {
        // Remove the creature from the initiative list
        static void Postfix(CreatureGuid creature)
        {
            if (InitUtils.Initiatives.ContainsKey(creature))
            {
                InitUtils.Initiatives.Remove(creature);
                AutoInitiativePlugin.logger.LogInfo($"Removed {creature} from Initiative");
            }
        }
    }

    // Patch to clear the initiative list when the initiative manager is cleared
    [HarmonyPatch(typeof(UI_InitativeManager), "ClearEdit")]
    public class InitiativeManagerClearPatch
    {
        // Clear the initiative list
        static void Prefix()
        {
            InitUtils.ClearInitiatives();
            AutoInitiativePlugin.logger.LogInfo($"Initiatives cleared");
        }
    }

    // Patch to set the edit state of the initiative manager
    [HarmonyPatch(typeof(UI_InitativeManager), "OpenEdit")]
    public class InitiativeManagerApplyEditPatch
    {
        // Set the edit state of the initiative manager
        static void Prefix(bool value)
        {
            InitUtils.EditIsOpen = value;
            AutoInitiativePlugin.logger.LogInfo($"Initiative Manager is open: {value}");
        }
    }

    // Patch to receive the dice roll results and set the initiative of the creature
    [HarmonyPatch(typeof(DiceManager), "RPC_DiceResult")]
    public class ReceiveDiceRolledPatch
    {
        
        // Receive the dice roll results and set the initiative of the creature
        static void Postfix(bool isGmOnly, byte[] diceListData, PhotonMessageInfo msgInfo, BrSerialize.Reader ____reader)
        {
            // Check if the GM mode is active and the initiative manager is open
            if (LocalClient.IsInGmMode && InitUtils.EditIsOpen)
            {
                // Deserialize the dice roll results
                BrSerializeHelpers.DeserializeFromByteArray(____reader, diceListData, DiceManager.RollResults.Deserialize, out DiceManager.RollResults thing);

                bool flag = false;
                int num = 0;
                int initiativeResult = 0;
                initiativeResult = 0;

                int initiativeModifier = 0;

                // Loop through the dice roll results
                // This section here is a modification of LA's code from their AutoInitiative plugin
                foreach (DiceManager.RollResultsGroup resultsGroup in thing.ResultsGroups)
                {
                    num = 0;
                    if (resultsGroup.Name != null && Convert.ToString(resultsGroup.Name).Trim() != "")
                    {
                        if (resultsGroup.Name.Contains(AutoInitiativePlugin.InitiativeText.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            flag = true;
                        }
                    }

                    // Check if the dice roll results are for the initiative
                    if (!flag)
                    {
                        continue;
                    }

                    // Get the dice roll results
                    DiceManager.RollOperand.Which which = resultsGroup.Result.Get(out DiceManager.RollResultsOperation operation, out DiceManager.RollResult result, out DiceManager.RollValue value);
                    if (which.HasFlag(DiceManager.RollOperand.Which.Operation))
                    {
                        num = 1;
                        try
                        {
                            num = (operation.Operator != DiceManager.DiceOperator.Subtract) ? 1 : (-1);
                        }
                        catch (Exception)
                        {

                        }

                        // Loop through the dice roll operands and results
                        if (operation.Operands != null)
                        {
                            foreach (DiceManager.RollOperand operand in operation.Operands)
                            {
                                operand.Get(out operation, out result, out value);
                                if (result.Kind.RegisteredName == "<unknown>")
                                {
                                    int value2 = value.Value * num;
                                    initiativeResult += value2;

                                    initiativeModifier += value2;
                                    AutoInitiativePlugin.logger.LogInfo("Initiative Modifier: " + initiativeModifier);
                                    
                                    continue;
                                }

                                foreach (short result2 in result.Results)
                                {
                                    initiativeResult += result2;
                                }
                            }
                        }
                        else if (result.Results != null)
                        {
                            foreach (short result3 in result.Results)
                            {
                                initiativeResult += result3;
                            }
                        }
                    }
                }

                // Set the initiative of the creature
                if (flag)
                {
                    CreatureGuid[] creature = new CreatureGuid[0];

                    // Check if the creature is lassoed
                    if (thing.ClientId == LocalClient.Id 
                        && LocalClient.HasLassoedCreatures
                        && LocalClient.LassoedCount > 0
                        && LocalClient.TryGetLassoedCreatureIds(out var guids)
                        )
                    {
                        AutoInitiativePlugin.logger.LogInfo("Using Initiative for Lassoed Creatures");
                        creature = guids;
                    }
                    else
                    {
                        creature = new CreatureGuid[] { BoardSessionManager.GetLastSelectedCreatureGuid(thing.ClientId) };
                    }

                    // Remove any default values from the creature list
                    creature = creature.Where(c => c != default).ToArray();

                    // Check if a creature is selected
                    if (creature.Count() == 0)
                    {
                        AutoInitiativePlugin.logger.LogInfo("No Creature Selected");
                        return;
                    }

                    // Set the initiative of the creature
                    foreach(CreatureGuid c in creature)
                    {
                        AutoInitiativePlugin.logger.LogInfo("Creature " + c + " rolled " + initiativeResult);
                        InitUtils.Initiatives[c] = (initiativeResult, initiativeModifier);
                    }
                    
                    InitUtils.SetInitiatives();
                }
            }
        }
    }
}
