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

public class List<Type> {
    public class Node {
	public Type ob;
	public Node next;
	public Node prev;
	public Node( Type ob ) { this.ob = ob; }
	public Node Next { get { return next; } }
	public Node Prev { get { return prev; } }
    };

    Node head,tail;
    int length;

    public void Add( Type t ) {
	Node newNode = new Node(t);
	if( tail != null ) {
	    tail.next = newNode;
	    newNode.prev = tail;
	    tail = newNode;
	    length++;
	} else { tail = head = newNode; length = 1; }
    }

    public void Append( List<Type> l ) {
	for( Node head = l.Head; head != null; head = head.Next )
	    Add( head.ob );
    }

    public int Length { get { return length; } }
    public Node Head { get { return head; } }
    public Node Tail { get { return tail; } }
};
