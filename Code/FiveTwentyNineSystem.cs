// <copyright file="FiveTwentyNineSystem.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FiveTwentyNineTiles
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Prefabs;
    using Game.Serialization;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// The 529 tile mod system.
    /// </summary>
    internal sealed partial class FiveTwentyNineSystem : GameSystemBase, IPostDeserialize
    {
        private ILog _log;

        // Query to find milestones.
        private EntityQuery _milestoneQuery;

        // Query to find map tiles.
        private EntityQuery _mapTileQuery;

        // Query to find locked features.
        private EntityQuery _featureQuery;

        /// <summary>
        /// Called by the game in post-deserialization.
        /// </summary>
        /// <param name="context">Game context.</param>
        public void PostDeserialize(Context context)
        {
            // Unlock all tiles, if that's what we're doing.
            if (Mod.Instance.ActiveSettings.UnlockAll)
            {
                _log.Info("unlocking all tiles");
                EntityManager.RemoveComponent<Native>(_mapTileQuery.ToEntityArray(Allocator.Temp));

                return;
            }

            // Everything else requires a new game.
            if (context.purpose != Purpose.NewGame)
            {
                _log.Info("not starting a new game");
                return;
            }

            // Unlock extra tiles at start if that's what we're doing.
            if (Mod.Instance.ActiveSettings.ExtraTilesAtStart)
            {
                _log.Info("allocating extra tiles to start");

                // Unlock map tile purchasing feature.
                PrefabSystem prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
                foreach (Entity entity in _featureQuery.ToEntityArray(Allocator.Temp))
                {
                    if (EntityManager.TryGetComponent(entity, out PrefabData prefabData) && prefabSystem.GetPrefab<PrefabBase>(prefabData) is PrefabBase prefab)
                    {
                        // Looking for map tiles feature.
                        if (prefab.name.Equals("Map Tiles"))
                        {
                            // Remove locking.
                            EntityManager.RemoveComponent<Locked>(entity);
                            EntityManager.RemoveComponent<UnlockRequirement>(entity);

                            // Create new milestone entity with initial unlocked tile count.
                            EntityManager.AddComponentData(EntityManager.CreateEntity(), new MilestoneData { m_MapTiles = 88 });

                            // Done.
                            return;
                        }
                    }
                }

                // If we got here, something went wrong.
                _log.Error("error unlocking initial map tile limit");
            }

            // Otherwise, assign extra tiles to milestones, if that's what we're doing.
            else if (Mod.Instance.ActiveSettings.AssignToMilestones)
            {
                _log.Info("updating milestones");

                foreach (Entity entity in _milestoneQuery.ToEntityArray(Allocator.Temp))
                {
                    if (EntityManager.TryGetComponent(entity, out MilestoneData milestone))
                    {
                        UpdateMilestone(ref milestone);
                        EntityManager.SetComponentData(entity, milestone);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the system is created.
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            // Set log.
            _log = Mod.Instance.Log;

            // Initialize queries.
            _milestoneQuery = GetEntityQuery(ComponentType.ReadWrite<MilestoneData>());
            _mapTileQuery = GetEntityQuery(ComponentType.ReadOnly<MapTile>());
            _featureQuery = GetEntityQuery(ComponentType.ReadOnly<FeatureData>(), ComponentType.ReadOnly<PrefabData>(), ComponentType.ReadWrite<Locked>());
            RequireForUpdate(_milestoneQuery);
            RequireForUpdate(_mapTileQuery);
        }

        /// <summary>
        /// Called every update.
        /// </summary>
        protected override void OnUpdate()
        {
        }

        /// <summary>
        /// Updates a given milestone to increase the number of unlockable map tiles.
        /// </summary>
        /// <param name="milestone">Milestone to alter.</param>
        private void UpdateMilestone(ref MilestoneData milestone)
        {
            switch (milestone.m_MapTiles)
            {
                case 3:
                    milestone.m_MapTiles = 4;
                    break;
                case 4:
                    milestone.m_MapTiles = 5;
                    break;
                case 5:
                    milestone.m_MapTiles = 7;
                    break;
                case 6:
                    milestone.m_MapTiles = 7;
                    break;
                case 7:
                    milestone.m_MapTiles = 8;
                    break;
                case 8:
                    milestone.m_MapTiles = 10;
                    break;
                case 9:
                    milestone.m_MapTiles = 11;
                    break;
                case 10:
                    milestone.m_MapTiles = 12;
                    break;
                case 12:
                    milestone.m_MapTiles = 14;
                    break;
                case 15:
                    milestone.m_MapTiles = 18;
                    break;
                case 18:
                    milestone.m_MapTiles = 22;
                    break;
                case 21:
                    milestone.m_MapTiles = 25;
                    break;
                case 24:
                    milestone.m_MapTiles = 29;
                    break;
                case 28:
                    milestone.m_MapTiles = 34;
                    break;
                case 32:
                    milestone.m_MapTiles = 38;
                    break;
                case 36:
                    milestone.m_MapTiles = 43;
                    break;
                case 41:
                    milestone.m_MapTiles = 49;
                    break;
                case 46:
                    milestone.m_MapTiles = 55;
                    break;
                case 51:
                    milestone.m_MapTiles = 61;
                    break;
                case 56:
                    milestone.m_MapTiles = 68;
                    break;
            }
        }
    }
}
