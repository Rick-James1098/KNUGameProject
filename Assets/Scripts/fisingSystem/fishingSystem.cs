using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FishingSystem : MonoBehaviour
{
    [Header("Data Reference")]
    public PlayerData data;
    public FishSelector fishSelector;

    [Header("참조")]
    public Transform rodTip;
    public GameObject bobberPrefab;
    public Transform throwPoint;
    public Slider powerSlider;
    public LineRenderer fishingLine;
    public Animator animator;
    public GameObject fishingTool;

    [Header("확률 및 타이머")]
    public List<FishData> fishPool;
    public float biteChance = 0.4f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    public float chargeSpeed = 1.5f;

    private float currentPower = 0f;
    private bool isCharging = false;
    private TopDownBobber currentBobber;
    private PlayerMovement moveScript;

    void Awake()
    {
        moveScript = GetComponent<PlayerMovement>();
        if (fishingLine != null) fishingLine.enabled = false;
        if (fishingTool != null) fishingTool.SetActive(false);
    }

    void Update()
    {
        // 인벤토리 열려있으면 중단
        if (InventoryUI.Instance != null && InventoryUI.Instance.inventoryWindow.activeSelf) return;

        if (data.isFishing)
        {
            UpdateFishingLine();
            // 챔질 미니게임 UI가 꺼져 있을 때만 좌클릭으로 낚싯줄 회수 가능
            if (!data.isStrikeGameActive)
            {
                if (Input.GetMouseButtonDown(0)) RetrieveFishing();
            }
            return;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            // 진짜 장부 상태를 확인
            Debug.Log($"클릭됨! 고기통 장착 상태: {data.hasFishBucket}");
            // [수정] 장부를 확인해서 고기통이 없으면 리턴 (캐스팅 불가)
            if (data != null && !data.hasFishBucket)
            {
                Debug.LogWarning("고기통이 장착되어 있지 않아 낚시를 할 수 없습니다!");
                // 여기에 '고기통이 필요합니다' 같은 말풍선이나 UI 알림을 띄우면 좋습니다.
                return; 
            }
            isCharging = true;
            currentPower = 0f;
            powerSlider.gameObject.SetActive(true);
            PrepareFishingVisual();
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
            ThrowBobber();
        }
    }

    void PrepareFishingVisual()
    {
        fishingTool.SetActive(true);
        SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
        SpriteRenderer toolSR = fishingTool.GetComponent<SpriteRenderer>();
        
        // 방향에 따른 레이어 정렬
        toolSR.sortingOrder = (data.lastDirection.y > 0.1f) ? playerSR.sortingOrder - 1 : playerSR.sortingOrder + 1;
        
        animator.SetBool("isCharging", true);
        animator.SetFloat("DirX", data.lastDirection.x);
        animator.SetFloat("DirY", data.lastDirection.y);
    }

    void ThrowBobber()
    {
        data.isFishing = true;
        animator.SetBool("isCharging", false);
        animator.SetTrigger("Throw");
        animator.SetBool("isFishing", true);

        if (fishingLine != null) fishingLine.enabled = true;

        GameObject bobberObj = Instantiate(bobberPrefab, throwPoint.position, Quaternion.identity);
        currentBobber = bobberObj.GetComponent<TopDownBobber>();
        currentBobber.fishingSystem = this;
        currentBobber.Launch(data.lastDirection, currentPower);
    }

    public void OnBobberLanded(Vector3 landPos)
    {
        animator.SetBool("isOnWater", true);
        StartCoroutine(BiteProcess(landPos));
    }

    private IEnumerator BiteProcess(Vector3 bobberPos)
    {
        yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));

        while (data.isFishing)
        {
            if (Random.value <= biteChance)
            {
                FishDataFormat selectedFish = fishSelector.GetRandomFish(data.GetPlayerLuck());
                data.hookedFish = selectedFish;

                if (currentBobber != null) currentBobber.SetBitingVisual(true);
                data.isStrikeGameActive = true;

                yield break;

                /*data.hookedFish = SelectRandomFish();
                Debug.Log($"고기 결정됨: {data.hookedFish.itemName}"); // 로그를 찍어보세요!
                if (currentBobber != null) currentBobber.SetBitingVisual(true);
                
                // [핵심] 장부만 갱신하면 UI는 알아서 나타납니다.
                data.isStrikeGameActive = true; 
                yield break; */
            }
            yield return new WaitForSeconds(1.0f);
        }
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
        StopAllCoroutines();
        if (currentBobber != null) Destroy(currentBobber.gameObject);
        ResetFishingState();
    }

    public void ResetFishingState()
    {
        data.ResetCycle();
        if (animator != null)
        {
            animator.SetBool("isFishing", false);
            animator.SetBool("isOnWater", false);
            animator.SetBool("isCharging", false);
            // 혹시 모르니 Trigger도 리셋
            animator.ResetTrigger("Throw");
        }

        // 2. 시각적 요소 끄기
        if (fishingTool != null) fishingTool.SetActive(false);

        // 3. 라인 렌더러 좌표 초기화 및 비활성화
        if (fishingLine != null)
        {
            fishingLine.SetPosition(0, Vector3.zero);
            fishingLine.SetPosition(1, Vector3.zero);
            fishingLine.enabled = false;
        }
    }
}