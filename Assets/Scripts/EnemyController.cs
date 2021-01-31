using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int EnemyType = 0;

    [Range(0, 3)]
    public float Speed, InitSpeed;
    Vector2 CurrentTarget;
    CharacterController MyBody;
    Grid MyGrid;
    UnitController Player;
    HealthBar MyHealthBar;
    Animator MyAnimator;

    public int HitPoints = 5, MaxHitPoints = 5;
    bool IsDead => HitPoints < 1;
    float Calmness => Mathf.Lerp(0.2f, 1f, (float)HitPoints / MaxHitPoints);
    float DistToPlayer => Vector3.Distance(Player.GetComponent<CharacterController>().bounds.center, MyBody.bounds.center);

    Vector2 RandomLocalTarget => (Vector2)transform.position + Random.insideUnitCircle * Speed;
    public Vector3Int MyGridPos => MyGrid.WorldToCell(transform.position);

    public ParticleSystem BigExplode;

    public enum EnemyState {
        Wandering,
        Attacking,
        Dead
    }
    public EnemyState MyState = EnemyState.Wandering;

    public float AttackDistance = 1f, AttackReach = 1.5f, AttackDelay = 2f, AttackCooldown = 2f;
    [Range(0, 10)]
    public int AttackStrength = 10;

    // Start is called before the first frame update
    void Start()
    {
        MyGrid = GameObject.FindGameObjectWithTag("Grid")?.GetComponent<Grid>();
        Player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<UnitController>();
        MyBody = GetComponent<CharacterController>();
        CurrentTarget = GetNewTarget();
        MyHealthBar = GetComponentInChildren<HealthBar>();
        MyHealthBar.SetMaxHealth(MaxHitPoints);
        GetComponentInChildren<SkinnedMeshRenderer>().transform.rotation = Random.rotation;
        MyAnimator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        if(IsDead || MyState == EnemyState.Attacking) {
            return;
        } else {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * Calmness / 2, Time.fixedDeltaTime);
            Speed = Mathf.Lerp(Speed, Mathf.Lerp(3f, InitSpeed, Calmness), Time.fixedDeltaTime);
        }

        if(Player.MyGridPos == MyGridPos && Vector3.Distance(Player.transform.position, transform.position) < Speed * 2) {
            CurrentTarget = Player.transform.position + Vector3.up / 2;

            if(DistToPlayer < AttackDistance) {
                MyState = EnemyState.Attacking;
                StartCoroutine(PlayAttack());
            }
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
            MyHealthBar.SetHealth(HitPoints);
            SendMessage("Explode");
            other.gameObject.SendMessage("Explode");
            if(IsDead) {
                StartCoroutine(PlayDead());
            }
        }
    }

    IEnumerator PlayDead() {
        Speed = 0;
        MyState = EnemyState.Dead;
        GameObject.FindGameObjectWithTag("GameController").SendMessage("EnemyDied", this);
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }

    IEnumerator PlayAttack() {
        SendMessage("WarnAttackFirst");
        var startTime = Time.timeSinceLevelLoad;
        MyAnimator.speed = 3;
        var warningLength = GetComponent<UnitAudio>().AttackFinalWarning.length;
        yield return new WaitUntil(() => Time.timeSinceLevelLoad > startTime + AttackDelay /2);
        SendMessage("WarnAttackFinal");
        MyAnimator.speed = 6;
        yield return new WaitUntil(() => Time.timeSinceLevelLoad > startTime + AttackDelay);
        SendMessage("Fire");
        MyAnimator.speed = 2;
        yield return new WaitUntil(() => Time.timeSinceLevelLoad > startTime + AttackDelay + AttackCooldown);
        MyAnimator.speed = 1;
        MyState = EnemyState.Wandering;
    }

    void Fire() {
        Instantiate(BigExplode, transform);
        if(DistToPlayer < AttackReach) {
            Player.SendMessage("TakeDamage", AttackStrength);
        }
    }
}
