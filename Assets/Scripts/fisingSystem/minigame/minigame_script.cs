using UnityEngine;
using UnityEngine.SceneManagement;

public class FishingArcGame : MonoBehaviour
{
    public PlayerData data;

    [Header("UI 연결")]
    public GameObject arcVisual;      // 부채꼴 (호)
    public Transform pointerCircle;   // 드래그할 원

    [Header("설정")]
    public float timeLimit = 3.0f;
    public float arcDistance = 1.0f;  // 플레이어 뒤통수에서 떨어질 거리
    public float sideOffset = 1.0f;   // 위/아래 볼 때 좌우로 이동할 거리

    private float currentTimer;
    private bool isDragging = false;
    private Collider2D arcCollider;
    private Transform playerTransform;

    // 호의 원래 크기를 기억할 변수
    private Vector3 originalArcScale;
    
    // [추가] 빠른 드래그 관통 방지를 위한 이전 위치 저장
    private Vector3 lastPointerPos; 

    void Start()
    {
        arcCollider = arcVisual.GetComponentInChildren<Collider2D>();
        
        originalArcScale = arcVisual.transform.localScale;

        arcVisual.SetActive(false);
        pointerCircle.gameObject.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (data.isStrikeGameActive && !arcVisual.activeSelf)
        {
            StartUI();
        }

        if (!data.isStrikeGameActive) return;

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0) { Fail(); return; }

        HandleInput();
    }

    void StartUI()
    {
        currentTimer = timeLimit;

        if (playerTransform != null)
        {
            Vector2 lookDir = data.lastDirection.normalized;
            if (lookDir == Vector2.zero) lookDir = Vector2.down;

            Vector3 spawnPos = playerTransform.position - (Vector3)(lookDir * arcDistance);
            
            if (lookDir.y > 0.5f) spawnPos.x += sideOffset; 
            else if (lookDir.y < -0.5f) spawnPos.x -= sideOffset; 

            spawnPos.z = 0; 
            arcVisual.transform.position = spawnPos;

            arcVisual.transform.right = lookDir; 

            Vector3 currentScale = originalArcScale;
            if (lookDir.x < -0.5f) 
            {
                currentScale.x = -Mathf.Abs(originalArcScale.y); 
            }
            else
            {
                currentScale.x = Mathf.Abs(originalArcScale.y);
            }
            arcVisual.transform.localScale = currentScale;
        }

        if (data.currentBobber != null)
        {
            Vector3 bobberPos = data.currentBobber.transform.position;
            bobberPos.z = 0;
            pointerCircle.position = bobberPos;
        }

        arcVisual.SetActive(true);
        pointerCircle.gameObject.SetActive(true);
    }

    void HandleInput()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // 1. 드래그 시작 감지
        if (Input.GetMouseButtonDown(0))
        {
            float dist = Vector2.Distance(mousePos, pointerCircle.position);
            if (dist < 0.8f) 
            {
                isDragging = true;
                lastPointerPos = pointerCircle.position; // 시작 위치 저장
            }
        }

        // 2. 드래그 중 처리
        if (isDragging)
        {
            // --- [핵심 수정] 빠른 속도 관통 방지 (Linecast) ---
            // 이전 프레임 위치부터 현재 마우스 위치까지 레이저를 쏴서 충돌체가 있는지 검사
            RaycastHit2D[] hits = Physics2D.LinecastAll(lastPointerPos, mousePos);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == arcCollider)
                {
                    Debug.Log("성공! 닿자마자 판정 (Linecast)");
                    pointerCircle.position = mousePos; 
                    Success();
                    return; // 함수 즉시 종료
                }
            }

            // --- 점 판정 (마우스 현재 위치가 호 안에 들어왔는지 안전장치) ---
            if (arcCollider.OverlapPoint(mousePos))
            {
                Debug.Log("성공! 닿자마자 판정 (OverlapPoint)");
                pointerCircle.position = mousePos; 
                Success();
                return; 
            }

            // 호에 닿지 않은 채로 마우스를 뗐다면 실패 처리
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                Debug.Log("실패! 호에 닿기 전에 손을 뗐습니다.");
                Fail();
                return;
            }

            // 시각적 이동 및 이전 위치 갱신
            pointerCircle.position = mousePos;
            lastPointerPos = mousePos; 
        }
    }

    void Success()
    {
        data.isStrikeGameActive = false;
        arcVisual.SetActive(false);
        pointerCircle.gameObject.SetActive(false);
        isDragging = false;
        
        if (data.currentBobber != null) Destroy(data.currentBobber);

        SceneManager.LoadScene("ReelGimic");
    }

    void Fail()
    {
        data.ResetCycle();
        arcVisual.SetActive(false);
        pointerCircle.gameObject.SetActive(false);
        isDragging = false;

        if (data.currentBobber != null) Destroy(data.currentBobber);

        FishingSystem fSystem = FindObjectOfType<FishingSystem>();
        if (fSystem != null)
        {
            fSystem.RetrieveFishing();
        }
    }
}