using UnityEngine;

public class BasicHarpoon : MonoBehaviour
{
    public float rotateSpeed = 300f;    // 회전 속도
    public float shotSpeed = 30f;
    private float minAngle = -90f;
    private float maxAngle = 90f;
    private float currentZ = 0f;
    private bool isFired = false;
    private bool isHit = false;

    [Header("Effects")]
    public GameObject hitEffect; 

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
            }
        }
        else
        {
            if(isHit == false)
            {
                transform.Translate(Vector3.down * shotSpeed * Time.deltaTime);

                if (Mathf.Abs(transform.position.y) > 4f || Mathf.Abs(transform.position.x) > 7.5f || Mathf.Abs(transform.position.x) < -7.5f)
                {
                    Destroy(gameObject);
                }
            }
            
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
                transform.Translate(Vector3.down * 1f);

                isHit = true; // 작살 멈춤

                // 1. 맞은 물고기 가져오기
                
                
                if (fishScript != null)
                {
                    // 2. 물고기 얼음! (Destroy 하지 않음)
                    fishScript.End();
                }

                // 4. 스포너 찾아서 생성 중단 시키기
                // FindObjectOfType은 씬에 있는 FishSpawner를 찾아줍니다.
                FishSpawner spawner = FindObjectOfType<FishSpawner>();
                if (spawner != null)
                {
                    spawner.isOvered = true;
                }
            }
        }
    }
}
