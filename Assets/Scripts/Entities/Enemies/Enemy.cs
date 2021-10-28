using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    bool debug = false;

    [Header("References")]
    [SerializeField]
    Weapon[] weapons;

    [SerializeField]
    public GameObject Model;

    public LayersConfig GroundLayers;
    public LayersConfig ViewBlockedLayers;

    [Header("Attributes")]
    public bool Movable = true;
    public float HoverHeight = 1f;
    public float GravityMultiplier = 1f;
    public float AirDesacceleration = 0f;

    [Header("Options")]
    [SerializeField]
    [Tooltip("If true, lets the NavMeshAgent control the rotation of this enemy")]
    bool TurnToMoveDirection = false;

    [SerializeField]
    bool OnlyShootIfPlayerInView = true;

    [SerializeField]
    float CollisionDistance = 1f;

    Vector3 moveVelocity = Vector3.zero;

    [HideInInspector]
    public float Stunned = 0f;

    bool airborne = false;
    private NavMeshAgent pathAgent;
    Transform playerTransform;

    public UnityAction OnActiveUpdate;
    public UnityAction<Vector3> OnKnockbackCollision;

    void Start()
    {
        playerTransform = ActorsManager.Player.GetComponentInChildren<Camera>().transform;
        pathAgent = GetComponent<NavMeshAgent>();
        if (pathAgent)
        {
            pathAgent.updateRotation = TurnToMoveDirection;
            pathAgent.updateUpAxis = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        FeelGravity();
        FeelKnockback();

        if (Stunned <= 0f)
        {
            ActivateWeapons();
            if (OnActiveUpdate != null)
                OnActiveUpdate.Invoke();
        }
        else
            Stunned -= Time.deltaTime;
    }


    private void ActivateWeapons()
    {
        if (OnlyShootIfPlayerInView && !IsPlayerInView())
            return;

        foreach (Weapon weapon in weapons)
            if (weapon)
                weapon.Trigger();
    }

    public bool IsPlayerInView()
    {
        Vector3 playerDistance = playerTransform.position - Model.transform.position;

        // Check if inside field of view
        if (Vector3.Dot(Model.transform.forward, playerDistance) < 0f)
            return false;

        // Check if view is obstructed
        Ray ray = new Ray(Model.transform.position, playerTransform.position - Model.transform.position);
        Physics.Raycast(ray, out RaycastHit hit, 100f, ViewBlockedLayers.layers, QueryTriggerInteraction.Ignore);
        if (hit.collider && (hit.distance < playerDistance.magnitude))
            return false;

        return true;
    }


    private void FeelGravity()
    {
        if (!Movable)
            return;
        RaycastHit hit = RayToGround();
        airborne = (hit.collider == null);

        // Air
        if (airborne)
            moveVelocity += Vector3.down * GravityMultiplier * Constants.Gravity * Time.deltaTime;

        // Ground
        else
        {
            // Stop on the ground
            if (moveVelocity.y < 0f)
                moveVelocity = Vector3.ProjectOnPlane(moveVelocity, Vector3.up);

            if (pathAgent)
            {
                if (!pathAgent.enabled)
                    pathAgent.enabled = true;

                // Slowdown
                if (!pathAgent.updatePosition)
                    moveVelocity = Vector3.MoveTowards(moveVelocity, Vector3.zero, Mathf.Max(1f, moveVelocity.magnitude) * Time.deltaTime);
            }
        }
    }


    private void FeelKnockback()
    {
        // Automatic desacceleration (for airborne enemies)
        if (AirDesacceleration > 0 && moveVelocity.magnitude > 0)
        {
            moveVelocity -= moveVelocity.normalized * AirDesacceleration * Time.deltaTime;
            if (moveVelocity.magnitude < 0.5f)
                moveVelocity = Vector3.zero;
        }

        if (!pathAgent || !pathAgent.updatePosition)
        {
            // Collision
            Ray ray = new Ray(Model.transform.position, moveVelocity.normalized);
            Physics.Raycast(ray, out RaycastHit hitInfo, CollisionDistance, GroundLayers.layers, QueryTriggerInteraction.Ignore);
            if (hitInfo.collider)
            {
                if (OnKnockbackCollision != null)
                    OnKnockbackCollision.Invoke(moveVelocity);
                moveVelocity = Vector3.zero;
            }

            // Movement
            if (moveVelocity == Vector3.zero)
            {
                if (pathAgent)
                {
                    pathAgent.updatePosition = true;
                    pathAgent.Warp(transform.position);
                }
            } else
                transform.position += moveVelocity * Time.deltaTime;
        }
    }


    public RaycastHit RayToGround()
    {
        Ray ray;
        if (!pathAgent || !pathAgent.updatePosition)
            ray = new Ray(transform.position + Vector3.up, Vector3.down);
        else
            ray = new Ray(pathAgent.nextPosition + Vector3.up, Vector3.down);
        Physics.Raycast(ray, out RaycastHit hitInfo, HoverHeight + 1f, GroundLayers.layers, QueryTriggerInteraction.Ignore);
        return hitInfo;
    }


    public void ReceiveStun(float duration)
    {
        Stunned = duration;
        if (pathAgent && pathAgent.isOnNavMesh)
            pathAgent.isStopped = true;
    }

    public void ReceiveKnockback(Vector3 knockback)
    {
        if (!Movable)
            return;

        moveVelocity += (knockback / 2);

        // Parar no ch�o
        if (moveVelocity.y < 0f)
            moveVelocity = Vector3.ProjectOnPlane(moveVelocity, Vector3.up);

        if (pathAgent)
            pathAgent.updatePosition = false;
    }

    public void WarpPosition(Vector3 newPosition)
    {
        airborne = true;
        transform.position = newPosition;
    }


    private void OnDrawGizmos()
    {
        if (GetComponent<NavMeshAgent>())
        {
            Ray ray;
            if (!pathAgent)
                ray = new Ray(transform.position, Vector3.down);
            else
                ray = new Ray(pathAgent.nextPosition + Vector3.up, Vector3.down);
            //Ray ray = new Ray(Model.transform.position, Vector3.down);
            Gizmos.DrawRay(ray);
        }
    }
}
