# **`Data Layer: Asset Naming & Organization Standards`**

**`Design Philosophy:`** `To ensure long-term scalability, prevent broken references, and guarantee perfect alphabetical sorting in the Unity Inspector, all ScriptableObject assets must adhere to strict naming and organizational conventions.`

## **`1. The ID Convention ([type]_[family]_[tier])`**

**`Rule:`** `Never use the in-game display name for an internal ID. IDs are for the engine; display names are for the player.`

* *`Incorrect:`* `item_hi_potion (Breaks sorting, hard to rename later).`  
* *`Correct:`* `item_potion_02 (Sorts perfectly, display name can be changed freely).`

**`Format Structure:`**

* `type: The mechanical type of the asset (item, ability, status, class, enemy, encounter).`  
* `family: The logical grouping or baseline name of the asset (potion, cure, goblin).`  
* `tier: A two-digit number (01, 02, 99) denoting its rank, sequence, or level.`

**`Examples:`**

* `class_paladin (Tiers usually not needed for unique classes)`  
* `status_phalanx_01`  
* `ability_cure_01 (Display Name: "Cure")`  
* `ability_cure_02 (Display Name: "Cura" or "Cure II")`  
* `item_potion_01 (Display Name: "Potion")`  
* `item_potion_02 (Display Name: "Hi-Potion")`

## **`2. The File Naming Convention (FileName == AssetID)`**

**`Rule:`** `The name of the Unity .asset file must be exactly identical to the string ID typed inside the ScriptableObject's Inspector.`

* *`Incorrect:`* `Phalanx.asset (Is this the ability or the status? Searching the project for errors is difficult).`  
* *`Correct:`* `ability_phalanx_01.asset and status_phalanx_01.asset`

**`Why?`** `If the Unity console throws an error like [AbilityDatabase] Could not find ability with ID: ability_fireball_02, you can copy and paste that exact string into the Unity Project search bar (Ctrl+P) and the exact file will immediately appear.`

## **`3. Folder Organization`**

**`Rule:`** `Assets are grouped by their mechanical Category, not by the Class that uses them. Many classes may share the same abilities or statuses, so organizing by Category prevents duplicate assets.`

**`Standard Hierarchy (Assets/Ashes/Data/):`**

`Assets/Ashes/Data/`  
`├── Classes/`  
`│   ├── class_paladin.asset`  
`│   ├── class_white_mage.asset`  
`│`  
`├── Enemies/`  
`│   ├── enemy_goblin_01.asset`  
`│`  
`├── Items/`  
`│   ├── Consumables/`  
`│   │   ├── item_potion_01.asset`  
`│   ├── Combat/`  
`│   │   ├── item_bomb_01.asset`  
`│`  
`├── StatusEffects/`  
`│   ├── status_phalanx_01.asset`  
`│   ├── status_poison_01.asset`  
`│`  
`├── Encounters/`  
`│   ├── encounter_goblin_camp_01.asset`  
`│`  
`└── Abilities/`  
    `├── WeaponSkills/`  
    `│   ├── ability_basic_attack.asset`  
    `│   ├── ability_shield_bash_01.asset`  
    `├── WhiteMagic/`  
    `│   ├── ability_cure_01.asset`  
    `│   ├── ability_haste_01.asset`  
    `├── BlackMagic/`  
    `│   ├── ability_fireball_01.asset`  
    `├── HolyArts/`  
    `│   ├── ability_divine_cleave_01.asset`  
    `│   ├── ability_phalanx_01.asset`  
    `├── System/`  
    `│   ├── ability_system_follow.asset`

## **`Summary Checklist for New Assets`**

1. `[ ] Did I use [type]_[family]_[tier] for the ID?`  
2. `[ ] Does the .asset file name match the ID perfectly?`  
3. `[ ] Is the asset placed in the correct sub-folder based on its mechanical category?`  
4. `[ ] Did I click "Auto-Populate" on the Database adapter after creating it?`

