using System.Collections;
using UnityEngine;

public class TopDownBobber : MonoBehaviour
{
    [HideInInspector] public FishingSystem fishingSystem;
    public Transform visualChild;
    public AnimationCurve arcCurve; // 포물선 곡선 (인스펙터에서 설정)
    public float bobberHeight = 10f; // 찌 시작 높이
    public float maxDistance = 5f;  // 최대 사거리
    public float duration = 1f;    // 날아가는 시간
    public float heightMult = 2f;   // 포물선 높이

    [Header("이펙트")]
    public GameObject splashPrefab; // 1단계에서 만든 파티클 프리팹
    public LayerMask waterLayer;

    public void Launch(Vector2 direction, float power)
    {
        // 1. 착지 지점 계산 (현재 위치 + 방향 * 파워 * 최대사거리)
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + (direction * power * maxDistance);

        // 2. 코루틴으로 이동 시작
        StartCoroutine(FlyToTarget(startPos, targetPos));
    }

    IEnumerator FlyToTarget(Vector2 start, Vector2 target)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 1. 부모(그림자/바닥 위치)는 바닥을 따라 직선 이동
            transform.position = Vector2.Lerp(start, target, t);

            // 2. 자식(실제 찌)의 높이 계산
            // (시작할 때 손 높이에서 서서히 0(바닥)으로 내려가는 값) + (포물선 곡선 값)
            float currentHandHeight = Mathf.Lerp(bobberHeight, 0, t); 
            float arcHeight = arcCurve.Evaluate(t) * heightMult;

            visualChild.localPosition = new Vector2(0, currentHandHeight + arcHeight);

            yield return null;
        }
        
        // 착지 후 손 높이를 0으로 고정
        visualChild.localPosition = Vector2.zero;
        CheckLanding(target);
    }

    void CheckLanding(Vector2 landPos)
    {
        // 1. 해당 위치에 물이 있는지 확인 (OverlapPoint 사용)
        Collider2D hit = Physics2D.OverlapPoint(landPos, waterLayer);

        if (hit != null)
        {
            // 2. [물에 빠짐]
            Debug.Log("퐁당! 물에 빠졌습니다.");
            
            // 이펙트 생성
            Instantiate(splashPrefab, landPos, Quaternion.identity);
            
            // 물 위에서 둥둥 떠 있는 상태로 전환
            StartCoroutine(BobbingOnWater());
        }
        else
        {
            // 3. [땅에 떨어짐]
            Debug.Log("터벅. 땅입니다.");
            // 실패 처리 (예: 잠깐 대기 후 찌 파괴 또는 캐릭터에게 돌아오기)
            fishingSystem.ResetFishingState();
            Destroy(gameObject);
        }
    }
    IEnumerator BobbingOnWater()
    {
        Vector2 basePos = visualChild.localPosition;
        float timer = 0;
        
        while (true) // 낚시를 낚을 때까지 계속 반복
        {
            timer += Time.deltaTime * 2f; // 속도 조절
            float newY = Mathf.Sin(timer) * 0.5f; // 0.5만큼 위아래로 흔들림
            
            visualChild.localPosition = new Vector2(basePos.x, basePos.y + newY + 0.5f);
            yield return null;
        }
    }
}