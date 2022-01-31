//
// Copyright (c) 2010-2022 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using Antmicro.Renode.Core;

namespace Antmicro.Renode.Peripherals.Miscellaneous
{
    public class ResetPin : IGPIOReceiver
    {
        public ResetPin(Machine machine, bool invert = true)
        {
            inverted = invert;
            this.machine = machine;
            state = invert;
            sync = new object();
        }

        public void OnGPIO(int number, bool value)
        {
            if(number != 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            State = inverted ? !value : value;
        }

        public void Reset()
        {
            state = inverted;
        }

        public bool State
        {
            get => state;

            private set
            {
                lock(sync)
                {
                    if(value == state)
                    {
                        return;
                    }

                    state = value;

                    if(state != inverted)
                    {
                        machine.RequestReset();
                    }
                }
            }
        }

        private bool state;

        private readonly bool inverted;
        private readonly Machine machine;
        private readonly object sync;
    }
}