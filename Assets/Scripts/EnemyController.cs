using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Range(0, 3)]
    public float Speed, InitSpeed;
    Vector2 CurrentTarget;
    CharacterController MyBody;
    Grid MyGrid;
    UnitController Player;

    public int HitPoints = 5, MaxHitPoints = 5;
    bool IsDead => HitPoints < 1;
    float calmness => Mathf.Lerp(0.2f, 1f, (float)HitPoints / MaxHitPoints);

    Vector2 RandomLocalTarget => (Vector2)transform.position + Random.insideUnitCircle * Speed;
    public Vector3Int MyGridPos => MyGrid.WorldToCell(transform.position);

    // Start is called before the first frame update
    void Start()
    {
        MyGrid = GameObject.FindGameObjectWithTag("Grid")?.GetComponent<Grid>();
        Player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<UnitController>();
        MyBody = GetComponent<CharacterController>();
        CurrentTarget = GetNewTarget();
        
    }

    void FixedUpdate()
    {
        if(IsDead) {
            return;
        } else {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * calmness / 2, Time.fixedDeltaTime);
            Speed = Mathf.Lerp(Speed, Mathf.Lerp(3f, InitSpeed, calmness), Time.fixedDeltaTime);
        }

        if(Player.MyGridPos == MyGridPos && Vector3.Distance(Player.transform.position, transform.position) < Speed * 2) {
            CurrentTarget = Player.transform.position + Vector3.up / 2;
        }

        Vector2 targetDir = CurrentTarget - (Vector2)transform.position;
        if(targetDir.magnitude < Speed * Time.fixedDeltaTime) {
            MyBody.Move(targetDir);
            CurrentTarget = GetNewTarget();
        } else {
            MyBody.Move(targetDir.normalized * Speed * Time.deltaTime);
        }

        // Sanity check
        if(MyBody.velocity.magnitude < 1e-3) {
            CurrentTarget = GetNewTarget();
        }
    }

    Vector2 GetNewTarget() {
        var myBounds = MyGrid.GetBoundsLocal(MyGridPos);
        var targetCandidate = RandomLocalTarget;
        if(myBounds.Contains(targetCandidate)) {
            return targetCandidate;
        } else {
            return myBounds.ClosestPoint(RandomLocalTarget);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(!IsDead && other.CompareTag("Projectile")) {
            HitPoints -= 1;
            SendMessage("Explode");
            other.gameObject.SendMessage("Explode");
            if(IsDead) {
                StartCoroutine(PlayDead());
            }
        }
    }

    IEnumerator PlayDead() {
        Speed = 0;
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
