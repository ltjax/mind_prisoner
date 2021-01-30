using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public GameObject projectile;
    private readonly List<(GameObject go, ProjectileController controller)> alive = new List<(GameObject go, ProjectileController controller)>();
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
            if (controller.timeLeft <= 0.0f)
            {
                go.SetActive(false);
                freeList.Add(go);
                continue;
            }
            go.transform.Translate(controller.direction * Time.deltaTime);
        }

        alive.RemoveAll(x => x.controller.timeLeft <= 0.0f);
    }

    private void FixedUpdate()
    {
    }

    public void Spawn(Vector2 position, Vector2 direction)
    {
        var length = direction.magnitude;
        if (length < 0.01)
            return;

        var newProjectile = Acquire();
        newProjectile.name = string.Format("Projectile #{0}", ++counter);
        var controller = newProjectile.GetComponent<ProjectileController>();
        controller.direction = direction * (8.0f / length);
        controller.timeLeft = 1.0f;
        newProjectile.transform.position = position;
        alive.Add((newProjectile, controller));
    }

    public GameObject Acquire()
    {
        if (freeList.Count > 0)
        {
            var result = freeList.TakeLast();
            result.SetActive(true);
            return result;
        }
        return GameObject.Instantiate(projectile);
    }
}
