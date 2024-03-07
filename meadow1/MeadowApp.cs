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
        private IAnalogInputPort a1;
        private IAnalogInputPort a2;
        private IAnalogInputPort a3;
        ISerialMessagePort serialPort;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            a0 = Device.CreateAnalogInputPort(Device.Pins.A00);
            a1 = Device.CreateAnalogInputPort(Device.Pins.A01);
            a2 = Device.CreateAnalogInputPort(Device.Pins.A02);
            a3 = Device.CreateAnalogInputPort(Device.Pins.A03);
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
                var voltage2 = a1.Read().Result;
                var voltage3 = a2.Read().Result;
                var voltage4 = a3.Read().Result;
                Resolver.Log.Info($"{voltage1}");
                Resolver.Log.Info($"{voltage2}");
                Resolver.Log.Info($"{voltage3}");
                Resolver.Log.Info($"{voltage4}");
                
                // Wait for a second
                Thread.Sleep(900);
            }
        }
        
        
    }
}