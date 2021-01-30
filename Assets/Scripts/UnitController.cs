using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour {

    public const float NEGLIGIBLE = 1e-2f;

    public float CruisingSpeed;
    private SpriteRenderer MySprite;
    private Animator MyAnimator;
    private CharacterController MyBody;

    public Grid MyGrid;
    public Vector3Int MyGridPos;
    private GenerateRoom RoomManager;

    private Vector2 LastInput = Vector2.zero;

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
        MyBody = GetComponent<CharacterController>();
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
            Vector2 InputMove = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * CruisingSpeed;
            if(InputMove.sqrMagnitude > NEGLIGIBLE) {
                MyBody.Move(InputMove * Time.deltaTime);
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
