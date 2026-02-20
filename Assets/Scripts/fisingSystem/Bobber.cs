using System.Collections;
using UnityEngine;

public class TopDownBobber : MonoBehaviour
{
    public PlayerData data; // 장부
    [HideInInspector] public FishingSystem fishingSystem;
    public Transform visualChild; // 찌 스프라이트 (얘가 점프해야 함)

    public bool isSettled = false;
    
    [Header("포물선 설정")]
    public AnimationCurve arcCurve; // 인스펙터에서 ∩ 모양 확인
    public float bobberHeight = 0f; // 시작 높이 (손 높이)
    public float maxDistance = 7f;
    public float duration = 1f;
    public float heightMult = 2f;   // 점프 높이 배율

    [Header("이펙트")]
    public GameObject splashPrefab;
    public LayerMask waterLayer;

    private void Awake()
    {
        // 1. 미니게임을 위해 나 자신을 장부에 등록 (이것만 추가됨)
        if (data != null) data.currentBobber = this.gameObject;
    }

    public void Launch(Vector2 direction, float power)
    {
        isSettled = false;
        Vector2 start = transform.position;
        Vector2 target = start + (direction * power * maxDistance);
        
        // 2. 코루틴 시작
        StartCoroutine(FlyToTarget(start, target));
    }

    IEnumerator FlyToTarget(Vector2 start, Vector2 target)
    {
        float elapsed = 0;
        
        // 시작할 때 비주얼 위치 초기화
        if (visualChild == null) visualChild = transform.GetChild(0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // [핵심 복구] 
            // 몸체(Root)는 바닥을 따라 직선 이동
            transform.position = Vector2.Lerp(start, target, t);

            // 그림(Visual)은 위로 솟구침 (이 부분이 빠져서 바닥을 기어갔던 것!)
            // t가 0~1로 갈 때 Curve값에 따라 y축이 변함
            float h = Mathf.Lerp(bobberHeight, 0, t) + (arcCurve.Evaluate(t) * heightMult);
            
            // 로컬 좌표 y에 적용해서 점프하는 것처럼 보이게 함
            visualChild.localPosition = new Vector2(0, h);

            yield return null;
        }

        // 도착 후
        visualChild.localPosition = Vector2.zero; // 물에 착수
        CheckLanding(target);
    }

    void CheckLanding(Vector2 landPos)
    {
        // 물인지 땅인지 체크
        Collider2D hit = Physics2D.OverlapPoint(landPos, waterLayer);
        if (hit != null)
        {
            isSettled = true;
            Instantiate(splashPrefab, landPos, Quaternion.identity);
            
            // 시스템에 착수 알림
            if(fishingSystem != null) fishingSystem.OnBobberLanded(visualChild.position);
            
            StartCoroutine(BobbingOnWater());
        }
        else
        {
            // 땅이면 삭제
            Debug.Log("땅에 떨어짐");
            if(data != null) data.ResetCycle();
            
            if(fishingSystem != null) fishingSystem.RetrieveFishing();
            else Destroy(gameObject); // 시스템 없으면 그냥 파괴
        }
    }

    IEnumerator BobbingOnWater()
    {
        float timer = 0;
        while (true)
        {
            timer += Time.deltaTime * 2f;
            float newY = Mathf.Sin(timer) * 0.15f; 
            visualChild.localPosition = new Vector2(0, newY);
            yield return null;
        }
    }

    public void SetBitingVisual(bool isBiting)
    {
        if (isBiting)
        {
            StopAllCoroutines(); // 둥둥 떠 있는 거 멈춤
            visualChild.localPosition = new Vector3(0, -0.5f, 0); // 물속으로 쏙
            
            // 미니게임 시작 신호 (이게 필요해서 수정한 것)
            if (data != null) data.isStrikeGameActive = true;
        }
    }
}