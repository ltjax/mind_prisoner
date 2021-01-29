using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static readonly float ROOM_Y = 6.0f;
    private static readonly float ROOM_HEIGHT = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        var camera = GetComponent<Camera>();

        float ratio = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
        float height = ROOM_Y * 0.5f / ratio + ROOM_HEIGHT;

        var position = transform.position;

        camera.farClipPlane = height + 1.0f;
        transform.position = new Vector3(position.x, position.y, -height);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
