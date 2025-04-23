using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float XLimit = 5f;

    [Header("Jump/Slide Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float slideCooldown = 0.5f;

    [Header("Touch Settings")]
    [SerializeField] private float swipeThreshold = 100f;
    [SerializeField] private float maxSwipeTime = 0.5f;

    [Header("Tilt Settings")]
    [SerializeField] private float tiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 5f;

    private Rigidbody rb;
    private Animator anim;
    private CapsuleCollider col;

    private Vector2 touchStartPos;
    private float touchStartTime;
    private Coroutine jumpRoutine;

    private bool isGrounded = true;
    private bool canSlide = true;
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;
    private float groundY;

    private enum Direction { None, Left, Right }
    private Direction CurrentDirection = Direction.None;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();

        originalColliderCenter = col.center;
        originalColliderHeight = col.height;
        groundY = transform.position.y;
    }

    void Update()
    {
        DetectInput();
        MoveHorizontal();
        UpdateTilt();
        UpdateAnimator();
    }

    private void DetectInput()
    {
        CurrentDirection = Direction.None;

        // Touch input (for mobile WebGL and Android)
        var touch = Touchscreen.current;
        if (touch != null)
        {
            // Touch began
            if (touch.primaryTouch.press.wasPressedThisFrame)
            {
                touchStartPos = touch.primaryTouch.position.ReadValue();
                touchStartTime = Time.time;
            }
            // Touch held → horizontal movement
            if (touch.primaryTouch.press.isPressed)
            {
                float tx = touch.primaryTouch.position.x.ReadValue();
                CurrentDirection = tx < Screen.width / 2f ? Direction.Left : Direction.Right;
            }
            // Touch released → swipe up/down
            if (touch.primaryTouch.press.wasReleasedThisFrame)
            {
                Vector2 endPos = touch.primaryTouch.position.ReadValue();
                float deltaY = endPos.y - touchStartPos.y;
                float deltaTime = Time.time - touchStartTime;
                if (deltaTime <= maxSwipeTime && Mathf.Abs(deltaY) >= swipeThreshold)
                {
                    if (deltaY > 0) TryJump();
                    else TrySlide();
                }
                CurrentDirection = Direction.None;
            }
        }

        // Keyboard input (for desktop and WebGL)
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                CurrentDirection = Direction.Left;
            else if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                CurrentDirection = Direction.Right;

            if (keyboard.spaceKey.wasPressedThisFrame)
                TryJump();
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
                TrySlide();
        }
    }

    private void MoveHorizontal()
    {
        float dir = CurrentDirection == Direction.Right ? 1f :
                    CurrentDirection == Direction.Left ? -1f : 0f;
        Vector3 move = Vector3.right * dir * moveSpeed;
        rb.MovePosition(rb.position + move * Time.deltaTime);

        Vector3 p = rb.position;
        p.x = Mathf.Clamp(p.x, -XLimit, XLimit);
        rb.position = p;
    }

    private void UpdateTilt()
    {
        float targetZ = 0f;
        if (!anim.GetBool("isJumping") && !anim.GetBool("isSliding"))
        {
            if (CurrentDirection == Direction.Right) targetZ = -tiltAngle;
            if (CurrentDirection == Direction.Left) targetZ = tiltAngle;
        }
        Quaternion currentRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(
            currentRot.eulerAngles.x,
            currentRot.eulerAngles.y,
            targetZ);
        transform.rotation = Quaternion.Slerp(currentRot, targetRot, tiltSpeed * Time.deltaTime);
    }

    private void TryJump()
    {
        if (isGrounded && !anim.GetBool("isSliding"))
        {
            isGrounded = false;
            anim.SetBool("isJumping", true);
            if (jumpRoutine != null) StopCoroutine(jumpRoutine);
            jumpRoutine = StartCoroutine(JumpRoutine());
        }
    }

    private IEnumerator JumpRoutine()
    {
        float duration = 0.5f;
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float yOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            transform.position = startPos + Vector3.up * yOffset;
            yield return null;
        }
        transform.position = startPos;
        anim.SetBool("isJumping", false);
        isGrounded = true;
    }

    private void TrySlide()
    {
        if (isGrounded && canSlide && !anim.GetBool("isJumping"))
            StartCoroutine(SlideRoutine());
    }

    private IEnumerator SlideRoutine()
    {
        canSlide = false;
        anim.SetBool("isSliding", true);
        col.height = originalColliderHeight / 2f;
        col.center = originalColliderCenter - Vector3.up * (originalColliderHeight / 4f);
        yield return new WaitForSeconds(slideDuration);
        col.height = originalColliderHeight;
        col.center = originalColliderCenter;
        anim.SetBool("isSliding", false);
        yield return new WaitForSeconds(slideCooldown);
        canSlide = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("isJumping", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("JumpObstacle") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            Die(other.gameObject);
        if (other.CompareTag("SlideObstacle") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Slide"))
            Die(other.gameObject);
        if (other.CompareTag("Enemy"))
            Die(other.gameObject);
    }

    private void Die(GameObject obstacle)
    {
        obstacle.SetActive(false);
        anim.SetBool("isDead", true);
        enabled = false;
        if (jumpRoutine != null)
        {
            StopCoroutine(jumpRoutine);
            anim.SetBool("isJumping", false);
            anim.SetBool("isSliding", false);
        }
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                                               transform.rotation.eulerAngles.y,
                                               0f);
        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
        GameManager.Instance.Restart();
    }

    private void UpdateAnimator()
    {
        float normSpeed = GameManager.Instance.GetCurrentSpeed() / 8f > 1 ? GameManager.Instance.GetCurrentSpeed() : 1;
        anim.SetFloat("Speed", Mathf.Abs(normSpeed > 1f ? 1f : normSpeed));
    }
}
