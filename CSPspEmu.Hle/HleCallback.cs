﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle
{
	sealed public class HleCallback
	{
		public string Name { get; private set; }
		public uint Function { get; private set; }
		public object[] Arguments { get; private set; }
		public Action ExecutedNotify;

		private HleCallback()
		{
		}

		static public HleCallback Create(string Name, uint Function, params object[] Arguments)
		{
			return new HleCallback() { Name = Name, Function = Function, Arguments = Arguments };
		}

		public override string ToString()
		{
			return String.Format("HleCallback(Name='{0}', Function=0x{1:X})", Name, Function);
		}

		public void SetArgumentsToCpuThreadState(CpuThreadState CpuThreadState)
		{
			HleInterop.SetArgumentsToCpuThreadState(CpuThreadState, Function, Arguments);
		}
	}
}