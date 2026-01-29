using System.Collections.Generic;
using UnityEngine;

public class FishSelector : MonoBehaviour
{
    public List<FishData> fishDatabase; // 모든 물고기 리스트를 여기에 드래그 앤 드롭

    public FishData GetRandomFish()
    {
        float totalWeight = 0;
        foreach (var fish in fishDatabase)
        {
            totalWeight += fish.rarityWeight;
        }

        float pivot = Random.Range(0, totalWeight);
        float currentWeight = 0;

        foreach (var fish in fishDatabase)
        {
            currentWeight += fish.rarityWeight;
            if (pivot <= currentWeight)
            {
                return fish;
            }
        }
        return fishDatabase[0];
    }
}