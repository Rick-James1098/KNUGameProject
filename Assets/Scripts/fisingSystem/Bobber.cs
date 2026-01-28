using System.Collections;
using UnityEngine;

public class TopDownBobber : MonoBehaviour
{
    [HideInInspector] public FishingSystem fishingSystem;

    private PlayerMovement moveScript;
    public Transform visualChild;
    public AnimationCurve arcCurve; // 포물선 곡선 (인스펙터에서 설정)
    public float bobberHeight = 10f; // 찌 시작 높이
    public float maxDistance = 5f;  // 최대 사거리
    public float duration = 1f;    // 날아가는 시간
    public float heightMult = 2f;   // 포물선 높이

    public enum BobberState { Floating, Biting, Missed }
    public BobberState currentState;

    [Header("이펙트")]
    public GameObject splashPrefab; // 1단계에서 만든 파티클 프리팹
    public LayerMask waterLayer;

    void Start()
    {
        // [추가] 씬에 있는 PlayerMovement 스크립트를 가진 오브젝트를 찾아서 연결합니다.
        moveScript = GameObject.FindObjectOfType<PlayerMovement>();

        if (moveScript == null)
        {
            Debug.LogError("플레이어(PlayerMovement)를 찾을 수 없습니다!");
        }
    }

    public void StartFishing()
    {
        currentState = BobberState.Floating;
        StartCoroutine(BiteLogicRoutine());
    }

    IEnumerator BiteLogicRoutine()
    {
        Debug.Log("입질 대기 중...");

        // 1. 물고기가 물 때까지 무한 반복 (1초마다 체크)
        while (currentState == BobberState.Floating)
        {
            yield return new WaitForSeconds(1.0f); // 1초 대기

            // 50% 확률 체크 (0.0 ~ 1.0 사이의 값 중 0.5보다 작으면 성공)
            if (Random.value < 0.5f)
            {
                // [입질 발생!]
                break; 
            }
            
            // 물지 않았을 때: 찌가 살짝 출렁이는 연출 (선택 사항)
            Debug.Log("물고기가 근처를 지나갑니다...");
        }

        // 2. 진짜 입질 시작 (Biting 상태)
        currentState = BobberState.Biting;
        
        // [연출] 찌가 물속으로 쏙 들어감
        visualChild.localPosition = new Vector3(0, -0.5f, 0);

        // 3. [핵심] 플레이어의 미니게임 스크립트 실행!
        FishingArcGame miniGame = moveScript.GetComponent<FishingArcGame>();
        // TopDownBobber.cs 코루틴 내부
        if (miniGame != null)
        {
            // 1. visualChild.position (찌의 실제 그래픽 위치)
            // 2. -moveScript.lastDir (캐릭터의 뒤쪽 방향)
            miniGame.StartMiniGame(visualChild.position, -fishingSystem.moveScript.lastDir);
        }

        // 4. 제한 시간 (예: 1초 안에 미니게임을 성공 못 하면 도망감)
        yield return new WaitForSeconds(1.0f);

        if (currentState == BobberState.Biting)
        {
            Debug.Log("물고기가 도망갔습니다.");
            currentState = BobberState.Missed;
            miniGame.EndMiniGame(); // 미니게임 UI 끄기
            fishingSystem.RetrieveFishing(); // 실패 회수
        }
    }
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

            StartFishing();
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