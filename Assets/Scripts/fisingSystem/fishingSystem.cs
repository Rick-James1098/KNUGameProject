using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FishingSystem : MonoBehaviour
{
    [Header("참조")]
    public Transform rodTip;
    public PlayerMovement moveScript;
    public GameObject bobberPrefab;
    public Transform throwPoint;
    public Slider powerSlider;
    public LineRenderer fishingLine;
    public FishingArcGame arcGame; // 챔질 미니게임 연결
    public Animator animator;
    public GameObject fishingTool;
    

    [Header("확률 및 타이머")]
    public List<FishData> fishPool;   // 물고기 데이터 리스트
    public float biteChance = 0.4f;   // 입질 확률 (0~1)
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("설정")]
    public float chargeSpeed = 1.5f;
    private float currentPower = 0f;
    private bool isCharging = false;
    public bool isFishing = false;
    private TopDownBobber currentBobber;

    void Awake()
    {
        moveScript = GetComponent<PlayerMovement>();
        if (fishingLine != null) fishingLine.enabled = false;

        // [안전장치] 시작할 때 모든 상태를 초기화
        isFishing = false;
        isCharging = false;
        if (fishingTool != null) fishingTool.SetActive(false);
    }

    void Update()
    {
        // 1. UI 체크
        if (InventoryUI.Instance.inventoryWindow.activeSelf) return;

        // 2. 낚시 중일 때 로직
        if (isFishing)
        {
            UpdateFishingLine();

            // [핵심 수정] 
            // 1. 미니게임이 '진행 중'이 아닐 때만 (arcGame.isGameActive가 false일 때)
            // 2. 마우스를 클릭하면 '회수(Retrieve)'를 실행한다.
            // arcGame 자체가 아니라, 실제 미니게임 '비주얼'이 켜져 있는지 확인해야 합니다.
            if (arcGame != null && !arcGame.arcVisual.activeSelf) 
            {
                if (Input.GetMouseButtonDown(0)) RetrieveFishing();
            }
            return; // 낚시 중일 때는 아래 HandleInput(차징)으로 못 가게 막음
        }

        // 3. 낚시 중이 아닐 때만 차징 입력을 받음
        HandleInput();
    }

    public void StartFishing()
    {

        Debug.Log("낚시시작!");


        // 플레이어의 SpriteRenderer를 가져옵니다.  
        SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
        SpriteRenderer toolSR = fishingTool.GetComponent<SpriteRenderer>();
        // 방향에 따라 정렬 순서를 강제로 결정
        if (moveScript.lastDir.y > 0.1f) // 위를 보고 던질 때
        {
            toolSR.sortingOrder = playerSR.sortingOrder - 1; // 캐릭터 뒤로
        }
        else // 아래나 옆을 보고 던질 때
        {
            toolSR.sortingOrder = playerSR.sortingOrder + 1; // 캐릭터 앞으로
        }
        
        // 2. 낚싯대(도구) 활성화
        fishingTool.SetActive(true);
        animator.SetBool("isCharging", true); 
        animator.SetBool("isOnWater", false); // 리셋

        // 3. 현재 캐릭터가 보고 있는 방향(lastDir)을 애니메이터에 전달
        // 걷기 블렌드 트리에서 쓰던 파라미터와 이름을 똑같이 맞춰주세요!
        animator.SetFloat("DirX", moveScript.lastDir.x);
        animator.SetFloat("DirY", moveScript.lastDir.y);

        // 4. 애니메이션 트리거 작동
        animator.SetTrigger("Throw");
        animator.SetBool("isFishing", true);
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentPower = 0f;
            powerSlider.gameObject.SetActive(true);

            StartFishing();
        }

        if (isCharging)
        {
            currentPower += Time.deltaTime * chargeSpeed;
            currentPower = Mathf.Clamp01(currentPower);
            powerSlider.value = currentPower;
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;
            powerSlider.gameObject.SetActive(false);

            // 애니메이션 설정
            animator.SetBool("isCharging", false);
            animator.SetTrigger("Throw"); // 던지기 모션 1번 실행

            ThrowBobber();
        }
    }

    void ThrowBobber()
    {
        isFishing = true;
        moveScript.canMove = false; // 이동 봉쇄

        if (fishingLine != null) fishingLine.enabled = true;

        GameObject bobberObj = Instantiate(bobberPrefab, throwPoint.position, Quaternion.identity);
        currentBobber = bobberObj.GetComponent<TopDownBobber>();
        
        currentBobber.fishingSystem = this; // 지휘관 연결
        currentBobber.Launch(moveScript.lastDir, currentPower);
    }

    // [중요] 찌가 물에 닿으면 호출됨
    public void OnBobberLanded(Vector3 landPos)
    {
        animator.SetBool("isOnWater", true);
        Debug.Log("찌 착지 보고됨. 입질 프로세스 시작.");
        StartCoroutine(BiteProcess(landPos));
    }

    private IEnumerator BiteProcess(Vector3 bobberPos)
    {
        // 1. 처음 찌가 안착하고 나서 첫 번째 체크 전까지의 랜덤 대기 (긴장감)
        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

        // 2. 입질이 올 때까지 무한 반복
        while (isFishing) // 낚시 중인 상태라면 계속 반복
        {
            Debug.Log("입질 체크 중...");

            if (Random.value <= biteChance)
            {
                // [입질 성공!]
                Debug.Log("!!! 입질 발생 !!!");
                
                FishData selected = SelectRandomFish();
                if (selected == null)
                {
                    Debug.LogError("Fish Pool이 비어있습니다!");
                    RetrieveFishing();
                    yield break;
                }
                
                Debug.Log(selected.name);

                FishingDataManager.SelectedFish = selected;

                // 찌에게 입질 비주얼 명령 (쏙 들어가기)
                if (currentBobber != null) currentBobber.SetBitingVisual(true);

                // 챔질 미니게임 시작
                if (arcGame != null) arcGame.StartMiniGame(bobberPos, -moveScript.lastDir);

                // 입질이 성공했으니 루프를 완전히 종료합니다.
                yield break; 
            }

            // [입질 실패] 
            Debug.Log("물고기가 미끼를 그냥 지나쳤습니다... 1초 뒤 다시 체크합니다.");
            
            // (선택사항) 입질 실패 시 찌가 미세하게 '까딱'거리면 더 생동감이 납니다.
            // if (currentBobber != null) currentBobber.SetSmallWiggle(); 

            yield return new WaitForSeconds(1.0f); // 1초 대기 후 다음 주사위
        }
    }
    // FishingSystem 내부

    public void OnMiniGameResult(bool success)
    {
        if (success)
        {
            Debug.Log("물고기 획득 성공! 애니메이션 종료 프로세스 시작");
            // 여기에 "대단해!" 같은 승리 포즈 애니메이션 트리거를 넣어도 좋습니다.
        }
        else
        {
            Debug.Log("물고기 놓침... 아쉽다.");
        }

        // 미니게임이 끝났으니 이제 낚싯대를 집어넣고 캐릭터를 자유롭게 만듭니다.
        ResetFishingState();
    }

    private FishData SelectRandomFish()
    {
        int totalWeight = 0;
        foreach (var f in fishPool) totalWeight += f.rarityWeight;
        int pivot = Random.Range(0, totalWeight);
        int current = 0;
        foreach (var f in fishPool)
        {
            current += f.rarityWeight;
            if (pivot < current) return f;
        }
        return fishPool[0];
    }

    void UpdateFishingLine()
    {
        if (currentBobber != null && fishingLine != null)
        {
            fishingLine.SetPosition(0, rodTip.position);
            fishingLine.SetPosition(1, currentBobber.visualChild.position);
        }
    }

    public void RetrieveFishing()
    {   
        StopAllCoroutines(); // 진행 중인 입질 프로세스 중단
        if (currentBobber != null) Destroy(currentBobber.gameObject);
        ResetFishingState();
    }

    public void ResetFishingState()
    {
        isFishing = false;
        isCharging = false;

        // [핵심] 현재 필드에 있는 찌를 찾아서 파괴합니다.
        if (currentBobber != null)
        {
            Debug.Log("찌 파괴 완료");
            Destroy(currentBobber.gameObject);
            currentBobber = null; // 참조 초기화
        }

        // 2. 애니메이터 파라미터 리셋 (이게 있어야 평소 걷기/아이들로 돌아갑니다)
        if (animator != null)
        {
            animator.SetBool("isFishing", false);
            animator.SetBool("isCharging", false);
            animator.SetBool("isOnWater", false);
        }

        if (fishingTool != null)
        {
            fishingTool.SetActive(false);
        }
        
        if (moveScript != null) moveScript.canMove = true;
        if (fishingLine != null) fishingLine.enabled = false;
    }
}