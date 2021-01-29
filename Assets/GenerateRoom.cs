using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRoom : MonoBehaviour
{
    public Transform room;

    // Start is called before the first frame update
    void Start()
    {
        Vector2 roomSize = room.localScale;

        var position = new Vector2(0, 0);
        for (int i = 0; i < 3; ++i)
        {
            position += roomSize;
            var instance = GameObject.Instantiate(room);
            instance.position = position;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
