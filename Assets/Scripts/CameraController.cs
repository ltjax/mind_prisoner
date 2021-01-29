using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static readonly float ROOM_Y = 6.0f;

    // Start is called before the first frame update
    void Start()
    { 
    }

    // Update is called once per frame
    void Update()
    {
        var camera = GetComponent<Camera>();

        float ratio = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
        float height = ROOM_Y * 0.5f / ratio;

        var position = transform.position;
        
        transform.position = new Vector3(position.x, position.y, -height);
    }
}
