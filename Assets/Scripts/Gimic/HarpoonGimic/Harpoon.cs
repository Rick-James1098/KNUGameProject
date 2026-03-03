using UnityEngine;
using System.Collections;

public class Harpoon : MonoBehaviour
{
    private Transform targetFish; // 목표 물고기
    private bool isLaunched = false;
    private bool isSuccess = false;
    public float speed = 15f;
    private Animator animator;

    [Header("Settings")]
    public float tailOffset = 2.0f;

    [Header("Effects")]
    public GameObject hitEffect;
    public AudioSource audioSource;
    public AudioClip shootAudio;

    [Header("Rope System")]
    public GameObject ropePrefab;      
    private RopeRenderer currentRope;
    public Transform tailPoint;
    private Rigidbody2D rb;

    [Header("Pull Settings")]
    public float pullDistance = 1.0f;
    public float pullSpeed = 5f;
    public float stopTime = 1f; 
    void Start()
    {
        // 시작할 때 내 몸에 있는 애니메이터를 찾아둡니다.
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (ropePrefab != null)
        {
            // 1. 로프 생성 (현재 작살 위치 = 천장 위치에 생성)
            GameObject ropeObj = Instantiate(ropePrefab, tailPoint.position, Quaternion.identity);
            currentRope = ropeObj.GetComponent<RopeRenderer>();

            // 2. 작살 몸체(혹은 꼬리)에 Rigidbody2D가 있어야 연결 가능
            Rigidbody2D myRB = GetComponent<Rigidbody2D>();
            currentRope.AttachToTarget(rb, tailPoint);
        }
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

        Vector3 fixedTailPos = transform.position - (transform.right * tailOffset);
        
        Vector3 dir = targetFish.position - fixedTailPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.position = fixedTailPos + (transform.right * tailOffset);

        // 성공이면 물고기를 향해, 실패면 빗나가게(약간 위로 회전)
        if (!success && isLaunched == false)
        {
            transform.RotateAround(fixedTailPos, Vector3.forward, 15f);
        }

        StartCoroutine(ShootProcess());
    }

    IEnumerator ShootProcess()
    {
        // 1초 대기 (이때 작살은 조준된 상태로 멈춰있음)
        yield return new WaitForSeconds(0.5f);

        // 1초 뒤 발사 시작!
        isLaunched = true;

        audioSource.PlayOneShot(shootAudio);

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
            if (distance <= 0.05f)
            {   
                isLaunched = false;

                Vector3 hitPoint = new Vector3(targetFish.position.x - 0.5f, targetFish.position.y - 0.2f, 0);
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, hitPoint, Quaternion.identity);
                }
                
                if (animator != null)
                {
                    animator.SetBool("IsFlying", false);
                }

                CatchFish();
            }
        }
    }

    void CatchFish()
    {
        if (targetFish != null)
        {
            // 작살의 자식으로 설정 (같이 움직임)
            targetFish.SetParent(this.transform);
        }

        // 릴링 코루틴 시작
        StartCoroutine(PullRoutine());
    }

    IEnumerator PullRoutine()
    {
        yield return new WaitForSeconds(2.0f); // 잡고 나서 잠시 대기

        while (true)
        {
            // 현재 방향의 반대(뒤쪽)로 당김
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos - (transform.right * pullDistance); 

            float journey = 0f;
            while (journey < 1f)
            {
                journey += (pullSpeed * Time.deltaTime) / pullDistance;
                transform.position = Vector3.Lerp(startPos, targetPos, journey);
                yield return null; 
            }
            transform.position = targetPos; // 위치 보정

            yield return new WaitForSeconds(stopTime); // 잠시 멈춤

            // 화면 밖으로 나가면 파괴 (로프도 같이 사라짐)
            if (!GetComponent<Renderer>().isVisible)
            {
                Destroy(gameObject);
                yield break;
            }
        }
    }

    void OnDestroy()
    {
        // 내 로프가 아직 살아있다면 같이 없애줘!
        if (currentRope != null)
        {
            Destroy(currentRope.gameObject);
        }
    }
}
