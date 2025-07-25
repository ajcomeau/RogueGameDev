# Rogue C#

## PDF Edition now available through LeanPub!

June 2025: The Rogue C# series is now available for download in PDF format! I’m reviewing this series for continuation and if you’d like to support this work, 
you can get this offline edition from LeanPub.com.

This PDF includes all the content that you see on ComeauSoftware.com in one file for convenient offline reading. The downloadable edition also contains 
a detailed Table of Contents and each chapter is linked back to the original articles for access to more resources and future updates.

Visit the project page for more information:  https://www.comeausoftware.com/rogue-csharp-main/

Get your offline copy on LeanPub.com: https://leanpub.com/roguecsharp

### 7/13/2023

See https://www.comeausoftware.com/c-sharp/rogue-csharp-random-updates/ for the latest writeup on the Play Again feature and spawning of new
monsters mid-level.

### 6/29/2023

See https://www.comeausoftware.com/c-sharp/rogue-csharp-building-on-the-foundation/ for the latest writeup.

### 6/22/2023

The fight mechanics are present but might need some tweaking.  It seems to get too easy after the first couple of experience
levels.

The rest of the armor types, weapons and ammunition have been loaded.  The inventory display will identify which is being worn and wielded.
The appropriate commands are available on the help menu.

I'm working on code names for items that require identification.  Two scrolls have been added, Identify and Magic Mapping, but they're not
doing anything yet and there's no read command yet.  The program does successfully assign code names, however.

### 6/14/2023

I started out just working on the player stats display at the bottom of the screen. Then I wanted to really test them
so now the monsters are fighting back.  The attacks are still basic although monsters do have minimum and maximum attack
damage properties where the player is just taking up to half the monster's hit points randomly.

The player's hit points also regenerate now, with the speed of regeneration dependent on the level.  I'm still tuning that
to see what works.  Step carefully through those dungeons, though.  Hobgoblins still seem to be especially nasty on the first
level, even with a basic attack, and I've already died in th game a few times.  

See the latest writeup at:
https://www.comeausoftware.com/c-sharp/rogue-csharp-monsters-strike-back/


### 6/8/2023

I adjusted the controls on the main form to accomodate other displays and this will hopefully resolve any problems.
You might need to expand or maximize the form for proper viewing.  I have set a minimum size past which the form cannot
shrink.

On another machine, Visual Studio had a habit of losing the event settings for the main form after it was resized.
The Start button stops working and none of the game keys worked.  If this is happening, just reselect the events for
the Start button Click event and the KeyDown event on the form.


### 6/5/2023

Monsters are wandering around the map and you can attack them by running into them to get them out of your way.
The fight mechanics are really basic and the same for every monster.  I'll expand them later.

See the latest write-up on ComeauSoftware.com -
https://www.comeausoftware.com/c-sharp/rogue-csharp-monster-shuffle/

I'm working on a major refactoring of the program now that monsters are moving around the map.
Previously, everything was stored on the map itself and the program would read and write directly there.  

Then inventory came along, inventory objects were stored on the map array, along with a separate character to be displayed.
When I added monsters, I decided to store the current monsters in a class-level list with a display character on the map but 
monsters were disappearing mysteriously.

I finally decided to let the main function that renders the map place the monsters as needed rather than trying to update their
locations throughout the program in both the list and the map.  Then I decided that Inventory should use the same strategy.

This meant changes throughout the program.  Mostly it works now but there is a glitch or two and I'm working on that.



### 4/22/2023

Food collection and hunger now works.  Eating is necessary for survival and the game includes an R.I.P. screen.  The following keys now work and I'll be adding a help screen this week.
```
Arrows - movement

'd' - drop inventory

'e' - eat

's' - search for hidden doorways

'i' - show inventory

'>' - go down a staircase

'<' - go up a staircase (requires Amulet from level 26

ESC - return to map from inventory screen.
```
CTRL-D will enter a developer mode that shows the entire map.  After this, CTRL-N will switch out a map for a new one.

### 4/1/2023 - All 26 levels, the Amulet, fog of war and hidden doors are now online.  See the latest chapter at https://www.comeausoftware.com/c-sharp/rogue-csharp-hidden-doorways/.

### 3/6/2023 - I've started adding some new code and have some new chapters online. 

### 2/22/2023 - Still working on the writeup at ComeauSoftware.com.  See the latest video with a demo for the StringBuilder class at https://youtu.be/5eNpECYU1cY.  

### 2/13/2023 - I've been working on the best way to present the lessons for this project online, writing them up and making some code improvements to what's been done so far.  As a result, I haven't moved beyond the hallway generation.  I hope to get the game moving forward again soon, though.  Don't forget to check out the course chapters already online - https://www.comeausoftware.com/tech-category/rogue-csharp/ 

### 1/11/2023 - Initial randomized dungeon rooms working.
