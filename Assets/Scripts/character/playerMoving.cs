using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    // [추가] 외부(FishingSystem)에서 참조할 마지막 방향 데이터
    public Vector2 lastDir = Vector2.down;
    public bool canMove = true; // 낚시 중일때 외부에서 변경

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 1. 스프라이트 뒤집기 (h가 0이 아닐 때만)
        if (h != 0)
        {
            // 캐릭터의 스케일 값을 조절하여 전체를 반전시킵니다.
            // h가 1이면 오른쪽(원래 방향), -1이면 왼쪽(반전)
            // 주의: 유저님의 원래 스프라이트가 '왼쪽'을 보고 있다면 h > 0 ? -1 : 1 로 설정하세요.
            float direction = (h > 0) ? -1f : 1f; 
            transform.localScale = new Vector3(direction, 1, 1);
        }

        // 2. 대각선 이동 방지 로직
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

        // [핵심 추가] 움직임이 있을 때만 lastDir를 업데이트합니다.
        // 이렇게 하면 캐릭터가 멈춰도 마지막 방향을 계속 기억합니다.
        if (moveInput != Vector2.zero)
        {
            lastDir = moveInput;
        }

        // 3. 애니메이터에 값 전달
        if (animator != null)
        {
            if (moveInput != Vector2.zero)
            {
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
        if (!canMove) 
        {
            rb.linearVelocity = Vector2.zero; // 속도 초기화
            return; 
        }
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}