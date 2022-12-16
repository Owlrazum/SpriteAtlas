# SpriteAtlas Implementation

## Description

A trial task implementation, random generation of textures with sizes between 16x16 and 256x256, and their packing to the atlas.
The algorithm uses heurestics, the most notable of which are placement of biggest by area textures first and merging of free space rectangles in case of low area loss.
At first, binary tree was tried. Now it is an adjacency list of free sprites.

## Video

https://user-images.githubusercontent.com/67944523/208039633-fc59ca28-325b-4684-a436-0932971a3dce.mov


https://user-images.githubusercontent.com/67944523/208040662-83e67c1f-6b2b-40f4-957d-c26df9beaed6.mov
