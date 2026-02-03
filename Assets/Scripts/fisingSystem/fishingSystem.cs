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

    [Header("확률 및 타이머")]
    public List<FishData> fishPool;   // 물고기 데이터 리스트
    public float biteChance = 0.4f;   // 입질 확률 (0~1)
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("설정")]
    public float chargeSpeed = 1.5f;
    private float currentPower = 0f;
    private bool isCharging = false;
    private bool isFishing = false;
    private TopDownBobber currentBobber;

    void Awake()
    {
        moveScript = GetComponent<PlayerMovement>();
        if (fishingLine != null) fishingLine.enabled = false;
    }

    void Update()
    {
        // 1. 인벤토리 창이 켜져 있다면 모든 낚시 입력 무시
        if (InventoryUI.Instance.inventoryWindow.activeSelf) return;
                
        if (isFishing)
        {
            UpdateFishingLine();
            if (Input.GetMouseButtonDown(0)) RetrieveFishing(); // 낚시 중 클릭하면 회수
            return;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentPower = 0f;
            powerSlider.gameObject.SetActive(true);
        }

        if (isCharging)
        {
            currentPower += Time.deltaTime * chargeSpeed;
            currentPower = Mathf.Clamp01(currentPower);
            powerSlider.value = currentPower;
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            ThrowBobber();
            isCharging = false;
            powerSlider.gameObject.SetActive(false);
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
        currentBobber = null;
        moveScript.canMove = true;
        if (fishingLine != null) fishingLine.enabled = false;
    }
}