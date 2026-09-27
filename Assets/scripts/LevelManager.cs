using UnityEngine;

namespace RunnerGame
{
    public sealed class LevelManager : MonoBehaviour
    {
        public PlayerMotor Player { get; private set; }
        public Vector3 Spawn { get; private set; }
        public LevelLayout Layout { get; private set; }
        public MazeBuilder Builder { get; private set; }
        public CoinSpawner Coins { get; private set; }

        public void Load(GameConfig config, int index, GameSession session)
        {
            Layout = config.levels[index].CreateLayout();
            Builder = new GameObject("Maze").AddComponent<MazeBuilder>();
            Builder.transform.SetParent(transform, false);
            Builder.Build(Layout, config);
            Spawn = Builder.Position(Layout.Start, Layout);
            Player = Instantiate(config.playerPrefab, Spawn, Quaternion.identity).GetComponent<PlayerMotor>();
            Player.gameObject.AddComponent<PlayerRespawn>().Initialize(session, Builder.Bounds);
            Coins = gameObject.AddComponent<CoinSpawner>();
            Coins.Spawn(Layout, Builder, config, session, config.levels[index].coins);

            GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            goal.name = "Goal";
            goal.transform.position = Builder.Position(Layout.Goal, Layout, 0.15f);
            goal.transform.localScale = new Vector3(0.85f, 0.15f, 0.85f);
            goal.GetComponent<Renderer>().sharedMaterial = config.goalMaterial;
            // A separate trigger volume extends above the visible goal pad.
            Collider original = goal.GetComponent<Collider>();
            original.enabled = false;
            Destroy(original);
            GameObject trigger = new GameObject("Goal Trigger");
            trigger.transform.position = Builder.Position(Layout.Goal, Layout, 0.4f);
            trigger.transform.SetParent(transform, true);
            BoxCollider box = trigger.AddComponent<BoxCollider>();
            box.size = new Vector3(0.85f, 0.8f, 0.85f);
            box.isTrigger = true;
            trigger.AddComponent<GoalTrigger>().Initialize(session);

            var view = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(MazeCamera));
            view.tag = "MainCamera";
            var camera = view.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f);
            view.GetComponent<MazeCamera>().Configure(Builder.Bounds);
            var light = new GameObject("Sun").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(55f, -25f, 0f);
            RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.75f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        }
    }
}
