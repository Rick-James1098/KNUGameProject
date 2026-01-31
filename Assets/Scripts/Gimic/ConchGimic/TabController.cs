using UnityEngine;
using System.Collections;

public class TabController : MonoBehaviour
{
    private Animator animator;
    private ConchGameManager manager;
    private bool isClicked = false;
    public float lifeTime = 2.0f;

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

        // 매니저에게 "나 잡혔어!" 보고
        manager.OnClicked(gameObject.transform.position);

        // 즉시 삭제 (또는 클릭 성공 이펙트 재생 후 삭제)
        Destroy(gameObject);
    }

    IEnumerator LifeCycleRoutine()
    {
        // 2. 설정된 시간(애니메이션 길이)만큼 대기
        yield return new WaitForSeconds(lifeTime);

        // 3. 시간이 다 될 때까지 클릭 안 당했으면?
        if (!isClicked)
        {
            // 매니저에게 "나 사라질게, 다음 거 준비해" 보고
            manager.OnMissed();
            
            // 삭제
            Destroy(gameObject);
        }
    }
}
