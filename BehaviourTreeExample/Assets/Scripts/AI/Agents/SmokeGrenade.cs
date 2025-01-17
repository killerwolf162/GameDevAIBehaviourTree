using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeGrenade : MonoBehaviour
{
    [SerializeField] 
    private float smokeTimer = 10f;
    [SerializeField] 
    ParticleSystem partSystem;

    public float smokeRadius = 5;
    private float counter;


    void Start()
    {
        counter = smokeTimer;
        partSystem.Play();
    }

    private void FixedUpdate()
    {
        counter -= Time.deltaTime;
        if(counter < 0) 
        {
            partSystem.gameObject.SetActive(false);
            Destroy(this);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, smokeRadius);
    }
}
