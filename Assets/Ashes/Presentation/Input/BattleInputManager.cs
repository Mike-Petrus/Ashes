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
        if (controller == null)
        {
            return;
        }

        // Only process input if we are in a menu state
        // TODO: Not sure how I feel about this. Idle may need to act as a "free state"
        // instead of a "blocking state". Will have to see how it feels in testing later
        if (controller.CurrentState == InputState.Idle)
        {
            return;
        }

        var keyboard = Keyboard.current;

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