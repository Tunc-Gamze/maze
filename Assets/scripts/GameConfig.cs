using System;
using System.Collections.Generic;
using UnityEngine;

namespace RunnerGame
{
    public enum LevelSource { Authored = 0, Generated = 1 }

    [Serializable]
    public sealed class LevelDefinition
    {
        public string id;
        public string title;
        public LevelSource source;
        public TextAsset map;
        public MazeGenerationSettings generation = new MazeGenerationSettings();

        public LevelLayout CreateLayout()
        {
            switch (source)
            {
                case LevelSource.Authored:
                    if (map == null) throw new InvalidOperationException("Authored level needs a map: " + id);
                    return new LevelLayout(map.text);
                case LevelSource.Generated:
                    return MazeGenerator.Generate(generation);
                default:
                    throw new InvalidOperationException("Unknown level source: " + id);
            }
        }
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
                if (level == null || string.IsNullOrEmpty(level.id) || !ids.Add(level.id))
                    throw new InvalidOperationException("Each level needs a unique stable ID.");
                try { level.CreateLayout(); }
                catch (Exception error) { throw new InvalidOperationException("Invalid level '" + level.id + "': " + error.Message, error); }
            }
        }
    }
}
