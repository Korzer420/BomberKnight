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
- Move Counter: With the four options you can move the counter like you want.
  
## Drops
Bombs have a 4% to be dropped when an enemy is killed. However "Tenth enemy has the bomb" which means that killing ten enemies in a row without getting hit will guarantee a bomb drop.
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
- Shell Salvager: Increases the base drop chance for bombs by 200% (to 12% total).
- Bomb Master: Bombs no longer explode after 3 second but rather when you press down.
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
- Baldur Shell: When a hit is blocked by the shell, the last bomb in your bomb bag explodes at your position.
- Shape of Unn: If a grass bomb will be your next bomb, there is a 50% chance that it will cycle between all available bomb types each second.
- Stalwart Shell: Increases the i-frames from bounce bombs by 100% and from the pyromaniac charm by 40%.
- Deep Focus: Bombs now take 6 seconds to explode instead of 3. Increases explosion radius by 50%.
- Quick Focus: Decreases the cooldown to drop bombs by 2.5 seconds.
- Thorns of Agony: Bombs deals contact damage equal to your nail damage on enemy contact. Has a 5% to explode upon dealing damage.
- Grubsong: Bombs grant 2 soul per tick when in contact with enemies.

## Puzzle Solutions/Item Locations
These will list all locations and their respective puzzles to obtain the items. It is recommended to search this out by yourself obviously, so **SPOILERS**:

- Bomb Bag: Killing the Moss Knight in the arena below the Lifeblood will advise you to not come back. If you ignore that warning the moss knight in the arena will be buffed but drops the bag after killing them

- Bomb Bag In kingdoms edge to the right of Oro is a dummy which you can repeatedly hit to spawn a "few" great hoppers. One of them seems to have eaten one of the bomb bags. Killing them, will pass the bag to you.

- Bomb Bag In distant village, at the bottom of the room, you'll notice a few platforms. Entering the big one at the center of the bottom will let a purple spider appear, which throws with bombs. Again, Killing them grant the bomb bag.

- Spore bomb: In fungal core, next to the big mushroom corpse you'll notice a gray bomb. Listening to the dream nail dialogue will hint at what you have to do: First, let a Spore cloud (from Spore shroom or a Spore bomb itself) touch the bomb. It will change the color to yellow. Second, place a bomb next to it. Afterwards the bomb will blink and follow you through the room. Lastly, let the bomb touch the orange goo that the mushroom ogres spit.

- Bounce Bomb: On the bridge to the toll bench in city (where Zote can appear), you'll notice a stone sentry in the background. Bombing it will start the encounter against them. Defeat them to obtain the bombs

- Echo Bombs: In the resting grounds room before the seer a purple bomb will appear that slowly fades away while moving somewhere. If you follow the bomb to its destination (a wall at the bottom left) and place a bomb there, you can enter that wall. This will place you in the dream shield room, with the difference that the bomb item will spawn.

- Gold bombs: Killing the gorgeous husk, will hint at you that something is leaking to the floor. If you exit the room and go the water in the middle of the room, some geo is around there without you ever.. 
doing something there. Also the water is slightly yellow. If you jump in there you'll be Hazard respawned. But if you throw a bomb in there, the golden bombs will appear!

- Powerbombs: In the lifeblood core room, 5 transparent bombs are visible above the charm pedestal. If you place all other 5 different types of bombs on the pedestal, the power bomb will spawn. Note that the 5 bomb drops have to be done in one visit

- Pyromaniac: In the fog canyon room where you normally obtain a charm notch a Bugs corpse is visible in the top right. Killing enough enemies/popping enough explosive bubbles will repeatedly display messages that you have to go on. After killing every enemy and letting every bubble explode, you can fulfill the bugs last wish and combust them via a bomb which will drop the charm. Idea by Exempt-Medic :grublove: 

- Bomb Master: In the Cornifer room in QG a big red bomb is placed right next to the ghosts. Listening to them reveals that they stole that from a bug in Crystal peak. The bomb is quite unstable and blows up after 7 minutes in your inventory. Bringing this bomb to the crystallized bug, which you can found above the entrance to the crystal heart room, and bombing him with exactly this bomb, will spawn the charm. Fun Fact: This bomb does actually have a unique damage value that is above most other types of bombs. Only gold bombs with greed and enough geo, power bombs and the combined damage of echo bombs exceed this.
-Shell Salvager: In the junk pit, you might notice the big golden chest. Inspecting it will tell you that the inscription of 5 knights are visible. Furthermore (if you can see colors at least), you might notice that the other five chest have unique colors as well. If you don't skip the description on the item screen for echo bombs, you might remember that they can "invoke memories". Placing an echo bomb at ismas corpse, dryyas corpse, Hegemols shell (after defeating FK), in the Grey mourner room and next to Dung Defender sleeping place, will trigger a "memory" in which order the chest have to be hit. If you play with colorless indicator they will give the position of the chest rather than there color. Hitting the five chest in the correct order will open the golden chest. The order is randomized each time. Quite a hard one, I admit.

## Mod interaction

### Debug
- Adds a panel for various debug functions.

### Randomizer 4
- Allows the bomb items, charms and bomb bags to be placed in their "vanilla" locations or randomized with all other items.
