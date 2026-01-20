using UnityEngine;
using System.Collections;

public class HarpoonLauncher : MonoBehaviour
{
    private GameObject currentHarpoon;
    private bool isRespawning = false;

    // 생성 위치 (변수로 빼두면 관리하기 편합니다)
    private Vector3 spawnPos = new Vector3(0, 4f, 0);

    [SerializeField]
    private GameObject harpoon;

    void Start()
    {
        SpawnJaksal();
    }

    void Update()
    {
        // 1. 현재 작살이 파괴되어 사라졌고(null) 
        // 2. 지금 재생성 대기 중이 아니라면(!isRespawning)
        if (currentHarpoon == null && isRespawning == false)
        {
            // 3. 게임이 아직 안 끝났는지 확인 (성공했으면 더 만들면 안 되니까)
            FishSpawner spawner = FindObjectOfType<FishSpawner>();

            // spawner가 있고, 게임오버가 아닐 때만 재생성 시도
            if (spawner != null && spawner.isOvered == false)
            {
                StartCoroutine(RespawnRoutine());
            }
        }
    }

    void SpawnJaksal() 
    {
        currentHarpoon = Instantiate(harpoon, spawnPos, Quaternion.identity);
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true; // "지금 재생성 준비 중이다"라고 표시
        
        // 1초 대기 (원하는 시간만큼 조절하세요)
        yield return new WaitForSeconds(0.5f); 

        SpawnJaksal(); // 생성
        
        isRespawning = false; // 준비 끝
    }
}
