using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Freenect2
{
    [PluginInfo(Name = "Freenect2", 
	            Category = "Kinect2", 
	            Version = "Libfreenect", 
	            Author = "yamanaka", 
	            Tags = "DX11",
	            Help = "Provides access to a Kinect through the MSKinect API, supports Kinect Studio with device unplugged")]
    public class Freenect2Node : IPluginEvaluate, IDisposable
    {
        /* no multiple kinect support ahead
        [Input("Index", IsSingle = true)]
        IDiffSpread<int> FInIndex;
        */
        [Input("Serial", IsSingle = true)]
        protected ISpread<string> FInSerial;

        [Input("Enable Color", IsSingle = true, DefaultValue = 1)]
        protected IDiffSpread<bool> FInEnableColor;

        [Input("Enable Depth", IsSingle = true, DefaultValue = 1)]
        protected IDiffSpread<bool> FInDepthMode;

        [Input("Enabled", IsSingle = true)]
        protected IDiffSpread<bool> FInEnabled;

        [Input("Reset", IsBang = true)]
        protected ISpread<bool> FInReset;

        [Output("Kinect Runtime", IsSingle = true)]
        protected ISpread<KinectRuntime> FOutRuntime;

        [Output("Is Started", IsSingle = true)]
        protected ISpread<bool> FOutStarted;

        [Output("CxCyFxFy")]
        protected ISpread<Vector4D> FOutCxCyFxFy;

        [Output("Serial")]
        protected ISpread<string> FOutKinectID;

        private KinectRuntime runtime = new KinectRuntime();

        public void Evaluate(int SpreadMax)
        {
            bool reset = false;

            if (string.IsNullOrEmpty(this.FInSerial[0]))
            {
                reset = true;
            }

            if (this.FInSerial.IsChanged)
            {
                this.runtime.Assign(this.FInSerial[0]);
                reset = true;
            }
            
            if (this.FInReset[0] && !string.IsNullOrEmpty(this.FInSerial[0]))
            {
                reset = true;
            }

            if (this.FInEnabled.IsChanged || reset)
            {
                if (this.FInEnabled[0])
                {
                    this.runtime.Start();
                    var param = this.runtime.Runtime.InfraRedCameraParameters;
                    if (param.FocalLengthX == 0) param.FocalLengthX = 1;
                    if (param.FocalLengthY == 0) param.FocalLengthY = 1;
                    FOutCxCyFxFy[0] = new Vector4D(param.PrincipalPointX, param.PrincipalPointY, param.FocalLengthX, param.FocalLengthY);
                }
                else
                {
                    this.runtime.Stop();
                }

                reset = true;
            }

            if (this.FInDepthMode.IsChanged || reset)
            {
                this.runtime.SetDepthMode(this.FInDepthMode[0]);
            }


            if (this.FInEnableColor.IsChanged || reset)
            {
                this.runtime.SetColor(this.FInEnableColor[0]);
            }

            this.FOutRuntime[0] = runtime;
            this.FOutStarted[0] = runtime.IsStarted;


            if (runtime.Runtime != null)
            {
                //runtime only reports ID of the physically connected device. It seems Kinect Tools injected stream does not report ID of the device.
                this.FOutKinectID[0] = this.FInSerial[0];
            }
        }

        public void Dispose()
        {
            if (this.runtime != null)
            {
                this.runtime.Stop();
            }
        }
    }
}
