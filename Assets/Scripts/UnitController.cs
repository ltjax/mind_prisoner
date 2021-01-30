using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{

    public float CruisingSpeed;
    private SpriteRenderer MySprite;
    private Animator MyAnimator;
    private ProjectileManager projectileManager;

    public Grid MyGrid;
    public Vector3Int MyGridPos;

    private Camera MainCam;

    // Start is called before the first frame update
    void Start()
    {
        MySprite = GetComponent<SpriteRenderer>();
        MyAnimator = GetComponent<Animator>();
        projectileManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ProjectileManager>();
        MainCam = Camera.main;
    }

    void Update()
    {
    }

    private void SetAnimatorVelocity(Vector2 Vel)
    {
        if (MyAnimator != null)
        {
            MyAnimator.SetFloat("Velocity_Horizontal", Vel.x);
            MyAnimator.SetFloat("Velocity_Vertical", Vel.y);
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            var table = new List<(KeyCode key, Vector2Int offset)>
            {
                (KeyCode.LeftArrow, Vector2Int.left   ),
                (KeyCode.RightArrow, Vector2Int.right ),
                (KeyCode.UpArrow, Vector2Int.up       ),
                (KeyCode.DownArrow, Vector2Int.down   )
            };

            var roomManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GenerateRoom>();
            foreach (var (key, offset) in table)
            {
                if (Input.GetKeyDown(key))
                {
                    roomManager.RemoveRoom((Vector2Int)MyGridPos + offset, (Vector2Int)MyGridPos);
                    break;
                }
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }

        Vector2 InputMove = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * CruisingSpeed;
        if (InputMove.sqrMagnitude > 1e-5)
        {
            transform.Translate(InputMove * Time.deltaTime);
            SetAnimatorVelocity(InputMove);
        }
        else
        {
            SetAnimatorVelocity(Vector2.zero);
        }

        Vector3Int newGridPos = MyGrid.WorldToCell(transform.position);
        if (newGridPos != MyGridPos)
        {
            MyGridPos = newGridPos;
            MainCam.SendMessage(nameof(CameraController.MoveToCell), (Vector2Int)MyGridPos);
        }
    }

    void Fire()
    {
        projectileManager.Spawn(transform.position, Vector2.right);
    }
}
