﻿/* =============================================================================
 * File:   Img2Reader.cs
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
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace iDecryptIt.IO.Formats;

[PublicAPI]
public class Img2Reader : IDisposable
{
    // when C# 11 is released, replace these with UTF-8 string literals
    private static byte[] MAGIC = { (byte)'2', (byte)'g', (byte)'m', (byte)'I' };
    private static byte[] MAGIC_VERSION_TAG = { (byte)'s', (byte)'r', (byte)'e', (byte)'v' };

    private readonly Stream _input;

    private int _paddedLength = 0; // offset 10
    private byte[] _payload = Array.Empty<byte>();

    private Img2Reader(Stream input)
    {
        if (!input.CanSeek)
            throw new ArgumentException("Input must be seekable.", nameof(input));

        _input = input;
        _input.Position = 0;

        ParseHeader();
        ExtractPayload();
    }

    public static Img2Reader Parse(Stream input) =>
        new(input);

    private void ParseHeader()
    {
        byte[] header = new byte[0x400];
        if (_input.Read(header) != 0x400)
            throw new EndOfStreamException("Unexpected EOF while reading header.");

        Span<byte> headerSpan = header.AsSpan();

        // magic
        if (!MAGIC.SequenceEqual(header[..4]))
            throw new InvalidDataException("Input file is not an \"IMG2\" file.");

        // image type + epoch
        ImageType = BitConverter.ToUInt32(headerSpan[4..8]);
        SecurityEpoch = BitConverter.ToUInt16(headerSpan[8..0xA]);

        // length
        _paddedLength = BitConverter.ToInt32(headerSpan[0x10..0x14]);
        Length = BitConverter.ToInt32(headerSpan[0x14..0x18]);
        if (_paddedLength < Length)
            throw new InvalidDataException("Payload's padded length cannot less than unpadded length.");

        // version tag
        if (!MAGIC_VERSION_TAG.SequenceEqual(header[0x70..0x74]))
            throw new InvalidDataException("Input file's version ('vers') tag is missing.");
        VersionTagValue = header[0x78..0x90].ToStringNoTrailingNulls();

        // sanity check
        SpuriousDataInHeaderPadding = header.Skip(0x90).Any(b => b is not 0);
    }

    private void ExtractPayload()
    {
        Contract.Assert(_input.Position is 0x400);
        _payload = new byte[Length];
        _input.ReadExact(_payload);

        byte[] padding = new byte[_paddedLength - Length];
        if (_input.Read(padding) != padding.Length)
            throw new EndOfStreamException("Unexpected EOF while reading payload.");
        SpuriousDataInPayloadPadding = padding.Any(b => b is not 0);
    }

    public uint ImageType { get; private set; }
    public ushort SecurityEpoch { get; private set; }
    public string VersionTagValue { get; private set; } = "";
    public bool SpuriousDataInHeaderPadding { get; private set; }
    public bool SpuriousDataInPayloadPadding { get; private set; }

    public void Read(out byte[] payload)
    {
        payload = new byte[Length];
        Array.Copy(_payload, payload, Length);
    }

    public int Length { get; private set; }
    public byte this[int index] => _payload[index];

    public void Dispose()
    {
        _input.Dispose();
        GC.SuppressFinalize(this);
    }
}
