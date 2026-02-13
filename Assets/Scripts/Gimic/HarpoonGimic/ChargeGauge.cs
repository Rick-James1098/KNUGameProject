using System;
using UnityEngine;
using UnityEngine.UI;

public class StackGauge : MonoBehaviour
{
    private HarpoonGimicManager gameManager; // 매니저에게 보고하기 위한 변수
    private Harpoon harpoon;

    private bool isCharging = true;

    [Header("UI Objects")]
    public Image gaugeImage;       // Filled로 설정된 게이지바 이미지
    public GameObject targetLineMin; // 목표 구간 시작선 (아래쪽)
    public GameObject targetLineMax; // 목표 구간 끝선 (위쪽)

    [Header("Settings")]
    public float fillSpeed = 0.5f; // 게이지가 차오르는 속도 (0 ~ 1 사이 값/초)
    private float currentFillAmount = 0.0f;

    [Header("Target System")]
    private float targetMinAmount; // 목표 구간 최소값 (0.0 ~ 1.0)
    private float targetMaxAmount; // 목표 구간 최대값 (0.0 ~ 1.0)

    [Header("Layout Settings")]
    private float gaugeHeight = 877.5f; // 게이지바 전체 높이 (UI RectTransform 높이와 맞춰주세요)
    private float startY = -468.75f;     // 게이지바의 바닥 Y 좌표

    [Header("Audio Settings")]
    public AudioSource audioSource; // 효과음을 재생할 오디오 소스
    public AudioClip successAudio;     // 성공했을 때 소리 (띵동!)
    public AudioClip failAudio;        // 실패했을 때 소리 (띠~!)

    public void SetRandomTarget()
    {
        float rangeSize = UnityEngine.Random.Range(0.01f, 0.1f);

        // 목표 구간의 시작점 결정 (0.3 ~ 0.8 사이에서 랜덤)
        targetMinAmount = UnityEngine.Random.Range(0.3f, 0.8f - rangeSize);
        targetMaxAmount = targetMinAmount + rangeSize;

        // 3. 기준선 UI 위치 잡기
        targetLineMin.SetActive(true);
        targetLineMax.SetActive(true);

        // 게이지 값(0~1)을 Y좌표로 변환: 시작Y + (전체높이 * 비율)
        float minLineY = startY + (gaugeHeight * targetMinAmount);
        float maxLineY = startY + (gaugeHeight * targetMaxAmount);

        targetLineMin.transform.localPosition = new Vector3(0, minLineY, 0);
        targetLineMax.transform.localPosition = new Vector3(0, maxLineY, 0);
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
        
        fillSpeed = UnityEngine.Random.Range(0.5f, 3f);
    }
    void Update()
    {
        // 1. 마우스 누르는 중
        if (Input.GetMouseButton(0) && isCharging)
        {
            Charge();
        }

        // 2. 마우스 뗐을 때 (초기화 또는 발사)
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            isCharging = false;
            CheckSuccess();
        }
    }

    void Charge()
    {
        // 모든 블럭이 다 찼으면 더 이상 충전 안 함
        if (currentFillAmount >= 1.0f) return;

        // 시간 * 속도만큼 채움
        currentFillAmount += Time.deltaTime * fillSpeed;
        
        // UI 업데이트
        if (gaugeImage != null)
        {
            gaugeImage.fillAmount = currentFillAmount;
        }
    }

    void CheckSuccess()
    {
        // 정확히 목표 칸에 멈췄는지 확인
        bool isSuccess = (currentFillAmount >= targetMinAmount && currentFillAmount <= targetMaxAmount); // +1은 인덱스 차이 보정

        if (audioSource != null)
        {
            if (isSuccess && successAudio != null)
            {
                audioSource.PlayOneShot(successAudio);
            }
            else if (!isSuccess && failAudio != null)
            {
                audioSource.PlayOneShot(failAudio);
            }
        }

        if (harpoon != null && gameManager.isStarted == true)
        {
            harpoon.Shoot(isSuccess);
        }

        if (gameManager != null)
        {
            gameManager.OnRoundFinished(isSuccess);
        }
    }

    public void ResetGaugeUI()
    {
        currentFillAmount = 0.0f;
        isCharging = true;

        if (gaugeImage != null) gaugeImage.fillAmount = 0f;

        if (targetLineMin != null) targetLineMin.SetActive(false);
        if (targetLineMax != null) targetLineMax.SetActive(false);
    }
}