using UnityEngine;

public class BattleInputManager : MonoBehaviour
{
    private PlayerTurnController controller;
    private BattleControls inputControls;

    // The Bootstrapper will inject the controller
    public void Initialize(PlayerTurnController controller)
    {
        this.controller = controller;

        // 1. Instantiate the auto-generated input map
        inputControls = new BattleControls();

        // 2. Subscribe to the C# events!
        // "performed" fires exactly once when the button is successfully fully pressed.
        inputControls.Battle.Confirm.performed += ctx => SendInput(InputButton.Confirm);
        inputControls.Battle.Cancel.performed += ctx => SendInput(InputButton.Cancel);
        inputControls.Battle.Pursuit.performed += ctx => SendInput(InputButton.Pursuit);
        
        inputControls.Battle.Up.performed += ctx => SendInput(InputButton.Up);
        inputControls.Battle.Down.performed += ctx => SendInput(InputButton.Down);
        inputControls.Battle.Left.performed += ctx => SendInput(InputButton.Left);
        inputControls.Battle.Right.performed += ctx => SendInput(InputButton.Right);

        // 3. Turn the inputs on
        inputControls.Enable();
    }

    private void SendInput(InputButton button)
    {
        controller.ProcessInput(button);
    }

    // We only use Update for reading the continuous analog float values
    public void Update()
    {
        if (controller == null || controller.CurrentState is IdleState)
        {
            return;
        }

        // Read the live Vector2 value from the IKJL/Joystick binding
        Vector2 analogInput = inputControls.Battle.CursorMove.ReadValue<Vector2>();

        // Only process if the stick is actually being moved
        if (analogInput.sqrMagnitude > 0.01f)
        {
            // Note: X is horizontal, Y is vertical in 2D UI space. 
            // Our controller maps Y to the Z axis in 3D space!
            controller.ProcessAnalogInput(analogInput.x, analogInput.y, Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        if (inputControls != null)
        {
            inputControls.Disable();
            inputControls.Dispose();
        }
    }
}