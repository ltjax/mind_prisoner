using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour {

    public const float NEGLIGIBLE = 1e-2f;

    public float CruisingSpeed;
    private SpriteRenderer MySprite;
    private Animator MyAnimator;
    private Rigidbody MyBody;

    public Grid MyGrid;
    public Vector3Int MyGridPos;
    private GenerateRoom RoomManager;

    private List<(KeyCode key, Vector2Int offset)> DirectionTable = new List<(KeyCode key, Vector2Int offset)> {
                (KeyCode.LeftArrow, Vector2Int.left   ),
                (KeyCode.RightArrow, Vector2Int.right ),
                (KeyCode.UpArrow, Vector2Int.up       ),
                (KeyCode.DownArrow, Vector2Int.down   )
            };

    private Camera MainCam;

    // Start is called before the first frame update
    void Start() {
        MySprite = GetComponent<SpriteRenderer>();
        MyAnimator = GetComponent<Animator>();
        MainCam = Camera.main;
        MyBody = GetComponent<Rigidbody>();
        RoomManager = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GenerateRoom>();

        StartCoroutine(AnimationControl());
    }

    void FixedUpdate() {

        if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            foreach(var (key, offset) in DirectionTable) {
                if(Input.GetKeyDown(key)) {
                    RoomManager.RemoveRoom((Vector2Int)MyGridPos + offset, (Vector2Int)MyGridPos);
                    break;
                }
            }
            return;
        } else {
            Vector2 InputMove = Vector2.zero;
            foreach(var (key, offset) in DirectionTable) {
                if(Input.GetKey(key)) {
                    InputMove += offset;
                }
            }

            InputMove = new Vector2(InputMove.x * Mathf.Clamp01(1 - MyBody.velocity.x / CruisingSpeed),
                                    InputMove.y * Mathf.Clamp01(1 - MyBody.velocity.y / CruisingSpeed));
            if(InputMove.magnitude > NEGLIGIBLE) {
                MyBody.AddForce(InputMove);
            } else if(MyBody.velocity.magnitude > NEGLIGIBLE) {
                // Counter-force to stop immediately
                MyBody.AddForce(-MyBody.velocity * MyBody.mass / Time.fixedDeltaTime);
            }
        }

        Vector3Int newGridPos = MyGrid.WorldToCell(transform.position);
        if(newGridPos != MyGridPos) {
            MyGridPos = newGridPos;
            MainCam.SendMessage(nameof(CameraController.MoveToCell), (Vector2Int)MyGridPos);
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
}
