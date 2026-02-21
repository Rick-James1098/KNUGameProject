using UnityEngine;
using System.Collections;

public class EndingEffect : MonoBehaviour
{
    [Header("Settings")]
    public int bounceCount = 2;     // 몇 번 튈지 (2번)
    public float jumpHeight = 1.5f; // 얼마나 높게 뛸지
    public float duration = 0.4f;   // 한 번 뛰는 데 걸리는 시간
    public AudioSource audioSource;
    public AudioClip successAudio;

    void Start()
    {
        // 등장하자마자 튀기 시작
        StartCoroutine(BounceRoutine());
        audioSource.PlayOneShot(successAudio);
    }

    IEnumerator BounceRoutine()
    {
        Vector3 startPos = transform.position;
        // 원래 크기 저장 (튀면서 스케일 효과도 살짝 주면 더 쫀득합니다)
        Vector3 startScale = transform.localScale; 

        for (int i = 0; i < bounceCount; i++)
        {
            // 1. 위로 점프 (올라갈 때)
            float timer = 0f;
            while (timer <= 1.0f)
            {
                timer += Time.deltaTime / (duration / 2); // 절반 시간 동안 올라감
                
                // 포물선 이동 (Ease Out)
                float height = Mathf.Sin(timer * Mathf.PI * 0.5f) * jumpHeight;
                transform.position = startPos + new Vector3(0, height, 0);
                
                yield return null;
            }

            // 2. 아래로 착지 (내려올 때)
            timer = 0f;
            while (timer <= 1.0f)
            {
                timer += Time.deltaTime / (duration / 2); // 절반 시간 동안 내려옴
                
                // 포물선 이동 (Ease In)
                float height = Mathf.Cos(timer * Mathf.PI * 0.5f) * jumpHeight; 
                // (Cos는 1에서 0으로 가므로 높이 계산에 적합)
                transform.position = startPos + new Vector3(0, (1 - timer) * jumpHeight, 0); // 수정된 수식

                // 착지할 때 찌그러지는 연출 (선택 사항)
                if (timer > 0.8f) 
                {
                    transform.localScale = new Vector3(startScale.x * 1.2f, startScale.y * 0.8f, 1);
                }

                yield return null;
            }

            // 바닥에 닿음 -> 크기 복구
            transform.position = startPos;
            transform.localScale = startScale;
            
            // 다음 점프 전 아주 잠깐 대기
            yield return new WaitForSeconds(0.05f);
        }
    }
}
