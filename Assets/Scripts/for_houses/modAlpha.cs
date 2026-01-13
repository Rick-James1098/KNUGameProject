using UnityEngine;
using System.Collections;

public class FadeObject : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private SpriteRenderer houseSprite; // 집의 스프라이트 렌더러
    [SerializeField] [Range(0f, 1f)] private float fadeAlpha = 0.5f; // 가려졌을 때 투명도 (0.5 = 50%)
    [SerializeField] private float fadeSpeed = 5f; // 페이드 속도

    private float targetAlpha = 1f; // 목표 투명도

    private void Update()
    {
        // 현재 투명도에서 목표 투명도로 부드럽게 변경
        Color curColor = houseSprite.color;
        float newAlpha = Mathf.MoveTowards(curColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
        houseSprite.color = new Color(curColor.r, curColor.g, curColor.b, newAlpha);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 태그를 가진 오브젝트가 들어오면 투명하게
        if (other.CompareTag("Player"))
        {
            targetAlpha = fadeAlpha;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 플레이어가 나가면 다시 원래대로(1.0)
        if (other.CompareTag("Player"))
        {
            targetAlpha = 1f;
        }
    }
}