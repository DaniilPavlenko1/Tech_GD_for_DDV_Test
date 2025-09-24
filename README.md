📌 Overview

This project was created as part of a Technical Game Designer test assignment.
It demonstrates skills in Unity scene setup, gameplay scripting, AI behavior, UI/UX implementation, and optimization.

The goal of the assignment was to build a small but functional vertical slice showcasing:

Farming mechanics (planting, watering, growth stages, harvesting).

Interactive UI (world-space progress bars, sliders).

AI & Navigation (animals following the player, reacting to items).

Camera setup & player controls (Cinemachine FreeLook, smooth movement).

Optimization & clean-up of the scene for production readiness.

🎮 Features:
🌱 Farming System

Planting carrot seeds.

Watering crops with watering can animation.

Growth stages with progress bar UI.

Harvesting system with item collection.

🐾 AI & Interaction

NavMesh-based animal movement.

Animals (deer, chicken, dog, etc.) react to player interaction.

Feeding mechanic (e.g., giving carrots).

Temporary “Follow Player” state after interaction.

Heart particle effects when bonding with animals.

🧑‍🤝‍🧑 Player & Camera

Player movement via CharacterController (smooth navigation over objects).

Cinemachine FreeLook camera with fixed jitter issues.

Interaction system with triggers (doors, crops, animals).

🎨 UI / UX

World-space sliders and progress bars for crops.

Carrots counter.

🔧 Optimization & Clean-Up

Removed unused assets & objects.

Organized scene hierarchy.

Configured Skybox and lighting for consistent visuals.

🛠️ Tech Stack

Engine: Unity (2022.3.58f1 LTS)

Scripting: C#

AI: Unity NavMesh

Camera: Cinemachine

Version Control: GitHub

📹 Demo Video

👉 [https://drive.google.com/file/d/1tTA2g3-5DO-jYhmQjxxvE84vz0q8qJIy/view?usp=sharing]

🚀 How to Run

Clone the repository.

Open in Unity (same version as used in project).

Play the main scene: Scenes/Main Scene.unity.

Controls: WASD / Mouse / E (Interact) / Shift (Run) / C (Camera toggle) / Q (Wave)

### Materials & Render Pipeline
This project uses the **Unity Built-in Render Pipeline**.  
If you see pink (magenta) materials after opening the project:

1. Go to `Assets/ithappy/Animals_FREE/Render_Pipeline_Convert`.
2. Apply the preset **Unity_2021_Built-In_source** (double-click it).
3. Unity will automatically reassign the correct materials for the Built-in pipeline.

👤 Author

Daniil Pavlenko

🎮 Technical Game Designer