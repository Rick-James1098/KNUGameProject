using UnityEngine;
using System.Collections;

public class ConchFailEffect : MonoBehaviour
{
    [Header("Settings")]
    public float dropHeight = 5.0f; // 떨어지기 시작할 높이
    public float gravity = 20.0f;   // 중력 가속도 (클수록 빨리 떨어짐)
    public float bounceFactor = 0.5f; // 바닥에 닿았을 때 튀어 오르는 정도 (0 ~ 1)
    
    void Start()
    {
        // 시작하자마자 떨어지는 코루틴 실행
        StartCoroutine(DropRoutine());
    }

    IEnumerator DropRoutine()
    {
        // 1. 원래 배치된 위치(바닥 지점)를 기억해둡니다.
        Vector3 floorPos = transform.position;
        
        // 2. 물체를 위로 들어 올립니다. (시작 위치)
        float currentY = dropHeight;
        float velocity = 0f; // 현재 떨어지는 속도

        // 3. 물리 시뮬레이션 루프
        // (완전히 멈출 때까지 반복)
        while (true)
        {
            // 중력 적용 (속도가 점점 빨라짐)
            velocity -= gravity * Time.deltaTime;
            
            // 위치 적용
            currentY += velocity * Time.deltaTime;

            // 4. 바닥에 닿았는지 체크
            if (currentY <= 0)
            {
                currentY = 0; // 바닥 밑으로 못 가게 고정

                // 튀어 오르기 (속도 반전 + 에너지 손실)
                velocity = -velocity * bounceFactor;

                // 속도가 너무 줄어들면 멈춤 (루프 종료)
                if (Mathf.Abs(velocity) < 0.5f)
                {
                    break;
                }
            }

            // 계산된 Y값을 실제 오브젝트에 적용
            transform.position = floorPos + new Vector3(0, currentY, 0);

            yield return null; // 다음 프레임까지 대기
        }

        // 확실하게 바닥 위치로 고정하고 끝냄
        transform.position = floorPos;
    }
}
