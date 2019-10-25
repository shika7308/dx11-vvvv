using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using System.Runtime.InteropServices;
using libfreenect2Net;

namespace VVVV.Nodes.Freenect2
{
    [PluginInfo(Name = "Freenect Depth",
	            Category = "Kinect2", 
	            Version = "Libfreenect", 
	            Author = "yamanaka",
	            Tags = "DX11, texture",
	            Help = "Returns a 16bit depthmap from the Kinects depth camera.")]
    public class KinectDepthTextureNode : KinectBaseTextureNode
    {
        private IntPtr depthRead;
        private IntPtr depthWrite;

        private int _width;
        private int _height;

        public KinectDepthTextureNode() : base()
        {
            UpdateBuffer(512, 424);
        }

        private void DepthFrameReady(FrameType type, Frame frame)
        {
            if (frame == null)
                return;

            using (frame)
            {
                lock (m_lock)
                {
                    UpdateBuffer(frame.Width, frame.Height);
                    copy(frame, this.depthWrite);
                    var swap = this.depthRead;
                    this.depthRead = this.depthWrite;
                    this.depthWrite = swap;
                }

                this.FInvalidate = true;
                System.Threading.Interlocked.Increment(ref this.frameindex);
            }
        }

        private void UpdateBuffer(int width, int height)
        {
            if (this._width == width && this._height == height)
                return;

            this._width = width;
            this._height = height;
            this.Resized = true;

            if (this.depthRead == IntPtr.Zero)
            {
                this.depthRead = Marshal.AllocHGlobal(width * height * 2);
                this.depthWrite = Marshal.AllocHGlobal(width * height * 2);
            }
            else
            {
                this.depthRead = Marshal.ReAllocHGlobal(this.depthRead, (IntPtr)(width * height * 2));
                this.depthWrite = Marshal.ReAllocHGlobal(this.depthWrite, (IntPtr)(width * height * 2));
            }
        }

        unsafe private void copy(Frame source, IntPtr dest)
        {
            var src = (float*)source.Data;
            var dst = (ushort*)dest;
            var length = Width * Height;
            for (var i = 0; i < length; ++i, dst++, src++)
                //*dst = (ushort)(*src / 8000 * ushort.MaxValue);
                *dst = (ushort)*src;
        }

        protected override int Width
        {
            get { return this._width; }
        }

        protected override int Height
        {
            get { return this._height; }
        }

        protected override SlimDX.DXGI.Format Format
        {
            get { return SlimDX.DXGI.Format.R16_UNorm; }
        }

        protected override void CopyData(DX11DynamicTexture2D texture)
        {
            lock (m_lock)
            {
                texture.WriteData(this.depthRead, this.Width * this.Height * 2);
            }
        }

        protected override void OnRuntimeConnected()
        {
            this.runtime.OnDepthFrame += DepthFrameReady;
        }

        protected override void OnRuntimeDisconnected()
        {
            this.runtime.OnDepthFrame -= DepthFrameReady;
        }

        protected override void Disposing()
        {
            Marshal.FreeHGlobal(this.depthRead);
            Marshal.FreeHGlobal(depthWrite);
        }
    }
}
