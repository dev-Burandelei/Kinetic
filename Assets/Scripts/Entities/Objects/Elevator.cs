using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    Movepad movepad;
    MovingObject movingObject;
    Vector3 direction = Vector3.zero;

    [SerializeField]
    float Speed = 3f;

    [SerializeField]
    List<GameTrigger> Buttons;

    [SerializeField]
    Transform UpperPosition;
    [SerializeField]
    Transform LowerPosition;

    float goingUp = -1;

    // Start is called before the first frame update
    void Start()
    {
        movepad = GetComponentInChildren<Movepad>();
        movingObject = GetComponentInChildren<MovingObject>();
        direction.y = Speed* goingUp;

        foreach (GameTrigger button in Buttons)
        {
            button.OnTriggerActivate += ToggleDirection;
        }
    }

    void ToggleDirection()
    {
        movepad.enabled = true;
        goingUp = -goingUp;
        movepad.Sticky = (goingUp > 0f);
        UpdateDirection(Speed * goingUp);
    }


    // Update is called once per frame
    void Update()
    {
        if (Vector3.Dot(transform.position - UpperPosition.position, transform.position - LowerPosition.position) > 0f &&
            (goingUp < 0f && Vector3.Dot(Vector3.down, transform.position - LowerPosition.position) > 1f ||
            goingUp > 0f && Vector3.Dot(Vector3.up, transform.position - UpperPosition.position) > 1f))
        {
            UpdateDirection(0f);
            movepad.enabled = false;
        }
    }


    void UpdateDirection(float speed)
    {
        direction.y = speed;
        movepad.SetMoveDirection(direction);
        movingObject.SetMoveDirection(direction);
    }
}
