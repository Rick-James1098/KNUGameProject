using UnityEngine;

public class RopeRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform[] ropeSegments; // 물리 사슬 마디들
    public HingeJoint2D endJoint;    // 작살에 연결될 마지막 조인트
    private Transform tailTransform;

    void Start()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        
        // LineRenderer의 점 개수를 마디 개수와 맞춤
        lineRenderer.positionCount = ropeSegments.Length + 1;
    }

    void LateUpdate()
    {
        // 매 프레임마다 물리 마디들의 위치를 가져와서 선을 그림
        for (int i = 0; i < ropeSegments.Length; i++)
        {
            lineRenderer.SetPosition(i, ropeSegments[i].position);
        }

        int lastIndex = lineRenderer.positionCount - 1;
        
        if (tailTransform != null)
        {
            lineRenderer.SetPosition(lastIndex, tailTransform.position);
        }
        else if (ropeSegments.Length > 0)
        {
            // 아직 작살이 없으면 그냥 마지막 마디 위치에 그림
            lineRenderer.SetPosition(lastIndex, ropeSegments[ropeSegments.Length - 1].position);
        }
    }

    // 외부(작살)에서 호출해서 연결을 시도하는 함수
    public void AttachToTarget(Rigidbody2D targetRB)
    {
        if (endJoint != null && targetRB != null)
        {
            // 나중을 위해 작살의 트랜스폼 저장
            tailTransform = targetRB.transform;

            // 물리 관절 연결
            endJoint.transform.position = targetRB.transform.position;
            endJoint.autoConfigureConnectedAnchor = false;
            endJoint.connectedBody = targetRB;
            endJoint.anchor = Vector2.zero;
            endJoint.connectedAnchor = Vector2.zero; 
            endJoint.enabled = true;
        }
    }
}
