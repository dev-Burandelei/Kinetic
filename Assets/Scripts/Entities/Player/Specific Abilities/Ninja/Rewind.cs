using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rewind : Ability
{
    struct Status
    {
        public Vector3 position;
        public Quaternion rotation;
        public int health;

        public Status(Transform transform, int health)
        {
            position = transform.position;
            rotation = transform.rotation;
            this.health = health;
        }
    }

    [SerializeField]
    int SecondsBack = 5;
    float clock = 0f;
    bool running = false;

    PlayerCharacterController player;
    List<Status> lastTransforms;

    Health health;

    // Start is called before the first frame update
    void Start()
    {
        lastTransforms = new List<Status>(SecondsBack);
        player = GetComponentInParent<PlayerCharacterController>();
        health = player.GetComponent<Health>();
        lastTransforms.Add(new Status(player.transform, health.CurrentHealth));

        OnUpdate += Updating;
    }

    void Updating()
    {
        if (running)
            return;

        clock += Time.deltaTime;
        if (clock > 1f)
        {
            clock = 0f;
            lastTransforms.Insert(0, new Status(player.transform, health.CurrentHealth));
            if (lastTransforms.Count > SecondsBack)
                lastTransforms.RemoveAt(lastTransforms.Count-1);
        }
    }

    public override void Execute(Input input)
    {
        StartCoroutine(ExecuteRewind());
    }

    IEnumerator ExecuteRewind()
    {
        Status previousTransform = new Status(player.transform, health.CurrentHealth);
        running = true;

        foreach (Status status in lastTransforms)
        {
            for (int i = 0; i < 10; i++)
            {
                player.transform.position = Vector3.Lerp(previousTransform.position, status.position, i*0.1f);
                player.transform.rotation = Quaternion.Lerp(previousTransform.rotation, status.rotation, i * 0.1f);
                yield return new WaitForSecondsRealtime(0.01f);
            }
            health.CurrentHealth = status.health;
            previousTransform = status;
        }

        running = false;
    }

}
