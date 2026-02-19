using UnityEngine;
using UnityEngine.Tilemaps; // 타일맵 사용을 위해 필요

public class CameraFollowTilemap : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Map Settings")]
    public Tilemap targetTilemap; // 기준이 될 타일맵

    [Header("Movement Settings")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.5f;

    private float _leftLimit, _rightLimit, _bottomLimit, _topLimit;
    private Camera _cam;

    [Header("Movement Settings")]
    public float smoothTime = 0.15f; // 카메라가 도달하는 데 걸리는 대략적인 시간 (낮을수록 빠름)
    private Vector3 _currentVelocity = Vector3.zero; // SmoothDamp 내부에서 사용되는 속도 참조

    void Start()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;

        CalculateTilemapBoundaries();

        // 씬이 시작될 때 "Player" 태그를 가진 오브젝트를 자동으로 찾아서 타겟으로 설정
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null || targetTilemap == null) return;

        // 1. 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 2. SmoothDamp를 사용하여 부드럽게 이동 (Lerp보다 진동에 강함)
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, smoothTime);

        // 3. 타일맵 경계로 제한 (Clamping)
        float clampedX = Mathf.Clamp(smoothedPosition.x, _leftLimit, _rightLimit);
        float clampedY = Mathf.Clamp(smoothedPosition.y, _bottomLimit, _topLimit);

        // 4. 최종 위치 적용
        transform.position = new Vector3(clampedX, clampedY, smoothedPosition.z);
    }

    private void CalculateTilemapBoundaries()
    {
        // 타일맵에서 실제로 타일이 존재하는 영역의 경계를 가져옴
        targetTilemap.CompressBounds(); // 빈 공간을 제외하고 실제 타일이 있는 곳만 압축
        Bounds bounds = targetTilemap.localBounds;

        float camVertExtent = _cam.orthographicSize;
        float camHorzExtent = _cam.aspect * camVertExtent;

        // 타일맵의 World Space 좌표 기준으로 계산
        _leftLimit = bounds.min.x + camHorzExtent;
        _rightLimit = bounds.max.x - camHorzExtent;
        _bottomLimit = bounds.min.y + camVertExtent;
        _topLimit = bounds.max.y - camVertExtent;

        // 맵이 카메라보다 작을 경우 처리
        if (_leftLimit > _rightLimit) _leftLimit = _rightLimit = bounds.center.x;
        if (_bottomLimit > _topLimit) _bottomLimit = _topLimit = bounds.center.y;
    }
}