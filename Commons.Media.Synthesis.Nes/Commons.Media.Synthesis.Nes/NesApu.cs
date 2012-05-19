// Primary reference http://nesdev.parodius.com/apu_ref.txt
using System;
using System.Collections.Generic;

namespace Commons.Media.Synthesis.Nes
{
	public class NesApu
	{
		internal NesApu (NesMachine machine)
		{
			regs = new byte [0x16];
			square1 = new SquareOscillator (regs, 0);
			square2 = new SquareOscillator (regs, 4);
			triangle = new TriangleOscillator (regs);
			noise = new NoiseOscillator (regs);
			dmc = new DeltaModulator (regs);
			Output = new SoundModule ();
			
			oscillators = new SoundGenerator [] { square1, square2, triangle, noise, dmc };
		}
		
		SquareOscillator square1, square2;
		TriangleOscillator triangle;
		NoiseOscillator noise;
		DeltaModulator dmc;
		readonly byte [] regs;

		SoundGenerator [] oscillators;

		public SoundModule Output { get; private set; }
		
		// Here I omit index range check to maximize code inlining (ABC would still work with my own code, but inline is not likely).
		public byte GetRegister (int position)
		{
			return regs [position - 0x4000];
		}

		public void SetRegister (int position, byte value)
		{
			regs [position - 0x4000] = value;
		}
	}
	
	public abstract class SoundGenerator
	{
	}
	
	public class SquareOscillator : SoundGenerator
	{
		byte [] regs;
		int offset;
		
		public SquareOscillator (byte [] regs, int offset)
		{
			this.regs = regs;
		}
		
		
		// These properties should be inlined by the JIT so they wouldn't harm performance.
		// [xx------ -------- -------- --------]
		public byte DutyCycle {
			get { return (byte) ((regs [offset] >> 6) & 3); }
		}
		
		// [--x----- -------- -------- --------]
		public bool TimeCounterEnabled {
			get { return (regs [offset] & 0x20) != 0; }
		}
		
		// [---x---- -------- -------- --------]
		public bool EnvironmentDisabled {
			get { return (regs [offset] & 0x10) != 0; }
		}
		
		// [----xxxx -------- -------- --------]
		public byte Volume {
			get { return (byte) (regs [offset] & 0xF); }
		}
		
		// [-------- x------- -------- --------]
		public bool EnableSweep {
			get { return (regs [offset + 1] & 0x80) != 0; }
		}
		
		// [-------- -xxx---- -------- --------]
		public byte SweepPeriod {
			get { return (byte) ((regs [offset + 1] & 0x70) >> 4); }
		}
		
		// [-------- ----x--- -------- --------]
		public bool IsNegativeSweep {
			get { return (regs [offset + 1] & 0x8) != 0; }
		}
		
		// [-------- -----xxx -------- --------]
		public byte SweepShift {
			get { return (byte) (regs [offset + 1] & 0x7); }
		}
		
		// [-------- -------- xxxxxxxx -----xxx]
		public ushort Period {
			get { return (ushort) (regs [offset + 2] + ((regs [offset + 3] & 0x7) << 8)); }
		}
		
		// [-------- -------- -------- xxxxx---]
		public byte LengthIndex {
			get { return (byte) ((regs [offset + 3] & 0xFC) >> 3); }
		}
	}
	
	public class TriangleOscillator : SoundGenerator
	{
		byte [] regs;
		
		public TriangleOscillator (byte [] regs)
		{
			this.regs = regs;
		}
		
		// These properties should be inlined by the JIT so they wouldn't harm performance.
		// [x------- -------- --------]
		public bool TimeCounterEnabled {
			get { return (regs [8] & 0x80) != 0; }
		}
		
		// [-xxxxxxxx -------- --------]
		public byte LinearCounterLoad {
			get { return (byte) (regs [8] & ~0x80); }
		}
		
		// [-------- xxxxxxxxx -----xxx]
		public ushort Period {
			get { return (ushort) (regs [0xA] + ((regs [10] & 0x7) << 3)); }
		}
		
		// [-------- -------- xxxxx---]
		public byte LengthIndex {
			get { return (byte) ((regs [0xB] & 0xFC) >> 3); }
		}
	}
	
	public class NoiseOscillator : SoundGenerator
	{
		byte [] regs;
		
		public NoiseOscillator (byte [] regs)
		{
			this.regs = regs;
		}
		
		// [--x----- ******** -------- --------]
		public bool TimeCounterEnabled {
			get { return (regs [0xC] & 0x20) != 0; }
		}
		
		// [---x---- ******** -------- --------]
		public bool EnvironmentDisabled {
			get { return (regs [0xC] & 0x10) != 0; }
		}
		
		// [----xxxx ******** -------- --------]
		public byte Volume {
			get { return (byte) (regs [0xC] & 0xF); }
		}

		// [-------- ******** x------- --------]
		public bool ShortMode {
			get { return (regs [0xD] & 0x80) != 0; }
		}
		
		// [-------- ******** ----xxxx --------]
		public byte Period {
			get { return (byte) (regs [0xE] & 0xF); }
		}
		
		// [-------- ******** -------- xxxxx---]
		public byte LengthIndex {
			get { return (byte) ((regs [0xF] & 0xFC) >> 3); }
		}
	}
	
	public class DeltaModulator : SoundGenerator
	{
		byte [] regs;
		
		public DeltaModulator (byte [] regs)
		{
			this.regs = regs;
		}
		
		// not implemented yet.
	}
}

