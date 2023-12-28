using System.Collections.Generic;
using UnityEngine;

namespace Noita {

    /// <summary>
    /// A particle is a pixel that is in the process of falling.
    /// </summary>
    public class Particle {
        
        public Color color;
        public int x;
        public int y;
        public Vector2 velocity;
        
        private Vector2 _subPixelPositionRemainder;
        
        public Particle(Color color, int x, int y, Vector2 velocity) {
            this.color = color;
            this.x = x;
            this.y = y;
            this.velocity = velocity;
            
            // Draw the particle at the initial position.
            GameManager.Instance.levelTexture.SetPixel(x, y, color);
        }
        
        /// <summary>
        /// Update the particle's position.
        /// </summary>
        public void Update(ref List<Particle> fallingParticles, ref Node[,] grid, ref int index) {
            velocity += GameManager.Instance.gravity * Time.deltaTime;
            _subPixelPositionRemainder += velocity * Time.deltaTime;
            Vector2Int actualMoveAmount = new Vector2Int(
                Mathf.RoundToInt(_subPixelPositionRemainder.x),
                Mathf.RoundToInt(_subPixelPositionRemainder.y)
            );

            // If we didn't move a full pixel there's nothing to do.
            if (actualMoveAmount == Vector2.zero) {
                return;
            }

            // Erase the particle from the previous position.
            GameManager.Instance.levelTexture.SetPixel(x, y, new Color(0, 0, 0, 0));
            
            if (actualMoveAmount.x != 0) {
                _subPixelPositionRemainder.x -= actualMoveAmount.x;
            }

            if (actualMoveAmount.y != 0) {
                _subPixelPositionRemainder.y -= actualMoveAmount.y;
            }
            
            Node node = GameManager.Instance.GetNode(x + actualMoveAmount.x, y + actualMoveAmount.y);
            if (node != null && node.isEmpty) {
                x += actualMoveAmount.x;
                y += actualMoveAmount.y;
            }
            // We collided.
            else {
                if (GameManager.Instance.bounciness > 0) {
                    if (actualMoveAmount.x != 0) {
                        velocity.x = -velocity.x * GameManager.Instance.bounciness;
                    }
                    if (actualMoveAmount.y != 0) {
                        velocity.y = -velocity.y * GameManager.Instance.bounciness;
                    }
                }
                else {
                    velocity = Vector2.zero;
                }
                
                if (CheckIfStopped()) {
                    fallingParticles.RemoveAt(index);
                    index--;
                    // TODO: Currently we're losing a ton of particles, because we only put them back in the grid if
                    // they stop moving. So we could have a lot of particles ending up in the same grid cell. This also
                    // means we're not getting proper "falling sand" behaviour. It's not really noticeable when just
                    // blowing stuff up with the dynamite, but it's extremely noticeable when dropping sand.
                    // To fix this, this entire project needs to be rewritten so that every pixel is treated as a
                    // "falling sand particle", and this particle class should probably only apply to actually "flying"
                    // particles, that have been taken out of the simulation momentarily. This would need to be done
                    // at some point regardless to get a proper game out of this, but for now I can't be assed.
                    // I imagine the performance will tank when we do that, and that optimizations like chunking and
                    // multithreading will be necessary.
                    grid[x, y].isEmpty = false;
                }
            }
            
            // Draw the particle at the new position.
            GameManager.Instance.levelTexture.SetPixel(x, y, color);
        }
        
        /// <summary>
        /// Check if the particle has stopped moving completely, or if it should apply "falling sand" rules, and
        /// continue updating.
        /// </summary>
        /// <returns></returns>
        private bool CheckIfStopped() {
            if (GameManager.Instance.stickiness >= Random.value) {
                return true;
            }
            
            Node nodeDown = GameManager.Instance.GetNode(x, y - 1);
            if (nodeDown != null && nodeDown.isEmpty) {
                return false;
            }
            
            Node nodeLeft = GameManager.Instance.GetNode(x - 1, y);
            Node nodeDownLeft = GameManager.Instance.GetNode(x - 1, y - 1);
            if (nodeLeft != null && nodeLeft.isEmpty && nodeDownLeft != null && nodeDownLeft.isEmpty) {
                x -= 1;
                y -= 1;
                return false;
            }
            
            Node nodeRight = GameManager.Instance.GetNode(x + 1, y);
            Node nodeDownRight = GameManager.Instance.GetNode(x + 1, y - 1);
            if (nodeRight != null && nodeRight.isEmpty && nodeDownRight != null && nodeDownRight.isEmpty) {
                x += 1;
                y -= 1;
                return false;
            }

            return true;
        }
    }

}
