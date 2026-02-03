using UnityEngine;
using UnityEngine.UI;

public class ReelGameManager : MonoBehaviour
{
    [Header("References")]
    public RectTransform handleRect; 
    public RectTransform handleKnob;  
    public Image gaugeImage; // 성공 게이지
    public Image tensionGaugeImage; // 텐션 게이지
    public GameObject windingEffectObject;

    [Header("Difficulty Settings")]
    // 난이도에 따른 '게이지 차는 속도' (0.0 ~ 1.0)
    // X: 쉬움(0.2 = 한 바퀴에 20%), Y: 어려움(0.05 = 한 바퀴에 5%)
    public float changableDifficulty = 0.5f;
    // 난이도별 성공 게이지 차는 속도 (0.0 ~ 1.0)
    // 쉬움: 한 바퀴에 20%, 어려움: 한 바퀴에 5%
    public Vector2 fillSpeedRange = new Vector2(0.2f, 0.05f); 

    // [신규] 가만히 있을 때 텐션 자연 증가량 (초당 증가량)
    // 쉬움: 초당 5, 어려움: 초당 25 (가만히 있으면 4초만에 끊어짐)
    public Vector2 passiveTensionRange = new Vector2(5f, 25f);

    // [변경] 감을 때 추가되는 텐션 (한 바퀴당)
    // 요청하신 대로 '자연 증가량'보다는 영향력이 적도록 수치를 낮게 잡습니다.
    // 쉬움: 5, 어려움: 10
    public Vector2 windingTensionAddRange = new Vector2(5f, 10f); 
    
    // [변경] 풀 때 감소하는 텐션 (한 바퀴당)
    // 자연 증가와 감을 때의 증가를 모두 커버해야 하므로 수치가 커야 합니다.
    // 쉬움: 60, 어려움: 40
    public Vector2 unwindingTensionSubRange = new Vector2(60f, 40f);

    [Header("Fixed Settings")]
    public float maxTension = 100f;         // 텐션 최대치
    public float grabRadius = 80f;          // 잡는 범위
    public float decreasePenaltyPerTurn = 0.1f; // 풀 때 성공 게이지 감소량 (고정)
    public float sensitivity = 1.0f;        // 마우스 감도
    public float effectRotateMultiplier = 2.0f;

    // --- 내부 변수 (Inspector에서 안 보임) ---
    private float currentFillAmount = 0f;   // 현재 성공 게이지 (0.0 ~ 1.0)
    private float currentTension = 0f;      // 현재 텐션 (0 ~ 100)
    
    // 현재 난이도에 맞춰 결정된 수치들
    private float currentFillSpeed;       
    private float currentPassiveSpeed; // (신규) 초당 텐션 증가 속도
    private float currentWindingAdd;   // (변경) 감을 때 추가 텐션
    private float currentUnwindingSub; // (변경) 풀 때 감소 텐션
    private float previousAngle = 0f;
    private bool isDragging = false;
    private bool isGameActive = true;

    void Start()
    {
        // 매니저 없이 혼자 테스트할 때를 위해, 시작하자마자 기본 난이도(0.5)로 세팅합니다.
        // 만약 매니저가 나중에 SetupGame을 또 호출하면 그 값으로 덮어씌워지니 안전합니다.
        SetupGame(changableDifficulty);
    }

    public void SetupGame(float difficulty)
    {
        difficulty = Mathf.Clamp01(difficulty);

        // 1. 난이도별 수치 세팅 (Lerp)
        currentFillSpeed = Mathf.Lerp(fillSpeedRange.x, fillSpeedRange.y, difficulty);
        // [신규] 자연 증가 속도 결정
        currentPassiveSpeed = Mathf.Lerp(passiveTensionRange.x, passiveTensionRange.y, difficulty);

        // [변경] 감기/풀기 수치 결정
        currentWindingAdd = Mathf.Lerp(windingTensionAddRange.x, windingTensionAddRange.y, difficulty);
        currentUnwindingSub = Mathf.Lerp(unwindingTensionSubRange.x, unwindingTensionSubRange.y, difficulty);

        // 2. 변수 초기화
        currentFillAmount = 0f;
        currentTension = 0f;
        isDragging = false;
        previousAngle = 0f;
        isGameActive = true;
        
        // 3. UI 초기화
        if (gaugeImage != null) gaugeImage.fillAmount = 0f;
        if (tensionGaugeImage != null) tensionGaugeImage.fillAmount = 0f;
        if (windingEffectObject != null) windingEffectObject.SetActive(false);

        Debug.Log($"[낚시 시작] 난이도: {difficulty:F2}");
    }

    void Update()
    {
        if (!isGameActive) return;

        if (currentTension < maxTension)
        {
            currentTension += currentPassiveSpeed * Time.deltaTime;
        }

        // 1. 입력 처리
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseNearKnob())
            {
                isDragging = true;
                previousAngle = GetAngleFromCenter();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (windingEffectObject != null) windingEffectObject.SetActive(false);
        }

        // 2. 드래그 중 로직
        if (isDragging && Input.GetMouseButton(0))
        {
            float currentMouseAngle = GetAngleFromCenter();
            float deltaAngle = currentMouseAngle - previousAngle;

            // 각도 튐 방지
            if (deltaAngle < -180f) deltaAngle += 360f;
            if (deltaAngle > 180f) deltaAngle -= 360f;

            // 회전 비율 계산 (이번 프레임에 몇 바퀴 돌았나?)
            float turnRatio = deltaAngle / 360f;

            if (Mathf.Abs(deltaAngle) > 0.05f) // 움직임이 있을 때
            {
                if (deltaAngle > 0) 
                {
                    // [감기 - 반시계]
                    currentFillAmount += turnRatio * currentFillSpeed * sensitivity;
                    currentTension += turnRatio * currentWindingAdd * sensitivity;

                    
                }
                else 
                {
                    // [풀기 - 시계]
                    currentFillAmount += turnRatio * decreasePenaltyPerTurn * sensitivity;
                    currentTension += turnRatio * currentUnwindingSub * sensitivity;
                }
                if (windingEffectObject != null)
                {
                    if (!windingEffectObject.activeSelf) 
                         windingEffectObject.SetActive(true); // 안 켜져 있으면 켬

                    // Z축 기준으로 회전 (손잡이와 같은 방향)
                    // effectRotateMultiplier를 조절해 더 빠르게 돌릴 수도 있음
                    windingEffectObject.transform.Rotate(0, 0, deltaAngle * effectRotateMultiplier);
                }
            }
            else
            {
                // ★ 여기가 핵심입니다!
                // deltaAngle이 0.05보다 작음 = "마우스는 누르고 있지만 멈춰 있음"
                // 이때 이펙트를 끕니다.
                if (windingEffectObject != null && windingEffectObject.activeSelf) 
                {
                    windingEffectObject.SetActive(false);
                }
            }

            // 값 범위 제한
            currentFillAmount = Mathf.Clamp01(currentFillAmount);
            if (currentTension < 0) currentTension = 0;

            // 손잡이 회전 적용
            handleRect.Rotate(0, 0, deltaAngle);

            // 승패 체크 및 저장
            CheckGameStatus();
            previousAngle = currentMouseAngle;
        }
        
        // 3. UI 업데이트
        UpdateUI();
    }

    void CheckGameStatus()
    {
        // 성공: 게이지 100% (1.0) 도달
        if (currentFillAmount >= 1.0f)
        {
            isGameActive = false;
            Debug.Log("🎉 낚시 성공! 월척입니다!");

            if (windingEffectObject != null) windingEffectObject.SetActive(false);
            // 여기에 성공 연출 함수 호출
        }

        // 실패: 텐션 100 초과
        if (currentTension >= maxTension)
        {
            isGameActive = false;
            Debug.Log("💥 낚싯줄이 끊어졌습니다!");
            if(tensionGaugeImage != null) tensionGaugeImage.fillAmount = 1.0f; // 꽉 찬 상태로 보여줌

            if (windingEffectObject != null) windingEffectObject.SetActive(false);
            // 여기에 실패 연출 함수 호출
        }
    }

    void UpdateUI()
    {
        if (gaugeImage != null) 
            gaugeImage.fillAmount = currentFillAmount;
            
        if (tensionGaugeImage != null)
        {
            // 텐션은 0~100 값이므로 100으로 나눠서 0.0~1.0 비율로 변환
            tensionGaugeImage.fillAmount = currentTension / maxTension;
            
            // (선택) 텐션이 높으면 빨개지는 연출
            if (currentTension > 80f) tensionGaugeImage.color = Color.red;
            else tensionGaugeImage.color = Color.white; // 원래 색(또는 지정색)
        }
    }

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
}
