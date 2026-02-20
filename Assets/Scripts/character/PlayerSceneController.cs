using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneHandler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;
    private PlayerMovement moveScript;
    private FishingSystem fishingScript;
    private LineRenderer lineRenderer;

    [Header("자식 오브젝트들")]
    public GameObject fishingTool;        // 낚시 도구 본체
    public GameObject rodHolder;         // 낚싯대 홀더

    void Awake()
    {
        // 본체에 붙은 컴포넌트들을 미리 다 찾아둠
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        moveScript = GetComponent<PlayerMovement>();
        fishingScript = GetComponent<FishingSystem>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 배틀 씬이라면 기능 정지, 아니면 다시 시작
        bool isBattle = (scene.name == "ReelGimic");
        SetPlayerActive(!isBattle);
    }

    private void SetPlayerActive(bool isActive)
    {
        // 1. 본체 컴포넌트들
        if (spriteRenderer != null) spriteRenderer.enabled = isActive;
        if (fishingScript != null) fishingScript.enabled = isActive;
        
        // 2. 자식 오브젝트들 (도구와 홀더)
        if (fishingTool != null) fishingTool.SetActive(isActive);
        if (rodHolder != null) rodHolder.SetActive(isActive);

        // 3. 라인 렌더러 (낚싯줄) 강제 종료
        if (lineRenderer != null) lineRenderer.enabled = isActive;

        // 4. 배틀 씬으로 갈 때(isActive가 false일 때) 모든 낚시 상태 강제 리셋
        if (!isActive && fishingScript != null)
        {
            fishingScript.ResetFishingState();
        }
    }
}