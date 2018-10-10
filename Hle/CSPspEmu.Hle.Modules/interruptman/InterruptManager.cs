﻿using System;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.interruptman
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00000011)]
    public unsafe class InterruptManager : HleModuleHost
    {
        [Inject] HleInterruptManager HleInterruptManager;

        private static void CheckImplementedInterruptType(PspInterrupts PspInterrupt)
        {
            switch (PspInterrupt)
            {
                case PspInterrupts.PspVblankInt: break;

                default: //throw(new NotImplementedException($"Can't handle '{PspInterrupt}'"));
                    break;
            }
        }

        //Interrupts.Callback[int][int] handlers;
        //PspCallback[int][int] handlers;

        /// <summary>
        /// Register a sub interrupt handler.
        /// </summary>
        /// <param name="PspInterrupt">The interrupt number to register.</param>
        /// <param name="HandlerIndex">The sub interrupt handler number (user controlled) (0-15)</param>
        /// <param name="CallbackAddress">The interrupt handler</param>
        /// <param name="CallbackArgument">An argument passed to the interrupt handler</param>
        /// <returns>Less than 0 on error</returns>
        [HlePspFunction(NID = 0xCA04A2B9, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceKernelRegisterSubIntrHandler(PspInterrupts PspInterrupt, int HandlerIndex, uint CallbackAddress,
            uint CallbackArgument)
        {
            CheckImplementedInterruptType(PspInterrupt);

            var HleSubinterruptHandler =
                HleInterruptManager.GetInterruptHandler(PspInterrupt).SubinterruptHandlers[HandlerIndex];
            {
                HleSubinterruptHandler.Address = CallbackAddress;
                HleSubinterruptHandler.Argument = CallbackArgument;
            }

            return 0;
        }

        /*
        CallbacksHandler.Type convertPspSubInterruptsToCallbacksHandlerType(PspSubInterrupts intno) {
            switch (intno) {
                case PspSubInterrupts.PSP_DISPLAY_SUBINT: return CallbacksHandler.Type.VerticalBlank;
                default:
                    throw(new Exception("Unhandled convertPspSubInterruptsToCallbacksHandlerType.PspSubInterrupts"));
                break;
            }
        }
        */

        /// <summary>
        /// Enable a sub interrupt.
        /// </summary>
        /// <param name="PspInterrupt">The sub interrupt to enable.</param>
        /// <param name="HandlerIndex">The sub interrupt handler number (0-15)</param>
        /// <returns>Less than 0 on error</returns>
        [HlePspFunction(NID = 0xFB8E22EC, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceKernelEnableSubIntr(PspInterrupts PspInterrupt, int HandlerIndex)
        {
            CheckImplementedInterruptType(PspInterrupt);

            var HleSubinterruptHandler =
                HleInterruptManager.GetInterruptHandler(PspInterrupt).SubinterruptHandlers[HandlerIndex];
            {
                HleSubinterruptHandler.Enabled = true;
            }

            return 0;
        }

        /// <summary>
        /// Release a sub interrupt handler.
        /// </summary>
        /// <param name="PspInterrupt">The interrupt number to register.</param>
        /// <param name="HandlerIndex">The sub interrupt handler number (0-15)</param>
        /// <returns>Less than 0 on error</returns>
        [HlePspFunction(NID = 0xD61E6961, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceKernelReleaseSubIntrHandler(PspInterrupts PspInterrupt, int HandlerIndex)
        {
            CheckImplementedInterruptType(PspInterrupt);

            var HleSubinterruptHandler =
                HleInterruptManager.GetInterruptHandler(PspInterrupt).SubinterruptHandlers[HandlerIndex];
            {
                HleSubinterruptHandler.Enabled = false;
            }

            return 0;
        }

        /// <summary>
        /// Queries the status of a sub interrupt handler.
        /// </summary>
        /// <param name="intno">The interrupt number to register.</param>
        /// <param name="sub_intr_code">?</param>
        /// <param name="data"></param>
        /// <returns>Less than 0 on error</returns>
        [HlePspFunction(NID = 0xD2E8363F, FirmwareVersion = 150)]
        [HlePspFunction(NID = 0x36B1EF81, FirmwareVersion = 150)]
        public int sceKernelQueryIntrHandlerInfo(int intno, int sub_intr_code, PspIntrHandlerOptionParam* data)
        {
            throw (new NotImplementedException());
            /*
            unimplemented();
            return -1;
            */
        }
    }
}