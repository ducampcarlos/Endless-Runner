using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Speed of the player movement

    Rigidbody rb; // Reference to the Rigidbody component

    private enum Direction { None, Left, Right }

    private Direction CurrentDirection = Direction.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
    }

    // Update is called once per frame
    void Update()
    {
        #region Detect Movement
        CurrentDirection = Direction.None;

#if UNITY_ANDROID  || UNITY_EDITOR
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            float touchX = Touchscreen.current.primaryTouch.position.x.ReadValue();
            if (touchX < Screen.width / 2f)
                CurrentDirection = Direction.Left;
            else
                CurrentDirection = Direction.Right;
        }
#endif

#if UNITY_STANDALONE || UNITY_EDITOR
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
                CurrentDirection = Direction.Left;
            else if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
                CurrentDirection = Direction.Right;
        }
#endif
        #endregion

        // Move the player based on the current direction
        transform.Translate(transform.right * (CurrentDirection == Direction.Right ? 1 : CurrentDirection == Direction.Left ? -1 : 0) * moveSpeed * Time.deltaTime, Space.World);
    }
}
