using UnityEngine;
using UnityEngine.UI;
using TMPro; // 텍스트 품질을 위해 TextMeshPro 사용 권장
using System.Collections;

public class FishingResultHandler : MonoBehaviour
{
    public PlayerData playerData;
    public GameObject rewardUI;

    [Header("UI Data Elements")] // 데이터가 들어갈 UI 컴포넌트들
    public Image fishIcon;          // 물고기 그림
    public TextMeshProUGUI fishName; // 물고기 이름
    public TextMeshProUGUI fishRarity; // 물고기 등급 (선택사항)

    void Start()
    {
        Debug.Log($"보상 핸들러 작동 시작! 물고기 존재: {playerData.hookedFish != null}, 성공여부: {playerData.isBattleSuccess}");
        if (playerData.hookedFish != null && playerData.isBattleSuccess)
        {
            Debug.Log("조건 충족! 보상을 지급합니다.");
            ReceiveReward();
        }
        else if (playerData.hookedFish != null && !playerData.isBattleSuccess)
        {
            Debug.Log("조건 미충족으로 보상을 지급하지 않습니다.");
            playerData.ResetCycle(); 
        }
    }

    void ReceiveReward()
    {
        // 1. 인벤토리 추가
        InventoryManager.Instance?.AddItem(playerData.hookedFish);

        // 2. UI 데이터 세팅 (이 부분이 핵심!)
        FishDataFormat data = playerData.hookedFish;
        if (data != null)
        {
            fishIcon.sprite = data.icon; // FishData에 있는 이미지 적용
            fishName.text = data.itemName;     // FishData에 있는 이름 적용
            
            // 등급이 있다면 색상이나 텍스트를 다르게 줄 수도 있어!
            // fishRarity.text = data.rarity.ToString();
        }

        // 3. 보상 연출 시작
        StartCoroutine(ShowRewardSequence());
    }

    IEnumerator ShowRewardSequence()
    {
        rewardUI.SetActive(true);
        yield return new WaitForSeconds(2f);
        rewardUI.SetActive(false);

        playerData.ResetCycle();
    }
}