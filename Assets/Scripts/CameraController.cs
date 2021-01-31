using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float ROOM_Y = 6.0f;
    private const float ROOM_HEIGHT = 3.0f;
    private const float MOVE_SPEED = 10.0f;

    private Grid grid;
    private Vector2? target;

    void Awake() {
    // Caps the FPS when running in editor at a reasonable number
#if UNITY_EDITOR
     QualitySettings.vSyncCount = 0;  // VSync must be disabled
     Application.targetFrameRate = 45;
#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        var camera = GetComponent<Camera>();
        grid = GameObject.FindGameObjectWithTag("GridGlobal")?.GetComponent<Grid>();

        float ratio = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
        float height = ROOM_Y * 0.5f / ratio + ROOM_HEIGHT;

        var position = transform.position;

        camera.farClipPlane = height + 1.0f;
        transform.position = new Vector3(position.x, position.y, -height);
        
        // Force 4:3 aspect ratio
        var targetAspectRatio = 4f / 3f;
        if(targetAspectRatio < camera.aspect) {
            var diff = 1 - targetAspectRatio / camera.aspect;
            camera.rect = new Rect(diff / 2, 0, 1 - diff, 1);
        } else {
            var diff = 1 - camera.aspect / targetAspectRatio;
            camera.rect = new Rect(0, diff / 2, 1, 1 - diff);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (target != null)
        {
            var (move, ended) = SmoothMoveTo(target.Value);
            transform.Translate(move);
            if (ended)
            {
                target = null;
            }
        }
    }

    private void FixedUpdate()
    {
     
    }

    private (Vector2, bool) SmoothMoveTo(Vector2 target)
    {
        var difference = Vector2.Lerp(transform.position, target, Time.deltaTime * 6) - (Vector2)transform.position;
        return (difference, difference.magnitude < 0.001f);
    }

    public void MoveToCell(Vector2Int cell)
    {
        if(grid != null) {
            var center = grid.CellToWorld(new Vector3Int(cell.x, cell.y, 0)) + grid.cellSize * 0.5f;
            target = center;
        }
    }

    void JumpTo(Vector2 TargetPos) {
        transform.position = new Vector3(TargetPos.x, TargetPos.y, transform.position.z);
    }
}
