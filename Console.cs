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
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Console {
    public int cursorPercentage {
	get {
	    return CursorInfo.dwSize;
	}
    }

    public bool cursorVisible {
	get {
	    return CursorInfo.bVisible;
	}
    }

    public string title {
	get {
	    return System.Console.Title;
	}
    }

    public int viewWidth {
	get {
	    return ConsoleBuffer.srWindow.Right-ConsoleBuffer.srWindow.Left+1;
	}
    }

    public int viewHeight {
	get {
	    return ConsoleBuffer.srWindow.Bottom-ConsoleBuffer.srWindow.Top+1;
	}
    }

    public int viewX {
	get {
	    return ConsoleBuffer.srWindow.Left;
	}
    } 

    public int viewY {
	get {
	    return ConsoleBuffer.srWindow.Top;
	}
    }

    public int bufferWidth {
	get {
	    return ConsoleBuffer.dwSize.X;
	}
    }

    public int bufferHeight {
	get {
	    return ConsoleBuffer.dwSize.Y;
	}
    }

    public int cursorX {
	get {
	    return newCursorX;
	}
    }

    public int cursorY {
	get {
	    return newCursorY;
	}
    }

    public int NumColors {
	get {
	    return 16;
	}
    }

    public int cursorTime {
	get {
	    return Settings.cursorBlinkRate;
	}
    }

    public int cursorFore {
	get {
	    return Settings.cursorFore;
	}
    }

    public int cursorBack {
	get {
	    return Settings.cursorBack;
	}
    }

    public int drawAlpha {
	get {
	    return Settings.drawAlpha;
	}
    }

    public object settings {
	get {
	    return Settings;
	}
	set {
	    Settings = (ConsoleSettings)value;
	}
    }

    public unsafe Console() {
	win32.AllocConsole();
	InHandle = win32.GetStdHandle( win32.STD_INPUT_HANDLE );
	OutHandle = win32.GetStdHandle( win32.STD_OUTPUT_HANDLE );
	WinHandle = win32.GetConsoleWindow();
	win32.SetWindowLong( WinHandle, -20, 
			     win32.GetWindowLong( WinHandle, -20 ) | 0x80000 );
	win32.SetLayeredWindowAttributes( WinHandle, 0, 0x80, 3 );

	win32.GetConsoleScreenBufferInfo( OutHandle, ref ConsoleBuffer );

	ScreenData = new win32.CHAR_INFO[viewHeight * viewWidth];
	OldScreenData = new win32.CHAR_INFO[viewHeight * viewWidth];
	ScreenDataMem = 
	    Marshal.AllocHGlobal(viewHeight*viewWidth*sizeof(win32.CHAR_INFO));

	/* Disable Ctrl-C */
	win32.SetConsoleCtrlHandler( 0, true );

	win32.ShowWindow( WinHandle, 2 );
	win32.SetWindowLong
	    ( WinHandle, -20, 
	      (win32.GetWindowLong( WinHandle, -20 ) | 128) & ~0x40000 );

	TaskBarTypeLib.TaskbarListClass tlbc = new TaskBarTypeLib.TaskbarListClass();
	tlbc.HrInit();
	tlbc.DeleteTab(WinHandle);

	Settings = new ConsoleSettings();

	refresh();
    }

    public unsafe bool refresh() {
	int i;
	int vx = viewX,vy = viewY,vw = viewWidth,vh = viewHeight,
	    bw = bufferWidth,bh = bufferHeight;

	win32.GetConsoleScreenBufferInfo( OutHandle, ref ConsoleBuffer );
	win32.GetConsoleCursorInfo( InHandle, ref CursorInfo );

	newCursorX = ConsoleBuffer.dwCursorPosition.X - viewX;
	newCursorY = ConsoleBuffer.dwCursorPosition.Y - viewY;

	bool changed = bw != bufferWidth || bh != bufferHeight ||
	    vx != viewX || vy != viewY || vw != viewWidth || vh != viewHeight;

	if( vw != viewWidth || vh != viewHeight ) {
	    ScreenData = new win32.CHAR_INFO[viewHeight * viewWidth];
	    OldScreenData = new win32.CHAR_INFO[viewHeight * viewWidth];
	    ScreenDataMem = 
		Marshal.AllocHGlobal(viewHeight*viewWidth*sizeof(win32.CHAR_INFO));	    
	}

	win32.COORD where = new win32.COORD(0,0);
	win32.SMALL_RECT what = 
	    new win32.SMALL_RECT
	    ((short)viewX,(short)viewY,
	     (short)(viewX+viewWidth-1),(short)(viewY+viewHeight-1));

	win32.ReadConsoleOutput
	    ( OutHandle, 
	      ScreenDataMem,
	      (viewHeight << 16) | viewWidth,
	      0,
	      ref what );

	for( i = 0; i < ScreenData.Length; i++ ) {
	    ScreenData[i] = (win32.CHAR_INFO)Marshal.PtrToStructure
		( new IntPtr(ScreenDataMem.ToInt32() + (i * 4)), 
		  typeof(win32.CHAR_INFO) );
	}

	return changed;
    }

    public List<Point> damageList() {
	int row;
	List<Point> pts = new List<Point>();

	for( int i = 0; i < viewHeight; i++ ) {
	    row = i * viewWidth;
	    for( int j = 0; j < viewWidth; j++ ) {
		if( !ScreenData[row + j].Equals( OldScreenData[row + j] ) )
		    pts.Add( new Point(j,i) );
	    }
	}
	
	if( pts.Length > 0 || !cursorState || 
	    oldCursorX != newCursorX ||
	    oldCursorY != newCursorY ) {
	    if( oldCursorX != newCursorX || oldCursorY != newCursorY ) {
		pts.Add( new Point(oldCursorX, oldCursorY) );
	    }
	    
	    if( --cursorCount < 1 ) {
		cursorCount = cursorTime;
		cursorState = !cursorState;
	    }
	    pts.Add( new Point(newCursorX, newCursorY) );
	    
	    oldCursorX = newCursorX; oldCursorY = newCursorY;

	    for( int i = 0; i < OldScreenData.Length; i++ )
		OldScreenData[i] = ScreenData[i];
	}

	return pts;
    }

    public int getBackColor( int x, int y ) {
	if( y >= 0 && y < viewHeight && x >= 0 && x < viewWidth ) {
	    return (x == cursorX && y == cursorY && cursorState) ?
		cursorBack :
		(ScreenData[y * bufferWidth + x].Attributes >> 4) & 0xf;
	} else return 0;
    }

    public int getForeColor( int x, int y ) {
	if( y >= 0 && y < viewHeight && x >= 0 && x < viewWidth ) {
	    return (x == cursorX && y == cursorY && cursorState) ?
		cursorFore :
		ScreenData[y * bufferWidth + x].Attributes & 0xf;
	} else return 0;
    }

    public char getCharacter( int x, int y ) {
	if( y >= 0 && y < viewHeight && x >= 0 && x < viewWidth ) {
	    return ScreenData[y * bufferWidth + x].UnicodeChar;
	} else return '\0';
    }

    public int getBackColor( int c ) {
	return (int)((drawAlpha << 24) | Settings.colorTable[c]);
    }

    public int getForeColor( int c ) {
	return (int)(0xff000000 | Settings.colorTable[c]);
    }

    public void destroy() {
	win32.SendMessage( WinHandle, 16, 0, 0 );
    }

    public void scrollTo( float percentage ) {
	win32.SMALL_RECT r = ConsoleBuffer.srWindow;
	int newTop = (int)((bufferHeight - viewHeight) * percentage);
	r.Top = (short)newTop; r.Bottom = (short)(newTop + viewHeight - 1);
	win32.SetConsoleWindowInfo( OutHandle, true, ref r );
	refresh();
    }

    public unsafe void writeKey( bool down, short vk, int shift, char c ) {
	if( c != '\0' ) {
	    win32.SendMessage( WinHandle, 258, (int)c, 0 );
	} else {
	    win32.SendMessage( WinHandle, 256 + (!down ? 1 : 0), 
			       vk, 
			       1 | (win32.MapVirtualKey( vk, 0 ) << 16) |
			       ((down ? 0 : 1) << 30) | 
			       ((down ? 1 : 0) << 31) );
			       
	}
    }

    public class ConsoleSettings {
	public int cursorBlinkRate;
	public int []colorTable;
	public int cursorFore, cursorBack;
	public int drawAlpha;

	public ConsoleSettings() {
	    cursorFore = 0;
	    cursorBack = 10;
	    cursorBlinkRate = 5;
	    drawAlpha = 0xa0;
	    
	    colorTable = new int[] { 0x000000, 0x0000b0, 0x00b000, 0x00b0b0, 
				     0xb00000, 0xb000b0, 0xb0b000, 0xb0b0b0,
				     0x404040, 0x4040ff, 0x40ff40, 0x40ffff,
				     0xff4040, 0xff40ff, 0xffff40, 0xffffff };
	}
    }

    bool cursorState;
    int newCursorX, newCursorY, cursorCount;
    int InHandle, OutHandle, WinHandle, oldCursorX, oldCursorY;
    IntPtr ScreenDataMem;
    win32.CONSOLE_CURSOR_INFO CursorInfo;
    win32.CONSOLE_SCREEN_BUFFER_INFO ConsoleBuffer;
    win32.CHAR_INFO []ScreenData, OldScreenData;
    ConsoleSettings Settings;
};
