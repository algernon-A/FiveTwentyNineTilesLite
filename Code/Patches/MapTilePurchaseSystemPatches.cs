// <copyright file="MapTilePurchaseSystemPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Game.Simulation;
    using HarmonyLib;
    using Unity.Mathematics;

    /// <summary>
    /// Harmony patches for <see cref="MapTilePurchaseSystem"/> to implement per-tile cost limits.
    /// </summary>
    [HarmonyPatch(typeof(MapTilePurchaseSystem))]
    internal static class MapTilePurchaseSystemPatches
    {
        /// <summary>
        /// Harmony transpiler for <c>MapTilePurchaseSystem.UpdateStatus</c> to cap the cost for new tiles beyond 441.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateStatus")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateStatusTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Mod.Instance.Log.Info("transpiling " + original.DeclaringType + '.' + original.Name);

            // Lower bounds check for free first nine tiles.
            FieldInfo m_Cost = AccessTools.Field(typeof(MapTilePurchaseSystem), "m_Cost");
            bool firstCost = false;

            // Parse instructions.
            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Look for ldloc.s 5 followed by add (only instance in target).
                if (instruction.opcode == OpCodes.Ldloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 5)
                {
                    Mod.Instance.Log.Debug("found ldloc.s 5");
                    yield return instruction;

                    // Check for following add.
                    instructionEnumerator.MoveNext();
                    instruction = instructionEnumerator.Current;
                    if (instruction.opcode == OpCodes.Add)
                    {
                        Mod.Instance.Log.Debug("found add");
                        yield return instruction;

                        // Insert call to math.min(x, 441).
                        yield return new CodeInstruction(OpCodes.Ldc_I4, 441);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(math), nameof(math.min), new Type[] { typeof(int), typeof(int) }));

                        continue;
                    }
                }

                // Otherwise, looking for second store to MapTilePurchaseSystem.m_Cost.
                else if (instruction.StoresField(m_Cost))
                {
                    if (!firstCost)
                    {
                        firstCost = true;
                    }
                    else
                    {
                        // Insert call to our custom method.
                        Mod.Instance.Log.Debug("found second m_Cost store");
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MapTilePurchaseSystemPatches), nameof(CheckFreeTiles)));
                    }
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Checks to see if this tile is one of the first nine; if so, the cost is free.
        /// </summary>
        /// <param name="cost">Calculated tile cost.</param>
        /// <param name="numTiles">Number of selected tiles processed this update.</param>
        /// <param name="ownedTiles">Number of already owned tiles.</param>
        /// <returns>0 if this tile is one of the first nine, otherwise returns the calculated cost.</returns>
        private static float CheckFreeTiles(float cost, int numTiles, int ownedTiles)
        {
            // Check tile count.
            if (numTiles + ownedTiles <= 9)
            {
                // First nine tiles - return free tile.
                return 0f;
            }

            // Otherwise, not free - return the calculated cost.
            return cost;
        }
    }
}
