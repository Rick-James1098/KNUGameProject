using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FishingArcGame : MonoBehaviour
{
    [Header("위치 보정")]
    public float verticalLeftOffset = 1.0f;

    [Header("연결")]
    public GameObject arcVisual;
    public Transform pointerCircle;

    [Header("타이머 설정")]
    public float timeLimit = 1.0f; // 제한 시간 1초
    private float currentTimer;    // 현재 남은 시간

    [Header("설정")]
    public float grabDistance = 0.5f; // 너무 작으면 클릭하기 힘드니 0.5 정도로 추천

    private bool isGameActive = false;
    private bool isDragging = false;
    private Vector2 targetDirection;
    private Collider2D arcCollider;
    private Vector3 lastCirclePos;

    void Start()
    {
        arcCollider = arcVisual.GetComponentInChildren<Collider2D>();
    }

    public void StartMiniGame(Vector3 bobberPos, Vector2 backDir)
    {
        isGameActive = true;
        isDragging = false;
        currentTimer = timeLimit; // 시작할 때 타이머를 1초로 초기화
        
        targetDirection = backDir.normalized;

        pointerCircle.gameObject.SetActive(true);
        pointerCircle.position = bobberPos;

        arcVisual.SetActive(true);

        Vector3 finalPos = transform.position + (Vector3)(targetDirection * 1.5f);
        if (Mathf.Abs(targetDirection.y) > 0.5f)
        {
            finalPos.x -= verticalLeftOffset;
        }

        finalPos.z = 0;
        arcVisual.transform.position = finalPos;

        arcVisual.transform.up = targetDirection;
        arcVisual.transform.Rotate(0, 0, 90f);
    }

    void Update()
    {
        // 1. 게임 중이 아니면 아무것도 안 함
        if (!isGameActive) return;

        // 2. 타이머는 드래그 여부와 상관없이 항상 흘러야 함
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            Fail();
            return;
        }

        // 3. 마우스 좌표 계산 (반드시 WorldPoint!!)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // 4. 마우스 클릭 시 원 잡기 (아직 드래그 중이 아닐 때 실행되어야 함)
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            Collider2D hit = Physics2D.OverlapPoint(mousePos);
            if (hit != null && hit.transform == pointerCircle)
            {
                isDragging = true;
                Debug.Log("원 잡기 성공!");
            }
        }

        // 5. 드래그 중일 때의 처리
        if (isDragging)
        {
            // 떼는 순간 드래그 해제
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                return;
            }

            // 마우스 계속 누르고 있으면 이동 및 충돌 체크
            if (Input.GetMouseButton(0))
            {
                lastCirclePos = pointerCircle.position;
                pointerCircle.position = mousePos;

                if (IsTouchingArcEnhanced(lastCirclePos, mousePos))
                {
                    Success();
                }
            }
        }
    }
    bool IsTouchingArcEnhanced(Vector2 start, Vector2 end)
    {
        // 1. 현재 위치에서 겹쳐있는지 기본 체크
        if (arcCollider.OverlapPoint(end)) return true;

        // 2. 이전 위치에서 현재 위치까지 '선' 혹은 '원'을 쏴서 그 사이에 호가 있는지 체크 (CircleCast)
        float distance = Vector2.Distance(start, end);
        Vector2 direction = (end - start).normalized;

        // 원의 반지름만큼 두께를 가진 광선을 쏴서 그 경로에 arcCollider가 걸리는지 확인
        RaycastHit2D hit = Physics2D.CircleCast(start, 0.3f, direction, distance);

        if (hit.collider != null && hit.collider == arcCollider)
        {
            return true;
        }

        return false;
    }

    void Success()
    {
        isGameActive = false;
        Debug.Log("제한 시간 내 성공!");
        SceneManager.LoadScene("FishingGimmickScene");
    }

    // [추가] 실패 처리 함수
    void Fail()
    {
        isGameActive = false;
        isDragging = false;
        Debug.Log("시간 초과! 물고기가 도망갔습니다.");
        
        EndMiniGame(); // UI 끄기
        
        // 여기에 FishingSystem의 ResetFishingState()를 호출하는 코드를 추가하면 
        // 캐릭터가 다시 자유롭게 움직일 수 있게 됩니다.
        // GetComponent<FishingSystem>().ResetFishingState();
    }

    public void EndMiniGame()
    {
        isGameActive = false;
        arcVisual.SetActive(false);
        pointerCircle.gameObject.SetActive(false);
    }
}