using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    const string MoveVertical = "Vertical";
    const string MoveHorizontal = "Horizontal";

    const string MouseVertical = "Mouse Y";
    const string MouseHorizontal = "Mouse X";
    const string JoystickVertical = "Look Y";
    const string JoystickHorizontal = "Look X";

    const string Jump = "Jump";
    const string Switch = "Switch";
    const string Ability = "Ability";
    const string Slowtime = "Slowtime";

    [Tooltip("Sensitivity multiplier for moving the camera around")]
    [SerializeField]
    private float LookSensitivity = 1f;

    [Header("Inverting Axis (and Allies lol)")]
    [Tooltip("Used to flip the vertical input axis")]
    [SerializeField]
    private bool InvertYAxis = false;

    [Tooltip("Used to flip the horizontal input axis")]
    [SerializeField]
    private bool InvertXAxis = false;

    static Dictionary<KeyCode, int> keycodes = null;

    public bool inputEnabled { get; set; } = true;

    private float abilityTimer = 0f;


    // Start is called before the first frame update
    void Start()
    {
        if (keycodes == null)
        {
            keycodes = new Dictionary<KeyCode, int>();
            keycodes.Add(KeyCode.Alpha1, 1); keycodes.Add(KeyCode.Alpha2, 2); keycodes.Add(KeyCode.Alpha3, 3);
            keycodes.Add(KeyCode.Alpha4, 4); keycodes.Add(KeyCode.Alpha5, 5); keycodes.Add(KeyCode.Alpha6, 6);
            keycodes.Add(KeyCode.Alpha7, 7); keycodes.Add(KeyCode.Alpha8, 8); keycodes.Add(KeyCode.Alpha9, 9);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public Vector3 GetMoveInput()
    {
        if (inputEnabled && !Pause.Paused)
        {
            Vector3 move = new Vector3(Input.GetAxisRaw(MoveHorizontal), 0f, Input.GetAxisRaw(MoveVertical));

            // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
            move = Vector3.ClampMagnitude(move, 1);

            return move;
        }

        return Vector3.zero;
    }

    public float GetLookInputsHorizontal()
    {
        return GetMouseOrStickLookAxis(MouseHorizontal, JoystickHorizontal, InvertXAxis);
    }

    public float GetLookInputsVertical()
    {
        return GetMouseOrStickLookAxis(MouseVertical, JoystickVertical, !InvertYAxis);
    }


    public bool GetJump()
    {
        return inputEnabled && !Pause.Paused && Input.GetButtonDown(Jump);
    }

    public bool GetSwitch()
    {
        return inputEnabled && !Pause.Paused && Input.GetButtonDown(Switch);
    }

    public bool GetSlowtime()
    {
        return inputEnabled && !Pause.Paused && Input.GetButtonDown(Slowtime);
    }


    public bool GetAbility(int number)
    {
        if (Input.GetButton(Ability + number.ToString()))
            abilityTimer -= Time.deltaTime;

        if (abilityTimer <= 0f)
        {
            abilityTimer = 0.1f;
            return true;
        }

        return false;
        
    }

    public bool GetAbilityDown(int number)
    {
        return Input.GetButtonDown(Ability + number.ToString());
    }


    public bool GetAbilityUp(int number)
    {
        return Input.GetButtonUp(Ability + number.ToString());
    }


    public int GetSelectLoadoutInput()
    {
        if (inputEnabled && !Pause.Paused)
        {
            foreach (KeyCode key in keycodes.Keys)
            {
                if (Input.GetKeyDown(key))
                    return keycodes[key];
            }
        }

        return -1;
    }


    float GetMouseOrStickLookAxis(string mouseInputName, string stickInputName, bool inverter)
    {
        if (!inputEnabled || Pause.Paused)
            return 0f;

        // Check if this look input is coming from the mouse
        //bool isGamepad = Input.GetAxis(stickInputName) != 0f;
        //float i = isGamepad ? Input.GetAxis(stickInputName) : Input.GetAxisRaw(mouseInputName);
        bool isGamepad = false;
        float i = Input.GetAxisRaw(mouseInputName);

        // handle inverting vertical input
        if (inverter)
            i *= -1f;

        // apply sensitivity multiplier
        i *= LookSensitivity;

        if (isGamepad)
            // since mouse input is already deltaTime-dependant, only scale input with frame time if it's coming from sticks
            i *= Time.deltaTime;
        else
        {
            // reduce mouse input amount to be equivalent to stick movement
            i *= 0.01f;
            #if UNITY_WEBGL
                // Mouse tends to be even more sensitive in WebGL due to mouse acceleration, so reduce it even more
                i *= WebglLookSensitivityMultiplier;
            #endif
        }

        return i;
    }
}