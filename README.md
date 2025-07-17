# Verlet Integration Simulation in Unity

This project implements a real-time physics simulation using Verlet Integration within the Unity Engine. It simulates interconnected particles (dots) constrained by distance relationships (connections), allowing user interaction and dynamic updates.

![Preview](https://github.com/ideatogame/RopeSimulation/blob/main/RopeSimulation_Preview.gif?raw=true)

## Features

- **Verlet Integration** for stable and efficient particle simulation.
- **Dot and Connection System** for simulating physical structures.
- **User Interaction** through mouse input (e.g., breaking constraints, dragging points).
- **Visual Feedback** via Unity's `LineRenderer`.
- **Modular Design** separating solver, simulation logic, and rendering.
- **Asynchronous Updates** using `UniTask` for parallelism.

## Project Structure

- `VerletSolver/`
  - `Dot.cs`: Represents a single point in space with physics properties.
  - `Connection.cs`: Maintains a fixed distance between two dots.
  - `VerletSolver.cs`: Applies Verlet integration and solves constraints.

- `Simulation/`
  - `Simulation.cs`: Holds the setup and logic for a group of dots and their connections.
  - `SimulationController.cs`: Entry point; handles input, updates, and visualization.
  - `VisualDot.cs`: Manages the graphical representation of each dot.

## How to Use

1. Open the Unity project and attach `SimulationController` to a GameObject in the scene.
2. Press Play to start the simulation.
3. Interact using mouse input to drag or break connections between points.

## Requirements

- Unity 2020.3 or newer
- [UniTask](https://github.com/Cysharp/UniTask) (installed via UPM or manually)

## Notes

- All points are simulated using pure physics logic (no Rigidbody).
- LineRenderers are updated to reflect active connections.
- The system supports locked dots (immovable) and dynamic breaking of constraints.
