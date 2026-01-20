using UnityEngine;

public class HitEffect : MonoBehaviour
{
    public float destroyTime = 1.0f; // 애니메이션 길이만큼 설정

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
