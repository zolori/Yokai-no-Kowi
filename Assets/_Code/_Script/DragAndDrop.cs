using UnityEngine;

namespace _Code._Script
{
    public class DragAndDrop : MonoBehaviour
    {
        private Vector3 _mousePosition;
        private bool bIsYourTurn;

        private Vector3 GetMousePosition()
        {
            return Camera.main.WorldToScreenPoint(transform.position);
        }

        private void OnMouseDown()
        {
            // /!\ Without this, the drag and drop can break
            var xPos = gameObject.transform.position.x;
            var yPos = gameObject.transform.position.y;
            gameObject.transform.position = new Vector3(xPos, yPos, -1f);
            GameManager.Instance.currSelectedPiece = gameObject;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
            
            _mousePosition = Input.mousePosition - GetMousePosition();
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
                    GameObject dropArea = hit.collider.gameObject;
                    Debug.Log("GameObject sous la souris : " + dropArea.name);
                if (hit.collider.gameObject.GetComponent<Piece>() == null)
                {
                    // Vous avez trouvé le GameObject sous le pointeur de la souris
                    GameManager.Instance.Move(gameObject.GetComponent<Piece>(), dropArea.GetComponent<Tile>());
                    GameManager.Instance.currSelectedPiece = null;
                    break;
                }
            }
            
            // /!\ Without this, the drag and drop can break
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
            var xPos = gameObject.transform.position.x;
            var yPos = gameObject.transform.position.y;
            gameObject.transform.position = new Vector3(xPos, yPos, 0f);
        }

        private bool IsYourTurn()
        {
            
        }
    }
}
