# Noita

This is just a simple prototype showcasing pixel destruction/physics in Unity. Heavily inspired by the game Noita (https://store.steampowered.com/app/881100/Noita/). In fact at the time of writing the "level" in my prototype is just a screenshot of Noita.

Initially I started out following Sharp Accent's Lemmings tutorial (https://sharpaccent.com/?c=course&id=19), but I quickly realized I wanted to create something more like Noita instead.

I knew I wanted to try doing some pixel stuff in Unity, but I also wanted to test how "brute forced" you could do such things. Currently all the pixel destruction and physics takes place inside a single texture, using Texture2D.Apply() every frame to update the texture, and other "you should never do this" approaches, but even when testing on a 10 year old macbook this runs perfectly fluid, so clearly even if this is extremely unoptimized it's more than fine for an actual game.

Screenshot from the game as it stands at the moment:  
![Noita screenshot](/noita.png)

## In-game controls

**Movement:** WASD  
**Throw dynamite towards cursor:** Left mouse button  
**Erase pixels at cursor:** Right mouse button

## Credits/Copyright
### Sounds
https://freesound.org/people/ScouseMouseJB/sounds/329045/  
https://freesound.org/people/eardeer/sounds/402011/  
https://freesound.org/people/eardeer/sounds/402007/

### Noita level
The level in my game is currently a screenshot from Noita. I assume that's fine, but I'm noting it here regardless.
