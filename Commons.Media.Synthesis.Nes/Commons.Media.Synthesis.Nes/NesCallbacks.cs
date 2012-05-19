using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Commons.Media.Synthesis.Nes
{
	public delegate byte ReadHandler (NesCpu cpu, ushort address);
	public delegate void WriteHandler (NesCpu cpu, ushort address, byte data);
	public delegate byte CallHandler (NesCpu cpu, ushort address, byte data);

	public abstract class NesCallbacks
	{
		public abstract class Set<T>
		{
			public abstract T this [int address] { get; set; }
			public abstract bool TryGet (int address, out T handler);
		}
	
		public abstract Set<ReadHandler> Read { get; }
		public abstract Set<WriteHandler> Write { get; }
		public abstract Set<CallHandler> Call { get; }
	}
	
	internal class SimpleNesCallbacks : NesCallbacks
	{
		SimpleSet<ReadHandler> reads = new SimpleSet<ReadHandler> ();
		SimpleSet<WriteHandler> writes = new SimpleSet<WriteHandler> ();
		SimpleSet<CallHandler> calls  = new SimpleSet<CallHandler> ();
		
		class SimpleSet<T> : NesCallbacks.Set<T>
		{
			Dictionary<int,T> d = new Dictionary<int,T> ();
			
			public override T this [int address] {
				get { return d [address]; }
				set { d [address] = value; }
			}
			
			public override bool TryGet (int address, out T handler)
			{
				return d.TryGetValue (address, out handler);
			}
		}
		
		public override Set<ReadHandler> Read { get { return reads; } }
		public override Set<WriteHandler> Write { get { return writes; } }
		public override Set<CallHandler> Call { get { return calls; } }		
	}
	
	internal class Nes2a03Callbacks : NesCallbacks
	{
		Nes2a03Set<ReadHandler> reads;
		Nes2a03Set<WriteHandler> writes;
		Nes2a03Set<CallHandler> calls;
		
		public Nes2a03Callbacks ()
		{
			reads = new Nes2a03Set<ReadHandler> (ReadApu);
			writes = new Nes2a03Set<WriteHandler> (WriteApu);
			calls  = new Nes2a03Set<CallHandler> (CallApu);
		}
		
		byte ReadApu (NesCpu cpu, ushort address)
		{
			return cpu.Machine.Apu.GetRegister (address);
		}
		
		void WriteApu (NesCpu cpu, ushort address, byte data)
		{
			cpu.Machine.Apu.SetRegister (address, data);
		}
		
		byte CallApu (NesCpu cpu, ushort address, byte data)
		{
			throw new CpuInvalidOperationException ("Call operation to APU is not supported");
		}
		
		class Nes2a03Set<T> : NesCallbacks.Set<T>
		{
			T apu_handler;
			
			public Nes2a03Set (T apuHandler)
			{
				apu_handler = apuHandler;
			}

			public override T this [int address] {
				get {
					T ret;
					if (TryGet (address, out ret))
						return ret;
					throw new ArgumentException (String.Format ("Unsupported address: {0:X}", address));
				}
				set { throw new NotSupportedException (); }
			}
			
			public override bool TryGet (int address, out T handler)
			{
				// APU addresses
				if (0x4000 <= address && address <= 0x4017) {
					handler = apu_handler;
					return true;
				}
				
				// other addresses are not handled (at least yet)
				
				handler = default (T);
				return false;
			}
		}
		
		public override Set<ReadHandler> Read { get { return reads; } }
		public override Set<WriteHandler> Write { get { return writes; } }
		public override Set<CallHandler> Call { get { return calls; } }		
	}
}
