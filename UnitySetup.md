# Unity Scene Setup Instructions (3D FIFA-Style Soccer Prototype)

## 1) Project + Packages
1. Create/open a **3D (URP or Built-in)** Unity project.
2. Ensure the **AI Navigation** package is installed (for `NavMeshAgent`).

## 2) Scene Objects
1. Create a ground plane (`Field`) and mark it on a layer included in `BallController.groundMask`.
2. Add simple stadium boundaries using colliders.
3. Create two goals with trigger colliders:
   - `Goal_PlayerScores` (when opponent goal is hit)
   - `Goal_OpponentScores` (when player goal is hit)

## 3) Player Setup
1. Create `Player` capsule.
2. Add:
   - `CharacterController`
   - `PlayerController` script
3. Create a child transform named `BallHoldPoint` slightly in front of the player (e.g., `(0, 0.6, 0.8)`).
4. Assign `BallHoldPoint` in `PlayerController`.

## 4) AI Setup
1. Create `AI_Opponent` capsule.
2. Add:
   - `NavMeshAgent`
   - `AIController` script
3. Add child `BallHoldPoint` for AI and assign it.
4. Bake NavMesh:
   - Mark walkable surfaces.
   - Open **Window > AI > Navigation** and bake.

## 5) Ball Setup
1. Create sphere `Ball`.
2. Add:
   - `Rigidbody` (use interpolation enabled)
   - Sphere collider
   - `BallController` script
3. Suggested values:
   - `mass`: `0.43`
   - `drag`: `0.12`
   - `angularDrag`: `0.08`
   - `groundFriction`: `0.96`

## 6) Camera Setup
1. Position `Main Camera` behind player.
2. Add `CameraController` to camera.
3. Assign:
   - `target` = player transform
   - `player` = player controller
4. Tune sensitivity and offset as needed.

## 7) Game Manager + Goals
1. Create empty object `GameManager` and add `GameManager` script.
2. Assign:
   - `player`, `ai`, `ball`
   - start point transforms (`BallStart`, `PlayerStart`, `AIStart`)
3. Add `GoalTrigger` to each goal trigger collider:
   - Opponent goal trigger: `scoringTeamOnEnter = "Player"`
   - Player goal trigger: `scoringTeamOnEnter = "Opponent"`

## 8) Script References
1. Assign ball reference in `PlayerController` and `AIController` (or leave null for auto-find).
2. In `AIController`, assign:
   - `ownGoal`
   - `opponentGoal`

## 9) Input Controls
- **Move:** WASD / arrow keys
- **Sprint:** Left Shift
- **Pass:** E
- **Shoot:** Left Mouse Button
- **Restart after match end:** R

## 10) Performance Notes
- Physics logic is handled in `FixedUpdate` for the ball.
- Ball rigidbody interpolation is enabled for smoothness.
- AI decisions run at intervals (`decisionRate`) instead of every frame.
- Scripts are split by concern (player, ball, AI, camera, match flow).
