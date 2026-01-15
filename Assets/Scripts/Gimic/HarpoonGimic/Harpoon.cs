using NUnit.Framework;
using UnityEngine;

public class Harpoon : MonoBehaviour
{
    private Transform targetFish; // 목표 물고기
    private bool isLaunched = false;
    private bool isSuccess = false;
    public float speed = 15f;

    public void Setup(Transform fish, Camera cam)
    {
        targetFish = fish;
        
        // 1. 화면 왼쪽 상단 구석 좌표 구하기 (0.1, 0.9는 화면 비율)
        Vector3 startPos = cam.ViewportToWorldPoint(new Vector3(0.15f, 0.85f, 10));
        startPos.z = 0;
        transform.position = startPos;

        // 2. 물고기 바라보기 (각도 계산)
        Vector3 dir = targetFish.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void Shoot(bool success)
    {
        isSuccess = success;
        
        // 성공이면 물고기를 향해, 실패면 빗나가게(약간 위로 회전)
        if (!success && isLaunched == false)
        {
            transform.Rotate(0, 0, 15); // 15도 정도 빗나가게 회전
        }

        isLaunched = true;
    }

    void Update()
    {
        if (isLaunched)
        {
            // 작살이 날아감 (오른쪽 방향이 머리라고 가정)
            transform.Translate(Vector3.right * speed * Time.deltaTime);
            
            // 화면 밖으로 나가면 삭제 (선택 사항)
            if(!GetComponent<Renderer>().isVisible) Destroy(gameObject, 2f);
            
            float distance = Vector3.Distance(transform.position, targetFish.position);
            if (distance <= 0.02f)
            {
                isLaunched = false;
            }
        }
    }
}
