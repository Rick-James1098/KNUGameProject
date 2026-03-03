using System.Collections.Generic;
using UnityEngine;

public class FishSelector : MonoBehaviour
{
    public FishDataFormat GetRandomFish(int playerLuck)
    {
        Debug.Log("FishSelector generated");
        int currentMaxRarity = 50 + (int)(playerLuck * 0.2);

        float totalRarityWeight = 0f;        
        float[] rarityWeights = new float[currentMaxRarity + 1];

        float luckRatio = Mathf.Clamp01(playerLuck / 250f); 
        float compressionExponent = Mathf.Lerp(0.6f, 0.2f, luckRatio);

        for (int r = 1; r <= currentMaxRarity; r++)
        {
            // [핵심 로직 2] 희귀도가 높아질수록 확률이 크게 떨어지는 곡선형 기본 가중치
            // r이 커질수록 r^2로 나누어지므로 값이 가파르게 떨어집니다.
            float baseWeight = 10000f / Mathf.Pow((float)r, 2f); 
            
            // [핵심 로직 3] 지수(exponent)를 활용한 가중치 압축 적용
            float finalWeight = Mathf.Pow(baseWeight, compressionExponent);

            rarityWeights[r] = finalWeight;
            totalRarityWeight += finalWeight;
        }

        float pivot = Random.Range(0f, totalRarityWeight);
        float current = 0f;
        int targetRarity = 1;

        for (int r = 1; r <= currentMaxRarity; r++)
        {
            current += rarityWeights[r];
            if (pivot <= current)
            {
                targetRarity = r;
                break;
            }
        }
        
        List<FishDataFormat> allFish = new List<FishDataFormat>(FishDataManager.instance.fishDatabase.Values);
        List<FishDataFormat> candidateFish = new List<FishDataFormat>();

        foreach (var fish in allFish)
        {
            if (targetRarity >= fish.minRarity && targetRarity <= fish.maxRarity)
            {
                candidateFish.Add(fish);
            }
        }

        if (candidateFish.Count > 0)
        {
            int randomIndex = Random.Range(0, candidateFish.Count);
            Debug.Log("Luck: " + playerLuck + " / Rarity: " + targetRarity + " / Probability: " + (rarityWeights[targetRarity] / totalRarityWeight) * 100f + " / Selected Fish: " + candidateFish[randomIndex].itemName);
            return candidateFish[randomIndex];
        }
        else
        {
            Debug.Log($"희귀도 {targetRarity} 범위를 가진 물고기가 없습니다! 기본 물고기를 줍니다.");
            return allFish[0]; 
        }
    }
}