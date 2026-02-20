using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OceanAniManager : MonoBehaviour
{
    [Header("Animated Prefab")]
    public GameObject wave01;
    public GameObject wave02;
    public GameObject snail;

    
    [Header("Spawn Area")]
    public BoxCollider2D waveArea01;
    public BoxCollider2D waveArea02;
    public BoxCollider2D snailArea;
    public float minDistance = 2.0f;

    private List<GameObject> activeWaves = new List<GameObject>();

    void Start()
    {
        StartCoroutine(WaveSpawnRoutine(wave01, 1.0f, waveArea01));
        StartCoroutine(WaveSpawnRoutine(wave02, 2.0f, waveArea01));
        StartCoroutine(WaveSpawnRoutine(wave01, 2.0f, waveArea02));
        StartCoroutine(WaveSpawnRoutine(wave02, 1.0f, waveArea02));
        //StartCoroutine(SnailSpawnRoutine());
    }
    
    IEnumerator WaveSpawnRoutine(GameObject wavePrefab, float interval, BoxCollider2D waveArea)
    {
        interval = Random.Range(0.5f, 1.5f);
        while (true)
        {   
            Vector3? safePosition = GetValidSpawnPosition(waveArea);

            if (safePosition.HasValue)
            {
                GameObject spawnedObj = Instantiate(wavePrefab, safePosition.Value, Quaternion.identity);
                
                // 생성된 파도를 관리 리스트에 추가
                activeWaves.Add(spawnedObj);

                Animator anim = spawnedObj.GetComponent<Animator>();
                float animLength = 1.0f; 

                if (anim != null)
                {
                    yield return null; 
                    animLength = anim.GetCurrentAnimatorStateInfo(0).length;
                }

                float elapsedTime = 0f;
                while (elapsedTime < animLength)
                {
                    if (spawnedObj != null)
                    {
                        // 매 프레임마다 floatUpSpeed만큼 위(Vector3.up)로 이동
                        spawnedObj.transform.Translate(Vector3.up * 0.5f * Time.deltaTime);
                    }
                    
                    elapsedTime += Time.deltaTime;
                    yield return null; // 다음 프레임까지 대기
                }

                // 3. 파괴하기 전에 리스트에서 제거
                activeWaves.Remove(spawnedObj);
                Destroy(spawnedObj);
            }

            // 4. 지정된 간격(2초 또는 3초)만큼 대기 후 반복
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator SnailSpawnRoutine()
    {
        while (true)
        {   
            float x = Random.Range(snailArea.bounds.min.x, snailArea.bounds.max.x);
            float y = Random.Range(snailArea.bounds.min.y, snailArea.bounds.max.y);
            Vector3 randomPos = new Vector3(x, y, 0);

            GameObject spawnedObj = Instantiate(snail, randomPos, Quaternion.identity);
                
            Animator anim = spawnedObj.GetComponent<Animator>();
            float animLength = 1.0f; 

            if (anim != null)
            {
                yield return null; 
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                animLength = stateInfo.length;
            }
            yield return new WaitForSeconds(animLength);
            Destroy(spawnedObj);
            // 4. 지정된 간격(2초 또는 3초)만큼 대기 후 반복
            yield return new WaitForSeconds(2.0f);
            
        }
    }

    Vector3? GetValidSpawnPosition(BoxCollider2D waveArea)
    {
        // 중간에 파괴된 오브젝트가 리스트에 남아있을 경우를 대비해 null 정리
        activeWaves.RemoveAll(item => item == null);

        for (int i = 0; i < 100; i++)
        {
            float x = Random.Range(waveArea.bounds.min.x, waveArea.bounds.max.x);
            float y = Random.Range(waveArea.bounds.min.y, waveArea.bounds.max.y);
            Vector3 randomPos = new Vector3(x, y, 0);

            bool isOverlapping = false;

            // 현재 떠 있는 모든 파도와 거리 비교
            foreach (GameObject wave in activeWaves)
            {
                // 두 점 사이의 거리가 minDistance보다 작으면 겹친 것으로 판정
                if (Vector3.Distance(randomPos, wave.transform.position) < minDistance)
                {
                    isOverlapping = true;
                    break;
                }
            }

            // 안 겹치면 이 위치를 반환
            if (!isOverlapping)
            {
                return randomPos;
            }
        }

        return null; 
    }
}
