# Ashes Battle System Prototype

This repository contains a **prototype battle system for a turn-based RPG that combines an ATB system with spatial positioning** 

The goal of this project is to build a **robust, scalable RPG combat architecture** that separates gameplay logic from Unity presentation.

---

# Core Design Philosophy

The battle system is built around four major layers:

Simulation Layer
Preview Systems
Adapter Layer
Presentation Layer

```

### Simulation Layer
The **core gameplay logic**.  
Contains deterministic systems that control battle mechanics.

This layer is **engine-agnostic** and does not depend on Unity components.

Responsibilities:

- ATB time simulation
- Command execution
- Ability resolution
- Status effects
- Actor state

---

### Preview Systems
Preview systems calculate **predicted outcomes before a command executes**.

These allow the player to see:

- Damage previews
- Ability ranges
- Movement previews
- Targeting feedback

These systems **mirror the simulation logic without mutating state**.

---

### Adapter Layer
The adapter layer **connects Unity objects to the simulation systems**.

Responsibilities:

- Convert Unity input → simulation commands
- Convert simulation events → presentation updates
- Maintain references between GameObjects and simulation actors

Adapters isolate Unity dependencies so the core simulation remains clean.

---

### Presentation Layer

The presentation layer contains **Unity specific components**:

- UI
- Animations
- VFX
- NavMesh movement
- Camera control

Presentation reacts to **events emitted by the simulation**.

---

# Battle System Overview

The combat system uses an **Active Time Battle (ATB)** model.

```

BattleClock → ATBSystem → Actor Ready

```

When an actor becomes ready they may perform a **command**.

Each command consists of:

```

Move + Action

```

Where:

```

Move = reposition using NavMesh
Action = attack / ability / item / wait

```

The player constructs a full command before execution.

Examples:

### Example Turn 1
```

Move into melee range
Attack enemy

```

### Example Turn 2
```

Move to spell range
Cast fireball

```

### Example Turn 3
```

Move to defensive position
Wait

```

### Example Turn 4
```

Attack enemy
Move away

```

The command is **previewed before execution**, then sent to the **BattleActionQueue**.

---

# Architecture Overview

```

Battle Systems
│
├── EventBus
├── BattleClock
├── ATBSystem
├── BattleStateMachine
├── BattleCommandBuilder
├── BattleActionQueue
├── AbilitySystem
├── StatusEffectSystem
├── TargetingSystem
├── PathfindingSystem
├── RangeSystem
├── DamagePreviewSystem
└── BattleFactory

```

---

# Project Structure

```

Assets
│
├── Game
│
│   ├── Core
│   │   ├── Events
│   │   ├── Services
│   │   └── Utilities
│   │
│   ├── Battle
│   │   ├── Actors
│   │   ├── Commands
│   │   ├── Systems
│   │   ├── Events
│   │   ├── Simulation
│   │   └── Factories
│   │
│   ├── Preview
│   │   ├── DamagePreview
│   │   ├── RangePreview
│   │   └── PathPreview
│   │
│   ├── Adapters
│   │   ├── Battle
│   │   └── UI
│   │
│   └── Presentation
│       ├── UI
│       ├── Animations
│       └── VFX
│
├── Scenes
│   ├── Bootstrap
│   └── BattleTest
│
└── Testing

```

---

# Development Roadmap

The battle system is being built in **incremental phases** to keep the architecture stable while adding complexity.

---

# Phase 1 — ATB Simulation (Current Phase)

Goal:

```

Actors exist
Battle clock flows
ATB bars fill
Actors become ready

```

Core systems implemented in this phase:

```

EventBus
BattleClock
ATBSystem
BattleActor
BattleTestBootstrapper

```

Responsibilities:

### EventBus
Central event dispatcher used for decoupled communication between systems.

Example events:

```

BattleStartedEvent
ATBReadyEvent
CommandExecutedEvent

```

---

### BattleClock

Controls **time flow inside the battle**.

Features:

- Start / Stop battle time
- Pause during command execution
- Provide delta time to systems

Example:

```

BattleClock.Update(dt)

```

---

### ATBSystem

Manages **ATB meters for all actors**.

Responsibilities:

- Increment ATB over time
- Detect when actors become ready
- Emit readiness events

Flow:

```

BattleClock → ATBSystem → ActorReadyEvent

```

---

### BattleActor

Represents a combatant in the simulation.

Contains:

```

Stats
Position
ATB value
Status effects

```

Actors are **pure data objects** used by systems.

---

### BattleTestBootstrapper

Creates a **minimal battle environment for testing**.

Responsibilities:

- Initialize core systems
- Spawn test actors
- Start battle simulation

This allows the battle system to be developed **independently of the full game**.

---

# Phase 2 — Command Execution

Goal:

```

Actors can build and execute commands
Commands enter a queue
Commands resolve sequentially

```

Systems introduced:

```

BattleCommandBuilder
BattleActionQueue
MovementCommand
AbilityCommand
WaitCommand

```

Responsibilities:

### BattleCommandBuilder

Constructs a **complete command** before execution.

Example:

```

Move → Attack
Attack → Move
Move → Wait

```

Once built:

```

Command → BattleActionQueue

```

---

### BattleActionQueue

Handles command execution.

Responsibilities:

```

Queue commands
Execute commands in order
Pause battle time during execution

```

---

# Phase 3 — Ability System & Effect Pipeline (Planned)

Phase 3 will introduce the **ability resolution architecture**.

Goal:

```

Abilities execute through a modular effect pipeline

```

Key systems:

```

AbilitySystem
EffectPipeline
DamageEffect
StatusEffectSystem
TargetingSystem

```

---

### AbilitySystem

Manages ability definitions and execution.

Responsibilities:

```

Ability validation
Ability execution
Ability cooldowns

```

---

### Effect Pipeline

Abilities are composed of **modular effects**.

Example ability:

```

Fireball

```

Pipeline:

```

Targeting
↓
Range Validation
↓
DamageEffect
↓
ApplyBurnStatus
↓
Trigger VFX

```

This design allows abilities to be **highly reusable and composable**.

---

### StatusEffectSystem

Manages buffs and debuffs.

Examples:

```

Poison
Burn
Stun
Defense Buff

```

Responsibilities:

```

Apply effects
Tick durations
Modify actor stats

```

---

### TargetingSystem

Handles ability targeting rules.

Examples:

```

Single enemy
Area of effect
Ally
Self

```

---

# Long Term Goals

Future phases will expand the battle system with:

```

Movement system using NavMesh
Range previews
Damage previews
AI behavior
Ability animations
Multistep abilities
Environmental effects

```

The final system aims to support:

```

Large ability libraries
Complex status interactions
Predictable combat simulation
Clear player feedback

```

---

# Current Status

The project is currently focused on **building the simulation layer first**.

This ensures that:

```

Gameplay logic is stable
Systems remain modular
Unity dependencies stay isolated

```

Once the simulation foundation is complete, the remaining layers (preview, adapter, presentation) will be built on top.

---

# Author

Mike Petrus  
Computer Science — Game Systems / Rendering Focus
```

