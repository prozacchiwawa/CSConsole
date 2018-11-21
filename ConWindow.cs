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
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Windows.Forms;

public class ConWindow : Form {
    Console con;
    Font conFont;
    Bitmap dblBuffer, wallBuffer;
    Brush []foreBrushes;
    Brush []backBrushes;
    Graphics dblGraphic;
    Timer repaint;
    Point textSize;
    Wallpaper wallpaper;
    bool started, closed;
    WindowSettings Settings;
    RectangleF scrollArea, scrollPos;

    public class WindowSettings {
	public string fontName;
	public int fontSize;
	public int widthAdj;

	public WindowSettings() {
	    fontName = "Lucida Console";
	    fontSize = 12;
	    widthAdj = -2;
	}
    }

    public string fontName {
	get {
	    return Settings.fontName;
	}
    }

    public int fontSize {
	get {
	    return Settings.fontSize;
	}
    }

    public int widthAdj {
	get {
	    return Settings.widthAdj;
	}
    }

    private void sizeText() {
	Bitmap testBitmap = new Bitmap(1,1);
	Graphics testGraphics = Graphics.FromImage(testBitmap);
	conFont = new System.Drawing.Font(fontName, fontSize);
	textSize = MeasureDisplayStringWidth(testGraphics,"X",conFont);
	scrollArea = 
	    new RectangleF
	    ( (float)(con.viewWidth * textSize.X), (float)0,
	      (float)textSize.X, (float)(con.viewHeight * textSize.Y) );
    }

    public object settings {
	get {
	    return Settings;
	} 
	set {
	    Settings = (WindowSettings)value;
	    sizeText();
	    changeSize();
	    redraw(null);
	}
    }

    private void OnPaint( object Sender, PaintEventArgs e ) {
	Graphics g = e.Graphics;
	g.DrawImage( dblBuffer,0,0 );
	/* Scroll bar */
	g.DrawImage( wallBuffer, scrollArea, scrollArea, GraphicsUnit.Pixel );
	g.FillRectangle( backBrushes[con.cursorBack], scrollPos );

    }

    private void changeSize() {
	/* Make room for a simple scroll bar */
	ClientSize = new System.Drawing.Size
	    (textSize.X * (con.viewWidth + 1) + win32.GetSystemMetrics(32), 
	     textSize.Y * con.viewHeight + win32.GetSystemMetrics(33));
	dblBuffer = new Bitmap
	    (textSize.X * con.viewWidth, textSize.Y * con.viewHeight);
	dblGraphic = Graphics.FromImage(dblBuffer);
	if( !started ) {
	    repaint.Start();
	    Paint += new PaintEventHandler(OnPaint);
	    started = true;
	}
    }

    private void OnMoveSize( object sender, System.EventArgs e ) {
	changeSize();
	wallpaper.sizemove
	    (RectangleToScreen
	     (new Rectangle
	      (0,0,(con.viewWidth+1)*textSize.X,con.viewHeight*textSize.Y)));
	wallBuffer = wallpaper.getBackground();	
	redraw(null);
    }

    protected override bool IsInputKey( Keys data ) { return true; }

    private void redraw( List<Point> damage ) {
	Graphics g = CreateGraphics();
	Rectangle r;
	string single = "";
    
	if( wallBuffer == null ) return;

	if( damage != null ) 
	    for( List<Point>.Node head = damage.Head;
		 head != null;
		 head = head.Next ) {
		r = new Rectangle( head.ob.X * textSize.X, head.ob.Y * textSize.Y, textSize.X, textSize.Y );
		g.Clip.Union( new Region( r ) );
	    }

	if( damage == null || damage.Length > con.viewWidth / 4 ) {
	    g.Clip.Union( new Region( scrollArea ) );
	    scrollPos =
		new RectangleF
		( scrollArea.Left,
		  ((float)con.viewY / con.bufferHeight) * ClientSize.Height,
		  scrollArea.Width,
		  ((float)con.viewHeight / con.bufferHeight) * ClientSize.Height );
	    /* Scroll bar */
	    g.DrawImage( wallBuffer, scrollArea, scrollArea, GraphicsUnit.Pixel );
	    g.FillRectangle( backBrushes[con.cursorBack], scrollPos );

	    dblGraphic.DrawImage( wallBuffer, 0,0 );
	    for( int i = 0; i < con.viewHeight; i++ ) {
		for( int j = 0; j < con.viewWidth; j++ ) {
		    int left = j * textSize.X, top = i * textSize.Y;
		    r = new Rectangle
			( left,
			  top,
			  textSize.X,
			  textSize.Y );

		    dblGraphic.FillRectangle
			( backBrushes[con.getBackColor(j,i)], 
			  r );
		    string x = single + con.getCharacter(j,i);
		    dblGraphic.DrawString
			( x, conFont, foreBrushes[con.getForeColor(j,i)],
			  r.Left, r.Top );
		}
	    }
	    g.DrawImage( dblBuffer, 0,0 );
	} else {
	    for( List<Point>.Node head = damage.Head;
		 head != null;
		 head = head.Next ) {
		int left = head.ob.X * textSize.X, top = head.ob.Y * textSize.Y;
		r = new Rectangle
		    ( left,
		      top,
		      textSize.X,
		      textSize.Y );
		
		if( !(con.cursorX == head.ob.X && con.cursorY == head.ob.Y) )
		    dblGraphic.DrawImage( wallBuffer, r, r, GraphicsUnit.Pixel );
		dblGraphic.FillRectangle
		    ( backBrushes[con.getBackColor(head.ob.X,head.ob.Y)], 
		      r );
		string x = single + con.getCharacter(head.ob.X,head.ob.Y);
		dblGraphic.DrawString
		    ( x, conFont, foreBrushes[con.getForeColor(head.ob.X,head.ob.Y)],
		      r.Left, r.Top );
		g.DrawImage( dblBuffer, r, r, GraphicsUnit.Pixel );
	    }
	}

	g.Dispose();
    }

    private void OnRefreshTimer( object Sender, EventArgs e ) {
	bool change = con.refresh();
	List<Point> damage = con.damageList();

	if( change ) { changeSize(); redraw(null); }
	else redraw(damage);

	if( Text != con.title ) Text = con.title;
    }

    private void OnClose( object Sender, FormClosedEventArgs e ) {
	closed = true;
	con.destroy();
    }

    private void OnKeyDown( object sender, KeyEventArgs e ) {
	doWriteKey( true, e );
	e.Handled = true;
    }

    private void OnKeyUp( object sender, KeyEventArgs e ) {
	doWriteKey( false, e );
	e.Handled = true;
    }

    private void OnKeyPressed( object sender, KeyPressEventArgs e ) {
	con.writeKey( true, 0, 0, e.KeyChar );
    }

    private void doWriteKey( bool down, KeyEventArgs e ) {
	con.writeKey
	    ( down, 
	      (short)(e.KeyCode & ~(Keys.Alt | Keys.Control | Keys.Shift)),
	      (((e.KeyCode & Keys.Alt) == Keys.Alt) ? 
	       win32.LEFT_ALT_PRESSED : 0) |
	      (((e.KeyCode & Keys.Control) == Keys.Control) ? 
	       win32.LEFT_CTRL_PRESSED : 0) |
	      (((e.KeyCode & Keys.Shift) == Keys.Shift) ? 
	       win32.SHIFT_PRESSED : 0),
	      '\0' );
    }

    protected override bool ProcessDialogKey( Keys keydata ) {
	return true;
    }

    /* Thanks: http://www.codeproject.com/cs/media/measurestring.asp */
    public Point MeasureDisplayStringWidth
	(Graphics graphics, string text, Font font)
    {
	System.Drawing.StringFormat format  = new System.Drawing.StringFormat ();
	System.Drawing.RectangleF   rect    = new System.Drawing.RectangleF
	    (0, 0, 1000, 1000);
	System.Drawing.CharacterRange[] ranges  = 
	    { new System.Drawing.CharacterRange(0, text.Length) };
	System.Drawing.Region[]         regions = new System.Drawing.Region[1];
	
	format.SetMeasurableCharacterRanges (ranges);
	
	regions = graphics.MeasureCharacterRanges (text, font, rect, format);
	rect    = regions[0].GetBounds (graphics);
	
	return new Point((int)(rect.Right + (float)widthAdj),(int)(rect.Bottom - rect.Top));
    }

    private void OnClick( object sender, MouseEventArgs e ) {
	if( e.X > (con.viewWidth * textSize.X) &&
	    scrollPos != null && 
	    e.Y < (con.viewHeight * textSize.Y) - scrollPos.Height ) {
	    con.scrollTo( (float)e.Y / (ClientSize.Height - scrollPos.Height) );
	}
    }

    public ConWindow( Console con ) {
	int i;

	this.con = con;
	SetStyle(ControlStyles.ResizeRedraw, true);

	Settings = new WindowSettings();
	sizeText();

	repaint = new Timer();
	repaint.Interval = 100;
	repaint.Tick += new EventHandler(OnRefreshTimer);

	wallpaper = new Wallpaper();

	foreBrushes = new Brush[con.NumColors];
	backBrushes = new Brush[con.NumColors];

	for( i = 0; i < backBrushes.Length; i++ ) {
	    backBrushes[i] = 
		new SolidBrush( Color.FromArgb(con.getBackColor(i)) );
	    foreBrushes[i] =
		new SolidBrush( Color.FromArgb(con.getForeColor(i)) );
	}
	this.SetStyle
	    ( ControlStyles.UserPaint |
	      ControlStyles.AllPaintingInWmPaint |
	      ControlStyles.OptimizedDoubleBuffer, true );

	FormClosed += new FormClosedEventHandler( OnClose );
	KeyDown += new KeyEventHandler( OnKeyDown );
	KeyUp   += new KeyEventHandler( OnKeyUp );
	KeyPress += new KeyPressEventHandler( OnKeyPressed );
	ResizeEnd += new EventHandler( OnMoveSize );
	MouseDown += new MouseEventHandler( OnClick );
	Load += new EventHandler( OnMoveSize );
    }

    public void go() {
	while( !closed ) Application.Run( this );
    }
};
    
