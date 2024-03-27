using UnityEngine;

namespace _Code._Script
{
    public class DragAndDrop : MonoBehaviour
    {
        private Vector3 MousePosition { get; set; }
        private GameObject _lastHoveredArea = null;
        private GameObject _originArea = null;
        
        /// <summary>
        /// GIVE THE MOUSE POSITION
        /// </summary>
        /// <returns>Return the mouse position</returns>
        private Vector3 GetMousePosition()
        {
            return Camera.main.WorldToScreenPoint(transform.position);
        }

        /// <summary>
        /// SNAP THE PIECE ON THE MOUSE AND SET FEW PARAMETERS TO HAVE IT ON TOP ON OTHER PIECES
        /// </summary>
        private void OnMouseDown()
        {
            // /!\ Without this, the drag and drop can break
            var xPos = gameObject.transform.position.x;
            var yPos = gameObject.transform.position.y;
            gameObject.transform.position = new Vector3(xPos, yPos, -1f);
            GameManager.Instance.CurrSelectedPiece = gameObject;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
            Debug.Log("aled");
            MousePosition = Input.mousePosition - GetMousePosition();
        }
        
        private void OnMouseDrag()
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition - MousePosition);
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Start a raycast from the mouse cursor position
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);

            // Filter the result to get the 2nd hit object with Piece script component on it
            foreach (RaycastHit2D hit in hits)
            {
                GameObject hoveringArea = hit.collider.gameObject;
                
                if (hoveringArea.GetComponent<Tile>() && hoveringArea.GetComponent<Tile>() != gameObject.GetComponentInParent<Tile>())
                {
                    if(hoveringArea.GetComponent<Tile>().bisPile)
                        break;
                    
                    if (_lastHoveredArea == hoveringArea)
                    {
                        hoveringArea.GetComponent<Tile>().SetHoveringColor();
                    }
                    else
                    {
                        if(_lastHoveredArea != null)
                            _lastHoveredArea.GetComponent<Tile>().SetBaseColor();
                        _lastHoveredArea = hoveringArea;
                    }
                    break;
                }

                if (hoveringArea.GetComponent<Tile>() == gameObject.GetComponentInParent<Tile>())
                {
                    Debug.Log(_originArea);
                    if (_originArea == null)
                        _originArea = hoveringArea;
                    _originArea.GetComponent<Tile>().SetOriginLocationColor();
                    
                    if(_lastHoveredArea)
                        _lastHoveredArea.GetComponent<Tile>().SetBaseColor();
                    break;
                }
            }
        }

        private void OnMouseUp()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Start a raycast from the mouse cursor position
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
            
            // Filter the result to get the 2nd hit object with Piece script component on it
            foreach (RaycastHit2D hit in hits)
            {
                GameObject dropArea = hit.collider.gameObject;
                
                if (dropArea.GetComponent<Tile>())
                {
                    if (dropArea.GetComponent<Tile>().bisPile)
                        break;
                    
                    GameManager.Instance.Move(gameObject.GetComponent<Piece>(), dropArea.GetComponent<Tile>());
                    GameManager.Instance.CurrSelectedPiece = null;
                    
                    dropArea.GetComponent<Tile>().SetBaseColor();
                    _originArea.GetComponent<Tile>().SetBaseColor();
                    if(_lastHoveredArea)
                        _lastHoveredArea.GetComponent<Tile>().SetBaseColor();
                    _originArea = null;
                    _lastHoveredArea = null;
                    
                    break;
                }
                GameManager.Instance.SetPieceAndMoveToParent(gameObject.GetComponent<Piece>(),
                    gameObject.GetComponentInParent<Tile>());
            }

            // /!\ Without this, the drag and drop can break
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
    }
}
