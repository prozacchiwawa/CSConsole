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

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

[System.Xml.Serialization.XmlInclude(typeof(Console.ConsoleSettings)),
 System.Xml.Serialization.XmlInclude(typeof(ConWindow.WindowSettings))]
public class Settings {
    public object ConsoleSettings;
    public object WindowSettings;

    public static string settingsFile {
	get {
	    return 
		Environment.GetFolderPath
		(Environment.SpecialFolder.ApplicationData) + 
		"\\CSConSettings.xml";
	}
    }

    public static Settings Load() {
	XmlSerializer s = new XmlSerializer( typeof( Settings ) );
	TextReader r;

	try {
	    r = new StreamReader(settingsFile);

	    return (Settings)s.Deserialize( r );
	} catch( FileNotFoundException e ) { 
	    return null; 
	} catch( InvalidOperationException e ) {
	    return null;
	}
    }

    public void WriteDefault() {
	XmlSerializer s = new XmlSerializer( typeof( Settings ) );
	TextWriter w;

	if( !File.Exists(settingsFile) ) {
	    w = new StreamWriter( settingsFile );
	    s.Serialize( w, this );
	    w.Close();
	}
    }
};
