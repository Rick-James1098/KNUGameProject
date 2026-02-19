using UnityEngine;
using UnityEngine.EventSystems;

public class SingletonEventSystem : MonoBehaviour
{
    private static EventSystem instance;

    void Awake()
    {
        if (instance != null && instance != GetComponent<EventSystem>())
        {
            // 이미 존재한다면 새로 생긴 나를 파괴
            Destroy(gameObject);
            return;
        }

        instance = GetComponent<EventSystem>();
        DontDestroyOnLoad(gameObject); // 하나만 남아서 씬을 계속 따라다님
    }
}