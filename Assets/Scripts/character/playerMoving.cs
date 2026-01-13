using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f; // 이동 속도

    private Rigidbody2D rb;
    private Animator animator;

    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    void Awake()
    {
        // 컴포넌트 연결
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. WASD 입력 받기 (Horizontal: A/D, Vertical: W/S)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0)
        {
            // h가 0보다 작으면(왼쪽 이동) true, 0보다 크면(오른쪽 이동) false
            // 즉, 왼쪽으로 갈 때만 그림을 X축으로 뒤집습니다.
            spriteRenderer.flipX = (h > 0);
        }

        // 2. 대각선 이동 방지 로직 (한 번에 한 축만 입력받기)
        if (h != 0)
        {
            moveInput = new Vector2(h, 0);
        }
        else if (v != 0)
        {
            moveInput = new Vector2(0, v);
        }
        else
        {
            moveInput = Vector2.zero;
        }

        // 3. 애니메이터에 값 전달 (애니메이션이 있을 경우)
        if (animator != null)
        {
            if (moveInput != Vector2.zero)
            {
                // 움직이는 중일 때만 방향 값 업데이트 (멈췄을 때 마지막 방향 유지)
                animator.SetFloat("InputX", moveInput.x);
                animator.SetFloat("InputY", moveInput.y);
                animator.SetBool("isMoving", true);
            }
            else
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    void FixedUpdate()
    {
        // 4. 실제 물리 이동 처리
        // FixedUpdate는 물리 연산을 위해 일정한 간격으로 실행됩니다.
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}