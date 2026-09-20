using UnityEngine;

namespace RunnerGame
{
    public sealed class MazeBuilder : MonoBehaviour
    {
        public const float CellSize = 1.4f;
        public Bounds Bounds { get; private set; }
        public Vector3 Position(Cell cell, LevelLayout map, float height = 0.35f) =>
            new Vector3((cell.X - (map.Width - 1) * 0.5f) * CellSize, height,
                ((map.Height - 1) * 0.5f - cell.Y) * CellSize);

        public void Build(LevelLayout map, GameConfig config)
        {
            float width = map.Width * CellSize, depth = map.Height * CellSize;
            Bounds = new Bounds(new Vector3(0f, 0.6f, 0f), new Vector3(width, 1.6f, depth));
            GameObject floor = Block("Floor", new Vector3(0f, -0.15f, 0f), new Vector3(width, 0.3f, depth), config.floorMaterial);
            floor.tag = "Ground";
            for (int y = 0; y < map.Height; y++)
            for (int x = 0; x < map.Width; x++)
                if (map.IsWall(x, y))
                    Block("Wall " + x + "," + y, Position(new Cell(x, y), map, 0.6f),
                        new Vector3(CellSize, 1.2f, CellSize), config.wallMaterial);
        }

        private GameObject Block(string label, Vector3 position, Vector3 scale, Material material)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = label;
            item.transform.SetParent(transform, false);
            item.transform.position = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            return item;
        }
    }
}
