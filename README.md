﻿This repository contains my VRChat mods and fixes. <br>
Join the [VRChat Modding Group discord](https://discord.gg/rCqKSvR) for official mods and support!

## Special Thanks
Thank you [Nitro. <3](https://github.com/nitrog0d) for the idea on the ToggleFullScreen; <br>
Thank you [AxisAngle#0001](https://discord.com/users/80798961836752896), that made the request for BetterDirections and helped with the logics; <br>
Also thank you cutie ([Gompo <3](https://github.com/gompocp)) for helping me a lot every time I needed you <3.

## BetterDirections
This mod was developed with the help and by request of [AxisAngle#0001](https://discord.com/users/80798961836752896). 

Current features:
* TLDR; 
	* Fixes the inconvenience that happens because of Euler angles when moving while looking up, usually while laying down or cuddling, for example.
* Full explanation:
	* If while laying down you look straight up and spin your view direction around the global Y axis (which naturally occurs because it’s impossible to perfectly align to it), the movement behavior will keep alternating the directions, which can be a little confusing or uncomfortable. This mod will make it unified so it gets really easy to move no matter where you’re looking, while still maintaining the direction you’re looking at as the “forward” one when walking without looking up.

## BetterPortalPlacement
I guess [Gompo](https://github.com/gompocp) needs a special thanks on this one because he gave me a pre-version of it that wasn't really functional, but it guided me on the entire thing so it helped a lot. He also helped a bit with organization and the code itself. <3

This mod is based on a feature that was requested on VRChat's [Feature Requests'](https://feedback.vrchat.com/feature-requests/p/improved-portal-drop-system) page.

Current features:
* When you click the "Drop Portal" button, you enter "Drop Portal" mode. Triggering will result in the portal being created at the indicated location;
	* This mode will show up a ring at the position where the portal is to be placed;
		* On VR, the default hand will be the one that was free when you lastly opened the Quick Menu;
		* If you click the trigger with that hand, the portal will be placed, if you use the other trigger, it'll alternate hands. Opening the QM again will cancel;
		* On Desktop, the placing position will be the cursor at the screen's center. To place, simply click with the mouse's left button.
	* If placing the portal is possible, the preview will be green as an indicator;
	* You won't be able to place if close to a player or spawn, and the preview will get red to show that. In exceptional cases, a popup with the error message will show. Clicking "Continue" will allow you to keep trying;
	* The radius in which you can drop a portal is [1.1m, 5.1m] (total range: 4m) from the local player.
* Current settings are:
	* Allow the default VRC placement, and only use the mod in case of error;
	* Use the confirmation popup or not;
	* There is an extra one to deactivate the mod completely.

## BetterSteadycam
This mod was firstly developed by [nitro.](https://github.com/nitrog0d), but now it's been maintained and updated by me. 

**Current features:**
* Lets you tweak the VRChat Steadycam (or smooth camera) feature (located on QuickMenu -> Camera);
	* Also allow usage on Desktop (with an option to toggle this on or off).
* Configurable FOV;
* Configurable smoothing;
* Option to render UI or not.
	* This can also be toggled by pressing F10.
	
## DesktopCamera
This mod was firstly developed by [nitro.](https://github.com/nitrog0d), but now it's been maintained and updated by me. 

**Current features:**
* This mod allows Desktop users to use the VRChat Camera feature and makes it easier to use for VR users;
* Add new options to the camera menu.
	* The explanation below can be confusing, I recommend you to just try it in-game after reading.

**Arrow Keys feature explanation:**
* You can move the camera/viewer position using the arrow keys **(TIP: if you hold alt it moves faster (speed is configurable))**;
* You can also move the camera/viewer up and down using Page Up and Page Down buttons.

**If you have a numpad:**
* You can rotate it pressing 2 and 8 (tilt up and down), 4 and 6 (tilt left and right), 7 and 9 (orientation);
* You can make the viewer look at you by pressing 1;
* You can reset the camera and the viewer by pressing 3;
* You can take a picture using the Plus button (+), and you can toggle the Camera Movement using the Minus button (-).  

**Move Around Camera feature explanation:**
* The camera will rotate around the user's camera instead of just going to another dimension.

**Camera Movement feature explanation:**
* Remember the arrow keys feature above? So, this is a toggle, you can toggle between the actual Camera and the Viewer, if you have it on the Camera, the arrow/numpad keys will move the Camera, if you have it on the Viewer, then the Viewer will move instead;
* Observation: The Viewer is the Camera view. ;
* **Warning: The Camera Space must be in "World" mode, or else the camera won't move**.

## ProneUiFix
**Current features:**
* Simple fix to the Desktop Ui glitch that happens on some avatars, that opening the menu while you're proning will cause it be mispositioned. (Update: bug has been fixed by VRChat and this is now useless)

## ToggleFullScreen
**Current features:**
* Fixes Alt+Enter resolution;
* Adds a simple toggle on "Settings -> Other Options Panel" that allows you to toggle Full-Screen Mode without getting a scuffed resolution and actually using your entire screen.
* Adds an option to change fullscreen resolution in game, located on MelonPrefs.

## TrackingRotator
This mod was firstly developed by [nitro.](https://github.com/nitrog0d), but now it's been maintained and updated by me. 

**Dependencies (at least one is required):**
  * [UIExpansionKit](https://github.com/knah/VRCMods) by [knah](https://github.com/knah), or/and;
  * [ActionMenuApi](https://github.com/gompocp/ActionMenuApi) by [gompo](https://github.com/gompocp).

**Current features:**
* This mod lets you rotate your tracking, it was made with "play while laying down" in mind;
* Adds a submenu on UiExpansionKit QuickMenu integration;
* You can change the rotation value, high precision value and "Reset rotation when a new world loads" in Mod settings (Quick menu/Settings/Mod settings).

## Installation
**Before installing:**
**Modding itself is against VRChat's ToS, so, according to the staff, it can lead to punishment, this mod included. Be careful while using this! I'm not responsible for any punishments.**

You will need to install [MelonLoader](https://discord.gg/2Wn3N2P) (discord link, see \#how-to-install), and all the dependencies that might be necessary for each mod.
After that, drop the mods .dll files in the `Mods` folder in your game's directory.
