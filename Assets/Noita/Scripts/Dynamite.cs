using UnityEngine;

namespace Noita {

    /// <summary>
    /// Dynamite is a throwable object that explodes when it hits something, or when the fuse runs out.
    /// </summary>
    public class Dynamite : MonoBehaviour {
        
        public AudioClip[] explosionSounds;
        
        public SpriteRenderer spriteRenderer;
        public new Collider2D collider;
        public float fuseTime = 3f;
        public float explosionRadius = 16f;
        public float rotationSpeed = 30f;
        [Tooltip("How large of a percentage of the particles we destroy  when the dynamite explodes should be thrown into the air?")]
        [Range(0f, 1f)]
        public float amountOfParticlesToThrowInTheAir = 1f;
        [HideInInspector]
        public Vector2 velocity;
        
        private Vector2 _subPixelPositionRemainder;
        private float _timeSinceSpawned = 0f;
        private bool _hasExploded;

        private void Update() {
            if (_hasExploded) {
                return;
            }
            
            spriteRenderer.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
            // Handle fuse.
            _timeSinceSpawned += Time.deltaTime;
            if (_timeSinceSpawned >= fuseTime) {
                Explode();
                return;
            }
            
            // Apply gravity.
            velocity += Physics2D.gravity * Time.deltaTime;
            _subPixelPositionRemainder += velocity * Time.deltaTime;

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
                Explode();
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
        
        private void Explode() {
            GetComponent<AudioSource>().clip = explosionSounds[Random.Range(0, explosionSounds.Length)];
            GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
            GetComponent<AudioSource>().Play();
            GameManager.Instance.ExplodeNodesInRadius(transform.position, explosionRadius, amountOfParticlesToThrowInTheAir);
            _hasExploded = true;
            spriteRenderer.enabled = false;
            Destroy(gameObject, 1f);
        }
    }

}
