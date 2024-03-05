using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace meadow1
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private IAnalogInputPort a0;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            
            a0 = Device.CreateAnalogInputPort(Device.Pins.A00);


            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");
            return ReadVoltage();
        }

        async Task ReadVoltage()
        {
            while (true)
            {
                var voltage1 = a0.Read().Result;
                Resolver.Log.Info($"Voltage A0: {voltage1}");
            }
        }
    }
}