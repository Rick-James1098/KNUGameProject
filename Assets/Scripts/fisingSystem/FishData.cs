using UnityEngine;

[CreateAssetMenu(fileName = "newFish", menuName = "Fishing/FishData")]
public class FishData : ScriptableObject
{
    public string fishName;
    public Sprite fishSprite;

    [Range(0, 100)]
    public int rarityWeight;

    [Header("캐칭 설정")]
    public float difficulty;
}
