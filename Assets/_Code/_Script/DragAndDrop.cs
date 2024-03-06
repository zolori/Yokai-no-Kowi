using UnityEngine;

namespace _Code._Script
{
    public class DragAndDrop : MonoBehaviour
    {
        private Vector3 _mousePosition;

        private Vector3 GetMousePosition()
        {
            return Camera.main.WorldToScreenPoint(transform.position);
        }

        private void OnMouseDown()
        {
            _mousePosition = Input.mousePosition - GetMousePosition();
            GameManager.Instance.currSelectedPiece = gameObject;
        }

        private void OnMouseDrag()
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition - _mousePosition);
        }

        private void OnMouseUp()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Lancer un raycast depuis la position de la souris
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);

            // Filtrer les résultats pour ignorer le premier objet touché
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject != this.gameObject)
                {
                    // Vous avez trouvé le GameObject sous le pointeur de la souris
                    GameObject dropArea = hit.collider.gameObject;
                    //Debug.Log("GameObject sous la souris : " + dropArea.name);
                    dropArea.GetComponent<Tile>().DragEnd(gameObject.GetComponent<Piece>());
                    break;
                }
            }
        }
    }
}
