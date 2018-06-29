Zenject:
 
using git:
 
repo: https://github.com/dertom95/Zenject.git
branch: uGameFramework
 
Build-DLLs: 
- open NonUnityBuild/Zenject.sln in VS
- BUILD-Solution (debug or release)
- copy Bin/[Debug/Release]/Zenject.dll and Zenject-usage.dll to this folder

Alternatively you can use the Sources:
- still you need to copy Zenject-usage.dll (see Build-DLLs)
- copy Zenject/UnityProject/Assets/Plugins/Zenject/Source to this folder

Only use Source-Folder OR DLL!! 
- e.g. rename Zenject.dll to Zenject._dll
- e.g. zip Source-Folder 
- e.g. delete the one that is not needed
