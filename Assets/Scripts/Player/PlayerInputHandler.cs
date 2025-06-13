using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerControl controls;

    public Vector2 MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpReleased { get; private set; }
    public bool DashPressed { get; private set; }

    private void Awake()
    {
        controls = new PlayerControl();

        controls.Movement.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        controls.Movement.Move.canceled += ctx => MoveInput = Vector2.zero;

        controls.Movement.Jump.started += ctx => JumpPressed = true;
        controls.Movement.Jump.canceled += ctx => JumpReleased = true;

        controls.Movement.Dash.started += ctx => DashPressed = true;
    }

    private void LateUpdate()
    {
        JumpPressed = false;
        JumpReleased = false;
        DashPressed = false;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
}
