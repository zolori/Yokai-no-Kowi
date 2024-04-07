using UnityEngine;

namespace _Code._Script
{
    public class DragAndDropTouch : MonoBehaviour
    {
        private float _dist;
        private bool _bIsDragging = false;
        private Vector3 _offset;
        private Transform _toDrag;
        private Camera _camera;
        private GameObject _lastHoveredArea = null;
        private GameObject _originArea = null;
        private Vector2 TouchPosition { get; set; }


        // Update is called once per frame
        private void Start()
        {
            _camera = Camera.main;
        }

        void Update()
        {
            if (Input.touchCount != 1)
            {
                _bIsDragging = false;
                return;
            }
            Touch touch = Input.touches[0];
            
            if (touch.phase == TouchPhase.Began)
            {
                Ray touchPosition = _camera.ScreenPointToRay(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(touchPosition.origin, Vector2.zero);

                if (hit.collider.GetComponent<DragAndDropTouch>() == this)
                {
                    // /!\ Without this, the drag and drop can break
                    var xPos = gameObject.transform.position.x;
                    var yPos = gameObject.transform.position.y;
                    gameObject.transform.position = new Vector3(xPos, yPos, -1f);
                    
                    GameManager.Instance.CurrSelectedPiece = gameObject;
                    gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    TouchPosition = touchPosition.origin;
                    _bIsDragging = true;
                }
            }

            if (_bIsDragging && touch.phase == TouchPhase.Moved)
            {
                transform.position = _camera.ScreenToWorldPoint(touch.position - TouchPosition);
                transform.position = new Vector3(transform.position.x, transform.position.y, -1.5f);
                Ray touchPosition = _camera.ScreenPointToRay(touch.position);

                // Start a raycast from the mouse cursor position
                RaycastHit2D[] hits = Physics2D.RaycastAll(touchPosition.origin, Vector2.zero);

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
                            _lastHoveredArea = hoveringArea;
                        }
                        break;
                    }

                    if (hoveringArea.GetComponent<Tile>() == gameObject.GetComponentInParent<Tile>())
                    {
                        if (_originArea == null)
                            _originArea = hoveringArea;
                        break;
                    }
                    
                    if(_lastHoveredArea)
                        _lastHoveredArea.GetComponent<Tile>().SetBaseColor();
                }
            }

            if (_bIsDragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                Vector2 touchPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

                // Start a raycast from the mouse cursor position
                RaycastHit2D[] hits = Physics2D.RaycastAll(touchPosition, Vector2.zero);
            
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
                
                _bIsDragging = false;
            }
        }
        
        private Vector3 GetTouchPosition()
        {
            return _camera.WorldToScreenPoint(transform.position);
        }

    }
}
