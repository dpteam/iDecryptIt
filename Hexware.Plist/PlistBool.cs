﻿/* =============================================================================
 * File:   PlistBool.cs
 * Author: Cole Johnson
 * =============================================================================
 * Copyright (c) 2012, 2014 Cole Johnson
 * 
 * This file is part of Hexware.Plist
 * 
 * Hexware.Plist is free software: you can redistribute it and/or modify it
 *   under the terms of the GNU Lesser General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or (at your
 *   option) any later version.
 * 
 * Hexware.Plist is distributed in the hope that it will be useful, but WITHOUT
 *   ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 *   FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
 *   License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 *   along with Hexware.Plist. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */
using System.IO;
using System.Xml;

namespace Hexware.Plist
{
    /// <summary>
    /// Represents a &lt;true /&gt; or &lt;false /&gt; tag using a <see cref="System.Boolean"/>
    /// </summary>
    public partial class PlistBool
    {
        /// <summary>
        /// Hexware.Plist.PlistBool constructor using a <see cref="System.Boolean"/>
        /// </summary>
        /// <param name="value">The value of this node</param>
        public PlistBool(bool value)
        {
            _value = value;
        }
    }
    public partial class PlistBool
    {
        internal static PlistBool ReadBinary(BinaryReader reader, byte firstbyte)
        {
            return new PlistBool(firstbyte == 0x09);
        }

        internal byte[] WriteBinary()
        {
            return new byte[]
            {
                (byte)(_value ? 0x09 : 0x08)
            };
        }

        internal static PlistBool ReadXml(XmlDocument reader, int index)
        {
            if (reader.ChildNodes[index].Name == "true")
                return new PlistBool(true);
            return new PlistBool(false);
        }

        internal void WriteXml(XmlNode tree, XmlDocument writer)
        {
            XmlElement element;
            element = writer.CreateElement(_value ? "true" : "false");
            tree.AppendChild(element);
        }
    }
    public partial class PlistBool : IPlistElement<bool, Primitive>
    {
        internal bool _value;

        /// <summary>
        /// Gets the Xml tag for this element
        /// </summary>
        public string XmlTag
        {
            get
            {
                return (_value) ? "true" : "false";
            }
        }

        /// <summary>
        /// Gets or sets the value of this element
        /// </summary>
        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Gets the type of this element as one of <see cref="Hexware.Plist.Container"/> or <see cref="Hexware.Plist.Primitive"/>
        /// </summary>
        public Primitive ElementType
        {
            get
            {
                return Primitive.Bool;
            }
        }

        /// <summary>
        /// Gets the length of this element when written in binary mode
        /// </summary>
        /// <returns>Containers return the amount inside while Primitives return the binary length</returns>
        public int GetPlistElementBinaryLength()
        {
            return 1;
        }
    }
}