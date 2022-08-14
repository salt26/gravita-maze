# GravitaMaze (중력 미로)
[![Version badge](https://img.shields.io/badge/Version-1.3.1-purple.svg)](https://github.com/salt26/gravita-maze/releases/tag/v1.3.1)  
[![Licence](https://img.shields.io/badge/License-MIT-green.svg)](./LICENSE)  
![Unity](https://img.shields.io/badge/unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white)  
![GitHub Actions](https://img.shields.io/badge/github%20actions-%232671E5.svg?style=for-the-badge&logo=githubactions&logoColor=white)

## Introduction
* Puzzle game
  * Manipulate gravity to escape the ball!
* English is fully available!
* This version(v1.3.1) is also a prototype.

### Download
#### [v1.3.1 for Android, Windows, and macOS](https://github.com/salt26/gravita-maze/releases/tag/v1.3.1)

### Updates (v1.3.0 -> v1.3.1)
#### Common
* In any game play mode,
  * Time pauses when the ball dies or the retry button is pressed. Time starts to go by again when you press any gravity manipulation buttons.
  * The timer UI changes color to pink while the time is paused. Its color turns purple while the time goes by.
  * When a ball escapes, it is shown an animation that moves slowly by gravity.
* Add a new BGM for the game play scenes(Tutorial, Adventure and Test phase in Editor). 
* In Adventure mode,
  * Huge scale of map balance patch is done.
    * Easy: 5 maps are replaced, and the time limit of a map is changed.
    * Normal: 7 maps are replaced.
    * Hard: 5 maps are replaced.
    * Insane: 7 maps are replaced.
  * Maps with shutters can also appear in adventure mode.
* You can enjoy some new maps (including shutters) in the editor that are not in adventure mode.
  * If you are using Android, please download `GravitaMaze.zip` and unzip it in root directory(`Internal storage`) using "My Files" app.
  * If you are using macOS, please  download `GravitaMaze.zip`, unzip it, and move `Maps` folder to the root directory of `GravitaMaze.app`.

#### Android
* The continuous integration(CI) targets Android API level to 28. (Android 9.0 'Pie')
  * This is because there are issues related to storage read/write permission when the target API level is 29 or higher.

<details>
<summary>Click here to expand or collapse the old update logs!</summary>

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
* You can enjoy some maps (including shutters) in the editor that are not in adventure mode.
  * If you are using Android, please download `GravitaMaze.zip` and unzip it in root directory(`Internal storage`) using "My Files" app.
  * If you are using macOS, please  download `GravitaMaze.zip`, unzip it, and move `Maps` folder to the root directory of `GravitaMaze.app`.
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
* You can enjoy some maps in the editor that are not in adventure mode.
  * If you are using Android, please download `GravitaMaze.zip` and unzip it in root directory(`Internal storage`) using "My Files" app.
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
#### Android
* Click [here](https://github.com/salt26/gravita-maze/releases/tag/v1.3.1)!
* Download `GravitaMaze.v1.3.1.a.zip` on your Android cell phone, unzip it, and execute `GravitaMaze.apk` to install it.
  * You may need to allow "Install unknown apps" (in "Settings" - "Apps" - "..." - "Special access" - "Install unknown apps")
  * When you run the app for the first time, you need to allow permission to write to external/internal storage.
    * If you deny permission, the app will not be able to create or load maps.
    * If you checked the “Do not ask me again” option, you must go into the application permission settings and manually turn on the permission for `GravitaMaze`.
* You can download `GravitaMaze.zip` and unzip it in root directory(`Internal storage`) using "My Files" app to enjoy the custom maps.
  * These maps are playable in the editor, not in Adventure mode.
* The gravity can be manipulated by touching the four "arrow" buttons.
* Whenever the ball dies, you can touch "retry" button to retry the map.
  * Time passes even if the ball is dead.
* Escape the ball before the time limit is over.
  * If you time out, you lose one life.

#### Windows
* Click [here](https://github.com/salt26/gravita-maze/releases/tag/v1.3.1)!
* Download `GravitaMaze.v1.3.1.w.zip` and unzip it.
* Execute `GravitaMaze.exe`.
* You don't need to download `GravitaMaze.zip` since the file is already included in `GravitaMaze.v1.3.1.w.zip`.
* The gravity can be manipulated by pressing the four "arrow" buttons.
* Whenever the ball dies, you can press "retry"(space) button to retry the map.
  * Time passes even if the ball is dead.
* Escape the ball before the time limit is over.
  * If you time out, you lose one life.

#### macOS
* Click [here](https://github.com/salt26/gravita-maze/releases/tag/v1.3.1)!
* Download `GravitaMaze.v1.3.1.m.zip` and unzip it.
* Execute `GravitaMaze.app`.
* You can download `GravitaMaze.zip`, unzip it, and move the `Maps` folder to the root directory of `GravitaMaze.app` to enjoy the custom maps.
* The gravity can be manipulated by pressing the four "arrow" buttons.
* Whenever the ball dies, you can press "retry"(space) button to retry the map.
  * Time passes even if the ball is dead.
* Escape the ball before the time limit is over.
  * If you time out, you lose one life.

### Map Editor
* You can use the map editor to make your own custom maps!
  * Also you can test and play the custom maps in the map editor.

### Play Modes
* There are a Tutorial mode and four Adventure modes.
* Other modes like Custom and Training will be available in the next version. (Coming soon!)

![Screenshot1](./Figures/Screenshot1.v1.3.1.png)

![Screenshot2](./Figures/Screenshot2.v1.3.1.png)
