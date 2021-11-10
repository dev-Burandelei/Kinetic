using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallArea : MonoBehaviour
{
    [SerializeField]
    float VerticalLimit;

    [SerializeField]
    Transform FallRespawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInChildren<PlayerFallHandler>())
        {
            other.GetComponentInChildren<PlayerFallHandler>().VerticalLimit = VerticalLimit;
            other.GetComponentInChildren<PlayerFallHandler>().FallRespawnPoint = FallRespawnPoint.position;
        }
    }
}
