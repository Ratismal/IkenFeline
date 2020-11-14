rm -rf release
mkdir release
cd release

REM IkenFeline
cp ../IkenFeline/bin/Debug/IkenFeline.dll IkenFeline.dll
REM cp ../LittleWitchWin.IkenFeline.mm/bin/Debug/Ikenfell.IkenFeline.mm.dll IkenfellWin.IkenFeline.mm.dll
REM cp IkenfellWin.IkenFeline.mm.dll IkenfellLinux.IkenFeline.mm.dll
cp ../game/IkenFelineWin.exe .
cp ../ikenfeline-uninstall.bat .
cp ../ikenfeline-install.bat .


REM MonoMod
cp ../lib/monomod/*.dll .
cp ../lib/monomod/*.exe .

cd ../game
MonoMod.RuntimeDetour.HookGen.exe --private LittleWitch.dll
MonoMod.RuntimeDetour.HookGen.exe --private GameEngine.dll
cp MMHOOK_LittleWitch.dll ../release
cp MMHOOK_GameEngine.dll ../release

cd ../release

mkdir IkenFeline
mv *.* IkenFeline

REM TestMod
mkdir TestMod
cd TestMod
mkdir TestMod
cd ..
cp ../game/IkenFeline/mods/TestMod/LittleWitch.TestMod.mm.dll TestMod/TestMod
cp ../game/IkenFeline/mods/TestMod/manifest.yaml TestMod/TestMod


cd IkenFeline
tar -czvf IkenFeline.tar.gz *
mv IkenFeline.tar.gz ..

cd ../TestMod
tar -czvf TestMod.tar.gz *
mv TestMod.tar.gz ..

cd ../..
