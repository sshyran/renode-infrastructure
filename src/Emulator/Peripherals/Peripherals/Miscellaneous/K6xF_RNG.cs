﻿//
// Copyright (c) 2010-2020 Antmicro
//
//  This file is licensed under the MIT License.
//  Full license text is available in 'licenses/MIT.txt'.
//
using System;
using System.Collections.Generic;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.Bus;

namespace Antmicro.Renode.Peripherals.Miscellaneous
{
    public class K6xF_RNG : IDoubleWordPeripheral, IKnownSize
    {
        public K6xF_RNG(Machine machine)
        {
            IRQ = new GPIO();

            var registerMap = new Dictionary<long, DoubleWordRegister>
            {
                {(long)Registers.Control, new DoubleWordRegister(this)
                    .WithReservedBits(5, 26)
                    .WithFlag(4, out sleep, name: "SLP")
                    .WithTaggedFlag("CLRI", 3)
                    .WithTaggedFlag("INTM", 2)
                    .WithTaggedFlag("HA", 1)
                    .WithFlag(0, out enable, name: "GO")
                },
                {(long)Registers.Status, new DoubleWordRegister(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(16, 8, name: "OREG_SIZE")
                    .WithValueField(8, 8, FieldMode.Read, valueProviderCallback: _ => (enable.Value) ? 1u : 0u, name: "OREG_LVL")
                    .WithReservedBits(5, 3)
                    .WithTaggedFlag("SLP", 4)
                    .WithTaggedFlag("ERRI", 3)
                    .WithTaggedFlag("ORU", 2)
                    .WithTaggedFlag("LRS", 1)
                    .WithTaggedFlag("SECV", 0)
                },
                {(long)Registers.Entropy, new DoubleWordRegister(this)
                    .WithValueField(0, 32, name:"EXT_ENT")
                },
                {(long)Registers.Output, new DoubleWordRegister(this)
                    .WithValueField(0, 32, FieldMode.Read, valueProviderCallback: _ => (enable.Value) ? unchecked((uint)rng.Next()) : 0u, name: "RNDOUT")
                }
            };

            registers = new DoubleWordRegisterCollection(this, registerMap);
        }

        public long Size => 0x1000;

        public uint ReadDoubleWord(long offset)
        {
            var value = registers.Read(offset);
            return value;
        }

        public void Reset()
        {
            registers.Reset();
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            registers.Write(offset, value);
        }

        private DoubleWordRegisterCollection registers;
        private PseudorandomNumberGenerator rng = EmulationManager.Instance.CurrentEmulation.RandomGenerator;
        private IFlagRegisterField enable;
        private IFlagRegisterField sleep;
        public GPIO IRQ { get; private set; }

        private enum Registers
        {
            Control = 0x0,
            Status  = 0x4,
            Entropy = 0x8,
            Output  = 0xC
        }
    }
}