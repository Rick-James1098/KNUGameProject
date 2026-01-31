using System.Runtime.Serialization;
using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class HarpoonGimicManager : MonoBehaviour
{
    [SerializeField]
    public Spawner spawner;
    public StackGauge gauge;
    public GameObject harpoonPrefab; // 작살 프리팹
    public Camera mainCamera;

    private GameObject currentFish;
    private GameObject currentHarpoon;
    public bool isStarted = true;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        // 1. 물고기 생성 (FishSpawner가 생성된 물고기를 반환하도록 수정 필요)
        currentFish = spawner.SpawnFish(); 

        // 2. 작살 생성
        GameObject harpoonObj = Instantiate(harpoonPrefab);
        currentHarpoon = harpoonObj;

        // 3. 작살에게 물고기 조준 시키기
        Harpoon harpoon = harpoonObj.GetComponent<Harpoon>();
        harpoon.Setup(currentFish.transform, mainCamera);

        // 4. 게이지에 작살 컨트롤러 넘겨주고, 목표 선 표시하기
        gauge.InitGame(harpoon, this);
        gauge.SetRandomTarget();

        isStarted = true;
    }

    public void StopCurrentFish()
    {
        if (currentFish != null)
        {
            FishMovement movement = currentFish.GetComponent<FishMovement>();
            if (movement != null)
            {
                movement.StopMoving();
            }
        }
    }

    public void OnRoundFinished()
    {
        if(isStarted == true)
        {
            StopCurrentFish();
            
            // 바로 리셋하면 작살 날아가는 게 안 보이니 2초 뒤에 리셋
            StartCoroutine(ResetGameRoutine());
            isStarted = false;
        }
    }

    IEnumerator ResetGameRoutine()
    {
        // 2초 대기 (작살이 날아가서 꽂히는 시간)
        yield return new WaitForSeconds(2.0f);

        // 1. 기존 오브젝트 청소 (Destroy)
        if (currentFish != null) Destroy(currentFish);
        if (currentHarpoon != null) Destroy(currentHarpoon);

        // 2. 게이지 초기화 (StackGauge에 Reset 함수 필요)
        gauge.ResetGaugeUI();

        // 3. 다음 판 시작!
        StartGame();
    }
}
