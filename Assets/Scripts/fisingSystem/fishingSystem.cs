using NUnit.Framework;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.UI;

public class FishingSystem : MonoBehaviour
{
    private bool isFishing = false;
    private TopDownBobber currentBobber;
    private PlayerMovement moveScript;
    public GameObject bobberPrefab;    // 찌 프리팹
    public Transform throwPoint;       // 찌가 나가는 위치 (캐릭터 손 근처)
    public Slider powerSlider;         // 1단계에서 만든 슬라이더
    
    public float maxForce = 15f;       // 최대 발사 힘
    public float chargeSpeed = 1.5f;   // 게이지 차는 속도
    
    private float currentPower = 0f;   // 현재 모인 힘 (0 ~ 1)
    private bool isCharging = false;   // 기 모으는 중인가?

    void Awake()
    {
        moveScript = GetComponent<PlayerMovement>();
    }

    void Update()
    {

        if (isFishing)
        {
            if (Input.GetMouseButton(0))
            {
                RetrieveFishing();
            }

            return;
        }
        // 1. 마우스 왼쪽 버튼을 꾹 누르기 시작할 때
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentPower = 0f;
            powerSlider.gameObject.SetActive(true); // 게이지 보이기
        }

        // 2. 누르고 있는 동안 게이지 충전
        if (isCharging)
        {
            currentPower += Time.deltaTime * chargeSpeed;
            if (currentPower > 1f) currentPower = 1f; // 최대치 고정
            
            powerSlider.value = currentPower; // UI 반영
        }

        // 3. 버튼을 뗐을 때 발사!
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            ThrowBobber();
            isCharging = false;
            powerSlider.gameObject.SetActive(false); // 게이지 숨기기
        }
    }

    // FishingSystem.cs 내부의 함수 수정
    void ThrowBobber()
    {
        isFishing = true;
        moveScript.canMove = false;
        // 1. 찌 생성
        GameObject bobber = Instantiate(bobberPrefab, throwPoint.position, Quaternion.identity);
        currentBobber = bobber.GetComponent<TopDownBobber>();
        currentBobber.fishingSystem = this;
        currentBobber.Launch(moveScript.lastDir, currentPower);
        // 2. 찌에 붙어있는 'TopDownBobber' 스크립트를 가져옴
        TopDownBobber bobberScript = bobber.GetComponent<TopDownBobber>();

        if (bobberScript != null)
        {
            // [수정] 이동 스크립트를 가져와서 그 안의 lastDir를 던지는 방향으로 씁니다.
            PlayerMovement moveScript = GetComponent<PlayerMovement>();
            Vector2 throwDir = moveScript.lastDir;
            Debug.Log("던지는 방향: " + throwDir);

            bobberScript.Launch(throwDir, currentPower);
        }
    }

    public void RetrieveFishing()
    {
        if (currentBobber != null)
        {
            Destroy(currentBobber.gameObject);
        }

        ResetFishingState();
    }

    public void ResetFishingState()
    {
        isFishing = false;
        currentBobber = null;
        moveScript.canMove = true;
        Debug.Log("낚시 종료, 이동 가능");
    }
}