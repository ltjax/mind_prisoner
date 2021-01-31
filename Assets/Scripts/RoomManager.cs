using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ConfiguredRoom
{
    public ConfiguredRoom(Transform room)
    {
        this.room = room;
        doorElement = new Transform[4];
    }

    public Transform room;
    public Transform[] doorElement;
    public Transform finish;

    public bool IsFinish => finish != null;
}

public class RoomManager : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Up,
        Right,
        Down
    }

    // This must be LURD order
    private static readonly Vector2Int[] NEIGHBORS = new Vector2Int[]
    {
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
    };

    public Transform room;
    public Transform enemy;
    public Transform blockW;
    public Transform blockN;
    public Transform blockE;
    public Transform blockS;
    public Transform finish;
    public GameObject ui;
    private Grid grid;
    private Grid enemyGrid;

    private IReadOnlyList<Transform> BlockList
    {
        get
        {
            return new Transform[] { blockW, blockN, blockE, blockS };
        }
    }

    private readonly Dictionary<Vector2Int, ConfiguredRoom> active = new Dictionary<Vector2Int, ConfiguredRoom>();
    private int roomId = 1;
    private int elementId = 1;

    List<Vector2Int> BorderFor(Dictionary<Vector2Int, ConfiguredRoom> actives)
    {
        var border = new HashSet<Vector2Int>();
        foreach (var active in actives)
        {
            foreach (var offset in NEIGHBORS)
            {
                var next = active.Key + offset;
                if (!actives.ContainsKey(next))
                {
                    border.Add(next);
                }
            }
        }
        return border.ToList();
    }

    /// <summary>
    /// Get the associated "door" with a position
    /// </summary>
    private (Vector2Int, Direction)? CellAndDirectionFor(Vector2 position, float maxDistance)
    {
        var cell = grid.WorldToCell(position);

        var offsetToDirection = new List<(Vector2 offset, Direction direction)>()
        {
            (new Vector2(0.0f, 0.5f), Direction.Left),
            (new Vector2(0.5f, 1.0f), Direction.Up),
            (new Vector2(1.0f, 0.5f), Direction.Right),
            (new Vector2(0.5f, 0.0f), Direction.Down),
        };

        float maxDistanceSqr = maxDistance * maxDistance;

        foreach (var (offset, direction) in offsetToDirection)
        {
            // The door position in cell-coordinates
            var reference = (Vector2Int)cell + offset;
            // The door position in world coordinates
            var point = (Vector2)grid.transform.TransformPoint(grid.CellToLocalInterpolated(reference));

            if ((point - position).sqrMagnitude < maxDistanceSqr)
                return ((Vector2Int)cell, direction);
        }

        return null;
    }

    public void ClosePathAt(Vector2 position)
    {
        var info = CellAndDirectionFor(position, 2.0f);
        if (info.HasValue)
        {
            var (cell, direction) = info.Value;
            ClosePathInDirection(cell, direction);
            ui.SendMessage("LetGo");
        }
    }

    public void ClosePathInDirection(Vector2Int currentCell, Direction direction)
    {
        var deleted = DeleteComponent(currentCell + NEIGHBORS[(int)direction], currentCell);
        var created = new List<Vector2Int>();

        foreach (var _ in deleted)
        {
            var border = BorderFor(active);

            // Pick one from the farther half
            var borderAndDistance = border
                .Select(current =>
                {
                    return (cell: current, distance: deleted.Min(other => (other - current).sqrMagnitude));
                })
                .OrderByDescending(x => x.distance)
                .ToList();

            var N = System.Math.Max(1, border.Count / 2);
            var pick = Random.Range(0, N);
            var newCell = borderAndDistance[pick].cell;

            created.Add(newCell);
            CreateRoom(newCell);
        }

        UpdateTransitions(deleted.Concat(created));
    }


    bool ShouldSpawnFinish()
    {
        const int THRESHOLD = 15;
        if (roomId <= THRESHOLD)
            return false;

        // Don't spawn twice
        foreach (var room in active)
        {
            if (room.Value.IsFinish)
                return false;
        }

        // Probability goes to 1 for later rooms
        var more = roomId - THRESHOLD;
        float Probability = 1.0f - 1.0f / (more * 0.3f);
        return Random.value <= Probability;
    }

    void SpawnFinish(Vector3 position, ConfiguredRoom room)
    {
        Debug.Log("Finish spawned!");
        var spawned = Instantiate(this.finish);
        room.finish = spawned;
        spawned.position = position;        
    }

    void CreateRoom(Vector2Int cell)
    {
        var position = grid.CellToLocal(new Vector3Int(cell.x, cell.y, 0));

        var instance = Instantiate(room);
        instance.name = string.Format("Room ({0},{1}) / #{2}", cell.x, cell.y, roomId++);
        instance.position = position;

        var result = new ConfiguredRoom(instance);

        // Either spawn the finish, or just enemies
        if (ShouldSpawnFinish())
        {
            SpawnFinish(position, result);
        }
        else
        {
            PopulateRoomWithEnemies(result);
        }

        active.Add(cell, result);
    }

    public void RemoveRoom(Vector2Int toDelete, Vector2Int floodStart)
    {
        var deletedRooms = DeleteComponent(toDelete, floodStart);
        UpdateTransitions(deletedRooms);
    }

    private IEnumerable<Vector2Int> DeleteComponent(Vector2Int toDelete, Vector2Int floodStart)
    {
        var visited = new HashSet<Vector2Int>();
        var todo = new List<Vector2Int> { floodStart };

        while (todo.Count != 0)
        {
            var current = todo.TakeLast();

            if (!active.ContainsKey(current))
                continue;

            visited.Add(current);

            foreach (var each in NEIGHBORS)
            {
                var next = current + each;
                if (!visited.Contains(next) && next != toDelete)
                    todo.Add(next);
            }
        }

        //Debug.Log(visited);

        var deletedRooms = active
            .Where(x => !visited.Contains(x.Key))
            .ToList();

        foreach (var toDespawn in deletedRooms)
        {
            Destroy(toDespawn.Value.room.gameObject);
            foreach (var element in toDespawn.Value.doorElement)
            {
                if (element == null)
                {
                    continue;
                }
                Destroy(element.gameObject);
            }
            active.Remove(toDespawn.Key);
        }

        return deletedRooms
            .Select(x => x.Key);
    }

    public ISet<Vector2Int> Hull(IEnumerable<Vector2Int> original)
    {
        var set = new HashSet<Vector2Int>();

        foreach (var cell in original)
        {
            foreach (var n in NEIGHBORS)
            {
                set.Add(cell + n);
            }
            set.Add(cell);
        }

        return set;
    }

    public void UpdateTransitions(IEnumerable<Vector2Int> changed)
    {
        // Update changed and all neighbors
        var hull = Hull(changed);
        var relevant = active
            .Where(x => hull.Contains(x.Key))
            .ToList();

        var blockList = BlockList;

        foreach (var each in relevant)
        {
            var cell = each.Key;
            for (int i = 0; i < 4; ++i)
            {
                bool hasNeighbor = active.ContainsKey(each.Key + NEIGHBORS[i]);
                bool hadNeighborBefore = each.Value.doorElement[i] == null;
                if (hadNeighborBefore == hasNeighbor)
                    continue;

                if (hasNeighbor)
                {
                    Destroy(each.Value.doorElement[i].gameObject);
                }
                else
                {
                    var instance = Instantiate(blockList[i]);
                    var position = grid.CellToLocal(new Vector3Int(cell.x, cell.y, 0));
                    instance.position = position;
                    instance.name = string.Format("Block ({0},{1}) d{2} / #{3}", cell.x, cell.y, i, elementId++);
                    each.Value.doorElement[i] = instance;
                }
            }
        }
    }

    void Awake()
    {
        if (Debug.isDebugBuild)
        {
            Random.InitState(1234567);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("GridGlobal")?.GetComponent<Grid>();
        enemyGrid = GameObject.FindGameObjectWithTag("Grid")?.GetComponent<Grid>();
        Debug.Assert(room != null);
        Debug.Assert(grid != null);
        Debug.Assert(enemyGrid != null);
        Debug.Assert(finish != null);
        Debug.Assert(BlockList.All(x => x != null));

        CreateRoom(new Vector2Int(0, 0));

        for (int i = 0; i < 10; ++i)
        {
            var border = BorderFor(active);
            int index = Random.Range(0, border.Count);
            CreateRoom(border[index]);
        }

        UpdateTransitions(active.Select(x => x.Key));
    }

    void PopulateRoomWithEnemies(ConfiguredRoom room)
    {
        // Pick a number from 2 to 5
        var enemyCount = Random.Range(2, 6);
        for (int i = 0; i < enemyCount; i++)
        {
            var nEnemy = Instantiate(enemy, GetRandomLocalEnemyPosition(room.room), Quaternion.identity, room.room);
            nEnemy.name = "Enemy Blob [" + Random.Range(10000, 100000) + "]";
            var nEnemyControl = nEnemy.GetComponent<EnemyController>();
            nEnemyControl.Speed = nEnemyControl.InitSpeed = Random.Range(0.2f, 1f);
            nEnemyControl.HitPoints = nEnemyControl.MaxHitPoints = Random.Range(3, 7);
        }
    }

    Vector3 GetRandomLocalEnemyPosition(Transform roomTransform)
    {
        var roomBounds = enemyGrid.GetBoundsLocal(enemyGrid.WorldToCell(roomTransform.position));
        return new Vector3(roomTransform.position.x + (Random.value - 0.5f) * roomBounds.size.x,
                           roomTransform.position.y + (Random.value - 0.5f) * roomBounds.size.y,
                           -0.5f);
    }

    public bool TryGetRoomAt(Vector2Int GridCoord, out Transform Room)
    {
        if (active.ContainsKey(GridCoord))
        {
            Room = active[GridCoord].room;
            return true;
        }
        Room = null;
        return false;
    }

    public bool HasRoom(Vector2Int cell)
    {
        return active.ContainsKey(cell);
    }

    private void Update() {
        if(Input.GetKeyUp(KeyCode.Escape)) {
            Application.Quit();
        }
    }
}
