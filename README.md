# CYIM - Unity Pathfinding Take-Home Project 

This repository is a submission for the [Pocket Worlds Unity Pathfinding Take-Home Project](https://github.com/pocketzworld/unity-tech-test).

## Getting Started

1. Clone this repository to your local machine:

   ```shell
   git clone https://github.com/GoodKnight/pocketzworld-unity-tech-test.git

2. Create a Unity project or use an existing one.

3. Open **Game.unity** scene.

4. Press **Play**, the player is able to click on the screen and the player object will attempt to avoid obstacles while finding the most direct path.

## Technical Details
This project implements pathfinding via the A* algorithm and allows for path smoothing through the use of node turn lines. Once the player object passes a turn line (offset from the node's location) it will start rotating and tracking towards the next node in the path.