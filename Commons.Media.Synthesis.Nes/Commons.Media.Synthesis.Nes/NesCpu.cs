// C# port of http://www.piumarta.com/software/lib6502/lib6502-1.2/

/* Copyright (c) 2005 Ian Piumarta (original) / Atsushi Eno (C#)
 * 
 * All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the 'Software'),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, provided that the above copyright notice(s) and this
 * permission notice appear in all copies of the Software and that both the
 * above copyright notice(s) and this permission notice appear in supporting
 * documentation.
 *
 * THE SOFTWARE IS PROVIDED 'AS IS'.  USE ENTIRELY AT YOUR OWN RISK.
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons.Media.Synthesis.Nes
{
	public class Callbacks
	{
		public Callbacks ()
		{
			Read = new Dictionary<int,ReadHandler> ();
			Write = new Dictionary<int,WriteHandler> ();
			Call = new Dictionary<int,CallHandler> ();
		}
		public IDictionary<int,ReadHandler> Read { get; set; }
		public IDictionary<int,WriteHandler> Write { get; set; }
		public IDictionary<int,CallHandler> Call { get; set; }
	}
	
	public delegate byte ReadHandler (NesCpu cpu, ushort address);
	public delegate void WriteHandler (NesCpu cpu, ushort address, byte data);
	public delegate byte CallHandler (NesCpu cpu, ushort address, byte data);

	public class NesMachine
	{
		public NesMachine ()
			: this (new NesCpu (), new NesPpu (), new NesApu ())
		{
		}
		
		public NesMachine (NesCpu cpu, NesPpu ppu, NesApu apu)
		{
		}
		
		public NesCpu Cpu { get; set; }
		public NesPpu Ppu { get; set; }
		public NesApu Apu { get; set; }
	}
	
	public class NesPpu
	{
		// TODO?
	}
	
	public class CpuStackOverflowException : Exception
	{
		public CpuStackOverflowException ()
		{
		}
		
		public CpuStackOverflowException (string message)
			: base (message)
		{
		}
	}
	
	public class CpuInvalidOperationException : Exception
	{
		public CpuInvalidOperationException ()
		{
		}
		
		public CpuInvalidOperationException (string message)
			: base (message)
		{
		}
	}
	
	public partial class NesCpu
	{
		Registers Registers { get; set; }
		byte [] Memory { get; set; }
		
		public Callbacks Callbacks { get; private set; }
		
		public NesCpu ()
		{
			Registers = new Registers ();
			Memory = new byte [0x10000];
			Callbacks = new Callbacks ();
		}
		
		public void Load (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			Array.Copy (data, Memory, data.Length);
		}
		
		public void Reset ()
		{
			unchecked {
				Registers.P &= (byte) ~Status.D;
				Registers.P |= Status.I;
				Registers.PC = GetVector (Vector.Rst);
			}
		}
		
		public void Nmi ()
		{
			Memory [0x0100 + Registers.S--] = (byte) (Registers.PC >> 8);
			Memory [0x0100 + Registers.S--] = (byte) (Registers.PC & 0xFF);
			Memory [0x0100 + Registers.S--] = Registers.P;
			Registers.P &= Status.B;
			Registers.P |= Status.I;
			Registers.PC = GetVector (Vector.Nmi);
		}
		
		public void Irq ()
		{
			if ((Registers.P & Status.I) != 0) {
				Memory [0x0100 + Registers.S--] = (byte) (Registers.PC >> 8);
				Memory [0x0100 + Registers.S--] = (byte) (Registers.PC & 0xFF);
				Memory [0x0100 + Registers.S--] = Registers.P;
				Registers.P &= Status.B;
				Registers.P |= Status.I;
				Registers.PC = GetVector (Vector.Irq);
			}
		}
		
		public void Run ()
		{
			DoRun ();
		}
		
		public int Disassemble (ushort addr, out string format)
		{
			return DoDisassemble (addr, out format);
		}
		
		public string Dump ()
		{
			var r = Registers;
			return String.Format ("PC={0:X04} SP={1:X04} A={2:X02} X={3:X02} Y={4:X02} P={5:X02} {6}",
				r.PC, 0x0100 + r.S, r.A, r.X, r.Y, r.P, r.FormatStatus ());
		}
		
		public ushort GetVector (Vector vector)
		{
			return (ushort) (Memory [vector.Lsb] | (Memory [vector.Msb] << 8));
		}
		
		public void SetVector (Vector vector, ushort addr)
		{
			Memory [vector.Lsb] = (byte) (addr & 0xFF);
			Memory [vector.Msb] = (byte) (addr >> 8);
		}
		
		// non-public member goes here.
		
	}
	
	public class Vector
	{
		public ushort Value { get; set; }
		public ushort Lsb { get; set; }
		public ushort Msb { get; set; }

		public static Vector Nmi = new Vector () { Value = 0xFFFA, Lsb = 0xFFFA, Msb = 0xFFFB };
		public static Vector Rst = new Vector () { Value = 0xFFFC, Lsb = 0xFFFC, Msb = 0xFFFD };
		public static Vector Irq = new Vector () { Value = 0xFFFE, Lsb = 0xFFFE, Msb = 0xFFFF };
	}

	// atsushieno's own code.

	public enum AddressingMode
	{
		Implied,
		Accumulator,
		Immediate,
		ZeroPage,
		ZeroPageX,
		ZeroPageY,
		Relative,
		Absolute,
		AbsoluteX,
		AbsoluteY,
		Indirect,
		IndirectX,
		IndirectY,
		// These are additional
		IndirectAbsoluteX,
		IndirectZeroPage,
	}

	public enum AssemblyOperations
	{
		LDA,
		LDX,
		LDY,
		STA,
		STX,
		STY,
		STZ, // 65C02
		TAX,
		TAY,
		TSX,
		TXA,
		TXS,
		TYA,
		
		ADC,
		AND,
		ASL,
		BIT,
		CMP,
		CPX,
		CPY,
		DEC,
		DEA, // 65C02
		DEX,
		DEY,
		EOR,
		INC,
		INA, // 65C02
		INX,
		INY,
		LSR,
		ORA,
		ROL,
		ROR,
		SBC,
		
		PHA,
		PHX, // 65C02
		PHY, // 65C02
		PHP,
		PLA,
		PLX, // 65C02
		PLY, // 65C02
		PLP,
		
		JMP,
		JSR,
		RTS,
		RTI,
		
		BCC,
		BCS,
		BEQ,
		BMI,
		BNE,
		BPL,
		BVC,
		BVS,
		BRA, // 65C02
		
		CLC,
		CLD,
		CLI,
		CLV,
		SEC,
		SED,
		SEI,
		
		TRB, // 65C02
		TSB, // 65C02
		
		BRK,
		NOP
	}
	
	public class OpCode
	{
		public OpCode (byte byteCode, AssemblyOperations operation, AddressingMode addressing, byte ticks)
		{
			ByteCode = byteCode;
			Operation = operation;
			Addressing = addressing;
			Ticks = ticks;
		}
		
		public byte ByteCode { get; private set; }
		public AssemblyOperations Operation { get; private set; }
		public AddressingMode Addressing { get; private set; }
		public byte Ticks { get; private set; }
	}
	
	// private stuff
	
	class Registers
	{
		public byte A { get; set; }
		public byte X { get; set; }
		public byte Y { get; set; }
		public byte P { get; set; }
		public byte S { get; set; }
		public ushort PC { get; set; }

		public bool N { get { return (P & Status.N) != 0; } }
		public bool V { get { return (P & Status.V) != 0; } }
		public bool B { get { return (P & Status.B) != 0; } }
		public bool D { get { return (P & Status.D) != 0; } }
		public bool I { get { return (P & Status.I) != 0; } }
		public bool Z { get { return (P & Status.Z) != 0; } }
		public bool C { get { return (P & Status.C) != 0; } }
		
		public string FormatStatus ()
		{
			return "" + (N ? 'N' : '-')
				+ (V ? 'V' : '-')
				+ (B ? 'B' : '-')
				+ (D ? 'D' : '-')
				+ (I ? 'I' : '-')
				+ (Z ? 'Z' : '-')
				+ (C ? 'C' : '-');
		}
	}
	
	static class Status
	{
		public const int C = 1,
		Z = 2,
		I = 4,
		D = 8,
		B = 16,
		X = 32,
		V = 64,
		N = 128;
	}
	
	// Cpu static stuff goes here.
	public partial class NesCpu
	{
		// Static members.
		
		static readonly OpCode [] OpCodes;
		
		static NesCpu ()
		{
			var ops = new OpCode [0x100];
			OpCodes = ops;

			// LDA
			ops [0xA9] = new OpCode (0xA9, AssemblyOperations.LDA, AddressingMode.Immediate, 2);
			ops [0xA5] = new OpCode (0xA5, AssemblyOperations.LDA, AddressingMode.ZeroPage, 3);
			ops [0xB5] = new OpCode (0xB5, AssemblyOperations.LDA, AddressingMode.ZeroPageX, 4);
			ops [0xAD] = new OpCode (0xAD, AssemblyOperations.LDA, AddressingMode.Absolute, 4);
			ops [0xBD] = new OpCode (0xBD, AssemblyOperations.LDA, AddressingMode.AbsoluteX, 4);
			ops [0xB9] = new OpCode (0xB9, AssemblyOperations.LDA, AddressingMode.AbsoluteY, 4);
			ops [0xA1] = new OpCode (0xA1, AssemblyOperations.LDA, AddressingMode.IndirectX, 6);
			ops [0xB1] = new OpCode (0xB1, AssemblyOperations.LDA, AddressingMode.IndirectY, 5);

			// LDX
			ops [0xA2] = new OpCode (0xA2, AssemblyOperations.LDX, AddressingMode.Immediate, 2);
			ops [0xA6] = new OpCode (0xA6, AssemblyOperations.LDX, AddressingMode.ZeroPage, 3);
			ops [0xB6] = new OpCode (0xB6, AssemblyOperations.LDX, AddressingMode.ZeroPageX, 4);
			ops [0xAE] = new OpCode (0xAE, AssemblyOperations.LDX, AddressingMode.Absolute, 4);
			ops [0xBE] = new OpCode (0xBE, AssemblyOperations.LDX, AddressingMode.AbsoluteX, 4);

			// LDY
			ops [0xA0] = new OpCode (0xA0, AssemblyOperations.LDY, AddressingMode.Immediate, 2);
			ops [0xA4] = new OpCode (0xA4, AssemblyOperations.LDY, AddressingMode.ZeroPage, 3);
			ops [0xB4] = new OpCode (0xB4, AssemblyOperations.LDY, AddressingMode.ZeroPageX, 4);
			ops [0xAC] = new OpCode (0xAC, AssemblyOperations.LDY, AddressingMode.Absolute, 4);
			ops [0xBC] = new OpCode (0xBC, AssemblyOperations.LDY, AddressingMode.AbsoluteX, 4);

			// STA
			ops [0x85] = new OpCode (0x85, AssemblyOperations.STA, AddressingMode.ZeroPage, 3);
			ops [0x95] = new OpCode (0x95, AssemblyOperations.STA, AddressingMode.ZeroPageX, 4);
			ops [0x8D] = new OpCode (0x8D, AssemblyOperations.STA, AddressingMode.Absolute, 4);
			ops [0x9D] = new OpCode (0x9D, AssemblyOperations.STA, AddressingMode.AbsoluteX, 5);
			ops [0x99] = new OpCode (0x99, AssemblyOperations.STA, AddressingMode.AbsoluteY, 5);
			ops [0x81] = new OpCode (0x81, AssemblyOperations.STA, AddressingMode.IndirectX, 6);
			ops [0x91] = new OpCode (0x91, AssemblyOperations.STA, AddressingMode.IndirectY, 6);

			// STX
			ops [0x86] = new OpCode (0x86, AssemblyOperations.STX, AddressingMode.ZeroPage, 3);
			ops [0x96] = new OpCode (0x96, AssemblyOperations.STX, AddressingMode.ZeroPageX, 4);
			ops [0x8E] = new OpCode (0x8E, AssemblyOperations.STX, AddressingMode.Absolute, 4);

			// STY
			ops [0x84] = new OpCode (0x84, AssemblyOperations.STY, AddressingMode.ZeroPage, 3);
			ops [0x94] = new OpCode (0x94, AssemblyOperations.STY, AddressingMode.ZeroPageX, 4);
			ops [0x8C] = new OpCode (0x8C, AssemblyOperations.STY, AddressingMode.Absolute, 4);
			
			// STZ (65C02)
			ops [0x64] = new OpCode (0x64, AssemblyOperations.STZ, AddressingMode.ZeroPage, 3);
			ops [0x74] = new OpCode (0x74, AssemblyOperations.STZ, AddressingMode.ZeroPageX, 4);
			ops [0x9C] = new OpCode (0x9C, AssemblyOperations.STZ, AddressingMode.Absolute, 4);
			ops [0x9E] = new OpCode (0x9E, AssemblyOperations.STZ, AddressingMode.AbsoluteX, 5);

			// TAX
			ops [0xAA] = new OpCode (0xAA, AssemblyOperations.TAX, AddressingMode.Implied, 2);

			// TAY
			ops [0xA8] = new OpCode (0xA8, AssemblyOperations.TAY, AddressingMode.Implied, 2);

			// TSX
			ops [0xBA] = new OpCode (0xBA, AssemblyOperations.TSX, AddressingMode.Implied, 2);

			// TXA
			ops [0x8A] = new OpCode (0x8A, AssemblyOperations.TXA, AddressingMode.Implied, 2);

			// TXS
			ops [0x9A] = new OpCode (0x9A, AssemblyOperations.TXS, AddressingMode.Implied, 2);

			// TYA
			ops [0x98] = new OpCode (0x98, AssemblyOperations.TYA, AddressingMode.Implied, 2);

			// ADC
			ops [0x69] = new OpCode (0x69, AssemblyOperations.ADC, AddressingMode.Immediate, 2);
			ops [0x65] = new OpCode (0x65, AssemblyOperations.ADC, AddressingMode.ZeroPage, 3);
			ops [0x75] = new OpCode (0x75, AssemblyOperations.ADC, AddressingMode.ZeroPageX, 4);
			ops [0x6D] = new OpCode (0x6D, AssemblyOperations.ADC, AddressingMode.Absolute, 4);
			ops [0x7D] = new OpCode (0x7D, AssemblyOperations.ADC, AddressingMode.AbsoluteX, 4);
			ops [0x79] = new OpCode (0x79, AssemblyOperations.ADC, AddressingMode.AbsoluteY, 4);
			ops [0x61] = new OpCode (0x61, AssemblyOperations.ADC, AddressingMode.IndirectX, 6);
			ops [0x71] = new OpCode (0x71, AssemblyOperations.ADC, AddressingMode.IndirectY, 5);

			// AND
			ops [0x29] = new OpCode (0x29, AssemblyOperations.AND, AddressingMode.Immediate, 2);
			ops [0x25] = new OpCode (0x25, AssemblyOperations.AND, AddressingMode.ZeroPage, 3);
			ops [0x35] = new OpCode (0x35, AssemblyOperations.AND, AddressingMode.ZeroPageX, 4);
			ops [0x2D] = new OpCode (0x2D, AssemblyOperations.AND, AddressingMode.Absolute, 4);
			ops [0x3D] = new OpCode (0x3D, AssemblyOperations.AND, AddressingMode.AbsoluteX, 4);
			ops [0x39] = new OpCode (0x39, AssemblyOperations.AND, AddressingMode.AbsoluteY, 4);
			ops [0x21] = new OpCode (0x21, AssemblyOperations.AND, AddressingMode.IndirectX, 6);
			ops [0x31] = new OpCode (0x31, AssemblyOperations.AND, AddressingMode.IndirectY, 5);

			// ASL
			ops [0x0A] = new OpCode (0x0A, AssemblyOperations.ASL, AddressingMode.Immediate, 2);
			ops [0x06] = new OpCode (0x06, AssemblyOperations.ASL, AddressingMode.ZeroPage, 5);
			ops [0x16] = new OpCode (0x16, AssemblyOperations.ASL, AddressingMode.ZeroPageX, 6);
			ops [0x0E] = new OpCode (0x0E, AssemblyOperations.ASL, AddressingMode.Absolute, 6);
			ops [0x1E] = new OpCode (0x1E, AssemblyOperations.ASL, AddressingMode.AbsoluteX, 7);

			// BIT
			ops [0x24] = new OpCode (0x24, AssemblyOperations.BIT, AddressingMode.ZeroPage, 3);
			ops [0x2C] = new OpCode (0x2C, AssemblyOperations.BIT, AddressingMode.Absolute, 4);

			// CMP
			ops [0xC9] = new OpCode (0xC9, AssemblyOperations.CMP, AddressingMode.Immediate, 2);
			ops [0xC5] = new OpCode (0xC5, AssemblyOperations.CMP, AddressingMode.ZeroPage, 3);
			ops [0xD5] = new OpCode (0xD5, AssemblyOperations.CMP, AddressingMode.ZeroPageX, 4);
			ops [0xCD] = new OpCode (0xCD, AssemblyOperations.CMP, AddressingMode.Absolute, 4);
			ops [0xDD] = new OpCode (0xDD, AssemblyOperations.CMP, AddressingMode.AbsoluteX, 4);
			ops [0xD9] = new OpCode (0xD9, AssemblyOperations.CMP, AddressingMode.AbsoluteY, 4);
			ops [0xC1] = new OpCode (0xC1, AssemblyOperations.CMP, AddressingMode.IndirectX, 6);
			ops [0xD1] = new OpCode (0xD1, AssemblyOperations.CMP, AddressingMode.IndirectY, 5);

			// CPX
			ops [0xE0] = new OpCode (0xE0, AssemblyOperations.CPX, AddressingMode.Immediate, 2);
			ops [0xE4] = new OpCode (0xE4, AssemblyOperations.CPX, AddressingMode.ZeroPage, 3);
			ops [0xEC] = new OpCode (0xEC, AssemblyOperations.CPX, AddressingMode.Absolute, 4);

			// CPY
			ops [0xC0] = new OpCode (0xC0, AssemblyOperations.CPY, AddressingMode.Immediate, 2);
			ops [0xC4] = new OpCode (0xC4, AssemblyOperations.CPY, AddressingMode.ZeroPage, 3);
			ops [0xCC] = new OpCode (0xCC, AssemblyOperations.CPY, AddressingMode.Absolute, 4);

			// DEC
			ops [0xC6] = new OpCode (0xC6, AssemblyOperations.DEC, AddressingMode.ZeroPage, 5);
			ops [0xD6] = new OpCode (0xD6, AssemblyOperations.DEC, AddressingMode.ZeroPageX, 6);
			ops [0xCE] = new OpCode (0xCE, AssemblyOperations.DEC, AddressingMode.Absolute, 6);
			ops [0xEE] = new OpCode (0xEE, AssemblyOperations.DEC, AddressingMode.AbsoluteX, 7);

			// DEA (65C02)
			ops [0x3A] = new OpCode (0x3A, AssemblyOperations.DEA, AddressingMode.Implied, 2);

			// DEX
			ops [0xCA] = new OpCode (0xCA, AssemblyOperations.DEX, AddressingMode.Implied, 2);

			// DEY
			ops [0x88] = new OpCode (0x88, AssemblyOperations.DEY, AddressingMode.Implied, 2);

			// EOR
			ops [0x49] = new OpCode (0x49, AssemblyOperations.EOR, AddressingMode.Immediate, 2);
			ops [0x45] = new OpCode (0x45, AssemblyOperations.EOR, AddressingMode.ZeroPage, 3);
			ops [0x55] = new OpCode (0x55, AssemblyOperations.EOR, AddressingMode.ZeroPageX, 4);
			ops [0x4D] = new OpCode (0x4D, AssemblyOperations.EOR, AddressingMode.Absolute, 4);
			ops [0x5D] = new OpCode (0x5D, AssemblyOperations.EOR, AddressingMode.AbsoluteX, 4);
			ops [0x59] = new OpCode (0x59, AssemblyOperations.EOR, AddressingMode.AbsoluteY, 4);
			ops [0x41] = new OpCode (0x41, AssemblyOperations.EOR, AddressingMode.IndirectX, 6);
			ops [0x51] = new OpCode (0x51, AssemblyOperations.EOR, AddressingMode.IndirectY, 5);

			// INC
			ops [0xE6] = new OpCode (0xE6, AssemblyOperations.INC, AddressingMode.ZeroPage, 5);
			ops [0xF6] = new OpCode (0xF6, AssemblyOperations.INC, AddressingMode.ZeroPageX, 6);
			ops [0xEE] = new OpCode (0xEE, AssemblyOperations.INC, AddressingMode.Absolute, 6);
			ops [0xFE] = new OpCode (0xFE, AssemblyOperations.INC, AddressingMode.AbsoluteX, 7);

			// INA (65C02)
			ops [0x1A] = new OpCode (0x1A, AssemblyOperations.INA, AddressingMode.Implied, 2);

			// INX
			ops [0xE8] = new OpCode (0xE8, AssemblyOperations.INX, AddressingMode.Implied, 2);

			// INY
			ops [0xC8] = new OpCode (0xC8, AssemblyOperations.INY, AddressingMode.Implied, 2);

			// LSR
			ops [0x4A] = new OpCode (0x4A, AssemblyOperations.LSR, AddressingMode.Accumulator, 2);
			ops [0x46] = new OpCode (0x46, AssemblyOperations.LSR, AddressingMode.ZeroPage, 5);
			ops [0x56] = new OpCode (0x56, AssemblyOperations.LSR, AddressingMode.ZeroPageX, 6);
			ops [0x4E] = new OpCode (0x4E, AssemblyOperations.LSR, AddressingMode.Absolute, 6);
			ops [0x5E] = new OpCode (0x5E, AssemblyOperations.LSR, AddressingMode.AbsoluteX, 7);

			// ORA
			ops [0x09] = new OpCode (0x09, AssemblyOperations.ORA, AddressingMode.Immediate, 2);
			ops [0x05] = new OpCode (0x05, AssemblyOperations.ORA, AddressingMode.ZeroPage, 3);
			ops [0x15] = new OpCode (0x15, AssemblyOperations.ORA, AddressingMode.ZeroPageX, 4);
			ops [0x0D] = new OpCode (0x0D, AssemblyOperations.ORA, AddressingMode.Absolute, 4);
			ops [0x1D] = new OpCode (0x1D, AssemblyOperations.ORA, AddressingMode.AbsoluteX, 4);
			ops [0x19] = new OpCode (0x19, AssemblyOperations.ORA, AddressingMode.AbsoluteY, 4);
			ops [0x01] = new OpCode (0x01, AssemblyOperations.ORA, AddressingMode.IndirectX, 6);
			ops [0x11] = new OpCode (0x11, AssemblyOperations.ORA, AddressingMode.IndirectY, 5);

			// ROL
			ops [0x2A] = new OpCode (0x2A, AssemblyOperations.ROL, AddressingMode.Accumulator, 2);
			ops [0x26] = new OpCode (0x26, AssemblyOperations.ROL, AddressingMode.ZeroPage, 5);
			ops [0x36] = new OpCode (0x36, AssemblyOperations.ROL, AddressingMode.ZeroPageX, 6);
			ops [0x2E] = new OpCode (0x2E, AssemblyOperations.ROL, AddressingMode.Absolute, 6);
			ops [0x3E] = new OpCode (0x3E, AssemblyOperations.ROL, AddressingMode.AbsoluteX, 7);

			// ROR
			ops [0x6A] = new OpCode (0x6A, AssemblyOperations.ROR, AddressingMode.Accumulator, 2);
			ops [0x76] = new OpCode (0x76, AssemblyOperations.ROR, AddressingMode.ZeroPage, 5);
			ops [0x66] = new OpCode (0x66, AssemblyOperations.ROR, AddressingMode.ZeroPageX, 6);
			ops [0x6E] = new OpCode (0x6E, AssemblyOperations.ROR, AddressingMode.Absolute, 6);
			ops [0x7E] = new OpCode (0x7E, AssemblyOperations.ROR, AddressingMode.AbsoluteX, 7);

			// SBC
			ops [0xE9] = new OpCode (0xE9, AssemblyOperations.SBC, AddressingMode.Immediate, 2);
			ops [0xE5] = new OpCode (0xE5, AssemblyOperations.SBC, AddressingMode.ZeroPage, 3);
			ops [0xF5] = new OpCode (0xF5, AssemblyOperations.SBC, AddressingMode.ZeroPageX, 4);
			ops [0xED] = new OpCode (0xED, AssemblyOperations.SBC, AddressingMode.Absolute, 4);
			ops [0xFD] = new OpCode (0xFD, AssemblyOperations.SBC, AddressingMode.AbsoluteX, 4);
			ops [0xF9] = new OpCode (0xF9, AssemblyOperations.SBC, AddressingMode.AbsoluteY, 4);
			ops [0xE1] = new OpCode (0xE1, AssemblyOperations.SBC, AddressingMode.IndirectX, 6);
			ops [0xF1] = new OpCode (0xF1, AssemblyOperations.SBC, AddressingMode.IndirectY, 5);

			// Stack ops
			ops [0x48] = new OpCode (0x48, AssemblyOperations.PHA, AddressingMode.Implied, 3);
			ops [0xDA] = new OpCode (0x48, AssemblyOperations.PHX, AddressingMode.Implied, 3);
			ops [0x5A] = new OpCode (0x48, AssemblyOperations.PHY, AddressingMode.Implied, 3);
			ops [0x08] = new OpCode (0x08, AssemblyOperations.PHP, AddressingMode.Implied, 3);
			ops [0x68] = new OpCode (0x68, AssemblyOperations.PLA, AddressingMode.Implied, 4);
			ops [0xFA] = new OpCode (0x48, AssemblyOperations.PLX, AddressingMode.Implied, 4);
			ops [0x7A] = new OpCode (0x48, AssemblyOperations.PLY, AddressingMode.Implied, 4);
			ops [0x28] = new OpCode (0x28, AssemblyOperations.PLP, AddressingMode.Implied, 4);

			// Jumps
			ops [0x4C] = new OpCode (0x4C, AssemblyOperations.JMP, AddressingMode.Absolute, 3);
			ops [0x6C] = new OpCode (0x6C, AssemblyOperations.JMP, AddressingMode.Indirect, 5);
			ops [0x20] = new OpCode (0x20, AssemblyOperations.JSR, AddressingMode.Absolute, 6);
			ops [0x60] = new OpCode (0x60, AssemblyOperations.RTS, AddressingMode.Implied, 6);
			ops [0x40] = new OpCode (0x40, AssemblyOperations.RTI, AddressingMode.Implied, 6);

			// Branches
			ops [0x90] = new OpCode (0x90, AssemblyOperations.BCC, AddressingMode.Relative, 2);
			ops [0xB0] = new OpCode (0xB0, AssemblyOperations.BCS, AddressingMode.Relative, 2);
			ops [0xF0] = new OpCode (0xF0, AssemblyOperations.BEQ, AddressingMode.Relative, 2);
			ops [0x30] = new OpCode (0x30, AssemblyOperations.BMI, AddressingMode.Relative, 2);
			ops [0xD0] = new OpCode (0xD0, AssemblyOperations.BNE, AddressingMode.Relative, 2);
			ops [0x10] = new OpCode (0x10, AssemblyOperations.BPL, AddressingMode.Relative, 2);
			ops [0x50] = new OpCode (0x50, AssemblyOperations.BVC, AddressingMode.Relative, 2);
			ops [0x70] = new OpCode (0x70, AssemblyOperations.BVS, AddressingMode.Relative, 2);

			// BRA (65C02)
			ops [0x80] = new OpCode (0x70, AssemblyOperations.BRA, AddressingMode.Relative, 2);

			// Flag ops
			ops [0x18] = new OpCode (0x18, AssemblyOperations.CLC, AddressingMode.Implied, 2);
			ops [0xD8] = new OpCode (0xD8, AssemblyOperations.CLD, AddressingMode.Implied, 2);
			ops [0x58] = new OpCode (0x58, AssemblyOperations.CLI, AddressingMode.Implied, 2);
			ops [0xB8] = new OpCode (0xB8, AssemblyOperations.CLV, AddressingMode.Implied, 2);
			ops [0x38] = new OpCode (0x38, AssemblyOperations.SEC, AddressingMode.Implied, 2);
			ops [0xF8] = new OpCode (0xF8, AssemblyOperations.SED, AddressingMode.Implied, 2);
			ops [0x78] = new OpCode (0x78, AssemblyOperations.SEI, AddressingMode.Implied, 2);

			// Test bits (65C02)
			ops [0x14] = new OpCode (0x04, AssemblyOperations.TRB, AddressingMode.ZeroPage, 3);
			ops [0x1C] = new OpCode (0x0C, AssemblyOperations.TRB, AddressingMode.Absolute, 4);
			ops [0x04] = new OpCode (0x04, AssemblyOperations.TSB, AddressingMode.ZeroPage, 3);
			ops [0x0C] = new OpCode (0x0C, AssemblyOperations.TSB, AddressingMode.Absolute, 4);

			// Misc
			ops [0x00] = new OpCode (0x00, AssemblyOperations.BRK, AddressingMode.Implied, 7);
			ops [0xEA] = new OpCode (0xEA, AssemblyOperations.NOP, AddressingMode.Implied, 2);
		}
		
		public static OpCode GetOpCode (byte code)
		{
			return OpCodes [code];
		}
	}
	
	// Cpu private stuff goes here.
	public partial class NesCpu
	{
		void DoRun ()
		{
			byte [] memory = Memory;
			ushort ea = 0;
			byte a = 0, x = 0, y = 0, p = 0, s = 0;
			ushort pc = 0;
			
			Action internalize = delegate {
				a = Registers.A;
				x = Registers.X;
				y = Registers.Y;
				p = Registers.P;
				s = Registers.S;
				pc = Registers.PC;
			};
			
			Action externalize = delegate {
				Registers.A = a;
				Registers.X = x;
				Registers.Y = y;
				Registers.P = p;
				Registers.S = s;
				Registers.PC = pc;
			};
			
			internalize ();
			
			// Initialize functions with local variables.
			//
			// This makes sense when it was totally inlined in C.
			// This makes little sense when it is instantiated as a delegate in a method, from performance perspective...
			// I'm likely to convert those delegates into "inline"
			// code that will be expanded like in C defs to make
			// them fully "locally" executed.

			// memory operations.
			Action<ushort,byte> putMemory = (addr, data) => {
				WriteHandler cb;
				if (Callbacks.Write.TryGetValue (addr, out cb))
					cb (this, addr, data);
				else
					memory [addr] = data;
			};
			
			Func<ushort,byte> getMemory = (addr) => {
				ReadHandler cb;
				if (Callbacks.Read.TryGetValue (addr, out cb))
					return cb (this, addr);
				else
					return memory [addr];
			};
			
			// stack operations
			Action<byte> push = (data) => { if (s == 0) throw new CpuStackOverflowException (); memory [0x0100 + s--] = data; };
			Func<byte> pop = () => { return memory [++s + 0x0100]; };
			
			// get flag operations
			Func<byte> getN = () => (byte) (p & Status.N);
			Func<byte> getV = () => (byte) (p & Status.V);
			Func<byte> getB = () => (byte) (p & Status.B);
			Func<byte> getD = () => (byte) (p & Status.D);
			Func<byte> getI = () => (byte) (p & Status.I);
			Func<byte> getZ = () => (byte) (p & Status.Z);
			Func<byte> getC = () => (byte) (p & Status.C);
			
			// set flag operations
			Action<bool,bool,bool,bool> setNVZC = (bool n, bool v, bool z, bool c) => { p = (byte) ((p & ~(Status.N | Status.V | Status.Z | Status.C)) | (n ? 1 : 0) | ((v ? 1 : 0)<<6) | ((z ? 1: 0)<<1) | (c ? 1 : 0)); };
			Action<bool,bool,bool> setNZC = (bool n, bool z, bool c) => { p = (byte) ((p & ~(Status.N | Status.Z | Status.C)) | (n ? 1 : 0) | ((z ? 1: 0)<<1) | (c ? 1 : 0)); };
			Action<bool,bool> setNZ = (bool n, bool z) => { p = (byte) ((p & ~(Status.N | Status.Z)) | (n ? 1 : 0) | ((z ? 1 : 0)<<1)); };
			Action<bool> setZ = (bool z) => { p = (byte) ((p & ~(Status.Z)) |((z ? 1 : 0)<<1)); };
			Action<bool> setC = (bool c) => { p = (byte) ((p & ~(Status.C)) | (c ? 1 : 0)); };

			// addressing actions
			var addressing_actions = new Action<int> [((AddressingMode []) Enum.GetValues (typeof (AddressingMode))).Select (v => (int) v).Max () + 1];
			addressing_actions [(int) AddressingMode.Implied] = (ticks) => { Tick (ticks); };
			addressing_actions [(int) AddressingMode.Immediate] = (ticks) => { Tick (ticks); ea = pc++; };
			addressing_actions [(int) AddressingMode.Absolute] = (ticks) => { Tick (ticks); ea = (ushort) (memory [pc] + (memory [pc + 1] << 8)); pc++; pc++; };
			addressing_actions [(int) AddressingMode.Relative] = (ticks) => {
				Tick (ticks);
				ea = memory [pc++];
				if ((ea & 0x80) != 0)
					ea -= 0x100;
				TickIf ((byte) (ea >> 8) != (byte) (pc >> 8));
				};
			addressing_actions [(int) AddressingMode.Indirect] = (ticks) => {
				Tick (ticks);
				ushort tmp;
				tmp = (ushort) (memory [pc] + (memory [pc + 1] << 8));
				ea = (ushort) (memory [tmp] + (memory [tmp + 1] << 8));
				pc++;
				pc++;
				};
			addressing_actions [(int) AddressingMode.AbsoluteX] = (ticks) => {
				Tick (ticks);
				ea = (ushort) (memory [pc] + (memory [pc + 1] << 8));
				pc++;
				pc++;
				TickIf ((ticks == 4) && ((byte) (ea >> 8) != (byte) ((ea + x) >> 8)));
				ea += x;
				};
			addressing_actions [(int) AddressingMode.AbsoluteY] = (ticks) => {
				Tick (ticks);
				ea = (ushort) (memory [pc] + (memory [pc + 1] << 8));
				pc++;
				pc++;
				TickIf ((ticks == 4) && ((byte) (ea >> 8) != (byte) ((ea + y) >> 8)));
				ea += y;
				};
			addressing_actions [(int) AddressingMode.ZeroPage] = (ticks) => { Tick (ticks); ea = memory [pc++]; };
			addressing_actions [(int) AddressingMode.ZeroPageX] = (ticks) => { Tick (ticks); ea = (ushort) (memory [pc++] + x); ea &= 0x00FF; };
			addressing_actions [(int) AddressingMode.ZeroPageY] = (ticks) => { Tick (ticks); ea = (ushort) (memory [pc++] + y); ea &= 0x00FF; };
			addressing_actions [(int) AddressingMode.IndirectX] = (ticks) => { Tick (ticks); ushort tmp = (ushort) (memory [pc++] + x); ea = (ushort) (memory [tmp] + (memory [tmp + 1] << 8)); };
			addressing_actions [(int) AddressingMode.IndirectY] = (ticks) => {
				Tick (ticks);
				var tmp = memory [pc++];
				ea = (ushort) (memory [tmp] + (memory [tmp + 1] << 8));
				TickIf ((ticks == 5) && ((byte) (ea >> 8) != (byte) ((ea + y) >> 8)));
				ea += y;
				};
			addressing_actions [(int) AddressingMode.IndirectAbsoluteX] = (ticks) => {
				Tick (ticks);
				ushort tmp = (ushort) (memory [pc] + (memory [pc + 1] << 8) + x);
				ea = (ushort) (memory [tmp] + (memory [tmp + 1] << 8));
				};
			addressing_actions [(int) AddressingMode.IndirectZeroPage] = (ticks) => {
				Tick (ticks);
				byte tmp = memory [pc++];
				ea = (ushort) (memory [tmp] + (memory [tmp + 1] << 8));
				};

			var operator_actions = new Action<OpCode, Action<int>> [((AssemblyOperations []) Enum.GetValues (typeof (AssemblyOperations))).Select (v => (int) v).Max () + 1];
			
			// operator actions
			
			operator_actions [(int) AssemblyOperations.ADC] = (op, adrmode) => {
				adrmode (op.Ticks);
				unchecked {
					byte b = getMemory (ea);
					if (getD () != 0) {
						int c = a + b + getC ();
						int v = (sbyte) a + (sbyte) b + getC ();
						// fetch ();
						a = (byte) c;
						setNVZC (0 != (a & 0x80), (((a & 0x80) > 0) ^ (v < 0)), (a == 0), (c & 0x100) > 0);
						// next ();
					} else {
						int l, h, _s;
						// inelegant & slow, but consistent with the hw for illegal digits
						l= (a & 0x0F) + (b & 0x0F) + getC();
						h= (a & 0xF0) + (b & 0xF0);
						if (l >= 0x0A) { l -= 0x0A; h += 0x10; }
						if (h >= 0xA0) { h -= 0xA0; }
						// fetch ();
						_s = h | (l & 0x0F);
						// only C is valid on NMOS 6502
setNVZC (0 != (_s & 0x80), !(0 != ((a ^ b) & 0x80) && 0 != ((a ^ _s) & 0x80)), _s == 0, 0 != (h & 0x80));
						a = (byte) _s;
						Tick (1);
						// next ();
					}
				}
			};
			
			operator_actions [(int) AssemblyOperations.SBC] = (op, adrmode) => {
				adrmode (op.Ticks);
				unchecked {
					byte B = getMemory (ea);
					if (getD () != 0) {
						int b = 1 - (p & 0x01);
						int c = a - B - b;
						int v= (sbyte) a - (sbyte) B - b;
						// fetch ();
						a = (byte) c;
						setNVZC (0 != (a & 0x80), ((a & 0x80) > 0) ^ ((v & 0x100) != 0), a == 0, c >= 0);
						// next ();
					} else {
						// this is verbatim ADC, with a 10's complemented operand
						int l, h, _s;
						B = (byte) (0x99 - B);
						l= (a & 0x0F) + (B & 0x0F) + getC ();
						h= (a & 0xF0) + (B & 0xF0);
						if (l >= 0x0A) { l -= 0x0A;  h += 0x10; }
						if (h >= 0xA0) { h -= 0xA0; }
						// fetch ();
						_s = h | (l & 0x0F);
						// only C is valid on NMOS 6502
						setNVZC (0 != (_s & 0x80), !(0 != ((a ^ B) & 0x80) && 0 != ((a ^ _s) & 0x80)), _s == 0, 0 != (h & 0x80));
						a = (byte) _s;
						Tick (1);
						// next ();
					}
				}
			};
			
			Action<OpCode,Action<int>,byte> cmpR = (op, adrmode, R) => {
				adrmode (op.Ticks);
				// fetch ();
				unchecked {
					byte B = getMemory (ea);
					byte d = (byte) (R - B);
					setNZC (0 != (d & 0x80), d == 0, R >= B);
				}
			};
			operator_actions [(int) AssemblyOperations.CMP] = (op, adrmode) => { cmpR (op, adrmode, a); };
			operator_actions [(int) AssemblyOperations.CPX] = (op, adrmode) => { cmpR (op, adrmode, x); };
			operator_actions [(int) AssemblyOperations.CPY] = (op, adrmode) => { cmpR (op, adrmode, y); };
			
			operator_actions [(int) AssemblyOperations.DEC] = (op, adrmode) => {
				adrmode (op.Ticks);
				// fetch ();
				unchecked {
					byte B = getMemory (ea);
					--B;
					putMemory (ea, B);
					setNZ (0 != (B & 0x80), B == 0);
				}
				// next ();
			};
			Action<OpCode,Action<int>,byte> decR = (op, adrmode, R) => {
				// fetch ();
				adrmode (op.Ticks);
				--R;
				setNZ (0 != (R & 0x80), R == 0);
				// next ();
			};
			operator_actions [(int) AssemblyOperations.DEA] = (op, adrmode) => { decR (op, adrmode, a); };
			operator_actions [(int) AssemblyOperations.DEX] = (op, adrmode) => { decR (op, adrmode, x); };
			operator_actions [(int) AssemblyOperations.DEY] = (op, adrmode) => { decR (op, adrmode, y); };
			
			operator_actions [(int) AssemblyOperations.INC] = (op, adrmode) => {
				adrmode (op.Ticks);
				// fetch ();
				unchecked {
					byte B = getMemory (ea);
					++B;
					putMemory (ea, B);
					setNZ (0 != (B & 0x80), B == 0);
				}
				// next ();
			};
			Action<OpCode,Action<int>,byte> incR = (op, adrmode, R) => {
				// fetch ();
				adrmode (op.Ticks);
				++R;
				setNZ (0 != (R & 0x80), R == 0);
				// next ();
			};
			operator_actions [(int) AssemblyOperations.INA] = (op, adrmode) => { incR (op, adrmode, a); };
			operator_actions [(int) AssemblyOperations.INX] = (op, adrmode) => { incR (op, adrmode, x); };
			operator_actions [(int) AssemblyOperations.INY] = (op, adrmode) => { incR (op, adrmode, y); };
			
			operator_actions [(int) AssemblyOperations.BIT] = (op, adrmode) => {
				adrmode (op.Ticks);
				// fetch ();
				unchecked {
					byte B = getMemory (ea);
					p = (byte) ((p & ~(Status.N | Status.V | Status.Z)) | (B & (0xC0)) | (((a & B) == 0 ? 1 : 0) << 1));
				}
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.TSB] = (op, adrmode) => {
				adrmode (op.Ticks);
				// fetch ();
				unchecked {
					byte b = getMemory (ea);
					b |= a;
					putMemory (ea, b);
					setZ (b == 0);
				}
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.TRB] = (op, adrmode) => {
				adrmode (op.Ticks);
				// fetch ();
				unchecked {
					byte b = getMemory (ea);
					b |= (byte) (a ^ 0xFF);
					putMemory (ea, b);
					setZ (b == 0);
				}
				// next ();
			};
			
			Action<OpCode,Action<int>,char>  bitwise = (op, adrmode, ch) => {
				adrmode (op.Ticks);
				// fetch ();
				switch (ch) {
				case '&': a &= getMemory (ea); break;
				case '^': a ^= getMemory (ea); break;
				case '|': a |= getMemory (ea); break;
				}
				setNZ (0 != (a & 0x80), a == 0);
				// next ();
			};
			operator_actions [(int) AssemblyOperations.AND] = (op, adrmode) => { bitwise (op, adrmode, '&'); };
			operator_actions [(int) AssemblyOperations.EOR] = (op, adrmode) => { bitwise (op, adrmode, '^'); };
			operator_actions [(int) AssemblyOperations.ORA] = (op, adrmode) => { bitwise (op, adrmode, '|'); };

			Action<OpCode,Action<int>> asl_misc = (op, adrmode) => {
				adrmode (op.Ticks);
				unchecked {
					uint i = (uint) (getMemory (ea) << 1);
					putMemory (ea, (byte) i);
					// fetch ();
					setNZC (0 != (i & 0x80), i == 0, 0 != (i >> 8));
				}
				// next ();
			};

			Action<OpCode,Action<int>> asla = (op, adrmode) => {
				adrmode (op.Ticks);
				// fetch ();
				unchecked {
					int c = a >> 7;
					a <<= 1;
					setNZC (0 != (a & 0x80), a == 0, c != 0);
				}
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.ASL] = (op, adrmode) => {
				if (op.Addressing == AddressingMode.Accumulator)
					asla (op, adrmode);
				else
					asl_misc (op, adrmode);
			};

			Action<OpCode,Action<int>> lsr_misc = (op, adrmode) => {
				adrmode (op.Ticks);
				unchecked {
					byte b = getMemory (ea);
					int c = b & 1;
					// fetch ();
					b >>= 1;
					putMemory (ea, b);
					setNZC (false, b == 0, c != 0);
				}
				// next ();
			};

			Action<OpCode,Action<int>> lsra = (op, adrmode) => {
				adrmode (op.Ticks);
				// fetch ();
				unchecked {
					int c = a & 1;
					a >>= 1;
					setNZC (false, a == 0, c != 0);
				}
				// next ();
			};

			operator_actions [(int) AssemblyOperations.LSR] = (op, adrmode) => {
				if (op.Addressing == AddressingMode.Accumulator)
					lsra (op, adrmode);
				else
					lsr_misc (op, adrmode);
			};

			Action<OpCode,Action<int>> rol_misc = (op, adrmode) => {
				adrmode (op.Ticks);
				unchecked {
					ushort b = (ushort) ((getMemory (ea) << 1) | getC ());
					//fetch ();
					putMemory (ea, (byte) b);
					setNZC (0 != (b & 0x80), 0 == (b & 0xFF), 0 != (b >> 8));
				}
				// next ();
			};

			Action<OpCode,Action<int>> rola = (op, adrmode) => {
				adrmode (op.Ticks);
				//fetch ();
				unchecked {
					ushort b = (ushort) ((a << 1) | getC ());
					a = (byte) b;
					putMemory (ea, (byte) b);
					setNZC (0 != (a & 0x80), 0 == (a & 0xFF), 0 != (b >> 8));
				}
				// next ();
			};

			operator_actions [(int) AssemblyOperations.ROL] = (op, adrmode) => {
				if (op.Addressing == AddressingMode.Accumulator)
					rola (op, adrmode);
				else
					rol_misc (op, adrmode);
			};

			Action<OpCode,Action<int>> ror_misc = (op, adrmode) => {
				adrmode (op.Ticks);
				unchecked {
					int c = getC ();
					byte m = getMemory (ea);
					ushort b= (ushort) ((c << 7) | (m >> 1));
					//fetch ();
					putMemory (ea, (byte) b);
					setNZC (0 != (b & 0x80), b == 0, 0 != (m & 1));
				}
				// next ();
			};

			Action<OpCode,Action<int>> rora = (op, adrmode) => {
				adrmode (op.Ticks);
				//fetch ();
				unchecked {
					int ci = getC ();
					int co = a & 1;
					//fetch ();
					a = (byte) ((ci << 7) | (a >> 1));
					setNZC (0 != (a & 0x80), a == 0, 0 != co);
				}
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.ROR] = (op, adrmode) => {
				if (op.Addressing == AddressingMode.Accumulator)
					rora (op, adrmode);
				else
					ror_misc (op, adrmode);
			};

			// FIXME: when replacing the source with macros, use direct assignment to S instead of returning S as the original source does.
			Func<OpCode,Action<int>,byte,byte> tRS = (op, adrmode, R) => {
				// fetch ();
				Tick (op.Ticks);
				byte S = R;
				setNZ (0 != (S & 0x80), S == 0);
				// next ();
				return S;
			};
			operator_actions [(int) AssemblyOperations.TAX] = (op, adrmode) => { x = tRS (op, adrmode, a); };
			operator_actions [(int) AssemblyOperations.TXA] = (op, adrmode) => { a = tRS (op, adrmode, x); };
			operator_actions [(int) AssemblyOperations.TAY] = (op, adrmode) => { y = tRS (op, adrmode, a); };
			operator_actions [(int) AssemblyOperations.TYA] = (op, adrmode) => { a = tRS (op, adrmode, y); };
			operator_actions [(int) AssemblyOperations.TSX] = (op, adrmode) => { x = tRS (op, adrmode, s); };
			
			operator_actions [(int) AssemblyOperations.TXS] = (op, adrmode) => {
				// fetch ();
				Tick (op.Ticks);
				s = x;
				// next ();
			};
			
			// FIXME: when replacing the source with macros, use direct assignment to S instead of returning S as the original source does.
			Func<OpCode,Action<int>,byte> ldR = (op, adrmode) => {
				adrmode (op.Ticks);
				// fetch ();
				var R = getMemory (ea);
				setNZ (0 == (R & 0x80), R == 0);
				// next ();
				return R;
			};
			operator_actions [(int) AssemblyOperations.LDA] = (op, adrmode) => { a = ldR (op, adrmode); };
			operator_actions [(int) AssemblyOperations.LDX] = (op, adrmode) => { x = ldR (op, adrmode); };
			operator_actions [(int) AssemblyOperations.LDY] = (op, adrmode) => { y = ldR (op, adrmode); };
			
			Action<OpCode,Action<int>,byte> stR = (op, adrmode, R) => {
				adrmode (op.Ticks);
				// fetch ();
				putMemory (ea, R);
				// next ();
			};
			operator_actions [(int) AssemblyOperations.STA] = (op, adrmode) => { stR (op, adrmode, a); };
			operator_actions [(int) AssemblyOperations.STX] = (op, adrmode) => { stR (op, adrmode, x); };
			operator_actions [(int) AssemblyOperations.STY] = (op, adrmode) => { stR (op, adrmode, y); };
			operator_actions [(int) AssemblyOperations.STZ] = (op, adrmode) => { stR (op, adrmode, 0); };
			
			Action<OpCode,Action<int>,bool> branch = (op, adrmode, cond) => {
				if (cond) {
					adrmode (op.Ticks);
					pc += ea;
					Tick (1);
				} else {
					Tick (op.Ticks);
					pc++;
				}
				// fetch ();
				// next ();
			};
			operator_actions [(int) AssemblyOperations.BCC] = (op, adrmode) => { branch (op, adrmode, getC () == 0); };
			operator_actions [(int) AssemblyOperations.BCS] = (op, adrmode) => { branch (op, adrmode, getC () != 0); };
			operator_actions [(int) AssemblyOperations.BNE] = (op, adrmode) => { branch (op, adrmode, getZ () == 0); };
			operator_actions [(int) AssemblyOperations.BEQ] = (op, adrmode) => { branch (op, adrmode, getZ () != 0); };
			operator_actions [(int) AssemblyOperations.BPL] = (op, adrmode) => { branch (op, adrmode, getN () == 0); };
			operator_actions [(int) AssemblyOperations.BMI] = (op, adrmode) => { branch (op, adrmode, getN () != 0); };
			operator_actions [(int) AssemblyOperations.BVC] = (op, adrmode) => { branch (op, adrmode, getV () == 0); };
			operator_actions [(int) AssemblyOperations.BVS] = (op, adrmode) => { branch (op, adrmode, getV () != 0); };
			
			operator_actions [(int) AssemblyOperations.BRA] = (op, adrmode) => {
				adrmode (op.Ticks);
				pc += ea;
				// fetch ();
				Tick (1);
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.JMP] = (op, adrmode) => {
				adrmode (op.Ticks);
				pc = ea;
				CallHandler cb;
				if (Callbacks.Call.TryGetValue (ea, out cb)) {
					ushort addr;
					externalize ();
					if ((addr = cb (this, ea, 0)) != 0) {
						internalize ();
						pc = addr;
					}
				}
				// fetch ();
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.JSR] = (op, adrmode) => {
				pc++;
				push ((byte) (pc >> 8));
				push ((byte) (pc & 0xFF));
				pc--;
				adrmode (op.Ticks);
				CallHandler cb;
				if (Callbacks.Call.TryGetValue (ea, out cb)) {
					ushort addr;
					externalize ();
					if ((addr = cb (this, ea, 0)) != 0) {
						internalize ();
						pc = addr;
						// fetch ();
						// next ();
					}
				}
				pc = ea;
				// fetch ();
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.RTS] = (op, adrmode) => {
				Tick (op.Ticks);
				pc = pop ();
				pc |= (ushort) (pop () << 8);
				pc++;
				// fetch ();
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.BRK] = (op, adrmode) => {
				Tick (op.Ticks);
				pc++;
				push ((byte) (pc >> 8));
				push ((byte) (pc & 0xFF));
				p |= Status.B;
				push (p);
				p |= Status.I;
				unchecked {
					ushort hdlr = (ushort) (getMemory (0xFFFE) + (getMemory (0xFFFF) << 8));
					CallHandler cb;
					if (Callbacks.Call.TryGetValue (hdlr, out cb)) {
						ushort addr;
						externalize ();
						if ((addr = cb (this, (ushort) (pc - 2), 0)) != 0) {
							internalize ();
							hdlr = addr;
						}
					}
					pc = hdlr;
				}
				// fetch ();
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.RTI] = (op, adrmode) => {
				Tick (op.Ticks);
				p = pop ();
				pc = pop ();
				pc |= (ushort) (pop () << 8);
				// fetch ();
				// next ();
			};
			
			operator_actions [(int) AssemblyOperations.NOP] = (op, adrmode) => {
				// fetch ();
				Tick (op.Ticks);
				// next ();
			};
			
			Action<OpCode,Action<int>,byte> phR = (op, adrmode, R) => {
				// fetch ();
				Tick (op.Ticks);
				push (R);
				// next ();
			};
			operator_actions [(int) AssemblyOperations.PHA] = (op, adrmode) => { phR (op, adrmode, a); };
			operator_actions [(int) AssemblyOperations.PHX] = (op, adrmode) => { phR (op, adrmode, x); };
			operator_actions [(int) AssemblyOperations.PHY] = (op, adrmode) => { phR (op, adrmode, y); };
			operator_actions [(int) AssemblyOperations.PHP] = (op, adrmode) => { phR (op, adrmode, p); };
			
			// FIXME: when replacing the source with macros, use direct assignment to R instead of returning R as the original source does.
			Func<OpCode,Action<int>,byte> plR = (op, adrmode) => {
				// fetch ();
				Tick (op.Ticks);
				var R = pop ();
				setNZ (0 != (R & 0x80), R == 0);
				// next ();
				return R;
			};
			operator_actions [(int) AssemblyOperations.PLA] = (op, adrmode) => { a = plR (op, adrmode); };
			operator_actions [(int) AssemblyOperations.PLX] = (op, adrmode) => { x = plR (op, adrmode); };
			operator_actions [(int) AssemblyOperations.PLY] = (op, adrmode) => { y = plR (op, adrmode); };
			
			operator_actions [(int) AssemblyOperations.PLP] = (op, adrmode) => {
				// fetch ();
				Tick (op.Ticks);
				p = pop ();
				// next ();
			};
			
			Action<OpCode,Action<int>,byte> clF = (op, adrmode, F) => {
				// fetch ();
				Tick (op.Ticks);
				p &= (byte) ~F;
				// next ();
			};
			operator_actions [(int) AssemblyOperations.CLC] = (op, adrmode) => { clF (op, adrmode, Status.C); };
			operator_actions [(int) AssemblyOperations.CLD] = (op, adrmode) => { clF (op, adrmode, Status.D); };
			operator_actions [(int) AssemblyOperations.CLI] = (op, adrmode) => { clF (op, adrmode, Status.I); };
			operator_actions [(int) AssemblyOperations.CLV] = (op, adrmode) => { clF (op, adrmode, Status.V); };
			
			Action<OpCode,Action<int>,byte> seF = (op, adrmode, F) => {
				// fetch ();
				Tick (op.Ticks);
				p |= F;
				// next ();
			};
			operator_actions [(int) AssemblyOperations.SEC] = (op, adrmode) => { seF (op, adrmode, Status.C); };
			operator_actions [(int) AssemblyOperations.SED] = (op, adrmode) => { seF (op, adrmode, Status.D); };
			operator_actions [(int) AssemblyOperations.SEI] = (op, adrmode) => { seF (op, adrmode, Status.I); };
			
			// Now we run things with all those delegates above.
			
			while (true) {
				var b = Memory [pc++];
				var code = OpCodes [b];
				if (code == null)
					throw new CpuInvalidOperationException (String.Format ("Invalid operation code: {0:X02}", b));
				operator_actions [(int) code.Operation] (code, addressing_actions [(int) code.Addressing]);
			}
		}
		
		void Tick (int n)
		{
			// do nothing
		}
		
		void TickIf (bool eval)
		{
			// do nothing
		}
		
		int DoDisassemble (ushort addr, out string format)
		{
			byte b = Memory [addr];
			var op = OpCodes [b];
			switch (op.Addressing) {
			case AddressingMode.Implied:
				format = String.Format ("{0} ", op.Operation); return 1;
			case AddressingMode.Immediate:
				format = String.Format ("{0} #{1:X02}", op.Operation, Memory [addr + 1]); return 2;
			case AddressingMode.ZeroPage:
				format = String.Format ("{0} {1:X02}", op.Operation, Memory [addr + 1]); return 2;
			case AddressingMode.ZeroPageX:
				format = String.Format ("{0} {1:X02},X", op.Operation, Memory [addr + 1]); return 2;
			case AddressingMode.ZeroPageY:
				format = String.Format ("{0} {1:X02},Y", op.Operation, Memory [addr + 1]); return 2;
			case AddressingMode.Absolute:
				format = String.Format ("{0} {1:X02}{2:X02}", op.Operation, Memory [addr + 2], Memory [addr + 1]); return 3;
			case AddressingMode.AbsoluteX:
				format = String.Format ("{0} {1:X02}{2:X02},X", op.Operation, Memory [addr + 2], Memory [addr + 1]); return 3;
			case AddressingMode.AbsoluteY:
				format = String.Format ("{0} {1:X02}{2:X02},Y", op.Operation, Memory [addr + 2], Memory [addr + 1]); return 3;
			case AddressingMode.Relative:
				format = String.Format ("{0} {1:X04}", op.Operation, addr + 2 + Memory [addr + 1]); return 2;
			case AddressingMode.Indirect:
				format = String.Format ("{0} ({1:X02}{2:X02})", op.Operation, Memory [addr + 2], Memory [addr + 1]); return 3;
			case AddressingMode.IndirectZeroPage:
				format = String.Format ("{0} ({1:X02})", op.Operation, Memory [addr + 1]); return 2;
			case AddressingMode.IndirectX:
				format = String.Format ("{0} ({1:X02},X)", op.Operation, Memory [addr + 1]); return 2;
			case AddressingMode.IndirectY:
				format = String.Format ("{0} ({1:X02},Y)", op.Operation, Memory [addr + 1]); return 2;
			case AddressingMode.IndirectAbsoluteX:
				format = String.Format ("{0} ({1:X02},{2:X02},X)", op.Operation, Memory [addr + 2], Memory [addr + 1]); return 3;
			}
			
			throw new InvalidOperationException (String.Format ("Unexpected AddressingMode: {0}", op.Addressing));
		}
	}
}
