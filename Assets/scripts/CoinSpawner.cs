using UnityEngine;

namespace RunnerGame
{
    public sealed class CoinSpawner : MonoBehaviour
    {
        public void Spawn(LevelLayout map, MazeBuilder builder, GameConfig config, GameSession session)
        {
            // Authored C cells are checked by LevelLayout's flood fill before spawning.
            for (int i = 0; i < map.Coins.Count; i++)
            {
                GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coin.name = "Coin " + i;
                coin.transform.SetParent(transform, false);
                coin.transform.position = builder.Position(map.Coins[i], map, 0.35f);
                coin.transform.localScale = Vector3.one * 0.36f;
                coin.GetComponent<Renderer>().sharedMaterial = config.coinMaterial;
                coin.GetComponent<SphereCollider>().isTrigger = true;
                coin.AddComponent<Coin>().Initialize(session, i);
            }
        }
    }
}
