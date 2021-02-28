# Python Console Mod

Python Console mod allows you to programatically interact with Cities:Skylines game engine through an easy-to-learn python interface.

## Developer isntructions

After first build:

- Open build folder `PythonConsole\SkylinesRemotePythonDotnet\bin\Debug`
- Open mod target folder `C:\Users\<Username>\AppData\Local\Colossal Order\Cities_Skylines\Addons\Mods\PythonConsole`
- Create a zip file `SkylinesRemotePython.zip` containing the contents of the build folder and move it to the mod target folder
  - For next builds you might copy only the `SkylinesRemotePythonDotnet.exe` to the zip file (if it has changed)
- Open example scripts folder `PythonConsole\PythonConsole\Resource\Examples`
- Create a zip file `ExamplePythonScripts.zip` containing the contents of the folder and move it to the mod target folder
  - You need to repeat this only if you change any of the example script files
- The mod target folder should now contain these 4 files: `ExamplePythonScripts.zip`, `PythonConsole.dll`, `SkylinesPythonShared.dll`, `SkylinesRemotePython.zip`

### How does this mod work?

The mod consists of 3 different parts:

 - The Cities:Skylines mod itself
 - External application that runs the [IronPython](https://ironpython.net/) engine
   - Unfortunately the engine cannot run directly inside the game, as it requires dotnet framework 4.0 or higher (C:S runs on version 3.5)
 - Shared library that contains messaging protocol and shared logic
