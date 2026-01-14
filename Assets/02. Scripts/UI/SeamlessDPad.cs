using UnityEngine;
using UnityEngine.EventSystems;

namespace Division.UI
{
    public class SeamlessDPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        // 외부에서 가져갈 입력 값
        public Vector2 InputVector { get; private set; }

        [Header("Settings")]
        public float deadZone = 10f; // 중심부 터치 무시 반경 (픽셀 단위)
            
        [Header("Visuals")]
        public UnityEngine.UI.Image imgUp;
        public UnityEngine.UI.Image imgDown;
        public UnityEngine.UI.Image imgLeft;
        public UnityEngine.UI.Image imgRight;
        
        // UI 크기를 계산하기 위한 변수
        private RectTransform rectTransform;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // 1. 눌렀을 때
        public void OnPointerDown(PointerEventData eventData)
        {
            CalculateInput(eventData);
        }

        // 2. 드래그 중일 때 (비비기 구현의 핵심)
        public void OnDrag(PointerEventData eventData)
        {
            CalculateInput(eventData);
        }

        // 3. 뗐을 때
        public void OnPointerUp(PointerEventData eventData)
        {
            InputVector = Vector2.zero; // 입력 초기화
            UpdateVisuals();
        }

        // 터치 위치를 기반으로 방향 계산
        private void CalculateInput(PointerEventData eventData)
        {
            Vector2 localPoint;
            
            // 스크린 좌표를 UI 내부 로컬 좌표로 변환
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                // 중심부(0,0)에서의 거리 체크 (데드존)
                if (localPoint.magnitude < deadZone)
                {
                    InputVector = Vector2.zero;
                    return;
                }

                // --- 방향 판별 로직 (4방향 D-Pad 방식) ---
                
                // X, Y 중 어느 쪽으로 더 많이 갔는지 비교
                if (Mathf.Abs(localPoint.x) > Mathf.Abs(localPoint.y))
                {
                    // 가로 방향이 더 강함 -> 좌/우
                    InputVector = localPoint.x > 0 ? Vector2.right : Vector2.left;
                }
                else
                {
                    // 세로 방향이 더 강함 -> 상/하
                    InputVector = localPoint.y > 0 ? Vector2.up : Vector2.down;
                }

                // *참고: 만약 대각선 이동도 허용하려면 위 로직 대신 아래 한 줄만 쓰세요.
                // InputVector = localPoint.normalized; 
            
            UpdateVisuals();
            
            }

        }

        private void UpdateVisuals()
        {
            // 모든 색상 초기화 (예: 반투명 흰색)
            Color normalColor = new Color(1, 1, 1, 0.5f);
            Color pressedColor = new Color(1, 1, 1, 1.0f);

            imgUp.color = normalColor;
            imgDown.color = normalColor;
            imgLeft.color = normalColor;
            imgRight.color = normalColor;

            // 현재 입력에 따라 해당 이미지 색상 변경
            if (InputVector == Vector2.up) imgUp.color = pressedColor;
            else if (InputVector == Vector2.down) imgDown.color = pressedColor;
            else if (InputVector == Vector2.left) imgLeft.color = pressedColor;
            else if (InputVector == Vector2.right) imgRight.color = pressedColor;
        }
    }  
}