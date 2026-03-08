using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class FishDataFormat : ItemData
{
    public int fishID;          // 고유번호
    public int minPrice;        // 최소 금액
    public int maxPrice;        // 최대 금액
    public int minRarity;       // 희귀도
    public int maxRarity;
    public int minResistance;   // 최소 저항값
    public int maxResistance;   // 최대 저항값
    public string equipment;
    public string map;
}

public class FishDataManager : MonoBehaviour
{
    public static FishDataManager instance;
    public Dictionary<int, FishDataFormat> fishDatabase = new Dictionary<int, FishDataFormat>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFishData();
    }

    void LoadFishData()
    {
        // 3. Resources 폴더에서 "Fish"라는 이름의 CSV 파일 불러오기 (확장자 제외)
        TextAsset csvData = Resources.Load<TextAsset>("FishDB");

        // 4. 엔터(줄바꿈)를 기준으로 한 줄씩 쪼개기
        string[] lines = csvData.text.Split('\n');

        // 5. 첫 번째 줄(인덱스 0)은 제목 칸이므로 건너뛰고 두 번째 줄(인덱스 1)부터 시작
        for (int i = 1; i < lines.Length; i++)
        {
            // 빈 줄이 있으면 에러가 나지 않게 건너뜀
            if (string.IsNullOrWhiteSpace(lines[i])) continue; 

            // 6. 한 줄을 다시 쉼표(,)를 기준으로 칸칸이 쪼개기
            string[] row = lines[i].Split(',');

            FishDataFormat fish = new FishDataFormat();
            
            // 7. 쪼갠 데이터를 그릇(클래스)에 알맞게 담아주기 (글자는 숫자로 변환)
            fish.fishID = int.Parse(row[0]);
            fish.itemName = row[1];
            fish.maxRarity = int.Parse(row[2]);
            fish.minRarity = int.Parse(row[3]);
            fish.maxResistance = int.Parse(row[4]);
            fish.minResistance = int.Parse(row[5]);
            fish.maxPrice = int.Parse(row[6]);
            fish.minPrice = int.Parse(row[7]);
            fish.equipment = row[8];
            fish.map = row[9];
            fish.icon = Resources.Load<Sprite>($"FishIcons/{fish.itemName}");
            // 8. 완성된 물고기 데이터를 딕셔너리에 추가!
            fishDatabase.Add(fish.fishID, fish);

            Debug.Log("물고기 데이터 로드 완료! 총 개수: " + fishDatabase.Count);
        }
    }
}
