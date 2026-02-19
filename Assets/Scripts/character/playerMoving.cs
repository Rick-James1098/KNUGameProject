using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Data Reference")]
    public PlayerData data;

    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public bool canMove = true; 

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 중복 방지 및 파괴 방지
        var objs = FindObjectsOfType<PlayerMovement>();
        if (objs.Length > 1) { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // 낚시 중이거나 UI가 열려있으면 이동 불가
        if (!canMove || data.isFishing || (InventoryUI.Instance != null && InventoryUI.Instance.IsAnyUIOpen))
        {
            moveInput = Vector2.zero;
            if (animator != null) animator.SetBool("isMoving", false);
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 대각선 이동 방지 및 입력 처리
        if (h != 0) moveInput = new Vector2(h, 0);
        else if (v != 0) moveInput = new Vector2(0, v);
        else moveInput = Vector2.zero;

        if (moveInput != Vector2.zero)
        {
            // 장부에 마지막 방향 기록
            data.lastDirection = moveInput;
            
            // 스프라이트 반전 (localScale 방식 유지)
            float direction = (moveInput.x > 0) ? -1f : 1f;
            if (moveInput.x != 0) transform.localScale = new Vector3(direction, 1, 1);
            else transform.localScale = new Vector3(-1f, 1, 1); // 위/아래 이동 시 기본값
        }

        // 애니메이션 파라미터 전달
        if (animator != null)
        {
            animator.SetBool("isMoving", moveInput != Vector2.zero);
            if (moveInput != Vector2.zero)
            {
                animator.SetFloat("InputX", moveInput.x);
                animator.SetFloat("InputY", moveInput.y);
            }
        }
    }

    void FixedUpdate()
    {
        if (canMove && !data.isFishing)
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        else
            rb.linearVelocity = Vector2.zero;
    }
}