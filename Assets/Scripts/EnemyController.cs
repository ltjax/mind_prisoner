using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Range(0, 3)]
    public float Speed;
    Vector2 CurrentTarget;
    CharacterController MyBody;
    Grid MyGrid;

    bool IsDead = false;

    Vector2 RandomLocalTarget => (Vector2)transform.position + Random.insideUnitCircle * Speed;

    // Start is called before the first frame update
    void Start()
    {
        MyGrid = GameObject.FindGameObjectWithTag("Grid")?.GetComponent<Grid>();
        MyBody = GetComponent<CharacterController>();
        CurrentTarget = GetNewTarget();
    }

    void FixedUpdate()
    {
        if(IsDead) {
            return;
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
        var myBounds = MyGrid.GetBoundsLocal(MyGrid.WorldToCell(transform.position));
        var targetCandidate = RandomLocalTarget;
        if(myBounds.Contains(targetCandidate)) {
            return targetCandidate;
        } else {
            return myBounds.ClosestPoint(RandomLocalTarget);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(!IsDead && other.CompareTag("Projectile")) {
            IsDead = true;
            StartCoroutine(PlayDead());
        }
    }

    IEnumerator PlayDead() {
        Speed = 0;
        SendMessage("Explode");
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }

}
