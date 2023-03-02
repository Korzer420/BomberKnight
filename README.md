# Bomber Knight
A Hollow Knight Mod which adds bombs. (Requires ItemChanger, SFCore and KorzUtils)

## Content

- Adds a "spell" to the game, which drops a bomb that explodes after 3 seconds dealing damage and breaking damaged floors (like dive does).
- Adds 3 new charms that synergies with bombs.
- Adds 14 interactions with other charms.
- Adds 3 bomb bags for capacity upgrades. (10, 20, 30 respectivly)
- Adds 6 different type of bombs.

Some locations are linked to puzzles, others are behind buffed/custom enemies.

## Controls
Using the cast button WITHOUT pressing left nor right will drop the bomb at the Knights location. This will reduce the bomb count by 1.
Powerbomb: Placing a bomb while holding down will create a power bomb instead (if all conditions, which you can find below, are met).

Bombs have a 3 second place cooldown.

**You can bind the bomb placing to the quick cast button instead, inside the mod menu.**

## Settings
- Drop Button: Determines which button has to be pressed (without left and right) to drop a bomb.
- Colorless Indicator: If you have problems seeing colors, you can activate this option. This will add:
  - A label to the bomb counter, so you can see what your next bomb will be.
  - An info text about how many bombs of which type do you have in the inventory (if you select the bomb bag).
  - Replace the colors in the color puzzle with positions (e.g red -> "right most").
  
## Drops
Bombs have a 1% to be dropped when an enemy is killed. However "Tenth enemy has the bomb" which means that killing ten enemies in a row without getting hit will guarantee a bomb drop.
Only enemies which would normally drop geo can drop bombs (excluding bosses). If an enemy where this condition isn't met is killed as the tenth enemy, the counter will grant you the bomb drop on the next enemy instead. Assuming you don't get hit in between of course...

What kind of bomb is dropped is luck dependent. On top of that, a few modifications can be done though.

The drop decision is made in this order:
Fixed drop -> Modifications drops -> Normal drops

### Fixed Drop 
If the enemy which drops a bomb is a mushroom enemy, it is guaranteed to drop a spore bomb (if unlocked).

### Modifications Drop
- If Heavy Blow is equipped: 40% to be a bounce bomb.
- If Spore Shroom is equipped: 40% to be a spore bomb.
- If Fragile Greed is equipped: 10% to be a gold bomb. Increased to 20% if it is Unbreakable Greed instead.

### Normal Drop
Common bomb: 60%
Uncommon bomb: 30%
Rare bomb: 10%

**If a category has no available bombs the chance is added equally to the other available category/ies.**

## Bombs
Currently there are 6 different bomb types which you can use.
- Grass Bomb (20 damage per hit, Common): No special effect.
- Spore Bomb (10 damage per hit, Uncommon): Leaves a spore cloud (like Sporeshroom) behind upon explosion.
- Bounce Bomb (1 damage per hit, Uncommon): Does not deal damage to the player. Instead the will be launched upwards for 0.2 seconds and receive i-frames.
- Gold Bomb (Up to 40 damage per hit (based on the players current geo), Rare): Drops up to 100 geo upon explosion.
- Echo Bomb (25 damage, Rare): Explodes 4 additional times in 3 seconds intervals. The damage is repeatedly decreased by 20% for each explosion.
- Power Bomb (50 damage, -): Bigger explosion radius. Breaks damaged floors/wall across the WHOLE room. Can only open some oneway passages. This bomb cannot be obtained normally and has to be "crafted" by spending 3 bombs and a lifeblood mask. Press down while placing a bomb to create a power bomb instead.

**Grass bombs are automatically unlocked, when obtaining a bomb bag!**

## Charms
This mod adds 3 new custom charm to enhance bomb mechanics:
- Shell Salvager: Doubles the bomb drops (more on that below)..
- Bomb Master: Bombs no longer explode after 3 second but rather when you press down while trying to place a bomb.
- Pyromaniac: Your explosions no longer deal damage to you, but still grant you i-frames. Getting "hit" by an explosion has a 25% to heal you instead.

### Charm Synergies
Bombs are further amplified by vanilla charms:
- Heavy Blow: Increases drop chance for Bounce bombs.
- Spore Shroom: Increases drop chance for Spore bombs.
- Fragile Greed: Increases drop chance for Gold bombs. Increases the damage cap of gold bombs to 80 (from 40) and the dropped geo amount from them. The damage cap and drop chance are further increased if it is Unbreakable Greed instead.
- Gathering Swarm: Grants bombs a homing effect and let them ignore terrain.
- Sprintmaster: Doubles the homing speed of bombs.
- Dashmaster: Doubles the air time granted by bounce bombs.
- Shaman Stone: Increases bomb damage by 30%. Bomb damage now also count as spell damage.
- Spell Twister: Grants a 25% chance for bombs to not be removed from the inventory when used.
- Baldur Shell: When a hit is blocked by the shell, the last bomb in your bomb bag explodes at your position. (Not implemented yet)
- Shape of Unn: If a grass bomb will be your next bomb, there is a 50% chance that it will cycle between all available bomb types each second.
- Stalwart Shell: Increases the i-frames from bounce bombs by 100% and from the pyromaniac charm by 40%.
- Deep Focus: Bombs now take 6 seconds to explode instead of 3. Increases explosion radius by 50%.
- Quick Focus: Decreases the cooldown to drop bombs by 2.5 seconds.
- Thorns of Agony: Bombs deals contact damage equal to your nail damage on enemy contact. Has a 5% to explode upon dealing damage.
- Grubsong: Bombs grant 2 soul per tick when in contact with enemies.

## Mod interaction

### Debug
- Adds a panel for various debug functions.

### Randomizer 4
- To do

### LoreMaster
- To do

### Curse Randomizer
- To do
