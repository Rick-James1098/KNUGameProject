using UnityEngine;
using System.Collections;

public class TabController : MonoBehaviour
{
    private Animator animator;
    private ConchGameManager manager;
    private bool isClicked = false;
    public float lifeTime = 2.0f;

    [Header("Effects")]
    public GameObject clickEffectPrefab;
    public AudioSource audioSource; 
    public AudioClip clickAudio;

    public void Setup(ConchGameManager gm, float speedMultiplier)
    {
        manager = gm;
        animator = GetComponent<Animator>();
        
        float originalClipLength = 2.0f;
        lifeTime = originalClipLength / speedMultiplier;

        if (animator != null) animator.speed = speedMultiplier;


        // 1. 애니메이션 길이만큼 기다렸다가 사라지는 타이머 시작
        StartCoroutine(LifeCycleRoutine());
    }
    
    private void OnMouseDown()
    {
        if (isClicked) return; // 이미 클릭했으면 무시

        isClicked = true;
        
        if (audioSource != null)
        {
            if (isClicked && clickAudio != null)
            {
                AudioSource.PlayClipAtPoint(clickAudio, transform.position);
            }
        }

        if (clickEffectPrefab != null)
        {
            // 현재 내 위치(transform.position)에 이펙트 생성
            GameObject effect = Instantiate(clickEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.4f);
        }

        manager.OnClicked(gameObject.transform.position);

        Destroy(gameObject);
    }

    IEnumerator LifeCycleRoutine()
    {
        // 2. 설정된 시간(애니메이션 길이)만큼 대기
        yield return new WaitForSeconds(lifeTime);

        if (!isClicked)
        {
            manager.OnMissed();
            
            Destroy(gameObject);
        }
    }
}
