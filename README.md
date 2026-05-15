# GTAV-BattleRoyale

A fully functional Battle Royale mod for Grand Theft Auto V, built in C# using ScriptHookV .NET. Written in 2020 by [@mrroibot](https://github.com/mrroibot) — solo, without AI assistance, and with zero Stack Overflow resources due to GTA V modding's legal gray area at the time. Forum threads, the ScriptHookV API headers, and the GTA V Native DB were the only references.

> **Preserved as-is from 2020.** This repository exists as a portfolio artifact. The code reflects a real working project, not a cleaned-up retrospective. Comments, debug notifications, and rough edges are intentional — they're part of the story.

---

## What It Does

A full Battle Royale game loop inside GTA V's open world:

- A cargo plane spawns outside the map boundary and flies toward the safe zone
- The plane's bomb bay opens as it crosses into the play area
- 99 AI-controlled bots spawn beneath the plane mid-flight, each in freefall
- At ~400m above terrain (not sea level — actual ground clearance), bots open parachutes and glide to pre-assigned landing zones
- Bots independently find vehicles, drive into the safe zone, exit, and fight
- The safe zone shrinks in stages, forcing encounters
- A kill feed, alive counter, health bar, armor bar, and ammo display run throughout
- When a bot dies, loot pickups and a drop crate spawn at the corpse

Supported modes: Solo (99 bots), Duo (48 teams), Quad (24 teams).

---

## Technical Architecture

### Core Scripts

| File                      | Responsibility                                           |
| ------------------------- | -------------------------------------------------------- |
| `GTAVBattleRoyaleMain.cs` | Game loop, menu, plane setup, safezone orchestration     |
| `AISpawner.cs`            | Mass ped spawning, drop sequencing, alive counter        |
| `AITask.cs`               | Per-bot AI brain — one Script instance per bot           |
| `Safezone.cs`             | Safezone state and blip management                       |
| `UIDraw.cs`               | All HUD rendering (health, ammo, kill feed, alive count) |
| `GameData.cs`             | Static data: 99 bot names, weapon component strings      |

### How the AI Works

Each bot runs as an independent ScriptHookV coroutine (`AITask`), ticking every 3 seconds. The bot progresses through implicit states:

```
FALLING → PARACHUTING → LANDING → EQUIPPING → MOVING TO ZONE → FIGHTING → DEAD
```

**Key decisions per tick:**

- Not in zone, no vehicle, far from zone → find or spawn a vehicle
- Not in zone, in vehicle → drive to zone
- Not in zone, near zone → run on foot
- In zone, enemy nearby → fight
- In zone, no enemy → wander
- Dead → drop loot, update kill feed, null own slot, self-terminate

### Solving the Far-Ped Problem

GTA V only activates ped pathing within ~100m of the player. For a 1000m+ battle royale map, this is a fundamental problem. The solution, discovered through GTAForums research:

```csharp
Function.Call(Hash.SET_ENTITY_LOAD_COLLISION_FLAG, ped, true);
Function.Call(Hash.SET_ENTITY_LOD_DIST, ped, 10000);
Function.Call(Hash.REQUEST_COLLISION_AT_COORD, ped.Position.X, ped.Position.Z, ped.Position.Y);
Function.Call(Hash.REQUEST_COLLISION_FOR_MODEL, ped.GetHashCode());
while (!Function.Call<bool>(Hash.HAS_COLLISION_LOADED_AROUND_ENTITY, ped)) {
    Wait(50);
}
```

Forcing collision load around each entity before assigning tasks makes the engine treat distant peds as active. Driving navigation works at any range due to road node streaming; on-foot pathing needs this explicit force.

### Staggered Spawning

Bots don't all spawn in one frame. `AISpawner` uses ScriptHookV's coroutine model to stagger group spawns:

```csharp
Wait(random.Next(50, 3000));
```

This yields execution back to the game engine between each squad, creating a natural drop sequence instead of a sudden population explosion.

### Off-Screen Combat Simulation

Simulating 99 bots fighting each other at all times is not feasible. The solution: bots far from the player that cannot find transport have a randomized survival chance — they either get a vehicle spawned nearby or are eliminated. This approximates off-screen combat without the CPU cost of running it.

```csharp
public void chanceToLive() {
    bool chanceToLive1 = random.Next(1, 10) < 6; // 60% get a vehicle
    bool chanceToLive2 = chance2 >= 6;            // 40% are eliminated
    ...
}
```

### Alive Counter

Dead bots null their own slot in `AISpawner.task[]` before self-terminating. The alive count is simply:

```csharp
task.Count(s => s != null) + playerTask.Count(s => s != null)
```

No explicit death tracking. The counter is a garbage collection audit.

---

## Algorithms Used (Unknowingly at the Time)

Looking back, this project independently implemented several named patterns:

- **Finite State Machine** — bot AI phase transitions
- **Cooperative Multitasking / Coroutines** — `Wait()` yielding inside spawn loops
- **Broadcast Pattern** — AISpawner propagating safezone state to all AITask instances
- **Weighted Random / Probability Tables** — loot assignment, firing patterns, survival chance
- **Spatial Proximity Queries** — zone membership, nearby vehicle detection
- **Lazy Initialization** — `setPedAttributes()` running once on first ground contact
- **Dead Reckoning** — approximating off-screen combat without simulating it
- **Observer / Event-Driven Architecture** — ScriptHookV's tick subscription model
- **Manual Reference Counting** — null slots as dead entity markers

---

## What I'd Do Differently Now

- The `AISpawner.OnTick` propagates safezone to all 110 slots every 10ms. A dirty flag would make this update-on-change only.
- The kill feed timer stacks event handlers on repeated calls — a classic C# event leak. Should unsubscribe before subscribing.
- `safezonePosition` is a `Vector3` struct; the null check against it always evaluates true. Should use a separate boolean flag.
- `isPedNearPlayer` in `findVehicle()` checks `< 0` — always false. Dead code that survived because the fallback path (`chanceToLive`) handled it anyway.
- The `static` arrays on `AISpawner` create tight coupling with `AITask`. A proper event system or dependency injection would be cleaner.
- 99 Script instances is unconventional. An entity-component approach with a single manager script would be more performant.

---

## Requirements

- Grand Theft Auto V (legitimate purchase required)
- [ScriptHookV](http://www.dev-c.com/gtav/scripthookv/)
- [ScriptHookV .NET](https://github.com/scripthookvdotnet/scripthookvdotnet)
- [NativeUI](https://github.com/Guadmaz/NativeUI)
- Visual Studio / .NET Framework 4.8

---

## Legal

This project is a personal portfolio artifact preserved for educational purposes. It is not intended for redistribution as a playable mod.

Grand Theft Auto V and all related assets, trademarks, and intellectual property belong to Rockstar Games. This project is not affiliated with or endorsed by Rockstar Games.

A legitimate purchased copy of GTA V is required to run this mod.

---

## Context

I wrote this in 2020 while learning C# — less than a month into the language, coming from a Java background in school. No AI tools. No Stack Overflow (GTA modding's legal gray area made code examples scarce). Resources were GTAForums threads, the ScriptHookV API headers, and the GTA V Native DB.

I never finished it. Got a job offer and moved on. Found the code in 2026 and apparently it was more complete than I remembered.

— Roi, 2026
