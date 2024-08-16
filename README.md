This repository contains my VRChat mods and fixes. <br>
Join the [VRChat Modding Group discord](https://discord.gg/rCqKSvR) for official mods and support!

## Special Thanks
Thank you [Nitro. <3](https://github.com/nitrog0d) for the idea on the ToggleFullScreen <br>
Also thank you cutie ([Gompo <3](https://github.com/gompocp)) for helping me with literally everything lol.

## ProneUiFix
Current features:
* Simple fix to the Desktop Ui glitch that happens on some avatars, that opening the menu while you're proning will cause it be mispositioned.

## ToggleFullScreen
Current features:
* Adds a simple toggle on "Settings -> Other Options Panel" that allows you to toggle Full-Screen Mode without getting a scuffed resolution and actually using your entire screen lol.

## BetterPortalPlacement
I guess [Gompo](https://github.com/gompocp) needs a special thanks on this one because he gave me a pre-version of it that wasn't really very functional, but it guided me on the entire thing so it helped a lot. He also helped a bit with organization and the code itself. <3
This mod is based on a feature that was requested on VRChat's [Feature Requests'](https://feedback.vrchat.com/feature-requests/p/improved-portal-drop-system) page.

Current features:
* When you click the "Drop Portal" button, you enter "Drop Portal" mode. Triggering will result in the portal being created at the indicated location;
	* This mode will show up a sphere on where the portal is to be placed;
		* On VR, the default hand will be the one that was free when you lastly opened the QuickMenu;
		* If you click the trigger with that hand, the portal will be placed, if you use the other trigger, it'll alternate hands. Opening the QM again will cancel;
		* On Desktop, the placing position will be the cursor at the screen's center. To place, simply click with the mouse's left button.
	* For now, trying to place it on an non-placeable position shows up the error message. Clicking "Continue" will allow you to keep trying.
* There is a setting to allow the default VRC placement, and only use the mod in case of error;
* There is an extra one to deactivate the mod completely.

To do:
* Use the portal prefab itself, as the preview object;
* Make the Previewing be green if placing is possible, or red if not and might as well play the ui error sound when trying to place it, instead of opening a popup;

## Installation
Before installing:  
**Modding itself is against VRChat's ToS, so, according to the staff, it can lead to punishment, this mod included. Be careful while using this! I'm not responsible for any punishments.**

You will need [MelonLoader](https://discord.gg/2Wn3N2P) (discord link, see \#how-to-install).
After that, drop the mods .dll files in the `Mods` folder in your game's directory.