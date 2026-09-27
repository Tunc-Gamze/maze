using System.Collections.Generic;
using UnityEngine;

namespace RunnerGame
{
    public sealed class CoinSpawner : MonoBehaviour
    {
        private readonly Dictionary<int, Coin> views = new Dictionary<int, Coin>();
        private GameSession session;
        private MazeBuilder builder;
        private LevelLayout map;
        private GameConfig config;
        private CoinSpawnSettings settings;
        public TimedCoinCycle Cycle { get; private set; }
        public bool IsTimed => Cycle != null;
        public int Budget { get; private set; }
        public int ActiveCount => views.Count;

        public void Spawn(LevelLayout layout, MazeBuilder maze, GameConfig gameConfig, GameSession owner, CoinSpawnSettings coinSettings)
        {
            map = layout; builder = maze; config = gameConfig; session = owner; settings = coinSettings;
            settings.Validate(map);
            Budget = settings.Budget(map);
            if (settings.mode == CoinBehavior.Timed)
            {
                Cycle = new TimedCoinCycle(map, settings);
                Cycle.CoinSpawned += entry => Create(entry.Id, entry.Cell);
                Cycle.CoinExpired += Remove;
            }
            else
                for (int i = 0; i < map.Coins.Count; i++) Create(i, map.Coins[i]);
        }

        private void Update()
        {
            if (Cycle == null || session == null) return;
            Cycle.Tick(Time.deltaTime, session.CoinTimeRunning, SafeToSpawn);
            foreach (var entry in Cycle.Active)
                if (views.TryGetValue(entry.Id, out Coin coin))
                    coin.SetLifetime((float)Cycle.Remaining(entry), settings.warningSeconds);
        }

        private bool SafeToSpawn(Cell cell)
        {
            Vector3 position = builder.Position(cell, map, 0.35f);
            Vector3 player = session.Player.transform.position;
            player.y = position.y;
            if ((position - player).sqrMagnitude < 1f) return false;
            // Includes dynamic blockers and other triggers; floor is below this sphere.
            return !Physics.CheckSphere(position, 0.22f, Physics.AllLayers, QueryTriggerInteraction.Collide);
        }

        private void Create(int id, Cell cell)
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coin.name = "Coin " + id;
            coin.transform.SetParent(transform, false);
            coin.transform.position = builder.Position(cell, map, 0.35f);
            coin.transform.localScale = Vector3.one * 0.36f;
            coin.GetComponent<Renderer>().sharedMaterial = config.coinMaterial;
            coin.GetComponent<SphereCollider>().isTrigger = true;
            Coin view = coin.AddComponent<Coin>();
            view.Initialize(session, id);
            views.Add(id, view);
        }

        public bool TryCollect(int id)
        {
            if (!views.ContainsKey(id) || (Cycle != null && !Cycle.TryCollect(id))) return false;
            Remove(id);
            return true;
        }

        private void Remove(int id)
        {
            if (!views.TryGetValue(id, out Coin view)) return;
            views.Remove(id);
            if (view == null) return;
            view.gameObject.SetActive(false);
            Destroy(view.gameObject);
        }
    }
}
