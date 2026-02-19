using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요!
using System.Collections.Generic;

public class PersistentUI : MonoBehaviour
{
    private static PersistentUI instance;

    // UI를 숨기고 싶은 씬 이름들을 리스트로 관리합니다.
    [Header("UI를 숨길 씬 목록")]
    public List<string> hiddenScenes = new List<string> { "BattleScene"};

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 씬이 로드될 때마다 실행될 이벤트를 등록합니다.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드되었을 때 호출되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 현재 씬 이름이 hiddenScenes 목록에 있는지 확인합니다.
        if (hiddenScenes.Contains(scene.name))
        {
            SetUIVisibility(false); // 낚시 배틀 씬이면 숨김
        }
        else
        {
            SetUIVisibility(true);  // 마을이나 집이면 다시 보여줌
        }
    }

    private void SetUIVisibility(bool visible)
    {
        // 방법 1: 오브젝트 자체를 끄기 (간단함)
        // transform.GetChild(0).gameObject.SetActive(visible); 

        // 방법 2: 캔버스 컴포넌트만 끄기 (스크립트 로직은 유지하고 싶을 때 추천)
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null) canvas.enabled = visible;
        
        // 클릭 방지용 EventSystem도 같이 제어하고 싶다면 아래 추가
        // UnityEngine.EventSystems.EventSystem.current.gameObject.SetActive(visible);
    }
}