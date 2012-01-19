﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		public struct FplOptionsStruct
		{
			public int StructSize;
			public int Alignment;
		}

		public class FixedPool
		{
			public class WaitItem
			{
				public HleThread Thread;
				public Action WakeUp;
			}

			public HleState HleState;
			public HleMemoryManager MemoryManager;
			public string Name;
			public HleMemoryManager.Partitions PartitionId;
			public int Attributes;
			public int BlockSize;
			public int NumberOfBlocks;
			public MemoryPartition MemoryPartition;
			public List<uint> FreeBlocks;
			public List<uint> UsedBlocks;
			public List<WaitItem> WaitItemList;
			public FplOptionsStruct Options;

			public void Init()
			{
				var Alignment = Options.Alignment;
				if (Alignment == 0) Alignment = 1;
				var Partition = MemoryManager.GetPartition(PartitionId);
				this.MemoryPartition = Partition.Allocate(NumberOfBlocks * BlockSize, Hle.MemoryPartition.Anchor.Low, 0, Alignment);
				this.FreeBlocks = new List<uint>();
				this.UsedBlocks = new List<uint>();
				this.WaitItemList = new List<WaitItem>();
				for (int n = 0; n < NumberOfBlocks; n++)
				{
					this.FreeBlocks.Add(GetAddressFromBlockIndex(n));
				}

				//Console.Error.WriteLine(this);
			}

			public uint GetAddressFromBlockIndex(int Index)
			{
				return (uint)(MemoryPartition.Low + Index * BlockSize);
			}

			public void Allocate(PspPointer* DataPointer, uint* Timeout, bool HandleCallbacks)
			{
				if (!TryAllocate(DataPointer))
				{
					if (Timeout != null) throw (new NotImplementedException());
					var CurrentThread = HleState.ThreadManager.Current;
					CurrentThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Semaphore, "_sceKernelAllocateVplCB", (WakeUp) =>
					{
						WaitItemList.Add(new WaitItem()
						{
							Thread = CurrentThread,
							WakeUp = () =>
							{
								WakeUp();
								Allocate(DataPointer, Timeout, HandleCallbacks);
							},
						});
					}, HandleCallbacks: HandleCallbacks);
				}
			}

			public bool TryAllocate(PspPointer* DataPointer)
			{
				if (FreeBlocks.Count > 0)
				{
					var AllocatedBlock = FreeBlocks.First();
					FreeBlocks.Remove(AllocatedBlock);
					UsedBlocks.Add(AllocatedBlock);
					//Console.Error.WriteLine("TryAllocate(0x{0:X})", AllocatedBlock);
					*DataPointer = AllocatedBlock;

					return true;
				}
				else
				{
					return false;
				}
			}

			public void Free(PspPointer DataPointer)
			{
				if (!UsedBlocks.Contains(DataPointer))
				{
					throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMBLOCK));
				}
				UsedBlocks.Remove(DataPointer);
				FreeBlocks.Add(DataPointer);

				foreach (var WaitItem in WaitItemList.ToArray())
				{
					//Console.Error.WriteLine("Free!");
					WaitItemList.Remove(WaitItem);
					WaitItem.WakeUp();
					HleState.ThreadManager.Current.CpuThreadState.Yield();
					break;
				}
			}

			public override string ToString()
			{
				return this.ToStringDefault();
			}
		}

		public enum PoolId : int { }

		HleUidPoolSpecial<FixedPool, PoolId> FixedPoolList = new HleUidPoolSpecial<FixedPool, PoolId>()
		{
			OnKeyNotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_FPOOL,
		};

		/// <summary>
		/// Create a fixed pool
		/// </summary>
		/// <param name="Name">Name of the pool</param>
		/// <param name="PartitionId">The memory partition ID</param>
		/// <param name="Attributes">Attributes</param>
		/// <param name="BlockSize">Size of pool block</param>
		/// <param name="NumberOfBlocks">Number of blocks to allocate</param>
		/// <param name="Options">Options (set to NULL)</param>
		/// <returns>The UID of the created pool, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xC07BB470, FirmwareVersion = 150)]
		public PoolId sceKernelCreateFpl(string Name, HleMemoryManager.Partitions PartitionId, int Attributes, int BlockSize, int NumberOfBlocks, FplOptionsStruct* Options)
		{
			var FixedPool = new FixedPool()
			{
				HleState = HleState,
				MemoryManager = HleState.MemoryManager,
				Name = Name,
				PartitionId = PartitionId,
				Attributes = Attributes,
				BlockSize = BlockSize,
				NumberOfBlocks = NumberOfBlocks,
			};
			if (Options != null) FixedPool.Options = *Options;
			FixedPool.Init();

			return FixedPoolList.Create(FixedPool);
		}

		/// <summary>
		/// Try to allocate from the pool immediately.
		/// </summary>
		/// <param name="PoolId">The UID of the pool</param>
		/// <param name="DataPointerPointer">Receives the address of the allocated data</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x623AE665, FirmwareVersion = 150)]
		public int sceKernelTryAllocateFpl(PoolId PoolId, PspPointer* DataPointer)
		{
			var FixedPool = FixedPoolList.Get(PoolId);
			if (!FixedPool.TryAllocate(DataPointer))
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_NO_MEMORY));
			}

			return 0;
		}

		/// <summary>
		/// Allocate from the pool. It will wait for a free block to be available the specified time.
		/// </summary>
		/// <param name="PoolId">The UID of the pool</param>
		/// <param name="DataPointer">Receives the address of the allocated data</param>
		/// <param name="Timeout">Amount of time to wait for allocation?</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD979E9BF, FirmwareVersion = 150)]
		public int sceKernelAllocateFpl(PoolId PoolId, PspPointer* DataPointer, uint* Timeout)
		{
			var FixedPool = FixedPoolList.Get(PoolId);
			FixedPool.Allocate(DataPointer, Timeout, HandleCallbacks: false);
			return 0;
		}

		/// <summary>
		/// Free a block
		/// </summary>
		/// <param name="PoolId">The UID of the pool</param>
		/// <param name="DataPointer">The data block to deallocate</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xF6414A71, FirmwareVersion = 150)]
		public int sceKernelFreeFpl(PoolId PoolId, PspPointer DataPointer)
		{
			var FixedPool = FixedPoolList.Get(PoolId);
			FixedPool.Free(DataPointer);
			return 0;
		}

		/// <summary>
		/// Delete a fixed pool
		/// </summary>
		/// <param name="PoolId">The UID of the pool</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xED1410E0, FirmwareVersion = 150)]
		public int sceKernelDeleteFpl(PoolId PoolId)
		{
			FixedPoolList.Remove(PoolId);
			return 0;
		}
	}
}