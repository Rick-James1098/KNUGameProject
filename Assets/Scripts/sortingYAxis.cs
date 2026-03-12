using UnityEngine;
using UnityEngine.Rendering; // ⭐ SortingGroup을 사용하기 위해 반드시 추가해야 합니다!

public class SortingOrderController : MonoBehaviour
{
    [SerializeField] private int baseOrder = 5000; // 기준점 (넉넉하게 잡음)
    [SerializeField] private bool isStatic = false; // 건물처럼 안 움직이는건 true
    
    private SpriteRenderer spriteRenderer;
    private SortingGroup sortingGroup; // ⭐ 비닐봉지(SortingGroup) 변수 추가

    void Awake()
    {
        // 게임이 시작될 때 두 가지를 모두 찾아봅니다.
        sortingGroup = GetComponent<SortingGroup>();
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
        int newOrder = baseOrder - (int)(transform.position.y * 100);

        // 1순위: 만약 이 오브젝트에 SortingGroup(비닐봉지)이 있다면? 
        // -> 캐릭터 본체의 SpriteRenderer는 내버려 두고, 비닐봉지 전체의 순서를 바꿉니다!
        if (sortingGroup != null)
        {
            sortingGroup.sortingOrder = newOrder;
        }
        // 2순위: SortingGroup은 없고 SpriteRenderer만 있다면?
        // -> 기존처럼 평범하게 나무나 일반 몬스터의 순서를 바꿉니다. (다른 문제 안 생김!)
        else if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = newOrder;
        }
    }
}