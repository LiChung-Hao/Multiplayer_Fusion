using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class Bullet : NetworkBehaviour
{
    [Networked]
    private TickTimer life { get; set; }

    [SerializeField]
    private float bulletSpeed = 5f;

    public override void Spawned() //like "start" in monobehaviour
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f); //NetworkBehaviour will look for runner and save it as Runner
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
        {
            Runner.Despawn(Object); //the object this script attached to
        }
        else
        {
            transform.position += bulletSpeed * transform.forward * Runner.DeltaTime;
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();

            player.TakeDamage(10);

            Runner.Despawn(Object);
        }
    }
}
