﻿This repository contains my VRChat mods and fixes. <br>
Join the [VRChat Modding Group discord](https://discord.gg/rCqKSvR) for official mods and support!

## Special Thanks
Thank you [Nitro. <3](https://github.com/nitrog0d) for the idea on the ToggleFullScreen; <br>
Thank you [AxisAngle#0001](https://discord.com/users/80798961836752896), that made the request for BetterDirections and helped with the logics; <br>
Also thank you cutie ([Gompo <3](https://github.com/gompocp)) for helping me a lot every time I needed you <3.

## ProneUiFix
Current features:
* Simple fix to the Desktop Ui glitch that happens on some avatars, that opening the menu while you're proning will cause it be mispositioned.

## ToggleFullScreen
Current features:
* Fixes Alt+Enter resolution;
* Adds a simple toggle on "Settings -> Other Options Panel" that allows you to toggle Full-Screen Mode without getting a scuffed resolution and actually using your entire screen.

## BetterPortalPlacement
I guess [Gompo](https://github.com/gompocp) needs a special thanks on this one because he gave me a pre-version of it that wasn't really very functional, but it guided me on the entire thing so it helped a lot. He also helped a bit with organization and the code itself. <3 <br>
This mod is based on a feature that was requested on VRChat's [Feature Requests'](https://feedback.vrchat.com/feature-requests/p/improved-portal-drop-system) page.

Current features:
* When you click the "Drop Portal" button, you enter "Drop Portal" mode. Triggering will result in the portal being created at the indicated location;
	* This mode will show up a white ring on where the portal is to be placed;
		* On VR, the default hand will be the one that was free when you lastly opened the Quick Menu;
		* If you click the trigger with that hand, the portal will be placed, if you use the other trigger, it'll alternate hands. Opening the QM again will cancel;
		* On Desktop, the placing position will be the cursor at the screen's center. To place, simply click with the mouse's left button.
	* You won't be able to place if close to a player or spawn, and the preview will get red to show that. In exceptional cases, a popup with the error message will show. Clicking "Continue" will allow you to keep trying;
	* The radius in which you can drop a portal is [1.1m, 5.1m] (total range: 4m) from the local player.
* Current settings are:
	* Allow the default VRC placement, and only use the mod in case of error;
	* Use the confirmation popup or not;
	* There is an extra one to deactivate the mod completely.

## BetterDirections
Current features:
* TLDR; 
	* Fixes the inconvenience that happens because of Euler angles during movementation while looking up, that happens usually while laying down or cuddling, for example.
* Full explanation:
	* If while laying down you look straight up and spin your view direction around the global Y axis (which naturally occurs because it’s impossible to perfectly align to it), the movement behavior will keep alternating the directions, which can be a little confusing or uncomfortable. This mod will make it unified so it gets really easy to move no matter where you’re looking, while still maintaining the direction you’re looking at as the “forward” one when walking without looking up.

## Installation
Before installing:  
**Modding itself is against VRChat's ToS, so, according to the staff, it can lead to punishment, this mod included. Be careful while using this! I'm not responsible for any punishments.**

You will need [MelonLoader](https://discord.gg/2Wn3N2P) (discord link, see \#how-to-install).
After that, drop the mods .dll files in the `Mods` folder in your game's directory.
