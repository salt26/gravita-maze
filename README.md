# GravitaMaze

**[Read in English](./README.md)**  
**[한국어로 읽기](./README.ko.md)**

[![Version badge](https://img.shields.io/badge/Version-1.6.4-purple.svg)](https://github.com/salt26/gravita-maze/releases/tag/v1.6.4)    
![Unity](https://img.shields.io/badge/unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white)

<img src="./Figures/Banner_16-3.png">

## Introduction

### "Escape complex mazes by manipulating gravity!"

* Puzzle game
* Make your own map by using the space-cube builder.
* Korean is available!
  * English is temporarily unavailable. Sorry for the inconvenience.
* This is a pre-release.
  * You can listen the latest news on [our blog](https://blog.naver.com/ravitastudio).
* Check out our latest news below.
  * Youtube: https://youtube.com/@Ravita_Studio/
  * Naver Blog: https://blog.naver.com/ravitastudio (Korean only)

## Download

### [v1.6.4 for Android, Windows, and macOS](https://github.com/salt26/gravita-maze/releases/tag/v1.6.4)

> Note: this demo version is playable only until December 2023.

---

## Update Logs

<details>
<summary>Click here to expand or collapse the old update logs!</summary>

### Updates (v1.5.0 -> v1.6.4)

#### Graphics
* Apply completely renewed graphics!

#### UI
* Redesign main menu to use mouse click instead of manipulating gravity.
  * Adventure -> Challenge
  * Custom -> Explore
  * Map editor -> Create
  * Settings -> (temporarily unavailable)
  * Credit -> (temporarily unavailable)
* Change Name.
  * Map -> Space-cube
  * Ball -> Capsule
  * Map editor -> Space-cube builder
* Add new levels to Challenge mode.
  * Easy < **Simple** < Normal < Hard < **Extreme** < Insane
* Display of the current difficulty during Challenge play.
* Remove First scene.
* Add Splash scene.
  * In case of inability to connect to the server via the network, gameplay is unavailable; instead, links to our blog and YouTube are displayed.
  * New BGM in Splash scene.
* Fix bugs related to holes when opening space-cubes in Create.
* Require author name input when saving space-cubes in Create.
* Modify Test phase in Create to always offer 'retry with time' instead of simply 'retry'.
* Game automatically pauses when the application loses focus.
* Only Korean language available now.
  * English is temporarily unavailable.

#### Game Balance
* Balanced according to the new difficulty classification system in Challenge mode
  * New Easy is easier than the old Easy
  * New Simple is similar to the old Easy
  * New Normal consists of relatively easy space-cubes from the old Normal
  * New Hard includes relatively difficult space-cubes from the old Normal and relatively easy ones from the old Hard
  * New Extreme is slightly harder than the old Hard
  * New Insane is similar to the old Insane
* Add new space-cubes

### Updates (v1.4.4 -> v1.5.0)

* Add animated background particles to better convey the sense of manipulating gravity.
* Add a new type of tile: A tile that is a hole.
* Fix UI bugs occurring on macOS. 
* Alter mobile vibration.
* Update credit text.
* Tutorial overhaul: Reorder steps and lower difficulty. 

### Updates (v1.4.3 -> v1.4.4)

* Add vibration on mobile.
* Add a resurrection feature when all lives have been depleted in Adventure mode: Upon resurrection, players can retry the same map. However, retry attempts are ineligible for receiving stars.
* Optimize map tile rendering.
* Add a guide for the time skip button.
* Fix iron sound effect bug.
* Fix UI and resolution bugs on macOS.
* Fix a bug where background music occasionally attenuated when manipulating gravity rapidly.
* Fix a bug in the map editor related to folder creation.
* Remove unnecessary sprites.
* Rearrange maps to adjust difficulty for Easy, Normal, and Hard levels in Adventure mode.

### Updates (v1.4.2 -> v1.4.3)

* Add a ninth map with a short time limit to the tutorial: extend the time limit after two timeouts.
* Add animation of objects moving due to gravity when the player dies.
* Change the color of the timer bar according to the remaining time.
* Preserve the volume and language settings of the game even after restarting.
* Change macOS shortcuts to use `Option+Space` and `Option+Enter` (on Windows, use `Ctrl+Space` and `Ctrl+Enter`, respectively).
* Fix the bug where the map file is not displayed correctly when entering the Custom mode.
* Fix the bug where gravity manipulation and retry were possible via keyboard while the pause window is open.
* Fix tooltip and button-related bugs.
* Change the game title in Korean from "중력 미로" to "중력미로". The English title remains the same as "GravitaMaze".
* Change the display of "Korean" to "한국어" in the language settings.
* Change the dropdown UI in the language settings.
* Make some modifications to the localized text.
* Modify the credit text.
* Upgrade the Unity version.
* Update the license.

### Updates (v1.4.1 -> v1.4.2)

* Show tooltips on various buttons when hovering on the PC platform.
* On the PC platform, use Ctrl + Space for "retry with time refill" and Ctrl + Enter to proceed when time runs out
  * Show PC-specific tooltips when hovering over these buttons.
* Modify some guiding texts in the tutorial.
* Resolve critical issue in v1.4.1 where the program occasionally freezes.
* Provide a Korean version of the release document.
* Add links to Fantrie, YouTube, and other latest news on the GitHub release document.

### Updates (v1.4.0 -> v1.4.1)

> Important: v1.4.1 has a critical bug! Please download v1.4.2 or later.

* Localization (English and Korean)
* Change fonts.
* Add Settings scene.
* Improve Tutorial mode.
* Improve UI/UX.
* Add several Training maps.
* Expand the size of the four types of gravity manipulation buttons again.
* Fix some bugs.

### Updates (v1.3.1 -> v1.4.0)

#### Sound

* Add various sound effects
* Add background music for editor scene

#### Considerations for First-time Users

* Add a first-time user scene that allows users to start the tutorial immediately after installing the app.
  * Skip is also available.
* Provide a detailed tutorial tooltip.
* Minor changes in tutorial maps

#### Add New Modes

* Add Custom mode
  * You can play by selecting the map you want.
  * From now, you don't need to enter editor mode to play custom maps.
  * For each map, record the number of attempts until the first time you clear it and keep it stuffed semi-permanently.
* Add Training mode
  * You can practice by type of gimmicks.
  * For each map, record the number of attempts until the first time you clear it and keep it stuffed semi-permanently.
* Add credit scene

#### Giving a Sense of Accomplishment and Motivation

* Reorganize result UI of Tutorial and Adventure mode with some animations and SFXs.
* Add star system
  * If you clear the Tutorial, you will receive three stars.
  * If you clear one of the four Adventure levels, you will receive stars differently depending on the number of lives left.
  * In the mode selection scene and the adventure level selection scene, you can see the highest number of stars acquired at each level. It remains after restarting or updating the app.
* Add series of map system to Adventure mode
  * You can experience various maps than before.
  * It increases the life of a repeat play.
* Add 'God' difficulty to custom mode
  * Maps that require 20 to 50 tries or more for an expert player.
* Add new maps and adjust map balance

#### Improving convenience and user experience

* Expand the size of the four types of gravity manipulation buttons.
* All the letters of iron were changed to bright colors overall.
* In the mode selection scene and the adventure level selection scene, maps are reorganized so that there is no need to press the retry button.
* Reorganize menu UI displayed when game is paused in Tutorial, Adventure, Custom and Training mode.
  * Background volume and sound effects volume can be adjusted from the Pause menu.
  * You can skip the map (make remaining time to zero) from the Pause menu.
* Change the folder name so that map folders appear in ascending order for difficulty in Custom mode.
* Change the image and add animation of the portal.

### Updates (v1.3.0 -> v1.3.1)

#### Common

* In any game play mode,
  * Time pauses when the ball dies or the retry button is pressed. Time starts to go by again when you press any gravity manipulation buttons.
  * The timer UI changes color to pink while the time is paused. Its color turns purple while the time goes by.
  * When a ball escapes, it is shown an animation that moves slowly by gravity.
* Add a new BGM for the game play scenes (Tutorial, Adventure and Test phase in Editor).
* In Adventure mode,
  * Huge scale of map balance patch is done.
    * Easy: 5 maps are replaced, and the time limit of a map is changed.
    * Normal: 7 maps are replaced.
    * Hard: 5 maps are replaced.
    * Insane: 7 maps are replaced.
  * Maps with shutters can also appear in adventure mode.

#### Android

* The continuous integration(CI) targets Android API level to 28. (Android 9.0 'Pie')
  * This is because there are issues related to storage read/write permission when the target API level is 29 or higher.

### Updates (v.1.2.1 -> v1.3.0)

#### Common

* The Shutter has added!
  * Until the ball passes, the shutter is the same as no wall.
  * Once a ball passes, the shutter is treated as a wall.
* Add a BGM for the main scene.
* In Tutorial mode,
  * Two maps are added, including shutters.
* In Editor mode,
  * You can place shutters in your maps.
  * If the folder is empty, show the text "Empty!"
  * Fixed a bug related to the long file name.
  * Fixed a bug related to the scroll bar in the Open or Save UI.
* More type of screen resolution is supported.
  * 9:22 is now supported. (Portrait)
* The continuous integration(CI) was added to automatically build for Android, Windows and macOS.

#### Android

* The target API level is set to 28. (Android 9.0 'Pie')
  * This is because there are issues related to storage read/write permission when the target API level is 29 or higher.

### Updates (v.1.1.0 -> v.1.2.1)

#### Common

* In Tutorial mode,
  * The progress is displayed.
  * You can pause and resume the game.
  * When you leave or complete the game, the results window is displayed.
* In Adventure mode,
  * Huge scale of map balance patch is done.
    * Easy: 5 lives, 10 maps to escape, more easier than before!
    * Normal: 5 lives, 10 maps to escape, a little easier than before.
    * Hard: 7 lives, 10 maps to escape
    * Insane: 10 lives, 10 maps to escape, more harder than before!
  * The remaining life and progress are displayed.
  * You can pause and resume the game.
  * When you leave or complete the game, the results window is displayed.
* Many types of screen resolution are supported.
  * 9:16, 9:18, 9:18.5, 9:19, 9:19.5, 9:20, 9:20.5, 9:21 are supported. (Portrait)
  * 3:4 is not supported.

#### Android

* You can press the Back key to press the Pause button in Tutorial and Adventure mode.

#### Windows

* You can press the Enter key to press the Next button in Tutorial and Adventure mode.
* You can press the Esc key to press the Pause button in Tutorial and Adventure mode.

#### macOS

* You can press the Enter key to press the Next button in Tutorial and Adventure mode.
* You can press the Esc key to press the Pause button in Tutorial and Adventure mode.

### Updates (v.1.0.2 -> v.1.1.0)

#### Common

* Adventure mode is now playable!
  * There are Easy, Normal, Hard, and Insane levels.
  * In adventure mode, the map is randomly rotated or flipped.
  * There are five lives given, but they are not displayed in the UI yet.
* Even if you modify the map file(`.txt`) directly to increase the time limit to more than 30 seconds, the maximum time limit is set to 30 seconds.

### Updates (v.1.0.1 -> v.1.0.2)

#### Common

* The default value for the time limit has increased from 10 seconds to 30 seconds.
* Several maps have been added.

#### Android

* Maps can now be saved on internal storage rather than on the app's internal data.
  * You can share your own map or download other's map!
  * The map files are saved in `GravitaMaze/Maps`.

</details>

---

## How to Play
> Note: this demo version is playable only until December 2023.

### Android

* Click [here](https://github.com/salt26/gravita-maze/releases/tag/v1.6.4)!
* Download `GravitaMaze.v1.6.4.a.zip` on your Android phone, unzip it, and execute `GravitaMaze.apk` to install it.
  * When the `Install unknown app` message appears, choose `Ignore and install`
  * If Google Play Protect shows `Blocked by Play Protect` message, ***DO NOT CLICK OK MESSAGE***. Instead, click `Details -> Install anyway (unsafe)`.
  * `Send app for scanning?` message might appear. You can choose whatever option you want.
  * If you can't download apk file in a browser, grant `Install Unknown Apps` permission to that browser in `Settings - Apps - ... icon - Special Access - Install Unknown Apps`.
  * When you run the app for the first time, you need to allow permission to write to external/internal storage.
    * If you deny permission, the app will not be able to create or load maps.
    * If you checked the `Do not ask me again` option, you must go into the application permission settings and manually turn on the permission for `GravitaMaze`.

### Windows

* Click [here](https://github.com/salt26/gravita-maze/releases/tag/v1.6.4)!
* Download `GravitaMaze.v1.6.4.w.zip` and unzip it.
* Execute `GravitaMaze.exe`.

### macOS

* Click [here](https://github.com/salt26/gravita-maze/releases/tag/v1.6.4)!
* Download `GravitaMaze.v1.6.4.m.zip` and unzip it.
* Execute `GravitaMaze.app`.

## Space-cube builder

* You can enter Create mode to make your own custom space-cubes!

## Play Modes

* There are Tutorial, Challenge, Explore and Create modes.
* The gravity can be manipulated by touching the four "arrow" buttons or pressing arrow keys.
* Whenever the capsule has destroyed, you can touch "retry" button or press space key to retry the map.
  * Time stops when the capsule has destroyed.
  * Time passes after you manipulate gravity.
* Make the capsule escape from the space-cube before the time limit is over.
  * If you time out, you lose one life.

**For more information, please check out our [blog](https://blog.naver.com/ravitastudio) or [YouTube](https://youtube.com/@Ravita_Studio).**

![Screenshot1](./Figures/Screenshot1_v1.6.4_9-16.png)

![Screenshot2](./Figures/Screenshot2_v1.6.4_9-16.png)

![Screenshot3](./Figures/Screenshot5_v1.6.4_9-16.png)
