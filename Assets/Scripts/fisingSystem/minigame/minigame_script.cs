using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening; 

public class FishingArcGame : MonoBehaviour
{
    public PlayerData data;

    [Header("UI 연결")]
    public GameObject arcVisual;      // 부채꼴 (호)
    public Transform pointerCircle;   // 드래그할 원 (빨간 원)

    [Header("설정")]
    public float timeLimit = 3.0f;
    public float arcDistance = 1.0f;  
    public float sideOffset = 1.0f;   
    
    [Header("애니메이션 설정")]
    public float jumpHeight = 1.0f;   // 빨간 원이 튀어오를 높이
    // [수정됨] 0.5f -> 0.2f로 줄여서 찌와 더 가까운(낮은) 곳에 착지하게 함
    public float landOffset = 0.2f;   

    private float currentTimer;
    private bool isDragging = false;
    private Collider2D arcCollider;
    private Transform playerTransform;

    private Vector3 originalArcScale;
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

        // === [1. 파란색 호(arcVisual) 애니메이션] ===
        if (playerTransform != null)
        {
            Vector2 lookDir = data.lastDirection.normalized;
            if (lookDir == Vector2.zero) lookDir = Vector2.down;

            Vector3 spawnPos = playerTransform.position - (Vector3)(lookDir * arcDistance);
            if (lookDir.y > 0.5f) spawnPos.x += sideOffset; 
            else if (lookDir.y < -0.5f) spawnPos.x -= sideOffset; 
            spawnPos.z = 0; 
            
            arcVisual.transform.right = lookDir; 
            Vector3 targetScale = originalArcScale;
            if (lookDir.x < -0.5f) targetScale.x = -Mathf.Abs(originalArcScale.y); 
            else targetScale.x = Mathf.Abs(originalArcScale.y);

            arcVisual.transform.DOKill(); 
            
            arcVisual.transform.position = playerTransform.position;
            arcVisual.transform.localScale = Vector3.zero;

            SpriteRenderer arcSprite = arcVisual.GetComponentInChildren<SpriteRenderer>();
            if (arcSprite != null)
            {
                Color c = arcSprite.color;
                c.a = 0f;
                arcSprite.color = c;
                
                arcSprite.DOFade(1f, 0.5f);
            }

            arcVisual.SetActive(true);

            // [수정됨] Ease.OutBack(바운스) -> Ease.OutQuad(부드럽게 커지다 멈춤)로 변경
            arcVisual.transform.DOMove(spawnPos, 0.5f).SetEase(Ease.OutQuad);
            arcVisual.transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutQuad);
        }

        // === [2. 빨간색 원(pointerCircle) 애니메이션] ===
        if (data.currentBobber != null)
        {
            pointerCircle.DOKill(); 

            Vector3 bobberPos = data.currentBobber.transform.position;
            bobberPos.z = 0;

            pointerCircle.position = bobberPos;
            pointerCircle.gameObject.SetActive(true);

            // [수정됨] landOffset(0.2f)이 적용되어 찌보다 살~짝만 높게 착지함
            Vector3 targetLandPos = bobberPos + new Vector3(0, landOffset, 0);

            pointerCircle.DOJump(targetLandPos, jumpHeight, 1, 0.5f).SetEase(Ease.OutQuad);
            
            lastPointerPos = targetLandPos; 
        }
    }

    void HandleInput()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        if (Input.GetMouseButtonDown(0))
        {
            float dist = Vector2.Distance(mousePos, pointerCircle.position);
            if (dist < 0.8f) 
            {
                isDragging = true;
                lastPointerPos = pointerCircle.position; 
            }
        }

        if (isDragging)
        {
            RaycastHit2D[] hits = Physics2D.LinecastAll(lastPointerPos, mousePos);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == arcCollider)
                {
                    pointerCircle.position = mousePos; 
                    Success();
                    return; 
                }
            }

            if (arcCollider.OverlapPoint(mousePos))
            {
                pointerCircle.position = mousePos; 
                Success();
                return; 
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                Fail();
                return;
            }

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