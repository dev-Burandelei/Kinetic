using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitscanWeapon : Weapon
{
    [Header("Hitscan")]
    [Tooltip("Max distance to which the bullets travel")]
    public float MaxDistance = 100f;
    [SerializeField]
    bool StopAtFirstHit = true;

    [Tooltip("Prefab containing hit animation")]
    [SerializeField]
    GameObject Sparks;
    ObjectManager.PoolableType SparksType;

    List<Health> enemies = new List<Health>(25);
    public UnityAction<RaycastHit> OnHit;


    void Awake()
    {
        if (Sparks != null)
            SparksType = Sparks.GetComponent<Poolable>().Type;
    }

    public override void Shoot(Vector3 direction)
    {
        if (StopAtFirstHit)
        {
            Physics.Raycast(Mouth.position, direction, out RaycastHit hit, MaxDistance, HitLayers.layers);
            AttackHit(hit);
        }
        else
        {
            Physics.Raycast(Mouth.position, direction, out RaycastHit rayHit, MaxDistance, HitLayers.layers);
            Vector3 extremePoint;
            if (rayHit.collider)
                extremePoint = rayHit.point;
            else
                extremePoint = Mouth.position + direction * MaxDistance;

            enemies.Clear();
            RaycastHit[] hits = Physics.BoxCastAll((Mouth.position + extremePoint) / 2, new Vector3(0.75f, 0.75f, 0.75f), direction, 
                Quaternion.identity, MaxDistance, HitLayers.layers);
            foreach (RaycastHit hit in hits)
                AttackHit(hit);
        }
    }


    void AttackHit(RaycastHit hit)
    {
        if (!hit.collider || !hit.collider.GetComponent<Damageable>() || enemies.Contains(hit.collider.GetComponent<Damageable>().GetHealth()))
            return;

        if (Sparks != null)
        {
            GameObject newObject = ObjectManager.OM.SpawnObjectFromPool(SparksType, Sparks);
            newObject.transform.position = hit.point;
            newObject.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Random.insideUnitSphere, hit.normal),
                hit.normal);
        }

        OnHit?.Invoke(hit);
        enemies.Add(hit.collider.GetComponent<Damageable>().GetHealth());
        GetComponent<Attack>().AttackTarget(hit.collider.gameObject);
    }



}