using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour {

    public const float NEGLIGIBLE = 1e-2f;

    public float CruisingSpeed;
    private SpriteRenderer MySprite;
    private Animator MyAnimator;
    private CharacterController MyBody;
    private ProjectileManager projectileManager;

    public PlayerHealthBar healthBar;

    private Grid MyGrid;
    public Vector3Int MyGridPos;
    private RoomManager RoomManager;

    private List<(KeyCode key, Vector2Int offset)> DirectionTable = new List<(KeyCode key, Vector2Int offset)> {
                (KeyCode.LeftArrow, Vector2Int.left   ),
                (KeyCode.RightArrow, Vector2Int.right ),
                (KeyCode.UpArrow, Vector2Int.up       ),
                (KeyCode.DownArrow, Vector2Int.down   ),
                (KeyCode.A, Vector2Int.left   ),
                (KeyCode.D, Vector2Int.right ),
                (KeyCode.W, Vector2Int.up       ),
                (KeyCode.S, Vector2Int.down   )
            };

    private Camera MainCam;

    private bool FreeCameraMode = false;
    private const KeyCode FreeCameraToggle = KeyCode.LeftShift;
    private Vector2Int CameraCellPos;

    // Start is called before the first frame update
    void Start() {
        MySprite = GetComponent<SpriteRenderer>();
        MyAnimator = GetComponent<Animator>();
        MyGrid = GameObject.FindGameObjectWithTag("GridGlobal")?.GetComponent<Grid>();
        projectileManager = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<ProjectileManager>();
        MainCam = Camera.main;
        PointCameraAtMe();
        MyBody = GetComponent<CharacterController>();
        RoomManager = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<RoomManager>();
        healthBar.SetHealth(10);

        StartCoroutine(AnimationControl());
    }

    void FixedUpdate() {

        if(Input.GetKeyDown(FreeCameraToggle)) {
            SendMessage("StartFreeCam");
        } else if(Input.GetKeyUp(FreeCameraToggle)) {
            SendMessage("StopFreeCam");
        }

        if(FreeCameraMode) {
            var newCamPos = CameraCellPos;
            foreach(var (key, offset) in DirectionTable) {
                if(Input.GetKeyDown(key)) {
                    newCamPos += offset;
                }
            }
            if(!RoomManager.TryGetRoomAt(newCamPos, out Transform r)) {
                SendMessage("PlayCantDo");
            } else {
                CameraCellPos = newCamPos;
                MainCam.SendMessage("MoveToCell", CameraCellPos);
            }

            if(Input.GetKeyUp(KeyCode.Delete)) {
                if(CameraCellPos == (Vector2Int)MyGridPos) {
                    SendMessage("PlayCantDo");
                } else {
                    RoomManager.RemoveRoom(CameraCellPos, (Vector2Int)MyGridPos);
                    SendMessage("DeleteRoom");
                    PointCameraAtMe();
                }
            }
        } else {
            Vector2 InputMove = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * CruisingSpeed;
            if(InputMove.magnitude > NEGLIGIBLE) {
                MyBody.Move(InputMove * Time.deltaTime);
            }

            if(Input.GetMouseButtonDown(0)) {
                SendMessage("Fire");
            }

            if(Input.GetMouseButtonDown(1)) {
                SendMessage(nameof(ClosePath));
            }

            Vector3Int newGridPos = MyGrid.WorldToCell(transform.position);
            if(newGridPos != MyGridPos) {
                MyGridPos = newGridPos;
                MainCam.SendMessage(nameof(CameraController.MoveToCell), (Vector2Int)MyGridPos);
            }
        }
    }

    private IEnumerator AnimationControl() {
        while(true) {
            yield return new WaitUntil(() => MyBody.velocity.magnitude > NEGLIGIBLE);
            while(MyBody.velocity.magnitude > NEGLIGIBLE) {
                SetAnimatorVelocity(MyBody.velocity);
                yield return new WaitForFixedUpdate();
            }
            SetAnimatorVelocity(Vector3.zero);
        }
    }

    private void SetAnimatorVelocity(Vector2 Vel) {
        if(MyAnimator != null) {
            MyAnimator.SetFloat("Velocity_Horizontal", Vel.x);
            MyAnimator.SetFloat("Velocity_Vertical", Vel.y);
        }
    }
	
    void Fire() {
        var ray = MainCam.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit)) {
            var direction = hit.point - transform.position;
            projectileManager.Spawn(MyBody.ClosestPointOnBounds(hit.point), direction);
        }
        // Just intersect with z = 0
        else if(ray.direction.z != 0.0f) {
            var lambda = -ray.origin.z / ray.direction.z;
            var target = ((Vector2)ray.origin) + lambda * ((Vector2)ray.direction);
            var direction = target - (Vector2)transform.position;
            projectileManager.Spawn(MyBody.ClosestPointOnBounds(target), direction);
        }
    }

    void ClosePath()
    {
        RoomManager.ClosePathAt(transform.position);
    }

    void PointCameraAtMe() {
        CameraCellPos = (Vector2Int)MyGridPos;
        MainCam.SendMessage("MoveToCell", CameraCellPos);
    }

    void StartFreeCam() {
        FreeCameraMode = true;
    }

    void StopFreeCam() {
        FreeCameraMode = false;
        PointCameraAtMe();
    }

    void TakeDamage(int Amount) {
        Debug.Assert(Amount > 0);
        healthBar.SetHealth(Mathf.Max(0, healthBar.CurrentHealth - Amount));
        if(healthBar.CurrentHealth < 1) {
            SendMessage("GameOver");
        }
    }

    void GameOver() {
        // TODO
    }
}
