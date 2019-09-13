using libfreenect2Net;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Freenect2
{
    [PluginInfo(Name = "Kinect Finder",
                Category = "Devices",
                Version = "Libfreenect",
                Author = "yamanaka",
                Tags = "DX11",
                Help = "Find all Kinect2 connected to machine.")]

    public class KinectFinder : IPluginEvaluate
    {
        public static Context Context = new Context();

        [Input("Scan", IsBang = true)]
        public ISpread<bool> FInScan;

        [Output("Kinect ID 1")]
        public ISpread<string> FOutKinectID1;

        [Output("Kinect ID 2")]
        public ISpread<string> FOutKinectID2;

        [Output("Kinect ID 3")]
        public ISpread<string> FOutKinectID3;

        [Output("Kinect ID 4")]
        public ISpread<string> FOutKinectID4;

        private Action<string>[] ids;
        private Context kinectHandler => Context;

        public KinectFinder()
        {
            ids = new Action<string>[]
            {
                s => FOutKinectID1[0] = s,
                s => FOutKinectID2[0] = s,
                s => FOutKinectID3[0] = s,
                s => FOutKinectID4[0] = s,
            };
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInScan[0])
            {
                foreach (var i in ids)
                    i(string.Empty);
                var cnt = Math.Min(ids.Length - 1, kinectHandler.EnumerateDevices());
                for (var i = 0; i < cnt; i++)
                    ids[i](kinectHandler.GetDeviceSerialNumber(i));
            }
        }
    }
}
