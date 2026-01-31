using UnityEngine;
using UnityEngine.UIElements;

public class ConchGameManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject clickTarget; // 클릭할 원 프리팹 연
    public Vector2 minPosition;   // 생성 범위 최소 (좌측 하단)
    public Vector2 maxPosition;   // 생성 범위 최대 (우측 상단)

    [Header("Difficulty Balance")]
    public AnimationCurve difficultyCurve;

    private float currentTime = 20f;       // 현재 남은 시간
    private bool isPlaying = false;  // 게임 진행 중인지 여부
    private float difficulty = 10f;
    private float count = 0f;

    [Header("Score Effect")]
    // [변경] 프리팹 하나가 아니라, 여러 개를 담을 배열로 선언합니다.
    public GameObject[] scorePrefabs; 
    public GameObject donePrefab;
    
    public Vector3 effectOffset = new Vector3(0.5f, 0.5f, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        float randomRoll = Random.value;
        difficulty = difficultyCurve.Evaluate(randomRoll);
        Debug.Log(difficulty);
        isPlaying = true;
        SpawnTarget();
    }

    void Update()
    {
        // 게임 중이 아니면 타이머 멈춤
        if (!isPlaying) return;

        // 2. 시간 감소 (카운트다운)
        currentTime -= Time.deltaTime;

        // 3. 시간이 다 됐는지 체크
        if (currentTime <= 0)
        {
            EndGame();
        }
    }

    public void SpawnTarget()
    {
        if (!isPlaying) return;
        // 1. 랜덤 위치 계산
        float x = Random.Range(minPosition.x, maxPosition.x);
        float y = Random.Range(minPosition.y, maxPosition.y);
        Vector3 pos = new Vector3(x, y, 0);
        // 2. 드럼 생성
        GameObject tab = Instantiate(clickTarget, pos, Quaternion.identity);

        // 3. 드럼에게 매니저(나)를 알려줌
        tab.GetComponent<TabController>().Setup(this, difficulty);
    }

    public void OnClicked(Vector3 pos)
    {   
        count++;
        Debug.Log(count);

        int prefabIndex = (int)count - 1;

        if (scorePrefabs != null && prefabIndex < scorePrefabs.Length)
        {
            GameObject prefabToSpawn = scorePrefabs[prefabIndex];

            if (prefabToSpawn != null)
            {
                Vector3 spawnPos = pos + effectOffset;
                Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            }
        }

        if (count >= 12)
        {
            GameClear(); // 승리!
        }
        else
        {
            SpawnTarget(); 
        }
    }

    void GameClear()
    {
        isPlaying = false; // 게임 정지 (클릭 막음, 타이머 멈춤)

        // 1. 남은 드럼 싹 지우기 (깔끔하게)
        TabController[] activeDrums = FindObjectsOfType<TabController>();
        foreach (TabController drum in activeDrums)
        {
            Destroy(drum.gameObject);
        }

        // 2. DONE 프리팹 소환!
        if (donePrefab != null)
        {
            // 화면 중앙(0,0,0)에 생성 (카메라 위치에 따라 조절 필요하면 new Vector3(0, 0, 0) 수정)
            Instantiate(donePrefab, Vector3.zero, Quaternion.identity);
        }
        
        // 여기에 이후 씬 전환이나 버튼 활성화 코드 추가 가능
    }

    public void OnMissed()
    {
        SpawnTarget();
    }

    void EndGame()
    {
        isPlaying = false;
        currentTime = 0;
    }

}
