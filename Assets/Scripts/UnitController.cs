using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour {

    public float CruisingSpeed;
    private SpriteRenderer MySprite;
    private Animator MyAnimator;

    public Grid MyGrid;
    public Vector3Int MyGridPos;

    private Camera MainCam;

    // Start is called before the first frame update
    void Start() {
        MySprite = GetComponent<SpriteRenderer>();
        MyAnimator = GetComponent<Animator>();
        MainCam = Camera.main;

    }

    void Update() {
        Vector2 InputMove = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * CruisingSpeed;
        if(InputMove.sqrMagnitude > 1e-5) {
            transform.Translate(InputMove * Time.deltaTime);
            SetAnimatorVelocity(InputMove);
        } else {
            SetAnimatorVelocity(Vector2.zero);
        }
    }

    private void SetAnimatorVelocity(Vector2 Vel) {
        if(MyAnimator != null) {
            MyAnimator.SetFloat("Velocity_Horizontal", Vel.x);
            MyAnimator.SetFloat("Velocity_Vertical", Vel.y);
        }
    }

    void FixedUpdate() {
        MainCam.SendMessage("JumpTo", (Vector2)transform.position);
        MyGridPos = MyGrid.WorldToCell(transform.position);
    }

}
