using UnityEngine;
using UnityEngine.EventSystems;

namespace _Code._Script.ChildPieces
{
    public class DragAndDrop : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private RectTransform _rectTransform;
       [SerializeField] private GameObject _piece, _tile;
    
        public void OnDrag(PointerEventData eventData)
        {
            _piece.GetComponent<RectTransform>().anchoredPosition += eventData.delta;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.pointerDrag.GetComponent<Piece>())
            {
                Debug.Log("piece dragged is true");
                _piece = eventData.pointerDrag;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerEnter.GetComponent<Tile>())
            {
                _tile = eventData.pointerEnter;
                _tile.GetComponent<Tile>().DragEnd(_piece.GetComponent<Piece>());
            }
        }
    }
}
