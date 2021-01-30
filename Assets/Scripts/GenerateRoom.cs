using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRoom : MonoBehaviour
{
    private static readonly Vector2 ROOM_SIZE = new Vector2(8.0f, 6.0f);
    public Transform room;
    public HashSet<Vector2Int> active = new HashSet<Vector2Int>(); // cell coordinates of existing rooms
    public HashSet<Vector2Int> border = new HashSet<Vector2Int>(); // All potential cells neighboring the exiting rooms

    void AddRoom(Vector2Int cell)
    {
        if (active.Contains(cell))
            return;

        border.Remove(cell);

        var neighbors = new Vector2Int[]
        {
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(0, 1),
        };

        active.Add(cell);
        foreach (var each in neighbors)
        {
            var neighborCell = each + cell;
            if (active.Contains(neighborCell))
                continue;

            border.Add(neighborCell);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(room != null);

        Vector2 roomSize = ROOM_SIZE;

        AddRoom(new Vector2Int(0, 0));

        for (int i = 0; i < 10; ++i)
        {
            int index = Random.Range(0, border.Count);
            AddRoom(border.ToArray()[index]);
        }

        foreach (var each in active)
        {
            var position = roomSize * each;
            var instance = GameObject.Instantiate(room);
            instance.position = position;
            instance.rotation = Quaternion.AngleAxis(-90.0f, new Vector3(1.0f, 0.0f, 0.0f));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
