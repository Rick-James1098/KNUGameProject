using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        // 1. 저장된 스폰 지점 이름이 있는지 확인
        if (string.IsNullOrEmpty(SpawnManager.nextSpawnPointName))
        {
            Debug.LogWarning("스폰 지점 이름이 비어있습니다! (첫 시작이거나 설정 오류)");
            return;
        }

        // 2. 현재 씬에서 해당 이름의 오브젝트 찾기
        GameObject spawnPoint = GameObject.Find(SpawnManager.nextSpawnPointName);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (spawnPoint != null && player != null)
        {
            // 3. 위치 이동 및 확인 로그
            player.transform.position = spawnPoint.transform.position;
            Debug.Log($"[성공] {player.name}를 {spawnPoint.name} 위치({spawnPoint.transform.position})로 이동시켰습니다.");
            SpawnManager.nextSpawnPointName = "";
        }
        else
        {
            // 4. 실패 원인 로그
            if (spawnPoint == null) Debug.LogError($"[실패] '{SpawnManager.nextSpawnPointName}'라는 이름의 오브젝트를 씬에서 찾을 수 없습니다!");
            if (player == null) Debug.LogError("[실패] 'Player' 태그를 가진 오브젝트가 현재 씬에 없습니다!");
        }
    }
}