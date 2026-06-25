# Architecture Audit Baseline

Date: 2026-04-26
Project: `Project_Architecture` (Unity 6 `6000.3.7f1`)
Scope: current state snapshot before refactor commits

## 1. Purpose

This document captures the current architecture baseline:

- active gameplay/menu scenes and custom components
- root gameplay prefabs and their custom components
- where `MonoBehaviour` is currently used
- classes that currently carry too much responsibility
- classes that are likely over-decomposed for their value

This is the reference point for the planned refactor series.

## 2. Scene Component Map

### `Assets/Scenes/MaInMenu.unity`

- `GameEP` -> `GameEntryPoint`
- `MainMenuEP` -> `MainMenuSceneEntryPoint`
- `Canvas` -> `MainMenuView`, `SettingsMenuView`

### `Assets/Scenes/MainScene.unity`

- `GameEP` -> `GameEntryPoint`
- `SceneEP` -> `GameplaySceneEntryPoint`
- `Controllers & Entrypoints` -> `InputService`
- `Canvas` -> `HudView`, `PauseMenuView`, `GameOverView`
- `GameOverController` -> `GameOverController`
- `Player` -> `HealthComponent`, `ManaComponent`, `PlayerStatsComponent`, `PlayerMovement`, `PlayerCombatSystem`, `PlayerView`
- `Model` -> `PlayerAnimationController`
- enemy instances include `EnemySaveId`

## 3. Root Prefab Component Map

### `Assets/Prefabs/Player.prefab`

- `Player` -> `HealthComponent`, `InputService`, `PlayerCombatSystem`, `PlayerMovement`
- `Model` -> `PlayerAnimationController`

Risk note: scene `MainScene` already has scene-level `InputService`, while `Player.prefab` also contains `InputService`.

### `Assets/Prefabs/Melee_Enemy.prefab`

- root `Melee_Enemy` -> `EnemyController`, `HealthComponent`
- `Model` -> `EnemyAnimationController`
- `UI` -> `HudView`

### `Assets/Prefabs/Range_Enemy.prefab`

- root `Range_Enemy` -> `EnemyController`, `HealthComponent`
- model child -> `EnemyAnimationController`
- `UI` -> `HudView`

### Projectile prefabs

- `Assets/Prefabs/Enemy_Projectile.prefab` -> `EnemyProjectile`
- `Assets/Prefabs/Magic_Projectile.prefab` -> `MagicProjectile`

## 4. MonoBehaviour Inventory (Custom Scripts)

Current scripts inheriting `MonoBehaviour` (directly or through `PanelViewBase`):

- `GameEntryPoint`
- `GameplaySceneEntryPoint`
- `MainMenuSceneEntryPoint`
- `EnemyController`
- `EnemySaveId`
- `EnemyAnimationController`
- `PlayerAnimationController`
- `PlayerCombatSystem`
- `EnemyProjectile`
- `MagicProjectile`
- `HealthComponent`
- `InputService`
- `ManaComponent`
- `PlayerController`
- `PlayerMovement`
- `PlayerStatsComponent`
- `PlayerView`
- `PanelViewBase`
- `GameOverController`
- `GameOverView`
- `HudView`
- `MainMenuView`
- `SettingsMenuView`
- `PauseMenuView`

Generated input wrapper `GameInputActions` is also `MonoBehaviour`-based in generated code shape, but it is not a gameplay architecture decision point.

## 5. Pure C# Service Layer Snapshot

Most classes in `Assets/Scripts/Services` are already non-`MonoBehaviour`:

- `EnemyStateRepository`
- `GameSaveInteractor`
- `GameSessionState`
- `JsonFileSaveService`
- `PlayerStateRepository`
- `SaveGameRepository`
- `SettingsRepository`
- `UnityAudioService`
- `UnitySceneLoader`

Conclusion: the statement "all services are MonoBehaviours" is no longer accurate globally, but the composition/scene layer still mixes Unity wiring with app logic.

## 6. High-Responsibility Hotspots

### `Assets/Scripts/Game/AI/EnemyController.cs`

Still combines many responsibilities in one `MonoBehaviour`:

- AI tick orchestration (`Update`)
- target discovery (`FindGameObjectWithTag`, `FindFirstObjectByType`)
- movement/attack orchestration
- projectile factory fallback and runtime `AddComponent`
- event publishing and debug logging
- large serialized config surface

### `Assets/Scripts/Entrypoints/Gameplay/GameplaySceneEntryPoint.cs`

Current scene root mixes:

- service registration
- scene object lookup
- player initialization
- save system assembly
- HUD source wiring
- pause menu assembly and ticking

### `Assets/Scripts/Game/Input/InputService.cs`

Single component currently does:

- input map/action lookup and validation
- callback lifecycle
- gameplay enable/disable policy
- cursor lock/visibility side effects

### `Assets/Scripts/Game/Combat/PlayerCombatSystem.cs`

Single component currently does:

- physical attack use case
- magic projectile use case
- cooldown state orchestration
- spawn policy and targeting logic
- debug logging

### `Assets/Scripts/UI/HUD/HudView.cs`

View currently does:

- source discovery (`FindGameObjectWithTag`, `FindFirstObjectByType`, parent search)
- controller/model creation
- lifecycle and per-frame update
- rendering

## 7. Potential Over-Decomposition

Likely low-value ultra-thin classes:

- `PauseMenuModel` (`bool IsPaused`)
- `GameOverModel` (`bool IsTriggered`)

Likely duplication / overlapping orchestration:

- `PlayerView` currently acts as update orchestrator
- `PlayerController` is conceptually similar and appears redundant in current setup

## 8. Target Rules for Refactor Series

These rules define "done" for architecture refactor commits:

1. Keep `MonoBehaviour` only for Unity adapter concerns:
   scene binding, serialized refs, transform/physics/animator lifecycle, prefab/view lifecycle.
2. Move app and domain logic into pure C# classes:
   AI decision state, cooldown policies, save orchestration, use cases, factories, runtime state.
3. Keep scene/prefab discovery in composition roots only:
   avoid `Find*` and runtime `AddComponent` inside gameplay/domain logic.
4. Keep views passive:
   no auto-wiring/service discovery in view classes.

## 9. Refactor Starting Point

Next planned commit after this baseline:

- `refactor(input): split input service from Unity component`
  - pure `InputActionInputService : IInputService, IDisposable`
  - thin `InputServiceComponent : MonoBehaviour` adapter for Unity lifecycle only
