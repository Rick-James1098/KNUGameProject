using UnityEngine;
using System.Collections;

public class BasicHarpoon : MonoBehaviour
{
    [Header("Settings")]
    public float rotateSpeed = 300f;    // 회전 속도
    public float shotSpeed = 30f;

    [Header("Pull Settings")]
    public float pullDistance = 1.0f;
    public float pullSpeed = 5f;
    public float stopTime = 1f;    // 중간에 멈추는 시간

    private float minAngle = -90f;
    private float maxAngle = 90f;
    private float currentZ = 0f;
    private bool isFired = false;
    private bool isHit = false;
    private bool isPulling = false;
    private Rigidbody2D rb;

    [Header("Effects")]
    public GameObject hitEffect; 
    public AudioSource audioSource;
    public AudioClip shotAudio;

    [Header("Rope Settings")]
    public GameObject ropePrefab;      // 만들어둔 RopeSystem 프리팹
    public Transform tailPoint;        // 줄이 매달릴 작살의 꼬리 위치 (빈 오브젝트)
    private RopeRenderer currentRope;  // 현재 생성된 줄

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (ropePrefab != null)
        {
            // 1. 로프 생성 (현재 작살 위치 = 천장 위치에 생성)
            GameObject ropeObj = Instantiate(ropePrefab, transform.position, Quaternion.identity);
            currentRope = ropeObj.GetComponent<RopeRenderer>();

            // 2. 작살 몸체(혹은 꼬리)에 Rigidbody2D가 있어야 연결 가능
            Rigidbody2D myRB = GetComponent<Rigidbody2D>();
            currentRope.AttachToTarget(rb, tailPoint);
            IgnoreRopeCollision(ropeObj);
        }
    }

    // 로프와 작살 충돌 판정 방지(충돌 무시)
    void IgnoreRopeCollision(GameObject ropeObj)
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null)
        {
            Collider2D[] ropeColliders = ropeObj.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D ropeCol in ropeColliders)
            {
                Physics2D.IgnoreCollision(myCollider, ropeCol, true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isFired == false)
        {
            float input = Input.GetAxis("Horizontal"); 
            currentZ -= input * rotateSpeed * Time.deltaTime;
            currentZ = Mathf.Clamp(currentZ, minAngle, maxAngle);
            transform.rotation = Quaternion.Euler(0, 0, -currentZ);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isFired = true; // 이제부터는 발사 모드로 전환
                audioSource.PlayOneShot(shotAudio);
            }
        }
    }

    void FixedUpdate()
    {
        if (isFired && !isHit)
        {
            // Translate 대신 속도(Velocity)를 직접 주입
            rb.linearVelocity = -transform.up * shotSpeed;

            // 화면 밖 파괴 체크
            if (Mathf.Abs(transform.position.y) > 4f || Mathf.Abs(transform.position.x) > 7.5f || Mathf.Abs(transform.position.x) < -7.5f)
            {
                Destroy(gameObject);
            }
        }
        else if (isPulling)
        {
            rb.linearVelocity = transform.up * pullSpeed;

            // 화면 위로 완전히 사라지면 파괴 (Y좌표 10은 상황에 맞춰 조절)
            if (transform.position.y > 5f) 
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // 발사 안 했거나 맞았으면 속도 0 (미끄러짐 방지)
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 이미 맞았으면 또 검사하지 않음 && 물고기랑 부딪혔는지 확인
        if (isHit == false && other.CompareTag("Fish"))
        {
            FishShadow fishScript = other.GetComponent<FishShadow>();

            if (fishScript != null && fishScript.catchable == true)
            {
                if (hitEffect != null)
                {
                    // "물고기(other)의 표면 중, 내(작살) 위치와 가장 가까운 점"을 찾음
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    hitPoint.y -= 0.5f;

                    // 피 생성! (회전은 기본값 혹은 작살 반대 방향 등 취향껏)
                    Instantiate(hitEffect, hitPoint, Quaternion.identity);
                }
                transform.Translate(Vector3.down * 0.5f);

                isHit = true; // 작살 멈춤

                if (fishScript != null)
                {   
                    Rigidbody2D fishRB = other.GetComponent<Rigidbody2D>();
                    if (fishRB != null) Destroy(fishRB);

                // 3. 물고기 충돌체 끄기 (다른데 부딪히지 않게)
                    Collider2D fishCol = other.GetComponent<Collider2D>();
                    if (fishCol != null) fishCol.enabled = false;

                    other.transform.SetParent(this.transform);
                    
                    fishScript.End();
                }

                // 스포너 찾아서 생성 중단 시키기
                // FindObjectOfType은 씬에 있는 FishSpawner를 찾아줍니다.
                FishSpawner spawner = FindObjectOfType<FishSpawner>();
                if (spawner != null)
                {
                    spawner.isOvered = true;
                }
                
                StartCoroutine(PullRoutine());
            }
        }
    }

    IEnumerator PullRoutine()
    {
        // 잡자마자는 잠시 대기 (2초)
        yield return new WaitForSeconds(2.0f);

        while (transform.position.y < 5f)
        {
            // 1. 목표 지점 계산 (현재 위치에서 위쪽으로 pullDistance만큼)
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + (transform.up * pullDistance); // 위로 당김

            float journey = 0f;
            
            // 2. 목표 지점까지 부드럽게 이동 (Move)
            while (journey < 1f)
            {
                journey += (pullSpeed * Time.deltaTime) / pullDistance;
                transform.position = Vector3.Lerp(startPos, targetPos, journey);
                yield return null; // 한 프레임 대기
            }

            // 3. 확실히 목표점에 도달
            transform.position = targetPos;

            // 4. 쉰다! (Stop) -> 이때는 코드가 아무것도 안 하므로 확실히 멈춤
            yield return new WaitForSeconds(stopTime);
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (currentRope != null)
        {
            Destroy(currentRope.gameObject);
        }
    }
}
