# Python Console Mod

Python Console mod allows you to programatically interact with Cities:Skylines game engine through an easy-to-learn python interface. User documentation can be found on [wiki](https://github.com/Strdate/PythonConsole/wiki). See the mod in [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2415465635).

## Developer isntructions

After first build:

- Open build folder `PythonConsole\SkylinesRemotePythonDotnet\bin\Debug`
- Open mod target folder `C:\Users\<Username>\AppData\Local\Colossal Order\Cities_Skylines\Addons\Mods\PythonConsole`
- Create a zip file `SkylinesRemotePython.zip` containing the contents of the build folder and move it to the mod target folder
  - For next builds you might copy only the `SkylinesRemotePythonDotnet.exe` to the zip file (if it has changed)
- Download [PyPy 2.7](https://www.pypy.org/download.html), navigate to `lib-python\2.7`, copy the content to a new folder `pypy` and put it to `SkylinesRemotePython.zip`
- Open example scripts folder `PythonConsole\PythonConsole\Resource\Examples`
- Create a zip file `ExamplePythonScripts.zip` containing the contents of the folder and move it to the mod target folder
  - You need to repeat this only if you change any of the example script files
- The mod target folder should now contain these 4 files: `ExamplePythonScripts.zip`, `PythonConsole.dll`, `SkylinesPythonShared.dll`, `SkylinesRemotePython.zip`
- You can skip some of these steps by copying the files directly from Steam Worksop release of the mod

### How does this mod work?

The mod consists of 3 different parts:

 - The Cities:Skylines mod itself (`PythonConsole.dll`)
 - External application that runs the [IronPython](https://ironpython.net/) engine
   - Unfortunately the engine cannot run directly inside the game, as it requires dotnet framework 4.0 or higher (C:S runs on mono version 3.5)
 - Shared library that contains messaging protocol and shared logic

How does it all work together?

 - The external application is shipped in a zip file with the mod
 - After the mod loads, the application is unzipped in `Cities_Skylines\SkylinesRemotePython` folder and launched
 - The application creates a tcp server to which the mod then connects

### How to develop the mod locally?

 - You can run and debug the extarnal Python application directly from Visual Studio
 - Go to mod settings in game and enable `Debug: Do not launch remote python console server` option
   - This will prevent the mod from launching its own instance of Python server
 - Now you can start the server in VS and the game will automatically connect to it after you open the Python Console dialog window
 - If you want to debug the `PythonConsole.dll` itself (within Cities:Skylines application domain), you need to attach a [dnSpy debugger](https://github.com/CitiesSkylinesMods/TMPE/wiki/Attaching-Debugger-to-Cities-Skylines) to the game

### How to expose a new API to the Python engine?

 - Define a new contract in [SkylinesPythonShared/Protocol/Contracts.cs](SkylinesPythonShared/Protocol/Contracts.cs)
 - Implement the API in [PythonConsole/PythonAPI/GameAPI.cs](PythonConsole/PythonAPI/GameAPI.cs)
   - Create a new static fuction with the same name as the contract
   - The contract will be automatically linked to its implementation
 - Expose the API in [SkylinesRemotePythonDotnet/PythonAPI/GameAPI.cs](https://github.com/Strdate/PythonConsole/blob/master/SkylinesRemotePythonDotnet/PythonAPI/GameAPI.cs) or wherever else you need

### Credits

This mod contains source code from [Move It](https://github.com/Quboid/CS-MoveIt) and [ModTools](https://github.com/bloodypenguin/Skylines-ModTools)
