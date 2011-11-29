﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Runner.Components.Display
{
	sealed public class DisplayComponentThread : ComponentThread
	{
		private HleInterruptManager HleInterruptManager;

		public override void InitializeComponent()
		{
			this.HleInterruptManager = PspEmulatorContext.GetInstance<HleInterruptManager>();
		}

		protected override string ThreadName { get { return "DisplayThread"; } }

		protected override void Main()
		{
			while (true)
			{
				ThreadTaskQueue.HandleEnqueued();
				if (!Running) return;

				HleInterruptManager.GetInterruptHandler(PspInterrupts.PSP_VBLANK_INT).Trigger();
				//PspDisplay.Update();
				Thread.Sleep(TimeSpan.FromSeconds(1.0 / 59.94));
			}
		}
	}
}