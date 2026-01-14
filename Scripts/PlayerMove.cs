using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Necesar pentru UI Slider

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMove : MonoBehaviour
{
    [Header("Setari Miscare")]
    public float walkSpeed = 8.0f;  // Viteza de mers (fostul moveSpeed)
    public float runSpeed = 14.0f;  // Viteza de alergare
    public float rotationSpeed = 10f;

    [Header("Sistem Stamina")]
    public Slider staminaSlider;     // Trage Slider-ul aici
    public float maxStamina = 100f;
    public float staminaDrain = 20f; // Cat consuma pe secunda
    public float staminaRegen = 15f; // Cat regenereaza pe secunda
    private float currentStamina;

    [Header("Saritura & Gravitatie")]
    public float jumpForce = 8f;
    public float groundGravityMultiplier = 2f;
    public float airGravityMultiplier = 1.5f;
    public LayerMask groundLayer;

    [Header("Check Sol")]
    public float rayDistance = 0.2f;
    public Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);
    public bool isGrounded;

    [Header("Cooldown Saritura")]
    private float lastJumpTime = 0f;
    private float jumpCooldown = 0.25f;

    [Header("Audio")]
    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 1.0f;

    public AudioClip runSound; // Sunet pasi
    [Range(0.1f, 1f)] public float stepRate = 0.4f;
    [Range(0f, 1f)] public float stepVolume = 0.5f;

    [Header("Audio Respiratie")]
    public AudioSource breathingSource; // Sursa separata pentru respirație (Loop)

    private float nextStepTime = 0f;
    private AudioSource sfxSource; // Sursa pentru pasi/sarituri

    [Header("Referinte")]
    public ThirdPersonCamera cameraScript;
    public Animator animator;

    private Rigidbody rb;
    private Vector2 moveInput;
    private CapsuleCollider playerCollider;

    // Variabila interna pentru viteza curenta
    private float activeSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        // Initializare Audio
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        // Setari Rigidbody
        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Referinte automate
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (cameraScript == null) cameraScript = FindObjectOfType<ThirdPersonCamera>();

        // Initializare Stamina
        currentStamina = maxStamina;
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }

        activeSpeed = walkSpeed;
    }

    void Update()
    {

        if (GameState.GameEnded) return;

        ReadMovementInput();
        CheckGround();

        // Calculam logica de Sprint si Stamina in Update
        HandleStaminaAndSprinting();

        HandleFootsteps();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryJump();
        }

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyCustomGravity();
    }

    // --- LOGICA NOUA PENTRU SPRINT SI STAMINA ---
    void HandleStaminaAndSprinting()
    {
        bool isMoving = moveInput.magnitude > 0.1f;
        bool isSprintingInput = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        // Verificam daca alergam: Apasam Shift + Ne miscam + Avem Stamina + Suntem pe sol
        if (isSprintingInput && isMoving && currentStamina > 0 && isGrounded)
        {
            activeSpeed = runSpeed;

            // Consum stamina
            currentStamina -= staminaDrain * Time.deltaTime;

            // Porneste sunetul de respiratie daca nu merge deja
            if (breathingSource != null && !breathingSource.isPlaying)
            {
                breathingSource.Play();
            }
        }
        else
        {
            activeSpeed = walkSpeed;

            // Regenerare stamina (daca nu e plina)
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegen * Time.deltaTime;
            }

            // Opreste sunetul de respiratie (fade out simplu sau stop direct)
            if (breathingSource != null && breathingSource.isPlaying)
            {
                breathingSource.Stop();
            }
        }

        // Clamp Stamina (sa nu iasa din intervalul 0 - 100)
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // Update UI
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }

    public void TeleportToPosition(Vector3 newPosition)
    {
        rb.linearVelocity = Vector3.zero;
        rb.position = newPosition;
        lastJumpTime = 0;
        Physics.SyncTransforms();
    }

    void TryJump()
    {
        // Putem sari doar daca avem stamina minima? (Optional, acum e scos)
        if (isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            isGrounded = false;
            lastJumpTime = Time.time;

            ManageFriction(false);

            Vector3 vel = rb.linearVelocity;
            vel.y = 0;
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (jumpSound != null)
            {
                sfxSource.PlayOneShot(jumpSound, jumpVolume);
            }

            if (animator) animator.SetTrigger("Jump");
        }
    }

    void HandleFootsteps()
    {
        // Pasii se aud mai des daca alergam
        float currentStepRate = (activeSpeed == runSpeed) ? stepRate * 0.6f : stepRate;

        if (isGrounded && moveInput.magnitude > 0.1f && runSound != null)
        {
            if (Time.time >= nextStepTime)
            {
                sfxSource.pitch = Random.Range(0.9f, 1.1f);
                sfxSource.PlayOneShot(runSound, stepVolume);

                nextStepTime = Time.time + currentStepRate;
            }
        }
        else
        {
            if (moveInput.magnitude < 0.1f)
                nextStepTime = 0;
        }
    }

    void ApplyCustomGravity()
    {
        if (Time.time < lastJumpTime + jumpCooldown) return;

        if (isGrounded)
        {
            rb.AddForce(Physics.gravity * (groundGravityMultiplier - 1f) * rb.mass);
        }
        else
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.AddForce(Physics.gravity * (airGravityMultiplier - 1f) * rb.mass);
            }
        }
    }

    void CheckGround()
    {
        if (Time.time < lastJumpTime + jumpCooldown)
        {
            isGrounded = false;
            ManageFriction(false);
            return;
        }

        Vector3 origin = transform.position + groundCheckOffset;
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, rayDistance + 0.1f, groundLayer))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < 50f)
            {
                isGrounded = true;
                ManageFriction(true);
                return;
            }
        }

        isGrounded = false;
        ManageFriction(false);
    }

    void ManageFriction(bool onGround)
    {
        if (playerCollider == null) return;

        if (onGround)
        {
            playerCollider.material.dynamicFriction = 0.6f;
            playerCollider.material.staticFriction = 0.6f;
            playerCollider.material.frictionCombine = PhysicsMaterialCombine.Average;
            playerCollider.material.bounciness = 0f;
            playerCollider.material.bounceCombine = PhysicsMaterialCombine.Minimum;
        }
        else
        {
            playerCollider.material.dynamicFriction = 0f;
            playerCollider.material.staticFriction = 0f;
            playerCollider.material.frictionCombine = PhysicsMaterialCombine.Minimum;
            playerCollider.material.bounciness = 0f;
            playerCollider.material.bounceCombine = PhysicsMaterialCombine.Minimum;
        }
    }

    void ReadMovementInput()
    {
        if (Keyboard.current == null) return;
        moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
    }

    void ApplyMovement()
    {
        Vector3 forward = cameraScript ? cameraScript.transform.forward : Vector3.forward;
        Vector3 right = cameraScript ? cameraScript.transform.right : Vector3.right;

        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 moveDir = forward * moveInput.y + right * moveInput.x;

        if (moveDir.magnitude > 0)
        {
            // MODIFICARE: Folosim activeSpeed in loc de moveSpeed fix
            Vector3 targetVelocity = moveDir.normalized * activeSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        // Limitare viteza pe Y cand nu e pe sol (Anti-SuperJump)
        if (!isGrounded && rb.linearVelocity.y > 0)
        {
            if (Time.time > lastJumpTime + 0.2f)
            {
                Vector3 clampedVel = rb.linearVelocity;
                clampedVel.y = Mathf.Min(clampedVel.y, 1f);
                rb.linearVelocity = clampedVel;
            }
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Daca alergam, multiplicam valoarea trimisa la animator ca sa se schimbe animatia
        float animationSpeed = moveInput.magnitude;
        if (activeSpeed == runSpeed) animationSpeed *= 2f; // Presupunand ca BlendTree-ul tau are Run la valoarea 2 sau 1+

        animator.SetFloat("Speed", animationSpeed, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    void OnDrawGizmos()
    {
        Vector3 origin = transform.position + groundCheckOffset;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * (rayDistance + 0.1f));
    }
}