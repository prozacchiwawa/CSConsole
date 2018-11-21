/*
  C# Console Replacement ... C# example of doing various windowsy things
  Copyright (C) 2006 Art Yerkes
  
  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.
  
  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  
  You should have received a copy of the GNU General Public License along
  with this program; if not, write to the Free Software Foundation, Inc.,
  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using System.Diagnostics;

class CSConsole {
    Console console;
    ConWindow conwindow;
    Settings settings;

    public CSConsole() {
	settings = Settings.Load();
	console = new Console();
	conwindow = new ConWindow( console );
	if( settings == null ) {
	    settings = new Settings();
	    settings.ConsoleSettings = console.settings;
	    settings.WindowSettings = conwindow.settings;
	    settings.WriteDefault();
	} else { 
	    console.settings = settings.ConsoleSettings;
	    conwindow.settings = settings.WindowSettings;
	}
    }

    public void go() {
	conwindow.go();
    }

    public static void Main( string []args ) {
	CSConsole c = new CSConsole();
	Process p = new Process();

	if( args.Length > 0 ) {
	    p.StartInfo.FileName = args[0];
	} else {
	    p.StartInfo.FileName = "cmd.exe";
	}
	p.StartInfo.UseShellExecute = false;
	if( args.Length == 2 ) {
	    p.StartInfo.Arguments = args[1];
	}
	p.Start();
	c.go();
    }
};
