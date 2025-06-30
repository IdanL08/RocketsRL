using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRocketController : MonoBehaviour
{
    [Header("Rocket Settings")]
    public float boostAcceleration = 10f;
    public float maxVerticalSpeed = 8f;
    public float fuel = 100f;
    public float fuelConsumptionPerSecond = 10f;

    [Header("Bounce Settings")]
    public float bounceMultiplier = 0.5f;
    public float minBounceVelocity = 3f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private int screenWidth;
    private float lastFallSpeed = 0f;

    void Start()
    {
        Application.targetFrameRate = 60;
        rb = GetComponent<Rigidbody2D>();
        screenWidth = Screen.width;
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        // Lock X position
        rb.position = new Vector2(0f, rb.position.y);

        // Track downward speed BEFORE Unity collision dampens it
        if (rb.linearVelocity.y < 0f)
            lastFallSpeed = -rb.linearVelocity.y;
        else
            lastFallSpeed = 0f;

        // Clamp vertical speed to avoid insane physics
        Vector2 velocity = rb.linearVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -maxVerticalSpeed, maxVerticalSpeed);
        rb.linearVelocity = velocity;
    }

    void HandleInput()
    {
        Vector2? inputPos = null;

        if (Input.touchCount > 0)
            inputPos = Input.GetTouch(0).position;
        else if (Input.GetMouseButton(0))
            inputPos = Input.mousePosition;

        if (inputPos.HasValue && fuel > 0)
        {
            float direction = inputPos.Value.x < screenWidth / 2 ? 1f : -1f;
            rb.AddForce(Vector2.up * direction * boostAcceleration, ForceMode2D.Force);
            fuel -= fuelConsumptionPerSecond * Time.deltaTime;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only bounce if we hit something in the groundLayer
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            if (lastFallSpeed >= minBounceVelocity)
            {
                float bounceSpeed = lastFallSpeed * bounceMultiplier;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceSpeed);
            }
        }
    }
}
