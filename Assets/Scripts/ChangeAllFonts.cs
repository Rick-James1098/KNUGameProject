using UnityEngine;
using UnityEditor;
using TMPro;

public class ChangeAllFonts : EditorWindow
{
    [MenuItem("Tools/폰트 일괄 교체 (Change All Fonts)")]
    public static void ShowWindow()
    {
        GetWindow<ChangeAllFonts>("폰트 교체기");
    }

    public TMP_FontAsset newFont; // 여기에 새 폰트를 넣을 겁니다

    void OnGUI()
    {
        GUILayout.Label("현재 씬의 모든 텍스트 폰트 변경", EditorStyles.boldLabel);
        
        // 1. 폰트 넣는 칸 만들기
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("새 폰트 넣기", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("변경 시작!"))
        {
            if (newFont == null)
            {
                Debug.LogError("새 폰트를 먼저 넣어주세요!");
                return;
            }
            ChangeFonts();
        }
    }

    void ChangeFonts()
    {
        // 씬에 있는 모든 TextMeshProUGUI 찾기
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true); // 비활성화된 것도 포함
        int count = 0;

        foreach (TextMeshProUGUI text in allTexts)
        {
            // Undo 기능 지원 (실수하면 Ctrl+Z 가능하게)
            Undo.RecordObject(text, "Change Font");
            
            text.font = newFont; // 폰트 교체
            EditorUtility.SetDirty(text); // 변경사항 저장 표시
            count++;
        }

        Debug.Log($"총 {count}개의 텍스트 폰트를 {newFont.name}으로 변경했습니다!");
    }
}