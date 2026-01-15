using UnityEngine;

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

    public void OnClicked()
    {   
        count++;
        Debug.Log(count);
        SpawnTarget();
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
