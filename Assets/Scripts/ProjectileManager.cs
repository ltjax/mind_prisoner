using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public GameObject projectile;
    public ParticleSystem hitEffect;
    private readonly List<(GameObject go, ProjectileController controller)> alive = new List<(GameObject go, ProjectileController controller)>();
    private readonly List<(GameObject go, ProjectileController controller)> hospice = new List<(GameObject go, ProjectileController controller)>();
    private readonly List<GameObject> freeList = new List<GameObject>();
    private int counter = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var (go, controller) in alive)
        {
            controller.timeLeft -= Time.deltaTime;
            if (controller.Dying)
            {
                Instantiate(hitEffect, go.transform.position, go.transform.rotation);
                hospice.Add((go, controller));
                controller.Disarm();
                continue;
            }
            go.transform.Translate(controller.direction * Time.deltaTime);
        }

        alive.RemoveAll(x => x.controller.Dying);

        // This is where projectiles go to die, we just have to wait for it
        foreach (var (go, controller) in hospice)
        {
            if (controller.ReadyToGo())
            {
                freeList.Add(go);
                go.SetActive(false);
            }
        }

        hospice.RemoveAll(x => !x.go.activeSelf);
    }


    public void Spawn(Vector3 fromPosition, Vector2 direction)
    {
        var length = direction.magnitude;
        if (length < 0.01)
            return;

        var newProjectile = Acquire();
        newProjectile.name = string.Format("Projectile #{0}", ++counter);
        var controller = newProjectile.GetComponent<ProjectileController>();
        controller.direction = direction * (12.0f / length);
        controller.timeLeft = 3.0f;
        newProjectile.transform.position = fromPosition;
        alive.Add((newProjectile, controller));
    }

    public GameObject Acquire()
    {
        if (freeList.Count > 0)
        {
            var result = freeList.TakeLast();
            result.SetActive(true);
            result.SendMessage("Rearm");
            return result;
        }
        return Instantiate(projectile);
    }
}
