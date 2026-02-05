using System.Collections;
using UnityEngine;

public class TopDownBobber : MonoBehaviour
{
    [HideInInspector] public FishingSystem fishingSystem;
    public Transform visualChild;

    public bool isSettled = false;
    
    [Header("포물선 설정")]
    public AnimationCurve arcCurve;
    public float bobberHeight = 2f;
    public float maxDistance = 7f;
    public float duration = 1f;
    public float heightMult = 2f;

    [Header("이펙트")]
    public GameObject splashPrefab;
    public LayerMask waterLayer;

    public void Launch(Vector2 direction, float power)
    {
        isSettled = false;
        Vector2 start = transform.position;
        Vector2 target = start + (direction * power * maxDistance);
        StartCoroutine(FlyToTarget(start, target));
    }

    IEnumerator FlyToTarget(Vector2 start, Vector2 target)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = Vector2.Lerp(start, target, t);
            float h = Mathf.Lerp(bobberHeight, 0, t) + (arcCurve.Evaluate(t) * heightMult);
            visualChild.localPosition = new Vector2(0, h);
            yield return null;
        }

        visualChild.localPosition = Vector2.zero;
        CheckLanding(target);
    }

    void CheckLanding(Vector2 landPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(landPos, waterLayer);
        if (hit != null)
        {
            isSettled = true;
            Instantiate(splashPrefab, landPos, Quaternion.identity);
            fishingSystem.OnBobberLanded(visualChild.position); // 지휘관에게 보고
            StartCoroutine(BobbingOnWater());
        }
        else
        {
            Debug.Log("땅에 떨어짐");
            fishingSystem.RetrieveFishing();
        }
    }

    IEnumerator BobbingOnWater()
    {
        float timer = 0;
        while (true)
        {
            timer += Time.deltaTime * 2f;
            float newY = Mathf.Sin(timer) * 0.15f; 
            visualChild.localPosition = new Vector2(0, newY);
            yield return null;
        }
    }

    // 지휘관이 호출하는 비주얼 변경 함수
    public void SetBitingVisual(bool isBiting)
    {
        if (isBiting)
        {
            StopAllCoroutines(); // 둥둥 떠 있는 애니메이션 중지
            visualChild.localPosition = new Vector3(0, -0.4f, 0); // 물속으로 쏙
        }
    }
}