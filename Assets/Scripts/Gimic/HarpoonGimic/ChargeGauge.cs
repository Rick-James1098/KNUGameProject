using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StackGauge : MonoBehaviour
{
    private HarpoonGimicManager gameManager; // 매니저에게 보고하기 위한 변수
    private Harpoon harpoon;

    private bool isCharging = true;
    private float chargeTimePerBlock = 0.1f;

    [Header("UI Objects")]
    public GameObject[] gaugeBlocks; // 유니티 에디터에서 블럭 이미지들을 순서대로 넣어주세요

    [Header("Settings")]
     // 블럭 하나가 켜지는 데 걸리는 시간
    private float currentTimer = 0.0f;
    private int currentBlockIndex = 0; // 현재 몇 번째 블럭까지 켜졌는지

    [Header("Target System")]
    public GameObject targetLine; // 아까 만든 빨간 선 UI 연결
    public int targetBlockIndex;  // 정답 블럭 번호 (자동 설정됨)
    public Harpoon harpoonPrefab; // 작살 컨트롤러 연결

    [Header("Calculation Settings")]
    public float blockHeight = 30f; // 블럭 하나의 높이 (Inspector에서 설정)
    public float spacing = 5f;      // 블럭 사이 간격 (Layout Group의 Spacing 값과 동일하게)
    public float startY = -150f;    // 첫 번째 블럭(0번)의 Y 좌표 (직접 찾아서 입력)

    public void SetRandomTarget()
    {
        targetBlockIndex = Random.Range(3, gaugeBlocks.Length);

        float calcY = startY + (targetBlockIndex * (blockHeight + spacing));

        targetLine.SetActive(true);

        // 부모(Container) 기준의 상대 좌표(LocalPosition)를 사용합니다.
        // X는 0 (가운데), Y는 계산된 값, Z는 0
        targetLine.transform.localPosition = new Vector3(0, calcY, 0);
    }

    void Start()
    {
        // 시작할 때 모든 블럭 끄기
        ResetGaugeUI();
    }

    public void InitGame(Harpoon harpoonCtrl, HarpoonGimicManager manager)
    {
        this.harpoon = harpoonCtrl;
        this.gameManager = manager;
        
        // 게이지 값 초기화
        currentTimer = 0;
        currentBlockIndex = 0;
        chargeTimePerBlock = Random.Range(0.01f, 0.2f);
    }
    void Update()
    {
        // 1. 마우스 누르는 중
        if (Input.GetMouseButton(0) && isCharging)
        {
            Charge();
        }

        // 2. 마우스 뗐을 때 (초기화 또는 발사)
        if (Input.GetMouseButtonUp(0))
        {
            isCharging = false;

            // 여기에 발사 로직 추가 가능
            CheckSuccess();
        }
    }

    void Charge()
    {
        // 모든 블럭이 다 찼으면 더 이상 충전 안 함
        if (currentBlockIndex >= gaugeBlocks.Length) return;

        // 시간 누적
        currentTimer += Time.deltaTime;

        // 누적 시간이 설정한 시간보다 커지면 블럭 하나 켜기
        if (currentTimer >= chargeTimePerBlock)
        {
            currentTimer = 0.0f; // 타이머 초기화
            
            // 현재 인덱스의 블럭을 켜고, 인덱스 증가
            if (currentBlockIndex < gaugeBlocks.Length)
            {
                gaugeBlocks[currentBlockIndex].SetActive(true);
                currentBlockIndex++;
            }
        }
    }

    void CheckSuccess()
    {
        // 정확히 목표 칸에 멈췄는지 확인
        bool isSuccess = (currentBlockIndex == targetBlockIndex); // +1은 인덱스 차이 보정

        if (harpoon != null && gameManager.isStarted == true)
        {
            harpoon.Shoot(isSuccess);
        }

        if (gameManager != null)
        {
            gameManager.OnRoundFinished();
        }
    }

    public void ResetGaugeUI()
    {
        // 블럭 다 끄기
        for (int i = 0; i < gaugeBlocks.Length; i++)
        {
            gaugeBlocks[i].SetActive(false);
        }
        
        // 빨간 선 끄기
        if (targetLine != null)
        {
            targetLine.SetActive(false);
        }

        currentBlockIndex = 0;
        currentTimer = 0;
        isCharging = true;
    }
}