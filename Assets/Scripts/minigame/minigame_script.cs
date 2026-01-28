using UnityEngine;
using UnityEngine.SceneManagement;

public class FishingArcGame : MonoBehaviour
{

    [Header("위치 보정")]
    public float verticalLeftOffset = 1.0f;


    [Header("연결")]
    public GameObject arcVisual;
    public Transform pointerCircle;

    [Header("설정")]
    public float arcRadius = 2.0f;
    public float arcAngleRange = 35f;
    public float grabDistance = 0.1f; // 마우스가 원에 얼마나 가까워야 잡히는지
    public float arcThickness = 0.3f;

    private bool isGameActive = false;
    private bool isDragging = false; // 원을 잡았는지 여부
    private Vector2 targetDirection; // 캐릭터 뒤쪽 방향
    private Vector3 initialCirclePos; // 찌가 있던 위치

    private Collider2D arcCollider;

    void Start()
    {
        // 시작할 때 미리 콜라이더를 가져옵니다. 
        // 자식 오브젝트(ArcGraphic)에 있다면 GetComponentInChildren을 씁니다.
        arcCollider = arcVisual.GetComponentInChildren<Collider2D>();
    }

    public void StartMiniGame(Vector3 bobberPos, Vector2 backDir)
    {
        isGameActive = true;
        isDragging = false;
        targetDirection = backDir.normalized;

        // 1. 원(Circle) 설정 (찌 위치)
        pointerCircle.gameObject.SetActive(true);
        pointerCircle.position = bobberPos;

        // 2. 호(Arc) 설정
        arcVisual.SetActive(true);

        // [위치] 플레이어 위치에서 등 뒤 방향(backDir)으로 1.5유닛만큼 떨어진 지점에 배치
        // localPosition 말고 월드 좌표(position)를 쓰면 캔버스 좌표계가 꼬여도 무조건 그 자리에 생깁니다.
        Vector3 finalPos = transform.position + (Vector3)(targetDirection * 1.5f);

        if (Mathf.Abs(targetDirection.y) > 0.5f)
        {
            finalPos.x -= verticalLeftOffset;
        }

        finalPos.z = 0;
        arcVisual.transform.position = finalPos;

        // [회전] 프리팹의 볼록한 부분(위쪽)을 등 뒤 방향(backDir)으로 향하게 합니다.
        // transform.up을 backDir로 맞추는 게 가장 직관적이고 빠릅니다.
        arcVisual.transform.up = targetDirection;
        arcVisual.transform.Rotate(0, 0, 90f);
    }
    void Update()
    {
        if (!isGameActive) return;

        // 마우스 월드 좌표 계산
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // 1. 마우스 왼쪽 버튼을 눌렀을 때 (Click Down)
        if (Input.GetMouseButtonDown(0))
        {
            float distToCircle = Vector2.Distance(mousePos, pointerCircle.position);
            
            // 원 근처에서 클릭했다면 드래그 시작
            if (distToCircle <= grabDistance)
            {
                isDragging = true;
                Debug.Log("원 잡기 성공!");
            }
        }

        // 2. 마우스를 누르고 있는 동안 (Hold)
        if (Input.GetMouseButton(0) && isDragging)
        {
            // 원이 마우스 위치를 따라옴
            pointerCircle.position = mousePos;

            // 3. 드래그 중인 원이 호(Arc)에 닿았는지 체크
            if (IsTouchingArc())
            {
                Success();
            }
        }

        // 4. 마우스 버튼을 뗐을 때 (Release)
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                Debug.Log("원을 놓쳤습니다.");
                
                // (선택사항) 놓쳤을 때 원을 다시 찌 위치로 되돌리고 싶다면:
                // pointerCircle.position = initialCirclePos;
            }
        }
    }

    bool IsTouchingArc()
    {
        if (arcCollider == null) return false;

        // 원의 콜라이더를 가져옵니다.
        Collider2D circleCol = pointerCircle.GetComponent<Collider2D>();
        
        // 두 콜라이더가 겹쳐있는지 확인하는 가장 간단한 방법
        return arcCollider.IsTouching(circleCol);
    }

    void Success()
    {
        isGameActive = false;
        Debug.Log("호에 명중! 씬 전환합니다.");
        SceneManager.LoadScene("FishingGimmickScene");
    }

    public void EndMiniGame()
    {
        isGameActive = false;
        arcVisual.SetActive(false);
        pointerCircle.gameObject.SetActive(false);
    }
}