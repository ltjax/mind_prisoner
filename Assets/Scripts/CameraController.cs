using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float ROOM_Y = 6.0f;
    private const float ROOM_HEIGHT = 3.0f;
    private const float MOVE_SPEED = 10.0f;

    private IEnumerable<(Vector2Int, KeyCode)> OFFSET_AND_KEYCODE = new (Vector2Int, KeyCode)[]
    {
            (new Vector2Int(-1, 0), KeyCode.A),
            (new Vector2Int(1, 0), KeyCode.D),
            (new Vector2Int(0, -1), KeyCode.S),
            (new Vector2Int(0, 1), KeyCode.W)
    };

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
        foreach (var (offset, keycode) in OFFSET_AND_KEYCODE)
        {
            if (Input.GetKeyDown(keycode))
            {
                var current = grid.LocalToCell(transform.position);
                MoveToCell((Vector2Int)current + offset);
                break;
            }
        }
    }

    private (Vector2, bool) SmoothMoveTo(Vector2 target)
    {
        var current = (Vector2)transform.position;
        var difference = target - current;
        var l = difference.magnitude;
        var speed = Time.deltaTime * MOVE_SPEED;
        if (l > speed)
        {
            difference *= speed / l;
            return (difference, false);
        }

        return (difference, true);
    }


    public void MoveToCell(Vector2Int cell)
    {
        var center = grid.CellToWorld(new Vector3Int(cell.x, cell.y, 0)) + grid.cellSize * 0.5f;
        target = center;
    }

    void JumpTo(Vector2 TargetPos) {
        transform.position = new Vector3(TargetPos.x, TargetPos.y, transform.position.z);
    }
}
