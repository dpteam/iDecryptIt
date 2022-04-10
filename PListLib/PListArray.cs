﻿/* =============================================================================
 * File:   PListArray.cs
 * Author: Cole Tobin
 * =============================================================================
 * Copyright (c) 2022 Cole Tobin
 *
 * This file is part of iDecryptIt.
 *
 * iDecryptIt is free software: you can redistribute it and/or modify it under
 *   the terms of the GNU General Public License as published by the Free
 *   Software Foundation, either version 3 of the License, or (at your option)
 *   any later version.
 *
 * iDecryptIt is distributed in the hope that it will be useful, but WITHOUT
 *   ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 *   FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 *   more details.
 *
 * You should have received a copy of the GNU General Public License along with
 *   iDecryptIt. If not, see <http://www.gnu.org/licenses/>.
 * =============================================================================
 */

using JetBrains.Annotations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace PListLib;

[PublicAPI]
public class PListArray : IPListElement<List<IPListElement>>, IPListElementInternals
{
    public PListElementType Type => PListElementType.Array;
    public bool SerializableAsXml => Value.All(elem => elem.SerializableAsXml);
    public dynamic UntypedValue => Value;
    public List<IPListElement> Value { get; set; }

    public PListArray(IPListElement[] value)
        : this(new List<IPListElement>(value))
    { }
    public PListArray(List<IPListElement> value)
    {
        Value = value;
    }

    internal static PListArray ReadXml(XmlNode node)
    {
        Debug.Assert(node.NodeType is XmlNodeType.Element);
        Debug.Assert(node.Name.ToLowerInvariant() is PListHelpers.XML_NAME_ARRAY);

        List<IPListElement> children = new(node.ChildNodes.Count);
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType is not XmlNodeType.Element)
                continue;
            children.Add(PListHelpers.ParseNode(child));
        }
        return new(children);
    }
}
