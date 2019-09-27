using UnityEngine;

public class BearController : MonoBehaviour
{
    public float speed;
    public float acceleration;
    public float gravity;

    Vector2 velocity;
    float velocityXSmoothing;

    Vector2 rcTopLeft;
    Vector2 rcTopRight;
    Vector2 rcBottomLeft;
    Vector2 rcBottomRight;

    int horizontalRayCount;
    int verticalRayCount;
    float horizontalRaySpacing;
    float verticalRaySpacing;

    const float distBetweenRays = 0.08f;
    const float boxColSkinWidth = 0.015f;

    public LayerMask collisionMask;
    CollisionInfo collisionInfo;

    enum State { Normal, Jumping, Crouching, Climbing, Dying };
    State curState = State.Normal;

    Animator animator;
    SpriteRenderer spriteRen;
    BoxCollider2D boxCol;
    public SimpleObject bearDeath;



    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRen = GetComponent<SpriteRenderer>();
        boxCol = GetComponent<BoxCollider2D>();

        CalculateRaySpacing();
    }

    void Update()
    {
        //Movement stuff
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (curState != State.Dying)
        {
            float targetVelocityX = directionalInput.x * speed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, acceleration);
            if (curState != State.Jumping) velocity.y -= gravity;

            Vector2 moveAmount = velocity * Time.deltaTime;

            UpdateRaycastPositions();

            collisionInfo.Reset();
            HorizontalCollisions(ref moveAmount);
            if (moveAmount.y != 0) VerticalCollisions(ref moveAmount);

            transform.Translate(moveAmount);

            //Flip sprite depending on direction
            if (directionalInput.x != 0)
                if ((spriteRen.flipX == false && directionalInput.x < 0) || (spriteRen.flipX == true && directionalInput.x > 0))
                    spriteRen.flipX = !spriteRen.flipX;

            animator.SetBool("Walking", (directionalInput.x != 0) ? true : false);
        }

        switch (curState)
        {
            case State.Normal:
                if (collisionInfo.below && directionalInput.y == 1)
                {
                    RaycastHit2D hit = Physics2D.Raycast(new Vector2(boxCol.bounds.min.x, boxCol.bounds.center.y), Vector2.right, boxCol.bounds.size.x, collisionMask);
                    Debug.DrawRay(new Vector2(boxCol.bounds.min.x, boxCol.bounds.center.y), Vector2.right * boxCol.bounds.size.x, Color.red);
                    if (hit)
                    {
                        curState = State.Climbing;
                        animator.Play("Climb");
                        LeanTween.moveY(gameObject, transform.position.y + 0.55f, 0.25f).setEase(LeanTweenType.easeInOutExpo).setOnComplete(() =>
                        {
                            curState = State.Normal;
                        });
                    }
                    else
                    {
                        curState = State.Jumping;
                        animator.Play("Jump");
                    }
                }
                else if (collisionInfo.below && directionalInput.y == -1)
                {
                    curState = State.Crouching;
                    animator.Play("Crouch");
                }
                break;

            case State.Jumping:
                break;

            case State.Crouching:
                break;

            case State.Dying:
                break;
        }
    }

    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = (spriteRen.flipX) ? -1 : 1;
        float rayLength = Mathf.Abs(moveAmount.x) + boxColSkinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == 1) ? rcBottomRight : rcBottomLeft;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                collisionInfo.left = directionX == -1;
                collisionInfo.right = directionX == 1;
            }
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + boxColSkinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? rcBottomLeft : rcTopLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                if (hit.distance != 0)
                {
                    moveAmount.y = (hit.distance - boxColSkinWidth) * directionY;
                    velocity.y = 0;
                }

                collisionInfo.above = directionY == 1;
                collisionInfo.below = directionY == -1;
            }
        }
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = boxCol.bounds;
        bounds.Expand(boxColSkinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        //Clamp the ray counts to between 2 and very big
        horizontalRayCount = Mathf.RoundToInt(boundsHeight / distBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / distBetweenRays);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    void UpdateRaycastPositions()
    {
        CalculateRaySpacing();

        Bounds bounds = boxCol.bounds;
        bounds.Expand(boxColSkinWidth * -2);

        rcTopLeft = new Vector2(bounds.min.x, bounds.max.y);
        rcTopRight = new Vector2(bounds.max.x, bounds.max.y);
        rcBottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        rcBottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    void KillBear()
    {
        curState = State.Dying;
        Instantiate(bearDeath, new Vector2(transform.position.x, transform.position.y + 0.5f), Quaternion.identity);
        transform.position = new Vector2(-3, 0);
        spriteRen.enabled = false;
        velocity = Vector2.zero;
        animator.Play("Stand");
        LeanTween.cancel(gameObject);

        LeanTween.delayedCall(gameObject, 1f, () =>
        {
            spriteRen.enabled = true;
            curState = State.Normal;
        });
    }

    void SetState(State newState)
    {
        curState = newState;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "DeathEntity")
        {
            KillBear();
        }
    }

    struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public void Reset()
        {
            above = below = false;
            left = right = false;
        }
    }
}