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
using System.Diagnostics;
using System.Drawing;
using System.Data;
using Microsoft.Win32;

class Wallpaper {
    public enum Style : int { Tiled, Centered, Stretched };
    public static string WallpaperBmp {
	get {
	    try {
		RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
		return (string)key.GetValue(@"Wallpaper");
	    } catch( Exception e ) { return ""; }
	}
    }

    public static Style WallpaperStyle {
	get {
	    RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
	    try {
		return key.GetValue(@"TileWallpaper").Equals("1") ?
		    Style.Tiled : 
		    key.GetValue(@"WallpaperStyle").Equals("1") ?
		    Style.Centered : Style.Stretched;
	    } catch( Exception e ) { return Style.Stretched; }
	}
    }

    public static Point ScreenSize {
	get {
	    return new Point
		(win32.GetSystemMetrics(0), win32.GetSystemMetrics(1));
	}
    }

    public Wallpaper() { }
    public void sizemove( Rectangle r ) { this.clientArea = r; }

    public Bitmap getBackground() {
	if( WallpaperBmp == "" ) return new Bitmap(1,1);
	Bitmap wpBitmap = new Bitmap( WallpaperBmp );
	Bitmap outBitmap = new Bitmap(clientArea.Width, clientArea.Height);
	Style style = WallpaperStyle;
	Graphics g = Graphics.FromImage(outBitmap);
	Point screenSize = ScreenSize;

	if( style == Style.Stretched ) {
	    g.DrawImage
		(wpBitmap, 
		 new Rectangle
		 (-clientArea.Left,-clientArea.Top,screenSize.X,screenSize.Y));
	} else if( style == Style.Tiled ) {
	    /* Not supported yet */
	} else if( style == Style.Centered ) {
	    g.DrawImage
		(wpBitmap,
		 new Point
		 (-clientArea.Left+screenSize.X/2-clientArea.Width/2,
		  -clientArea.Top +screenSize.Y/2-clientArea.Height/2));
	}

	return outBitmap;
    }

    Rectangle clientArea;
};
