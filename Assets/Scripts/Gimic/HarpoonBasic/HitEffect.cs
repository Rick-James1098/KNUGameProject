using UnityEngine;

public class HitEffect : MonoBehaviour
{
    public float destroyTime = 1.0f; // 애니메이션 길이만큼 설정
    public AudioSource myAudio; // 인스펙터에서 할당
    public AudioClip effectSound;

    void Start()
    {
        myAudio.PlayOneShot(effectSound);
        Destroy(gameObject, destroyTime);
    }
}
    