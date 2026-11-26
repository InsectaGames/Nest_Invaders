using System.Collections.Generic;
using UnityEngine;

public class GathererPathFinder : MonoBehaviour
{
    [Header("Área de navegación 2D")]
    [SerializeField] private Vector2 center = new Vector2(-1.09f, 2f);
    [SerializeField] private Vector2 size = new Vector2(36.62f, 10f);
    [SerializeField] private float cellSize = 0.25f;

    [Header("Obstáculos")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private float gathererRadius = 0.5f;
    [SerializeField] private float safetyMargin = 0.15f;

    private bool[,] walkable;
    private int width;
    private int height;
    private Vector2 origin;
    private bool initialized;

    private struct HeapItem
    {
        public int index;
        public float priority;

        public HeapItem(int index, float priority)
        {
            this.index = index;
            this.priority = priority;
        }
    }

    private class MinHeap
    {
        private readonly List<HeapItem> items = new();

        public int Count => items.Count;

        public void Push(int index, float priority)
        {
            items.Add(new HeapItem(index, priority));
            int i = items.Count - 1;

            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (items[parent].priority <= items[i].priority)
                    break;

                (items[parent], items[i]) = (items[i], items[parent]);
                i = parent;
            }
        }

        public HeapItem Pop()
        {
            HeapItem result = items[0];
            int last = items.Count - 1;

            items[0] = items[last];
            items.RemoveAt(last);

            int i = 0;

            while (true)
            {
                int left = i * 2 + 1;
                int right = left + 1;
                int smallest = i;

                if (left < items.Count &&
                    items[left].priority < items[smallest].priority)
                    smallest = left;

                if (right < items.Count &&
                    items[right].priority < items[smallest].priority)
                    smallest = right;

                if (smallest == i)
                    break;

                (items[i], items[smallest]) = (items[smallest], items[i]);
                i = smallest;
            }

            return result;
        }
    }

    public void BuildGrid()
    {
        width = Mathf.CeilToInt(size.x / cellSize);
        height = Mathf.CeilToInt(size.y / cellSize);
        origin = center - size * 0.5f;

        walkable = new bool[width, height];

        float clearance = gathererRadius + safetyMargin;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = CellToWorld(x, y);

                Collider2D obstacle = Physics2D.OverlapCircle(
                    position, clearance, obstacleLayers);

                walkable[x, y] = obstacle == null;
            }
        }

        initialized = true;
    }

    public bool FindPath(Vector2 start, Vector2 target, out Vector2[] path)
    {
        path = null;

        if (!initialized)
            return false;

        if (!FindNearestWalkable(start, out int startIndex) || !FindNearestWalkable(target, out int targetIndex))
            return false;

        if (startIndex == targetIndex)
        {
            path = new[] { CellToWorld(startIndex % width, startIndex / width) };
            return true;
        }

        int total = width * height;

        float[] gScore = new float[total];
        int[] parents = new int[total];
        bool[] closed = new bool[total];

        System.Array.Fill(gScore, float.PositiveInfinity);
        System.Array.Fill(parents, -1);

        MinHeap open = new MinHeap();

        gScore[startIndex] = 0f;
        open.Push(startIndex, Heuristic(startIndex, targetIndex));

        int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
        int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };

        while (open.Count > 0)
        {
            int current = open.Pop().index;

            if (closed[current])
                continue;

            if (current == targetIndex)
            {
                path = ReconstructPath(parents, startIndex, targetIndex);
                return path != null;
            }

            closed[current] = true;

            int cx = current % width;
            int cy = current / width;

            for (int i = 0; i < 8; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];

                if (!IsWalkable(nx, ny))
                    continue;

                // Impedir que atraviese esquinas entre dos obstáculos.
                if (i >= 4 && (!IsWalkable(cx + dx[i], cy) || !IsWalkable(cx, cy + dy[i])))
                    continue;

                int next = ny * width + nx;

                if (closed[next])
                    continue;

                float cost = i < 4 ? 1f : 1.4142f;
                float tentativeG = gScore[current] + cost;

                if (tentativeG >= gScore[next])
                    continue;

                parents[next] = current;
                gScore[next] = tentativeG;

                open.Push(next, tentativeG + Heuristic(next, targetIndex));
            }
        }

        return false;
    }

    private bool FindNearestWalkable(Vector2 position, out int result)
    {
        result = -1;
        float bestDistance = float.PositiveInfinity;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!walkable[x, y])
                    continue;

                float distance = (CellToWorld(x, y) - position).sqrMagnitude;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    result = y * width + x;
                }
            }
        }

        // No buscar destinos a más de 2 unidades de la posición solicitada.
        return result >= 0 && bestDistance <= 4f;
    }

    private Vector2[] ReconstructPath(int[] parents, int start, int target)
    {
        List<Vector2> result = new List<Vector2>();
        int current = target;

        while (current != start)
        {
            if (current < 0)
                return null;

            result.Add(CellToWorld(current % width, current / width));
            current = parents[current];
        }

        result.Reverse();
        return result.ToArray();
    }

    private float Heuristic(int a, int b)
    {
        int dx = Mathf.Abs(a % width - b % width);
        int dy = Mathf.Abs(a / width - b / width);

        return Mathf.Max(dx, dy) + 0.4142f * Mathf.Min(dx, dy);
    }

    private bool IsWalkable(int x, int y) => x >= 0 && x < width && y >= 0 && y < height && walkable[x, y];

    private Vector2 CellToWorld(int x, int y) => origin + new Vector2((x + 0.5f) * cellSize,(y + 0.5f) * cellSize);

    private void OnDrawGizmos()
    {
        if (walkable == null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);
            return;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gizmos.color = walkable[x, y] ? new Color(0f, 1f, 0f, 0.2f) : new Color(1f, 0f, 0f, 0.2f);

                Vector2 position = origin + new Vector2((x + 0.5f) * cellSize, (y + 0.5f) * cellSize);
                Gizmos.DrawCube(position, Vector3.one * cellSize * 0.9f);
            }
        }
    }
}