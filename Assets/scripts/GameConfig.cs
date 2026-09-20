using System;
using System.Collections.Generic;
using UnityEngine;

namespace RunnerGame
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public string id;
        public string title;
        public TextAsset map;
    }

    [CreateAssetMenu(menuName = "Runner Game/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        public GameObject playerPrefab;
        public Material floorMaterial;
        public Material wallMaterial;
        public Material goalMaterial;
        public Material coinMaterial;
        public LevelDefinition[] levels;

        public void Validate()
        {
            if (playerPrefab == null || playerPrefab.GetComponent<PlayerMotor>() == null)
                throw new InvalidOperationException("GameConfig needs the Player prefab.");
            if (playerPrefab.GetComponent<Rigidbody>() == null || playerPrefab.GetComponent<PlayerInput>() == null ||
                playerPrefab.GetComponent<Collider>() == null)
                throw new InvalidOperationException("Player root needs its Rigidbody, input and collider.");
            if (floorMaterial == null || wallMaterial == null || goalMaterial == null || coinMaterial == null)
                throw new InvalidOperationException("GameConfig materials are missing.");
            if (levels == null || levels.Length == 0) throw new InvalidOperationException("No levels configured.");
            var ids = new HashSet<string>();
            foreach (LevelDefinition level in levels)
            {
                if (level == null || string.IsNullOrEmpty(level.id) || !ids.Add(level.id) || level.map == null)
                    throw new InvalidOperationException("Each level needs a unique stable ID and a map.");
                new LevelLayout(level.map.text);
            }
        }
    }
}
