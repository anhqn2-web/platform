# Warrior Free Set - Complete Gameplay Framework

This folder contains a robust, polished, and modular gameplay framework designed for the **Warrior free set**. Every component is **plug-and-play**, requiring minimal to zero editor configuration.

---

## 1. Advanced Character Controller (`WarriorController.cs`)
The controller has been upgraded with a professional combat and movement feel:
- **Stamina System**: Actions like jumping, attacking, and sliding now consume stamina, which regenerates over time.
- **Combat Lunge**: Attacking now applies a forward "punch" to the movement, making combat feel weightful and responsive.
- **Screen Shake Triggers**: Successfully hitting an enemy or taking damage sends a signal to the camera for visual feedback.
- **Dynamic Collider**: Crouch automatically shrinks the physical hitbox to slide under obstacles.

## 2. Smooth Camera Follow (`CameraFollow.cs`)
- **Smooth Tracking**: Uses `SmoothDamp` to follow the player without jitter.
- **Integrated Screen Shake**: Built-in system to handle impacts and combat intensity.
- **Setup**: Attach to the **Main Camera**. It will automatically find the object tagged "Player".

## 3. Dynamic Enemy AI (`EnemyAI.cs`)
- **State-Based AI**: Automatically switches between **Idle**, **Chase**, and **Attack** based on player distance.
- **Floating Health Bar**: A health bar appears above the enemy's head **only when they are damaged** (health < 100%).
- **Setup**: Attach to any enemy sprite. It requires a `Rigidbody2D` (Freeze Rotation Z) and a `Collider2D` (Trigger). 
- **Combat**: It will automatically try to damage the player when in range.

## 4. Player HUD (`PlayerUI.cs`)
- **Auto-Generating UI**: If you don't have a UI set up, this script will **programmatically create** stylized health and stamina bars on your screen at runtime.
- **Setup**: Create an empty GameObject named `HUDManager` and attach this script.

---

## Controls Reference

| Action | Input | Details |
| :--- | :--- | :--- |
| **Move** | `A` / `D` | Runs and flips. |
| **Jump** | `Space` / `W` | Variable height; costs stamina. |
| **Attack** | `Left Click` / `J` | Sword swing + forward lunge; costs stamina. |
| **Crouch** | `S` / `Down Arrow` | Lowers profile. |
| **Slide** | `Left Shift` | Fast ground dash; costs stamina. |
| **Wall Slide** | Against Wall | Slide down slowly; can Wall-Jump. |

---

## Quick Setup Checklist
1. **Camera**: Attach `CameraFollow` to your Main Camera.
2. **Player**: Ensure your player object is tagged **"Player"** and has `WarriorController`.
3. **Enemies**: Attach `EnemyAI` to your enemy objects.
4. **HUD**: Attach `PlayerUI` to any empty GameObject.
5. **Layers**: Ensure your ground platforms are on a layer named **"Ground"**.
