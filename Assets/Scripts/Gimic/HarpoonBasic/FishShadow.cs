using UnityEngine;
using System.Collections;

public class FishShadow : MonoBehaviour
{
    private PolygonCollider2D moveArea;
    private Vector3 targetPosition;
    public bool catchable = false;
    [Header("Moving Settings")]
    public float speed = 30f;
    public float moveDurationMin = 2.0f; // 최소 이만큼은 움직이다가 멈춤
    public float moveDurationMax = 5.0f; // 최대 이만큼 움직이다가 멈춤
    public float stopDuration = 1.0f;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer; // 물고기 이미지를 보여주는 컴포넌트
    public Sprite leftSprite;  // 왼쪽 보는 이미지
    public Sprite rightSprite; // 오른쪽 보는 이미지

    [Header("Colliders")]
    public PolygonCollider2D leftCollider;
    public PolygonCollider2D rightCollider;

    [Header("Effects")]
    public GameObject surpriseMark; 
    public Vector3 leftMarkOffset = new Vector3(-0.1f, 0.1f, 0);
    public Vector3 rightMarkOffset = new Vector3(0.1f, 0.1f, 0);
    
    public GameObject leftRipple;   // 왼쪽 볼 때 켜질 녀석
    public GameObject rightRipple;  // 오른쪽 볼 때 켜질 녀석

    public void Setup(PolygonCollider2D area)
    {   
        moveArea = area;
        SetNewRandomTarget();
        StartCoroutine(RoamRoutine());
    }
    
    IEnumerator RoamRoutine()
    {
        while (true) // 무한 반복 (게임 끝날 때까지)
        {
            float roamingTime = Random.Range(moveDurationMin, moveDurationMax);
            float currentTimer = 0f;

            // 정해진 시간이 될 때까지 계속 움직임
            while (currentTimer < roamingTime)
            {
                // 1. 이동
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                
                // 2. 방향 전환 (스프라이트 & 콜라이더) - 매 프레임 체크
                UpdateDirectionSprite();

                // 3. 목적지에 도착했나요?
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    // 시간이 아직 남았는데 도착해버렸다면 -> 즉시 새로운 목적지 찾기
                    SetNewRandomTarget();
                }

                currentTimer += Time.deltaTime;
                yield return null; // 한 프레임 대기
            }

            if (leftRipple != null) leftRipple.SetActive(false);
            if (rightRipple != null) rightRipple.SetActive(false);

            catchable = true; // 작살 맞기 가능

            // 느낌표 생성 (Pivot 기준 + Offset)
            GameObject mark = null;
            if (surpriseMark != null)
            {
                if(spriteRenderer.sprite == rightSprite)
                    mark = Instantiate(surpriseMark, transform.position + rightMarkOffset, Quaternion.identity);
                else
                    mark = Instantiate(surpriseMark, transform.position + leftMarkOffset, Quaternion.identity);
                mark.transform.SetParent(transform); // 물고기를 따라다니게
            }

            // 멈춰있는 시간만큼 대기
            yield return new WaitForSeconds(stopDuration);

            // 정리하고 다시 헤엄치러 가기
            if (mark != null) Destroy(mark);
            catchable = false;
        }
    }

    // 폴리곤 범위 내 랜덤 좌표 찾기
    void SetNewRandomTarget()
    {
        if (moveArea == null) return;

        // 단순화를 위해 Bounds 내에서 랜덤을 뽑고, 실제 포함되는지 체크
        // (복잡한 모양일 경우 100번 시도 등의 안전장치 추가 가능)
        Vector2 randomPoint = Vector2.zero;
        int attempts = 0;
        
        while (attempts < 100)
        {
            float x = Random.Range(moveArea.bounds.min.x, moveArea.bounds.max.x);
            float y = Random.Range(moveArea.bounds.min.y, moveArea.bounds.max.y);
            Vector2 p = new Vector2(x, y);

            if (moveArea.OverlapPoint(p))
            {
                targetPosition = p;
                return;
            }
            attempts++;
        }
        // 못 찾으면 제자리
        targetPosition = transform.position;
    }

    void UpdateDirectionSprite()
    {
        if (spriteRenderer == null) return;

        // 목적지가 내 오른쪽이면
        if (targetPosition.x > transform.position.x)
        {
            if (rightSprite != null) 
            {
                spriteRenderer.sprite = rightSprite;

                if (rightCollider != null) rightCollider.enabled = true;
                if (leftCollider != null) leftCollider.enabled = false;

                if (rightRipple != null) rightRipple.SetActive(true);
                if (leftRipple != null) leftRipple.SetActive(false);
            }
        }
        // 목적지가 내 왼쪽이면
        else
        {
            if (leftSprite != null) 
            {
                spriteRenderer.sprite = leftSprite;

                if (leftCollider != null) leftCollider.enabled = true;
                if (rightCollider != null) rightCollider.enabled = false;

                if (leftRipple != null) leftRipple.SetActive(true);
                if (rightRipple != null) rightRipple.SetActive(false);
            }
        }
    }

    // 작살에 맞았을 때 (BasicHarpoon에서 호출됨)
    public void End()
    {
        StopAllCoroutines(); // 모든 움직임 정지
        catchable = false;   // 더 이상 맞지 않음

        if (leftRipple != null) leftRipple.SetActive(false);
        if (rightRipple != null) rightRipple.SetActive(false);
        
        // 자식 오브젝트(느낌표 등) 정리
        foreach (Transform child in transform)
        {
             Destroy(child.gameObject);
        }
    }
}
