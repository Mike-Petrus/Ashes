using UnityEngine;

public class BattleInputManager : MonoBehaviour
{
    private PlayerTurnController controller;
    private BattleControls inputControls;
    private CameraController cameraController;

    // The Bootstrapper will inject the controller
    public void Initialize(PlayerTurnController controller, CameraController cameraController)
    {
        this.controller = controller;
        this.cameraController = cameraController;

        // 1. Instantiate the auto-generated input map
        inputControls = new BattleControls();

        // 2. Subscribe to the C# events!
        // "performed" fires exactly once when the button is successfully fully pressed.
        inputControls.Battle.Confirm.performed += ctx => SendInput(InputButton.Confirm);
        inputControls.Battle.Cancel.performed += ctx => SendInput(InputButton.Cancel);
        inputControls.Battle.Pursuit.performed += ctx => SendInput(InputButton.Pursuit);
        inputControls.Battle.FreeAim.performed += ctx => SendInput(InputButton.FreeAim);
        
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

        float deadzoneSqr = 0.0625f; // 25% deadzone

        // 1. PROCESS RIGHT STICK (CAMERA) NATIVELY
        if (cameraController != null)
        {
            Vector2 rightStick = inputControls.Battle.CameraLook.ReadValue<Vector2>();
            if (rightStick.sqrMagnitude > deadzoneSqr)
            {
                cameraController.RotateCamera(rightStick.x, rightStick.y, Time.deltaTime);
            }
        }

        // 2. PROCESS LEFT STICK (CURSOR) WITH CAMERA TRANSLATION
        Vector2 leftStick = inputControls.Battle.CursorMove.ReadValue<Vector2>();
        if (leftStick.sqrMagnitude > deadzoneSqr)
        {
            float moveX = leftStick.x;
            float moveZ = leftStick.y;

            if (cameraController != null)
            {
                // Get Camera's flattened directional vectors
                Transform camTransform = cameraController.transform;
                Vector3 camForward = new Vector3(camTransform.forward.x, 0, camTransform.forward.z).normalized;
                Vector3 camRight = new Vector3(camTransform.right.x, 0, camTransform.right.z).normalized;

                // Translate stick input into camera-relative world space
                Vector3 worldMove = (camRight * leftStick.x) + (camForward * leftStick.y);
                
                moveX = worldMove.x;
                moveZ = worldMove.z;
            }

            // Pass the pure world-space intent into the domain
            controller.ProcessAnalogLeft(moveX, moveZ, Time.deltaTime);
        }

        // 3. TRIGGER EDGE PANNING
        // We do this every frame the cursor is active so the camera glides smoothly
        if (cameraController != null)
        {
            cameraController.HandleEdgePan(VectorAdapter.ToUnity(controller.CurrentCursorPosition));
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