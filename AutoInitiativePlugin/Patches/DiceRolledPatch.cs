using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoInitiative.Patches
{
    internal static class InitUtils
    {
        internal static bool EditIsOpen = false;
        internal static Dictionary<CreatureGuid, int> Initiatives = new Dictionary<CreatureGuid, int>();

        internal static void ClearInitiatives()
        {
            Initiatives.Clear();
            AutoInitiativePlugin.logger.LogInfo("Cleared Tracked Initiatives");
        }

        internal static void SetInitiatives()
        {
            CreatureGuid[] array = Initiatives
                .OrderByDescending(entry => entry.Value)
                .Select(entry => { AutoInitiativePlugin.logger.LogInfo($"{entry.Value}: {entry.Key}"); return entry.Key; }).ToArray();

            InitiativeManager.SetEditQueue(array);
        }
    }

    [HarmonyPatch(typeof(InitiativeManager), "RemoveCreatureFromEditQueue")]
    public class InitiativeManagerRemoveCreatureFromEditQueuePatch
    {
        static void Postfix(CreatureGuid creatureGuid)
        {
            if (InitUtils.Initiatives.ContainsKey(creatureGuid))
            {
                InitUtils.Initiatives.Remove(creatureGuid);
                AutoInitiativePlugin.logger.LogInfo($"Removed {creatureGuid} from Initiative");
            }
        }
    }

    [HarmonyPatch(typeof(InitiativeManager), "ReleaseCreature")]
    public class InitiativeManagerReleaseCreaturePatch
    {
        static void Postfix(CreatureGuid creature)
        {
            if (InitUtils.Initiatives.ContainsKey(creature))
            {
                InitUtils.Initiatives.Remove(creature);
                AutoInitiativePlugin.logger.LogInfo($"Removed {creature} from Initiative");
            }
        }
    }


    [HarmonyPatch(typeof(UI_InitativeManager), "ClearEdit")]
    public class InitiativeManagerClearPatch
    {
        static void Prefix()
        {
            InitUtils.ClearInitiatives();
            AutoInitiativePlugin.logger.LogInfo($"Initiatives cleared");
        }
    }

    [HarmonyPatch(typeof(UI_InitativeManager), "OpenEdit")]
    public class InitiativeManagerApplyEditPatch
    {
        static void Prefix(bool value)
        {
            InitUtils.EditIsOpen = value;
            AutoInitiativePlugin.logger.LogInfo($"Initiative Manager is open: {value}");
        }
    }

    [HarmonyPatch(typeof(DiceManager), "RPC_DiceResult")]
    public class ReceiveDiceRolledPatch
    {
        

        static void Postfix(bool isGmOnly, byte[] diceListData, PhotonMessageInfo msgInfo, BrSerialize.Reader ____reader)
        {
            if (LocalClient.IsInGmMode && InitUtils.EditIsOpen)
            {
                BrSerializeHelpers.DeserializeFromByteArray(____reader, diceListData, DiceManager.RollResults.Deserialize, out DiceManager.RollResults thing);

                bool flag = false;
                int num = 0;
                int num2 = 0;
                num2 = 0;
                
                foreach (DiceManager.RollResultsGroup resultsGroup in thing.ResultsGroups)
                {
                    num = 0;
                    if (resultsGroup.Name != null && Convert.ToString(resultsGroup.Name).Trim() != "")
                    {
                        if (resultsGroup.Name.Contains("Initiative", StringComparison.OrdinalIgnoreCase))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        continue;
                    }

                    DiceManager.RollOperand.Which which = resultsGroup.Result.Get(out DiceManager.RollResultsOperation operation, out DiceManager.RollResult result, out DiceManager.RollValue value);
                    if (which.HasFlag(DiceManager.RollOperand.Which.Operation))
                    {
                        num = 1;
                        try
                        {
                            num = ((operation.Operator != DiceManager.DiceOperator.Subtract) ? 1 : (-1));
                        }
                        catch (Exception)
                        {
                        }

                        if (operation.Operands != null)
                        {
                            foreach (DiceManager.RollOperand operand in operation.Operands)
                            {
                                operand.Get(out operation, out result, out value);
                                if (result.Kind.RegisteredName == "<unknown>")
                                {
                                    short value2 = value.Value;
                                    num2 += value.Value * num;
                                    continue;
                                }

                                foreach (short result2 in result.Results)
                                {
                                    num2 += result2;
                                }
                            }
                        }
                        else if (result.Results != null)
                        {
                            foreach (short result3 in result.Results)
                            {
                                num2 += result3;
                            }
                        }
                    }
                }

                if (flag)
                {
                    CreatureGuid[] creature = new CreatureGuid[0];

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

                    creature = creature.Where(c => c != default).ToArray();

                    if (creature.Count() == 0)
                    {
                        AutoInitiativePlugin.logger.LogInfo("No Creature Selected");
                        return;
                    }

                    foreach(CreatureGuid c in creature)
                    {
                        AutoInitiativePlugin.logger.LogInfo("Creature " + c + " rolled " + num2);
                        InitUtils.Initiatives[c] = num2;
                    }
                    
                    InitUtils.SetInitiatives();
                }
            }
        }
    }
}
