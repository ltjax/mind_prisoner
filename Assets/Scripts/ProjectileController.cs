using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{

    public Vector2 direction;
    public float timeLeft;
    private bool IsHarmless = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    void OnTriggerEnter(Collider other) {
        if(!IsHarmless && other.CompareTag("Room")) {
            IsHarmless = true;
            GetComponent<SphereCollider>().enabled = false;
        }
    }

    void Rearm() {
        IsHarmless = false;
        GetComponent<SphereCollider>().enabled = true;
    }
}
