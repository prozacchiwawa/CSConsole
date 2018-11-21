CSCONSOLE_MODULES=List win32 Console Settings Wallpaper ConWindow
CONSOLE_MODULES=List win32
CONWINDOW_MODULES=List win32 Console Wallpaper
SETTINGS_MODULES=Console ConWindow

.SUFFIXES: .cs .netmodule

.cs.netmodule:
	csc /debug+ /unsafe /target:module $<

all: CSConsole.exe

TaskBarTypeLib.dll: ITaskBarList.tlb
	tlbimp ITaskBarList.tlb /transform:dispret /out:TaskBarTypeLib /strictref

ITaskBarList.tlb: ITaskBarList.idl
	midl ITaskBarList.idl

Console.netmodule: TaskBarTypeLib.dll $(CONSOLE_MODULES:%=%.netmodule) Console.cs
	csc /debug+ /d:TRACE /target:module /unsafe $(CONSOLE_MODULES:%=/addmodule:%.netmodule) Console.cs /r:TaskBarTypeLib.dll 

ConWindow.netmodule: $(CONWINDOW_MODULES:%=%.netmodule) ConWindow.cs
	csc /debug+ /d:TRACE /target:module /unsafe $(CONWINDOW_MODULES:%=/addmodule:%.netmodule) ConWindow.cs

Settings.netmodule: $(SETTINGS_MODULES:%=%.netmodule) Settings.cs
	csc /debug+ /d:TRACE /target:module $(SETTINGS_MODULES:%=/addmodule:%.netmodule) Settings.cs

Wallpaper.netmodule: win32.netmodule Wallpaper.cs
	csc /debug+ /d:TRACE /target:module /addmodule:win32.netmodule Wallpaper.cs

CSConsole.exe: CSConsole.cs $(CSCONSOLE_MODULES:%=%.netmodule)
	csc /debug+ /d:TRACE /target:winexe /unsafe $(CSCONSOLE_MODULES:%=/addmodule:%.netmodule) CSConsole.cs

clean:
	del *.exe *.netmodule *.dll