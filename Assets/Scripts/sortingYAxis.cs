using UnityEngine;

public class SortingOrderController : MonoBehaviour
{
    [SerializeField] private int baseOrder = 5000; // 기준점 (넉넉하게 잡음)
    [SerializeField] private bool isStatic = false; // 건물처럼 안 움직이는건 true
    
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateSortingOrder();
    }

    void LateUpdate()
    {
        // 움직이는 캐릭터라면 매 프레임 위치를 계산해서 정렬 순서를 바꿈
        if (!isStatic)
        {
            UpdateSortingOrder();
        }
    }

    private void UpdateSortingOrder()
    {
        // Y값에 -100을 곱해서 정수로 만듭니다. 
        // Y가 1.5라면 -150, Y가 1.2라면 -120 -> Y가 더 낮은 1.2가 -120으로 더 큰 숫자가 됨 (앞으로 나옴)
        spriteRenderer.sortingOrder = baseOrder - (int)(transform.position.y * 100);
    }
}