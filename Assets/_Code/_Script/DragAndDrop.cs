using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Code._Script
{
    public class DragAndDrop : MonoBehaviour
    {
        [SerializeField] private GameObject _tile;
        private Vector3 mousePosition;

        private Vector3 GetMousePosition()
        {
            return Camera.main.WorldToScreenPoint(transform.position);
        }

        private void OnMouseDown()
        {
            mousePosition = Input.mousePosition - GetMousePosition();
        }

        private void OnMouseDrag()
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition - mousePosition);
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
                    Debug.Log("GameObject sous la souris : " + dropArea.name);
                    
                    

                    break;
                }
            }
        }
    }
}