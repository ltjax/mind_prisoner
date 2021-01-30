using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ProjectileController : MonoBehaviour
{
    public Vector2 direction;
    public float timeLeft;
    public bool isHarmless = false;
    private ParticleSystem trailParticles;

    // Start is called before the first frame update
    void Start()
    {
        trailParticles = GetComponentInChildren<ParticleSystem>();
        Debug.Assert(trailParticles != null);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Explode()
    {
        if (trailParticles != null)
        {
            //trailParticles.emission.enabled = false;
            Disarm();
        }
        else
        {
            Debug.Log("Huh?");
        }

        Debug.Log("Exploded!");
    }

    public void Disarm()
    {
        var emission = trailParticles.emission;
        emission.enabled = false;
        trailParticles.Stop();
    }

    public bool ReadyToGo()
    {
        return !trailParticles.IsAlive();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isHarmless && other.CompareTag("Room"))
        {
            isHarmless = true;
            GetComponent<SphereCollider>().enabled = false;
        }
    }

    void Rearm()
    {
        var emission = trailParticles.emission;
        emission.enabled = true;
        isHarmless = false;
        GetComponent<SphereCollider>().enabled = true;
    }
}
