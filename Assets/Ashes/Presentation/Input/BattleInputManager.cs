using UnityEngine;
using UnityEngine.InputSystem;

public class BattleInputManager : MonoBehaviour
{
    private PlayerTurnController controller;

    // The Bootstrapper will inject the controller
    public void Initialize(PlayerTurnController controller)
    {
        this.controller = controller;
    }

    public void Update()
    {
        if (controller == null || controller.CurrentState == InputState.Idle)
        {
            return;
        }

        // Only process input if we are in a menu state
        // TODO: Not sure how I feel about this. Idle may need to act as a "free state"
        // instead of a "blocking state". Will have to see how it feels in testing later

        var keyboard = Keyboard.current;

        // ==========================================
        // 1. CONTINUOUS MOVEMENT (Free-Aim Cursor)
        // Uses I, J, K, L to simulate an analog stick
        // =========================================
        // TODO: Set up proper input profiles and analog stick input
        float x = 0f;
        float y = 0f;

        // Use .isPressed for continuous sliding
        if (keyboard.lKey.isPressed) x += 1f;
        if (keyboard.jKey.isPressed) x -= 1f;
        if (keyboard.iKey.isPressed) y += 1f;
        if (keyboard.kKey.isPressed) y -= 1f;

        if (Mathf.Abs(x) > 0.01f || Mathf.Abs(y) > 0.01f)
        {
            // Pass it to the backend exactly like an analog stick!
            controller.ProcessAnalogInput(x, y, Time.deltaTime);
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            controller.ProcessInput(InputButton.Confirm);
        }
        else if (keyboard.escapeKey.wasPressedThisFrame)
        {
            controller.ProcessInput(InputButton.Cancel);
        }
        else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
        {
            controller.ProcessInput(InputButton.Up);
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
        {
            controller.ProcessInput(InputButton.Down);
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
        {
            controller.ProcessInput(InputButton.Left);
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
        {
            controller.ProcessInput(InputButton.Right);
        }
        else if (keyboard.pKey.wasPressedThisFrame)
        {
            controller.ProcessInput(InputButton.Pursuit);
        }
    }
}