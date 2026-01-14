using UnityEngine;

public class ConchGameManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject clickTarget; // 클릭할 원 프리팹 연
    public Vector2 minPosition;   // 생성 범위 최소 (좌측 하단)
    public Vector2 maxPosition;   // 생성 범위 최대 (우측 상단)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnTarget();
    }

    public void SpawnTarget()
    {
        // 1. 랜덤 위치 계산
        float x = Random.Range(minPosition.x, maxPosition.x);
        float y = Random.Range(minPosition.y, maxPosition.y);
        Vector3 pos = new Vector3(x, y, 0);

        // 2. 드럼 생성
        GameObject tab = Instantiate(clickTarget, pos, Quaternion.identity);

        // 3. 드럼에게 매니저(나)를 알려줌
        tab.GetComponent<TabController>().Setup(this);
    }

    public void OnClicked()
    {
        SpawnTarget();
    }

    public void OnMissed()
    {
        SpawnTarget();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
