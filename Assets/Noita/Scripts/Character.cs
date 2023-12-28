using UnityEngine;
using UnityEngine.EventSystems;

namespace Noita {

    /// <summary>
    /// A character that can move around and throw dynamite.
    ///
    /// Currently only used for the player.
    /// </summary>
    public class Character : MonoBehaviour {

        public Dynamite dynamitePrefab;
        public SpriteRenderer spriteRenderer;
        public new Collider2D collider;
        
        public float speed = 5f;
        public float throwSpeed = 150f;
        private Vector2 _velocity;

        public Vector2 _subPixelPositionRemainder;
        
        private Vector2 _position;

        private void Update() {
            _position = transform.position;
            
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) {
                Dynamite dynamite = Instantiate(dynamitePrefab, _position + Vector2.up * 5, Quaternion.identity);
                // Launch the dynamite towards the mouse position.
                Vector2 direction = (GameManager.Instance.MousePosition - _position).normalized;
                dynamite.velocity = direction * throwSpeed;
            }
            
            // 2D platformer movement.
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector2 movement = new Vector2(horizontal, vertical);
            
            _subPixelPositionRemainder += movement * speed * Time.deltaTime;
            Vector2 actualMoveAmount = new Vector2(
                Mathf.Round(_subPixelPositionRemainder.x),
                Mathf.Round(_subPixelPositionRemainder.y)
            );

            if (actualMoveAmount == Vector2.zero) {
                return;
            }
            
            if (actualMoveAmount.x != 0) {
                _subPixelPositionRemainder.x -= actualMoveAmount.x;
            }
            if (actualMoveAmount.y != 0) {
                _subPixelPositionRemainder.y -= actualMoveAmount.y;
            }
            
            if (CheckCollision(actualMoveAmount)) {
                return;
            }
            
            // Apply movement
            transform.Translate(actualMoveAmount);
        }
        
        private bool CheckCollision(Vector2 moveAmount) {
            // Create new bounds based on the sprite's current position and the move amount.
            Bounds bounds = collider.bounds;
            bounds.center += new Vector3(moveAmount.x, moveAmount.y, 0);
            // Get all the nodes overlapping the sprite.
            Node[] nodes = GameManager.Instance.GetNodesFromBounds(bounds);
            // Check if any of the nodes are filled.
            foreach (Node node in nodes) {
                if (node == null || node.isEmpty == false) {
                    return true;
                }
            }
            
            return false;
        }
    }

}
