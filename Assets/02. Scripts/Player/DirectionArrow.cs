using UnityEngine;
using UnityEngine.EventSystems;

namespace Player
{
    // 화살표 종류 정의
    public enum ArrowType { Up, Down, Left, Right }

    public class DirectionArrow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public ArrowType arrowType; // 인스펙터에서 상/하/좌/우 선택
        private MobileInputManager inputManager;

        void Start()
        {
            // 씬에 있는 매니저를 자동으로 찾음
            inputManager = GameObject.FindWithTag("MobileControllerCanvas").GetComponent<MobileInputManager>();
        }

        // 눌렀을 때
        public void OnPointerDown(PointerEventData eventData)
        {
            inputManager.SetPressState(arrowType, true);
        }

        // 뗐을 때
        public void OnPointerUp(PointerEventData eventData)
        {
            inputManager.SetPressState(arrowType, false);
        }

    }   
}