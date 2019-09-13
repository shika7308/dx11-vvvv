using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using libfreenect2Net;

namespace VVVV.Nodes.Freenect2
{
    public class KinectRuntime : IDisposable
    {
        public Device Runtime { get; private set; }

        public bool IsStarted { get; private set; }

        public event Action<FrameType, Frame> OnColorFrame;
        public event Action<FrameType, Frame> OnDepthFrame;

        [Flags]
        private enum CaptureMode
        {
            None = 0,
            Color = 1,
            Depth = 2,
            Both = 3
        }

        private CaptureMode Mode = CaptureMode.None;
        private string Serial;

        public KinectRuntime()
        {

        }

        public void Dispose()
        {
            Runtime?.Dispose();
        }

        public void Assign(string serial)
        {
            if (serial != Serial)
            {
                this.Stop();
                Serial = serial;
            }
        }

        public void SetDepthMode(bool enable)
        {
            if (this.Runtime == null)
                return;

            var mode = (enable) ?
                this.Mode | CaptureMode.Depth :
                this.Mode & ~CaptureMode.Depth;

            UpdateCaptureMode(mode);
        }

        public void SetColor(bool enable)
        {
            if (this.Runtime == null)
                return;

            var mode = (enable) ?
                this.Mode |= CaptureMode.Color :
                this.Mode &= ~CaptureMode.Color;

            UpdateCaptureMode(mode);
        }

        private void UpdateCaptureMode(CaptureMode mode)
        {
            if (this.Mode == mode)
                return;

            this.Runtime.Stop();
            switch (mode)
            {
                case CaptureMode.Both: this.Runtime.StartAll(); break;
                case CaptureMode.Depth: this.Runtime.StartDepth(); break;
                case CaptureMode.Color: this.Runtime.StartColor(); break;
            }
            this.Mode = mode;
        }

        private void UpdateCaptureMode()
        {
            switch (Mode)
            {
                case CaptureMode.Both: this.Runtime.StartAll(); break;
                case CaptureMode.Depth: this.Runtime.StartDepth(); break;
                case CaptureMode.Color: this.Runtime.StartColor(); break;
            }
        }


        #region Start

        private class FrameListener : IFrameListener
        {
            public Action<FrameType, Frame> OnFrame;

            /// <summary>
            /// Invoke OnFrame event.
            /// </summary>
            /// <param name="frameType">a frame type</param>
            /// <param name="frame">a frame object</param>
            /// <returns>a pair of frame type and frame object. The frame object must be dispose by yourself</returns>
            public bool OnNewFrame(FrameType frameType, Frame frame)
            {
                try
                {
                    OnFrame?.Invoke(frameType, frame);
                }
                catch
                {

                }
                return true;
            }
        }

        public void Start()
        {
            if (this.IsStarted)
                this.Stop();

            if (this.Runtime != null)
                this.Stop();

            try
            {
                this.Runtime = findDevice();
                var depthListener = new FrameListener();
                depthListener.OnFrame = (a, b) => OnDepthFrame?.Invoke(a, b);
                this.Runtime.SetDepthListener(depthListener);
                var colorListener = new FrameListener();
                colorListener.OnFrame = (a, b) => OnColorFrame?.Invoke(a, b);
                this.Runtime.SetColorListener(colorListener);
                UpdateCaptureMode();
                //Runtime.StartAll();
                this.IsStarted = true;
            }
            catch
            {
                this.IsStarted = false;
            }

            Device findDevice()
            {
                //var con = new Context();
                //var l = con.EnumerateDevices();
                //for (var i = 0; i < l; i++)
                //{
                //    if (con.GetDeviceSerialNumber(i) == Serial)
                //        return con.OpenDevice(i);
                //}
                return KinectFinder.Context.OpenDevice(Serial);
                throw new Exception($"Kinect device is not find: {Serial}");
            }
        }


        #endregion

        #region Stop
        public void Stop()
        {
            if (this.Runtime != null)
            {
                if (this.IsStarted)
                {
                    try
                    {
                        this.Runtime.Dispose();
                        this.Runtime = null;
                    }
                    catch
                    {

                    }
                }

                this.IsStarted = false;
            }
        }
        #endregion
    }
}
