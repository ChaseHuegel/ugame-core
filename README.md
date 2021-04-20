# wayward-core
Event-driven C# generic core 1st/3rd person Unity games built on Swordfish

Much of this uses Unity specific functions but is built to be as independent as possible so that the core functionality be shifted to other engines/uses with minimal refactoring.

The terrain uses the C# implementation of the FastNoise library. Some minor refactoring can switch over to any noise source.
Excuse the messy prototype for the terrain. The proof of concept is functioning as intended, rewriting a proper implementation with more extensibility and configuration.

The terrain was originally built to read images to map height, colors, tile types (textures), and object placement (trees). It was expanded to make use of noise for all aspects to create procedural terrain. The final implementation will allow the option between both and will include a proper tile based mapping (i.e. voxel terrain or 2D tileset based games) since the current "tile map" is hacky using hard-coded integer IDs. This allows for a dynamic open world with materials and behaviors attached to individual tiles that scales performantly at any world size.

Below is the first prototype using height and color maps to build terrain mesh and collision
https://user-images.githubusercontent.com/14932139/115452503-832c3b80-a1ec-11eb-893f-53abcf4c8ba1.mp4

The 2nd prototype with noise-based generation. I restructured the mesh to look smoother, added textured tiles, added object placement.
![fafb5e5afac287f9a82fea0b298fc9fc](https://user-images.githubusercontent.com/14932139/115452524-8a534980-a1ec-11eb-888c-e95b77a34e10.png)

Then I started baking shading to mimic shadows and ambient occlusion. This took some theory crafting, I couldn't just calculate angles to get steepness or determine crevices- that would be far too slow and to keep things scalable I can only use information on a tile-by-tile basis, no sampling neighbors. 

I realized I can approximate the steepness/angle with some very basic math that runs fast. I average the height of all 4 points that make up a tile then use the difference between that average and the exact height at each point. This gives me a rough shading. Higher difference = steeper angle = darker vertex. Then I use point's position relative to the sun's direction to determine how much to lighten the shading in order to fake shadows on distant terrain. 
![24b4cddcce146ec7ae04dbb10c547445](https://user-images.githubusercontent.com/14932139/115452651-afe05300-a1ec-11eb-9a64-3325d216f2b4.jpg)

The implementation isn't accurate to real life, but it doesn't need to be for distant terrain where I can't get realtime shadows.
![unknown](https://user-images.githubusercontent.com/14932139/115452609-a2c36400-a1ec-11eb-887b-aed6fab2f546.png)
