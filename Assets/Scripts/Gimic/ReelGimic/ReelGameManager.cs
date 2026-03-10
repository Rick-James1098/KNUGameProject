using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class NewFishingGame : MonoBehaviour
{
    [Header("UI References (Bar)")]
    public RectTransform fishingBar;    // 배경 긴 막대
    public RectTransform targetArea;    // 타겟 박스 
    public RectTransform fishIcon;      // 내 물고기
    public Image successGauge;          // 성공 게이지

    [Header("UI References")]
    public RectTransform handleRect;    // 회전하는 릴 몸통
    public RectTransform handleKnob;    // 잡는 손잡이
    public GameObject windingEffectObject; // 감는 이펙트
    public GameObject readyUI; 
    public GameObject startUI;

    [Header("Game Settings")]
    public float barHeight = 686f;      // 막대 전체 높이
    public float startProgress = 0.4f;  // 시작 게이지 (30%)
    public float grabRadius = 80f;     // 손잡이 잡는 판정 범위
    public float reelSensitivity = 0.5f; // 회전 감도 (클수록 조금 돌려도 많이 움직임)
    public float effectRotateMultiplier = 2.0f; // 이펙트 회전 속도

    [Header("Fish Physics")]
    private float fishPosition = 0f;     // 현재 물고기 Y 위치
    public float acceleration = 2.0f;
    public float resistance = 2.0f;
    public float maxSpeed = 500f;
    private float currentVelocity = 0f;

    // 릴 회전 계산용 변수
    private float previousAngle = 0f;
    private bool isDragging = false;

    [Header("Target AI")]
    public float targetMoveSpeed = 80f; // 타겟 이동 속도 (픽셀/초)
    public float changeDestDelay = 0.5f; // 목적지 도착 후 대기 시간
    private float targetPosition = 0f;
    private float targetDestination = 0f;
    private float aiWaitTimer = 0f;

    [Header("Progress Settings")]
    public float fillSpeed = 0.2f;
    public float drainSpeed = 0.1f;
    private float currentProgress;
    public string inGameSceneName = "InGameScene"; // 돌아갈 마을 씬 이름
    public PlayerData playerData;
    private bool isGameActive = false;
    private float difficultyPercent = 0f;

    void Start()
    {
        currentProgress = startProgress;
        fishPosition = 0f;
        targetPosition = barHeight / 2f;
        
        float rawResistance = playerData.hookedFish.currentResistance;
        float calculatedDifficulty = rawResistance - (playerData.technic + playerData.rodSkill);
        calculatedDifficulty = 150; //Mathf.Max(0f, calculatedDifficulty);
        difficultyPercent = calculatedDifficulty / 340f;
        difficultyPercent = Mathf.Clamp01(difficultyPercent);

        resistance = Mathf.Lerp(1.0f, 4.0f, difficultyPercent);
        float randomHeight = Mathf.Lerp(155f, 55f, difficultyPercent);
        targetArea.sizeDelta = new Vector2(targetArea.sizeDelta.x, randomHeight);

        fillSpeed = Mathf.Lerp(0.35f, 0.2f, difficultyPercent);
        drainSpeed = Mathf.Lerp(0.1f, 0.2f, difficultyPercent);
        
        if (windingEffectObject != null) windingEffectObject.SetActive(false);

        Debug.Log($"Resistance: {rawResistance} / technic: {playerData.technic} / rodSkill: {playerData.rodSkill} / difficulty: {calculatedDifficulty}");

        SetNewTargetDestination();
        
        StartCoroutine(GameReadySequence());
    }

    IEnumerator GameReadySequence()
    {
        // 1. 시작 전 초기화 (혹시 켜져 있을까 봐 둘 다 끕니다)
        if (readyUI != null) readyUI.SetActive(false);
        if (startUI != null) startUI.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        // 2. Ready 이미지 켜기
        if (readyUI != null) readyUI.SetActive(true);
        yield return new WaitForSeconds(3f); // 1.5초 대기

        // 3. Ready 끄고 Start 켜기
        if (readyUI != null) readyUI.SetActive(false);
        if (startUI != null) startUI.SetActive(true);
        yield return new WaitForSeconds(0.5f); // 0.5초 대기

        // 4. Start 끄고 본격적인 게임 조작 활성화
        if (startUI != null) startUI.SetActive(false);
        
        isGameActive = true;
    }

    void Update()
    {
        if (!isGameActive) return;

        HandleReelInput();
        HandleTargetAI();
        CheckCollision();
        UpdateUI();
    }

    void HandleReelInput()
    {
        // 마우스 클릭 시 손잡이 근처인지 확인
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseNearKnob())
            {
                isDragging = true;
                previousAngle = GetAngleFromCenter();
            }
        }

        // 마우스 떼면 드래그 종료
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (windingEffectObject != null) windingEffectObject.SetActive(false);
        }

        float inputForce = 0f;

        // 드래그 중일 때 회전량 계산
        if (isDragging && Input.GetMouseButton(0))
        {
            float currentAngle = GetAngleFromCenter();
            float deltaAngle = currentAngle - previousAngle;

            // 각도 튐 방지 (-180 ~ 180 경계 처리)
            if (deltaAngle < -180f) deltaAngle += 360f;
            if (deltaAngle > 180f) deltaAngle -= 360f;

            // 회전 적용 (손잡이 UI 돌리기)
            if (handleRect != null)
            {
                handleRect.Rotate(0, 0, deltaAngle);
            }

            if (Mathf.Abs(deltaAngle) > 0.05f)
            {
                if (windingEffectObject != null)
                {
                    if (!windingEffectObject.activeSelf) 
                        windingEffectObject.SetActive(true);

                    // 이펙트 회전 (multiplier로 속도 조절 가능)
                    windingEffectObject.transform.Rotate(0, 0, deltaAngle * effectRotateMultiplier);
                }
            }
            else
            {
                // 잡고는 있는데 안 돌리면 이펙트 끄기
                if (windingEffectObject != null && windingEffectObject.activeSelf)
                    windingEffectObject.SetActive(false);
            }

            // 각도 변화량을 이동량으로 변환
            // 시계방향(-) -> 위로(+), 반시계방향(+) -> 아래로(-)
            inputForce = -deltaAngle * acceleration;

            previousAngle = currentAngle;
        }
        // 최종 위치 적용
        currentVelocity += inputForce;
        currentVelocity = Mathf.Lerp(currentVelocity, 0, resistance * Time.deltaTime);
        currentVelocity = Mathf.Clamp(currentVelocity, -maxSpeed, maxSpeed);
        fishPosition += currentVelocity * Time.deltaTime;

        if (fishPosition >= barHeight - fishIcon.rect.height)
        {
            fishPosition = barHeight - fishIcon.rect.height;
            if (currentVelocity > 0) currentVelocity = 0f; // 벽에 박으면 정지
        }
        else if (fishPosition <= 0f)
        {
            fishPosition = 0f;
            if (currentVelocity < 0) currentVelocity = 0f; // 벽에 박으면 정지
        }
    }

    // --- [2] 타겟 AI 로직 (수정됨: 멈추지 않음) ---
    void HandleTargetAI()
    {
        float dist = Mathf.Abs(targetPosition - targetDestination);
        // 1. 목적지까지 이동 (MoveTowards 사용으로 정확한 도달 보장)
        if (dist > 1f)
        {
            targetPosition = Mathf.MoveTowards(targetPosition, targetDestination, targetMoveSpeed * Time.deltaTime);
        }
        else
        {
            // 2. 도착했으면 잠시 대기 후 새로운 목적지 설정
            aiWaitTimer -= Time.deltaTime;
            if (aiWaitTimer <= 0)
            {
                SetNewTargetDestination();
            }
        }
    }

    void SetNewTargetDestination()
    {
        // 랜덤 위치 설정
        targetDestination = Random.Range(0f, barHeight - targetArea.rect.height - 2f);
        
        // 랜덤 대기 시간 설정 (0초 ~ 0.5초 사이로 짧게)
        float maxWait = Mathf.Lerp(0.3f, 0.1f, difficultyPercent);
        aiWaitTimer = Random.Range(0.1f, maxWait);
        
        float minSpeed = Mathf.Lerp(300f, 500f, difficultyPercent);
        float maxSpeed = Mathf.Lerp(500f, 700f, difficultyPercent);
        targetMoveSpeed = Random.Range(minSpeed, maxSpeed);
    }

    // --- [3] 판정 로직 ---
    void CheckCollision()
    {
        float targetHeight = targetArea.rect.height;
        float minBound = targetPosition;
        float maxBound = targetPosition + targetHeight;

        // 물고기가 타겟 안에 있는지?
        bool isInside = (fishPosition + fishIcon.rect.height >= minBound && fishPosition <= maxBound);

        if (isInside)
        {
            targetArea.GetComponent<Image>().color = Color.green; // 성공 시 초록
            currentProgress += fillSpeed * Time.deltaTime;
        }
        else
        {
            targetArea.GetComponent<Image>().color = Color.white; // 실패 시 흰색
            currentProgress -= drainSpeed * Time.deltaTime;
        }

        currentProgress = Mathf.Clamp01(currentProgress);

        // 게임 종료 체크
        if (currentProgress >= 1.0f) GameEnd(true);
        else if (currentProgress <= 0.0f) GameEnd(false);
    }

    // --- [4] UI 업데이트 ---
    void UpdateUI()
    {
        // 앵커가 Bottom-Center라고 가정하고 Y값 갱신
        if (fishIcon != null)
            fishIcon.anchoredPosition = new Vector2(fishIcon.anchoredPosition.x, fishPosition);
        
        if (targetArea != null)
            targetArea.anchoredPosition = new Vector2(targetArea.anchoredPosition.x, targetPosition);

        if (successGauge != null)
            successGauge.fillAmount = currentProgress;
    }

    // --- 헬퍼 함수들 ---
    bool IsMouseNearKnob()
    {
        if (handleKnob == null) return true;
        
        Vector2 knobScreenPos = RectTransformUtility.WorldToScreenPoint(null, handleKnob.position);
        float distance = Vector2.Distance(Input.mousePosition, knobScreenPos);

        return distance <= grabRadius;
    }

    float GetAngleFromCenter()
    {
        if (handleRect == null) return 0f;

        Vector2 centerScreenPos = RectTransformUtility.WorldToScreenPoint(null, handleRect.position);
        Vector2 direction = (Vector2)Input.mousePosition - centerScreenPos;
        
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    void GameEnd(bool isSuccess)
    {
        isGameActive = false;
        if (isSuccess) 
        {
            Debug.Log("성공!");
            playerData.isBattleSuccess = isSuccess;
        }

        else Debug.Log("실패!");
        // 여기에 게임 종료 후 연출이나 씬 전환 코드 추가


        SceneManager.LoadScene(inGameSceneName);
    }
}