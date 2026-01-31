using NUnit.Framework;
using UnityEngine;

public class Harpoon : MonoBehaviour
{
    private Transform targetFish; // 목표 물고기
    private bool isLaunched = false;
    private bool isSuccess = false;
    public float speed = 15f;
    private Animator animator;

    [Header("Effects")]
    public GameObject hitEffect; // [추가] 피 이펙트 프리팹 연결

    void Start()
    {
        // 시작할 때 내 몸에 있는 애니메이터를 찾아둡니다.
        animator = GetComponent<Animator>();
    }

    public void Setup(Transform fish, Camera cam)
    {
        targetFish = fish;
        
        // 1. 화면 왼쪽 상단 구석 좌표 구하기 (0.1, 0.9는 화면 비율)
        Vector3 startPos = cam.ViewportToWorldPoint(new Vector3(0.12f, 0.78f, 10));
        startPos.z = 0;
        transform.position = startPos;
        transform.Rotate(0, 0, -45);
    }

    public void Shoot(bool success)
    {
        isSuccess = success;
        
        Vector3 dir = targetFish.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 성공이면 물고기를 향해, 실패면 빗나가게(약간 위로 회전)
        if (!success && isLaunched == false)
        {
            transform.Rotate(0, 0, 15); // 15도 정도 빗나가게 회전
        }

        isLaunched = true;

        if (animator != null)
        {
            animator.SetBool("IsFlying", true);
        }
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
                Vector3 hitPoint = new Vector3(targetFish.position.x - 0.5f, targetFish.position.y - 0.2f, 0);
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, hitPoint, Quaternion.identity);
                }
                isLaunched = false;

                if (animator != null)
                {
                    animator.SetBool("IsFlying", false);
                }
            }
        }
    }
}
