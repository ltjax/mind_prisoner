using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRoom : MonoBehaviour
{
    private static readonly Vector2 ROOM_SIZE = new Vector2(8.0f, 6.0f);
    private static readonly Vector2Int[] NEIGHBORS = new Vector2Int[]
    {
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(0, 1),
    };

    public Transform room;
    public Dictionary<Vector2Int, Transform> active = new Dictionary<Vector2Int, Transform>(); 
    private int roomId=1;

    List<Vector2Int> BorderFor(Dictionary<Vector2Int, Transform> actives)
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
        var position = ROOM_SIZE * cell;

        var instance = GameObject.Instantiate(room);
        instance.name = string.Format("Room ({0},{1}) / {2}", cell.x, cell.y, roomId++);
        instance.position = position;
        instance.rotation = Quaternion.AngleAxis(-90.0f, new Vector3(1.0f, 0.0f, 0.0f));

        active.Add(cell, instance);
    }

    public void RemoveRoom(Vector2Int toDelete, Vector2Int floodStart)
    {
        var visited = new HashSet<Vector2Int>();
        var todo = new List<Vector2Int>{ floodStart };

        while (todo.Count != 0)
        {
            var current = todo.Last();
            todo.RemoveAt(todo.Count - 1);

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
            GameObject.Destroy(toDespawn.Value.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(room != null);

        if(Debug.isDebugBuild) {
            Random.InitState(1234567);
        }

        AddRoom(new Vector2Int(0, 0));

        for (int i = 0; i < 10; ++i)
        {
            var border = BorderFor(active);
            int index = Random.Range(0, border.Count);
            AddRoom(border[index]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
