using UnityEngine;

public class ShopInteraction : MonoBehaviour
{
    public GameObject interactionKeyUI; // "Space: 상점 열기" 말풍선 UI (선택사항)

    private bool isPlayerNear = false;

    void Update()
    {
        // 플레이어가 근처에 있고, 스페이스바를 눌렀으며, 상점이 안 열려있을 때
        if (isPlayerNear && Input.GetKeyDown(KeyCode.Space))
        {// [수정] 드래그 연결 대신, 살아있는 Instance를 직접 부름
            ShopManager.Instance.OpenShop();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactionKeyUI != null) interactionKeyUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            ShopManager.Instance.CloseShop(); // 멀어지면 자동으로 닫기
            if (interactionKeyUI != null) interactionKeyUI.SetActive(false);
        }
    }
}