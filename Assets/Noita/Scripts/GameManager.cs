using System.Collections.Generic;
using TwiiK.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace Noita {

    /// <summary>
    /// The game manager handles too much at the moment.
    /// </summary>
    public class GameManager : Singleton<GameManager> {
        
        public Vector2 gravity = new Vector2(0, -156.96f); // -9,81 * 16.
        [FormerlySerializedAs("bounce")] [Range(0, 1)]
        public float bounciness = 0.0f;
        [Range(0, 1)]
        public float stickiness = 0.0f;
        
        public SpriteRenderer levelRenderer;
        [HideInInspector]
        public Texture2D levelTexture;
        
        private Node[,] _grid;
        private int _maxX;
        private int _maxY;
        private Node _currentNode;
        private Node _previousNode;
        private bool _levelTextureNeedsUpdate;
        private List<Particle> _fallingParticles = new List<Particle>();
        private Texture2D _levelTextureSourceFile;
        private Vector2 _mousePosition;
        
        public Vector2 MousePosition {
            get => _mousePosition;
        }
        
        private void Start() {
            CreateLevel();
        }

        private void Update() {
            GetMousePosition();
            HandleMouseInput();
            UpdateFallingParticles();
            
            if (_levelTextureNeedsUpdate) {
                _levelTextureNeedsUpdate = false;
                levelTexture.Apply();
            }
        }
        
        /// <summary>
        /// Create a grid of nodes from the level texture.
        /// </summary>
        private void CreateLevel() {
            levelTexture = Instantiate(levelRenderer.sprite.texture);

            _maxX = levelTexture.width;
            _maxY = levelTexture.height;
            _grid = new Node[_maxX, _maxY];

            for (int x = 0; x < _maxX; x++) {
                for (int y = 0; y < _maxY; y++) {
                    Node node = new Node();
                    node.x = x;
                    node.y = y;
                    Color color = levelTexture.GetPixel(x, y);
                    if (color.a == 0) {
                        node.isEmpty = true;
                    }
                    _grid[x, y] = node;
                }
            }
            
            levelRenderer.sprite = Sprite.Create(levelTexture, new Rect(0, 0, _maxX, _maxY), Vector2.zero, 1, 0, SpriteMeshType.FullRect);
        }

        /// <summary>
        /// Get the mouse position in world space.
        /// </summary>
        private void GetMousePosition() {
            _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _mousePosition.x = Mathf.FloorToInt(_mousePosition.x);
            _mousePosition.y = Mathf.FloorToInt(_mousePosition.y);
            _currentNode = GetNodeFromWorldPosition(_mousePosition);
        }

        /// <summary>
        /// Handle mouse input.
        /// </summary>
        private void HandleMouseInput() {
            if (_currentNode == null) {
                return;
            }

            if (Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject()) {
                if (_currentNode != _previousNode) {
                    _previousNode = _currentNode;
                 
                    for (int x = -6; x <= 6; x++) {
                        for (int y = -6; y <= 6; y++) {
                            Node node = GetNode(_currentNode.x + x, _currentNode.y + y);
                            if (node == null) {
                                continue;
                            }
                            
                            node.isEmpty = true;
                            levelTexture.SetPixel(node.x, node.y, new Color(0, 0, 0, 0));
                        }
                    }
                }
                
                _levelTextureNeedsUpdate = true;
            }
            
            if (Input.GetMouseButton(2) && !EventSystem.current.IsPointerOverGameObject()) {
                if (_currentNode != _previousNode) {
                    _previousNode = _currentNode;
                    
                    for (int x = -1; x <= 1; x++) {
                        for (int y = -1; y <= 1; y++) {
                            Node node = GetNode(_currentNode.x + x, _currentNode.y + y);
                            if (node == null) {
                                continue;
                            }

                            if (!node.isEmpty) {
                                continue;
                            }

                            node.isEmpty = false;
                            levelTexture.SetPixel(node.x, node.y, new Color(128f / 255, 128f / 255, 118f / 255, 1));
                        }
                    }
                }
                
                _levelTextureNeedsUpdate = true;
            }
            
            if (Input.GetMouseButton(3) && !EventSystem.current.IsPointerOverGameObject()) {
                Node node = GetNode(_currentNode.x, _currentNode.y);
                if (node.isEmpty) {
                    _fallingParticles.Add(new Particle(
                         new Color(246f / 255, 215f / 255, 176f / 255, 1),
                        _currentNode.x,
                        _currentNode.y,
                        Vector2.zero
                    ));
                }
                _levelTextureNeedsUpdate = true;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void UpdateFallingParticles() {
            if (_fallingParticles.Count == 0) {
                return;
            }
            
            _levelTextureNeedsUpdate = true;

            for (int i = 0; i < _fallingParticles.Count; i++) {
                _fallingParticles[i].Update(ref _fallingParticles, ref _grid, ref i);
            }
            
            DebugCanvas.Instance.UpdateFallingParticlesCount(_fallingParticles.Count);
        }

        /// <summary>
        /// Get the node from a world position.
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public Node[] GetNodesFromBounds(Bounds bounds) {
            int width = Mathf.CeilToInt(bounds.size.x);
            int height = Mathf.CeilToInt(bounds.size.y);
            
            int minX = Mathf.FloorToInt(bounds.min.x);
            int minY = Mathf.FloorToInt(bounds.min.y);
            int maxX = Mathf.CeilToInt(bounds.max.x);
            int maxY = Mathf.CeilToInt(bounds.max.y);

            Node[] nodes = new Node[width * height];
            int index = 0;
            for (int x = minX; x < maxX; x++) {
                for (int y = minY; y < maxY; y++) {
                    nodes[index] = GetNode(x, y);
                    index++;
                }
            }

            return nodes;
        }
        /// <summary>
        /// Destroy nodes in a radius around a position, and throw a percentage of them in the air.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="amountOfParticlesToThrowInTheAir"></param>
        /// <returns></returns>
        public void ExplodeNodesInRadius(Vector2 position, float radius, float amountOfParticlesToThrowInTheAir) {
            Node[] nodes = GetNodesInRadius(position, radius);
            foreach (Node node in nodes) {
                if (node == null) {
                    continue;
                }

                if (node.isEmpty) {
                    continue;
                }
                
                if (Random.value <= amountOfParticlesToThrowInTheAir) {
                    _fallingParticles.Add(new Particle(
                        levelTexture.GetPixel(node.x, node.y),
                        node.x,
                        node.y,
                        (new Vector2(node.x, node.y) - position) * 20f * Random.insideUnitCircle
                    ));
                }

                node.isEmpty = true;
                levelTexture.SetPixel(node.x, node.y, new Color(0, 0, 0, 0));
                _levelTextureNeedsUpdate = true;
            }
        }
        
        /// <summary>
        /// Get the nodes in a radius from a position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public Node[] GetNodesInRadius(Vector2 position, float radius) {
            int minX = Mathf.FloorToInt(position.x - radius);
            int minY = Mathf.FloorToInt(position.y - radius);
            int maxX = Mathf.CeilToInt(position.x + radius);
            int maxY = Mathf.CeilToInt(position.y + radius);

            List<Node> pointsInCircle = new List<Node>();
            for (int x = minX; x < maxX; x++) {
                for (int y = minY; y < maxY; y++) {
                    float distance = GetDistance(position, new Vector2(x, y));

                    if (distance <= radius) {
                        pointsInCircle.Add(GetNode(x, y));
                    }
                }
            }

            return pointsInCircle.ToArray();
        }

        /// <summary>
        /// Get a node from the grid.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Node GetNode(int x, int y) {
            if (x >= 0 && x < _maxX && y >= 0 && y < _maxY) {
                return _grid[x, y];
            }
            
            return null;
        }
        
        private static float GetDistance(Vector2 p1, Vector2 p2) {
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;

            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Get a node from the grid based on the world position.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        private Node GetNodeFromWorldPosition(Vector2 worldPosition) {
            int x = Mathf.FloorToInt(worldPosition.x);
            int y = Mathf.FloorToInt(worldPosition.y);
            return GetNode(x, y);
        }

    }

}
