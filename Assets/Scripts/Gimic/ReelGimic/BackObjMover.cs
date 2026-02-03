using UnityEngine;

public class BackgroundObject : MonoBehaviour
{
    [Header("Moving Setting")]
    public float speed = 5f;        // 이동 속도
    public float endX = 10f;        // 오른쪽 끝 (사라지는 지점)
    public float startX = -10f;     // 왼쪽 끝 (다시 나타나는 지점)

    void Update()
    {
        // 1. 왼쪽에서 오른쪽으로 계속 이동
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // 2. 만약 오브젝트가 endX(오른쪽 끝)를 넘어갔다면
        if (transform.position.x >= endX)
        {
            // 3. startX(왼쪽 끝)로 위치를 '재배치(Recycle)' 합니다.
            Vector3 newPos = transform.position;
            newPos.x = startX;
            transform.position = newPos;
        }
    }
}
