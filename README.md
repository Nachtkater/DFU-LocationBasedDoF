# DFU-LocationBasedEffects
Little mod to set separate values for Post Processing Effects depending on player environment.

I made this, since I like to use the DoF an Bloom effect pretty situational. 
Going through the effect settings everytime I entered/left a dungeon or the brightness of a scene changed drastically, I wanted to automate this.

Also, since the night vision during the cold months is pretty fine, this changed for me drastically when the game transisted to spring. 
For this, I spiced up the player based lighting a bit.

## Depth of Field
For depth of field, values for interior/exterior can be set.

## Bloom
Bloom has a setting for dark and bright environments.

### Dark
Is used:
- In dungeons
- After 7PM until 6AM when outside

### Bright
Is used:
- In other interior
- From 6AM to 7PM when outside

## Player light
Requires the "Player based light" setting of DFU

Be aware that I override the range values for Torches and Lanterns to 10 and 50. 
The thought here is, that while outside a lantern is being used during travels while in dungeons a torch is carried.
While I realize, this may not be optimal, I wasn't able to figure out how to make custom items recognized as light source.

While the range values are fix for now, all other behavior can be toggled.
So far I am unsure wether or not I tested all playstyle scenarios, but I hope I got everyone covered.

### Auto switch
This automatically switches between torch, lantern or none.
- Torch when in dungeons
- Lantern when outside and time is between 5PM and 8AM (just like city lights)
- None in other interiors or during the day when outside

### Auto refill
This keeps the light source condition between 0 and 3.
While I could have set the condition to an incredible high value, I prefer the flickering that should normally signalize the last seconds of usability.
(If you didn't get the items from setting three, their condition will probably display as useless, due to the low condition)

### Auto give
This gives the player a light source if no torch or lantern is in the inventory at the first time it's use would be triggered.
So when starting a new game, a torch should appear in the inventory and the lantern in the first night outside.
The items are named as "Dungeon Torch" and "Travel Lantern". They have a max condition of 3 so when combined with auto refill the always show as slightly used.

- So, those who want to keep going back to the inventory to switch on their own, disable auto switch
- If you want to start out naked until you get a light source on your own, disable auto give
