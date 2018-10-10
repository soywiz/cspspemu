﻿using System;

namespace CSPspEmu.Hle
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HlePspNotImplementedAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public bool PartialImplemented = true;

        /// <summary>
        /// 
        /// </summary>
        public bool Notice = true;
    }
}