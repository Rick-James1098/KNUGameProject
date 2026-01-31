using UnityEngine;

public class FloatingNums : MonoBehaviour
{
    public float destroyTime = 1.0f; // 1초 뒤 사라짐
    public Vector3 floatDirection = new Vector3(0, 1f, 0); // 위로 올라갈 방향
    public float floatSpeed = 1.0f;  // 올라가는 속도

    void Start()
    {
        // 생성되자마자 자신의 수명을 결정 (1초 뒤 파괴)
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 매 프레임 위로 둥둥 떠오르게 이동
        transform.position += floatDirection * floatSpeed * Time.deltaTime;
    }
}
