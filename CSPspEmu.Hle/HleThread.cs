﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Threading;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using System.Diagnostics;

namespace CSPspEmu.Hle
{
	public class HleThread
	{
		protected MethodCache MethodCache;

		public int Priority = 1;
		public int PriorityValue;
		protected GreenThread GreenThread;
		public CpuThreadState CpuThreadState { get; protected set; }
		protected int MinimalInstructionCountForYield = 1000000;
		public int Id;
		public String Name;
		public Status CurrentStatus;
		public WaitType CurrentWaitType;
		public DateTime AwakeOnTime;
		public MemoryPartition Stack;
		public uint EntryPoint;
		public int InitPriority;
		public uint Attribute;

		public enum WaitType
		{
			None = 0,
			Timer = 1,
		}

		public enum Status {
			Running = 1,
			Ready = 2,
			Waiting = 4,
			Suspend = 8,
			Stopped = 16,
			Killed = 32,
		}

		public HleThread(CpuThreadState CpuThreadState)
		{
			this.MethodCache = CpuThreadState.CpuProcessor.MethodCache;
			this.GreenThread = new GreenThread();
			this.CpuThreadState = CpuThreadState;
			this.PrepareThread();
		}

		protected void PrepareThread()
		{
			GreenThread.InitAndStartStopped(MainLoop);
		}

		protected void MainLoop()
		{
			while (true)
			{
				//Debug.WriteLine("Thread({0:X}) : PC: {1:X}", this.Id, CpuThreadState.PC);
				GetDelegateAt(CpuThreadState.PC)(CpuThreadState);
			}
		}

		// 8903E08

		public Action<CpuThreadState> GetDelegateAt(uint PC)
		{
			//var MethodCache = CpuThreadState.CpuProcessor.MethodCache;

			var Delegate = MethodCache.TryGetMethodAt(PC);
			if (Delegate == null)
			{
				MethodCache.SetMethodAt(
					PC,
					Delegate = CpuThreadState.CreateDelegateForPC(new PspMemoryStream(CpuThreadState.CpuProcessor.Memory), PC)
				);
			}

			return Delegate;
		}

		public void Step(int InstructionCountForYield = 1000000)
		{
			CpuThreadState.StepInstructionCount = InstructionCountForYield;
			//this.MinimalInstructionCountForYield = InstructionCountForYield;
			GreenThread.SwitchTo();
		}
	}
}
