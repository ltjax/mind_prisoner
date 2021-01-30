using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConfiguredRoom
{
    public ConfiguredRoom(Transform room)
    {
        doorElement = new Transform[4];
    }

    public Transform room;
    public Transform[] doorElement;
}

public class RoomManager : MonoBehaviour
{
    private static readonly Vector2 ROOM_SIZE = new Vector2(8.0f, 6.0f);

    // This must be LURD order
    private static readonly Vector2Int[] NEIGHBORS = new Vector2Int[]
    {
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
    };

    public Transform room;
    public Transform blockW;
    public Transform blockN;
    public Transform blockE;
    public Transform blockS;
    public Grid grid;

    private IReadOnlyList<Transform> BlockList
    {
        get
        {
            return new Transform[] { blockW, blockN, blockE, blockS };
        }
    }

    private readonly Dictionary<Vector2Int, ConfiguredRoom> active = new Dictionary<Vector2Int, ConfiguredRoom>(); 
    private int roomId=1;
    private int elementId=1;

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

    void AddRoom(Vector2Int cell)
    {
        var position = grid.CellToLocal(new Vector3Int(cell.x, cell.y, 0));

        var instance = Instantiate(room);
        instance.name = string.Format("Room ({0},{1}) / #{2}", cell.x, cell.y, roomId++);
        instance.position = position;
        instance.rotation = Quaternion.AngleAxis(-90.0f, new Vector3(1.0f, 0.0f, 0.0f));

        var result = new ConfiguredRoom(instance);

        active.Add(cell, result);
    }

    public void RemoveRoom(Vector2Int toDelete, Vector2Int floodStart)
    {
        var visited = new HashSet<Vector2Int>();
        var todo = new List<Vector2Int>{ floodStart };

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

        var deletedRooms = active.Where(x => !visited.Contains(x.Key));
        foreach (var toDespawn in deletedRooms)
        {
            GameObject.Destroy(toDespawn.Value.room.gameObject);
            foreach (var element in toDespawn.Value.doorElement)
            {
                if (element == null)
                {
                    continue;
                }
                GameObject.Destroy(element);
            }                
        }
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
                
                if (each.Value.doorElement[i] == null && !hasNeighbor)
                {
                    var instance = Instantiate(blockList[i]);
                    var position = grid.CellToLocal(new Vector3Int(cell.x, cell.y, 0));
                    instance.position = position;
                    instance.rotation = Quaternion.AngleAxis(-90.0f, new Vector3(1.0f, 0.0f, 0.0f));
                    instance.name = string.Format("Block ({0},{1}) d{2} / #{3}", cell.x, cell.y, i, elementId++);
                }
            }
        }
    }

    void Awake() {
        if(Debug.isDebugBuild) {
            Random.InitState(1234567);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(room != null);
        Debug.Assert(grid != null);

        AddRoom(new Vector2Int(0, 0));

        for (int i = 0; i < 10; ++i)
        {
            var border = BorderFor(active);
            int index = Random.Range(0, border.Count);
            AddRoom(border[index]);
        }

        UpdateTransitions(active.Select(x => x.Key));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
