using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // 씬 전환 이벤트를 받기 위해 필수!

public class FishingResultHandler : MonoBehaviour
{
    [Header("Data Reference")]
    public PlayerData playerData;

    [Header("UI Containers")]
    public GameObject rewardPanel;        // 보상 전체 패널 (평소엔 꺼져있음)
    public CanvasGroup textGroup;         // 이름, 등급 등 투명해질 텍스트 그룹
    public RectTransform fishIconRT;      // 날아갈 물고기 아이콘

    [Header("UI Data Elements")]
    public Image fishIconImage;
    public TextMeshProUGUI fishName;
    public TextMeshProUGUI fishRarity;    // 필요 없으면 안 써도 무방

    [Header("Animation Settings")]
    public RectTransform bucketTarget;    // 메인 캔버스에 있는 고기통 UI
    public float displayDuration = 1.0f;  // 중앙에서 멈춰있는 시간
    public float moveDuration = 0.7f;     // 날아가는 시간
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector2 iconInitialLocalPos;  

    void Awake()
    {
        // 1. 나중에 제자리로 돌아오기 위해 아이콘의 원래 위치 기억
        if (fishIconRT != null)
        {
            iconInitialLocalPos = fishIconRT.anchoredPosition;
        }
        
        // 2. 시작할 때 보상 패널은 꺼두기
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        // 스크립트가 활성화될 때 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 스크립트가 비활성화될 때 이벤트 구독 해제 (메모리 누수 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로딩이 완료될 때마다 유니티가 자동으로 호출해주는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // [주의] "InGameScene" 자리에 실제 메인 씬(맵이 있는 곳) 이름을 정확히 적어주세요!
        if (scene.name == "InGameScene") 
        {
            // 장부에 고기를 잡고 돌아왔다는 기록이 있다면
            if (playerData != null && playerData.hookedFish != null && playerData.isBattleSuccess)
            {
                // 두 번 실행되는 걸 막기 위해 성공 플래그를 즉시 해제
                playerData.isBattleSuccess = false; 
                
                ReceiveReward();
            }
        }
    }

    void ReceiveReward()
    {
        // 1. 인벤토리(고기통)에 실제 아이템 추가
        InventoryManager.Instance?.AddItem(playerData.hookedFish);

        // 2. UI에 물고기 정보 세팅
        FishData data = playerData.hookedFish;
        if (fishIconImage != null) fishIconImage.sprite = data.icon;
        if (fishName != null) fishName.text = data.name;

        // 3. 연출 코루틴 시작
        StartCoroutine(RewardFlowSequence());
    }

    IEnumerator RewardFlowSequence()
    {
        // [안전장치] 필수 UI가 파괴되었거나 없으면 에러 없이 장부만 비우고 종료
        if (fishIconRT == null || textGroup == null || rewardPanel == null)
        {
            Debug.LogWarning("UI 요소가 유실되어 보상 연출을 생략합니다.");
            playerData.ResetCycle();
            yield break; 
        }

        // --- 초기화 (이전 연출의 잔재를 지우기 위함) ---
        textGroup.alpha = 1f;
        fishIconRT.anchoredPosition = iconInitialLocalPos;
        fishIconRT.localScale = Vector3.one;
        rewardPanel.SetActive(true);

        // --- 1단계: 중앙에서 짠! 하고 잠시 대기 ---
        yield return new WaitForSeconds(displayDuration);

        // --- 2단계: 고기통으로 날아가기 ---
        Vector3 startPos = fishIconRT.position;
        Vector3 endPos = (bucketTarget != null) ? bucketTarget.position : fishIconRT.position;
        
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            // 이동 도중 씬이 꺼지거나 UI가 파괴되면 안전하게 코루틴 중단
            if (fishIconRT == null || textGroup == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            float curveT = moveCurve.Evaluate(t);

            // 텍스트는 서서히 투명하게 (Fade Out)
            textGroup.alpha = 1f - t;
            
            // 아이콘은 고기통 위치로 이동하면서 크기가 작아짐
            fishIconRT.position = Vector3.Lerp(startPos, endPos, curveT);
            fishIconRT.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.3f, 0.3f, 1f), curveT);

            yield return null;
        }

        // --- 3단계: 연출 종료 후 정리 ---
        if (rewardPanel != null) rewardPanel.SetActive(false);
        
        // [매우 중요] 연출이 다 끝난 이 시점에 장부(물고기 데이터)를 최종적으로 비움
        playerData.ResetCycle(); 
    }
}