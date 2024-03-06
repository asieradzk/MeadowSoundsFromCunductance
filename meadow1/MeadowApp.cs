using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using System.IO.Ports;

namespace meadow1
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private IAnalogInputPort a0;
        ISerialMessagePort serialPort;

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
                Resolver.Log.Info($"{voltage1}");
                // Wait for a second
                Thread.Sleep(900);
            }
        }
        
        
    }
}