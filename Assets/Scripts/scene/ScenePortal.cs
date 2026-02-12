using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    [Header("설정")]
    public string targetSceneName; // 이동할 씬 이름 (예: HouseInside)
    public string targetSpawnPointName; // 도착할 곳의 스폰 지점 이름

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 도착 지점 이름을 저장 (씬이 바뀌어도 유지되는 곳에 저장)
            SpawnManager.nextSpawnPointName = targetSpawnPointName;

            // 2. 씬 로드
            SceneManager.LoadScene(targetSceneName);
        }
    }
}