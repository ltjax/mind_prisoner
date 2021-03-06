﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private UIController uiController;

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
    private float actionTimeout = 0.0f;
    private float shootTimeout = 0.0f;

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
        uiController = GameObject.Find("/UI").GetComponent<UIController>();
        healthBar.SetHealth(10);

        InvokeRepeating("Falldown", 0.0f, 0.005f);
        SendMessage("Fall");

        StartCoroutine(AnimationControl());
    }

    void Falldown()
    {
        if (MyBody.transform.position.z < -0.5f)
        {
            MyBody.transform.Translate(new Vector3(0, 0, 0.5f));
        }
        else
        {
            CancelInvoke("Falldown");
            SendMessage("Crash");
        }
    }

    void FixedUpdate()
    {
        actionTimeout = Mathf.Max(0.0f, actionTimeout - Time.fixedDeltaTime);
        shootTimeout = Mathf.Max(0.0f, shootTimeout - Time.fixedDeltaTime);

        UpdateRoomState((Vector2Int)MyGridPos);

        if (Input.GetKeyDown(FreeCameraToggle)) {
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
                if (shootTimeout == 0.0f)
                {
                    SendMessage("Fire");
                    shootTimeout = 0.3f;
                }
            }

            if(Input.GetMouseButtonDown(1)) {
                SendMessage(nameof(ClosePath));
            }

            Vector3Int newGridPos = MyGrid.WorldToCell(transform.position);
            if(newGridPos != MyGridPos)
            {
                MyGridPos = newGridPos;
                var cell = (Vector2Int)MyGridPos;
                MainCam.SendMessage(nameof(CameraController.MoveToCell), cell);
                UpdateRoomState(cell);
            }
        }
    }

    private void UpdateRoomState(Vector2Int cell)
    {
        RoomManager.Visit(cell);
        uiController.UpdateMinimap(cell);
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

    Vector2? GetTargetPoint()
    {
        var ray = MainCam.ScreenPointToRay(Input.mousePosition);
        if (ray.direction.z == 0.0f)
            return null;

        var lambda = -ray.origin.z / ray.direction.z;
        var target = ray.origin + lambda * ray.direction;
       
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if ((ray.origin - hit.point).sqrMagnitude < (ray.origin - target).sqrMagnitude)
                target = hit.point;            
        }
        return target;
    }
	
    void Fire() {
        var target = GetTargetPoint();
        if (target.HasValue)
        {
            var start = (Vector2)MyBody.ClosestPointOnBounds(target.Value);
            var direction = target.Value - start;
            projectileManager.Spawn(MyBody.ClosestPointOnBounds(target.Value), direction);
        }
    }

    void ClosePath()
    {
        if (actionTimeout > 0.0f)
            return;

        RoomManager.ClosePathAt(transform.position);
        actionTimeout = 2.0f;
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
            SceneManager.LoadScene("GameOver");
        }
    }
}
