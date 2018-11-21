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
using System.Runtime.InteropServices;

class win32 {
    public const int STD_INPUT_HANDLE  = -10;
    public const int STD_OUTPUT_HANDLE = -11;
    public const int STD_ERROR_HANDLE  = -12;

    public const int KEY_EVENT                = 0x0001;
    public const int MOUSE_EVENT              = 0x0002;
    public const int WINDOW_BUFFER_SIZE_EVENT = 0x0004;
    public const int MENU_EVENT               = 0x0008;
    public const int FOCUS_EVENT              = 0x0010;

    /* 	The right ALT key is pressed. */
    public const int RIGHT_ALT_PRESSED = 0x0001; 
    /* 	The left ALT key is pressed. */
    public const int LEFT_ALT_PRESSED = 0x0002; 
    /* 	The right CTRL key is pressed. */
    public const int RIGHT_CTRL_PRESSED = 0x0004; 
    /* 	The left CTRL key is pressed. */
    public const int LEFT_CTRL_PRESSED = 0x0008; 
    /* 	The SHIFT key is pressed. */
    public const int SHIFT_PRESSED = 0x0010; 
    /* 	The NUM LOCK light is on. */
    public const int NUMLOCK_ON = 0x0020; 
    /* 	The SCROLL LOCK light is on. */
    public const int SCROLLLOCK_ON = 0x0040; 
    /* 	The CAPS LOCK light is on. */
    public const int CAPSLOCK_ON = 0x0080; 
    /* 	The key is enhanced. */
    public const int ENHANCED_KEY = 0x0100; 
    
    [StructLayout(LayoutKind.Sequential)]
	public struct CONSOLE_CURSOR_INFO {
	public int dwSize;
	public bool bVisible;
    };

    [StructLayout(LayoutKind.Sequential)]
	public struct SMALL_RECT {
	public short Left, Top, Right, Bottom;
	public SMALL_RECT( short l, short t, short r, short b ) {
	    Left = l; Top = t; Right = r; Bottom = b;
	}
    };

    [StructLayout(LayoutKind.Sequential)]
	public struct COORD {
	public short X, Y;
	public COORD( short x, short y ) { X = x; Y = y; }
    };

    [StructLayout(LayoutKind.Sequential)]
	public struct CONSOLE_SCREEN_BUFFER_INFO {
	public COORD dwSize;
	public COORD dwCursorPosition;
	public short wAttributes;
	public SMALL_RECT srWindow;
	public COORD dwMaximumWindowSize;
    };

    [StructLayout(LayoutKind.Sequential)]
	public struct CHAR_INFO {
	public char UnicodeChar;
	public short Attributes;
    };

    [StructLayout(LayoutKind.Sequential)]
	public struct KEY_EVENT_RECORD {
	public bool bKeyDown;
	public short wRepeatCount;
	public short wVirtualKeyCode;
	public short wVirtualScanCode;
	public char UnicodeChar;
	public int dwControlKeyState;
    };
    
    [StructLayout(LayoutKind.Sequential)]
	public struct MOUSE_EVENT_RECORD {
	public COORD dwMousePosition;
	public int   dwButtonState;
	public int   dwControlKeyState;
	public int   dwEventFlags;
    };

    [StructLayout(LayoutKind.Sequential)]
	public struct FOCUS_EVENT_RECORD {
	public bool bSetFocus;
    };

    [StructLayout(LayoutKind.Sequential)]
	public struct MENU_EVENT_RECORD {
	public bool dwCommandId;
    };

    [StructLayout(LayoutKind.Sequential)]
	public struct INPUT_RECORD {
	public short EventType;
	[StructLayout(LayoutKind.Explicit)]
	    public struct INPUT_RECORD_DATA {
	    [FieldOffset(0)] public KEY_EVENT_RECORD KeyEvent;
	    [FieldOffset(0)] public MOUSE_EVENT_RECORD MouseEvent;
	    [FieldOffset(0)] public MENU_EVENT_RECORD MenuEvent;
	    [FieldOffset(0)] public FOCUS_EVENT_RECORD FocusEvent;
	};
	public INPUT_RECORD_DATA Event;
    };

    [DllImport("kernel32.dll")]
	public extern static int GetStdHandle( int which );

    [DllImport("kernel32.dll")]
	public extern static bool GetConsoleCursorInfo
	( int conOut, ref CONSOLE_CURSOR_INFO Info );

    [DllImport("kernel32.dll")]
	public extern static int GetConsoleWindow();

    [DllImport("kernel32.dll")]
	public extern static bool WriteConsoleInput
	( int conIn, ref INPUT_RECORD data,
	  int length, out int written );

    [DllImport("kernel32.dll")]
	public extern static bool ReadConsoleOutput
	( int conOut, IntPtr BufPtr,
	  int bufsize, int bufcoord, 
	  ref SMALL_RECT read );

    [DllImport("kernel32.dll")]
	public extern static IntPtr LocalAlloc(int flags, int size);

    [DllImport("kernel32.dll")]
	public extern static void LocalFree( IntPtr ptr );

    [DllImport("kernel32.dll")]
	public extern static bool AllocConsole();

    [DllImport("kernel32.dll")]
	public extern static bool GetConsoleScreenBufferInfo
	( int conOut, ref CONSOLE_SCREEN_BUFFER_INFO info );

    [DllImport("kernel32.dll")]
	public extern static bool SetConsoleWindowInfo
	( int conOut, bool abs, ref SMALL_RECT window );

    [DllImport("kernel32.dll")]
	public extern static bool SetConsoleCtrlHandler
	( int routine, bool add );

    [DllImport("user32.dll")]
	public extern static bool ShowWindow( int WinHandle, int Show );

    [DllImport("user32.dll")]
	public extern static bool SetWindowLong( int winHandle, int what, int toWhat );
    
    [DllImport("user32.dll")]
	public extern static int GetWindowLong( int winHandle, int what );
    
    [DllImport("user32.dll")]
	public extern static bool SetLayeredWindowAttributes( int winHandle, int ckey, int alpha, int what );

    [DllImport("user32.dll")]
	public extern static int SendMessage( int winHandle, int msg, int wparam, int lparam );
    
    [DllImport("user32.dll")]
	public extern static int MapVirtualKey( int vk, int maptype );

    [DllImport("user32.dll")]
	public extern static int GetSystemMetrics( int sm );
};
