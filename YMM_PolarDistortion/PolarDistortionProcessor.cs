using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace YMM_PolarDistortion
{
    class PolarDistortionProcessor : IVideoEffectProcessor
    {
        public ID2D1Image Output => OutputInternal ?? Input ?? throw new NullReferenceException();

        ID2D1Image? Input { get; set; }

        ID2D1Bitmap? InputBitmap { get; set; }

        ID2D1Image? OutputInternal { get; set; }

        PolarDistortionCustomEffect Effect { get; }

        AffineTransform2D TransformEffect { get; }

        PolarDistortion Item { get; }

        IGraphicsDevicesAndContext Devices { get; }

        public PolarDistortionProcessor(IGraphicsDevicesAndContext devices, PolarDistortion item)
        {
            Devices = devices;
            Item = item;
            Effect = new PolarDistortionCustomEffect(devices);
            TransformEffect = new AffineTransform2D(devices.DeviceContext);
            if (Effect.IsEnabled)
            {
                OutputInternal = TransformEffect.Output;
            }
        }

        public void ClearInput()
        {
            if (Effect.IsEnabled)
            {
                Effect.SetInput(0, null, true);
            }
            TransformEffect.SetInput(0, null, true);
            InputBitmap?.Dispose();
            InputBitmap = null;
        }

        public void SetInput(ID2D1Image input)
        {
            InputBitmap?.Dispose();
            InputBitmap = null;
            Input = input;
            if (Effect.IsEnabled)
            {
                Effect.SetInput(0, Input, true);
                TransformEffect.SetInput(0, Effect.Output, true);
            }
        }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            if (!Effect.IsEnabled || Input == null)
            {
                return effectDescription.DrawDescription;
            }

            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var transform = (float)Item.Transform.GetValue(frame, length, fps) * 0.01F;

            var rect = Devices.DeviceContext.GetImageLocalBounds(Input);
            var width = Math.Min((int)MathF.Ceiling(rect.Right - rect.Left), short.MaxValue / 2);
            var height = Math.Min((int)MathF.Ceiling(rect.Bottom - rect.Top), short.MaxValue / 2);
            if (InputBitmap == null || InputBitmap.Size.Width != width || InputBitmap.Size.Height != height)
            {
                Effect.SetInput(0, null, true);
                InputBitmap?.Dispose();

                var bitmapProperty = new BitmapProperties1
                {
                    BitmapOptions = BitmapOptions.Target,
                    PixelFormat = new PixelFormat
                    {
                        AlphaMode = AlphaMode.Premultiplied,
                        Format = Vortice.DXGI.Format.B8G8R8A8_UNorm
                    }
                };
                try
                {
                    InputBitmap = Devices.DeviceContext.CreateBitmap(new SizeI(width, height), bitmapProperty);
                    Effect.SetInput(0, InputBitmap, true);
                }
                catch
                {
                    return effectDescription.DrawDescription;
                }
            }

            Devices.DeviceContext.Target = InputBitmap;
            Devices.DeviceContext.BeginDraw();
            Devices.DeviceContext.Clear(Colors.Transparent);
            Devices.DeviceContext.DrawImage(Input, new Vector2(-rect.Left, -rect.Top));
            Devices.DeviceContext.EndDraw();

            var hashCode = new HashCode();
            hashCode.Add(transform);
            hashCode.Add(InputBitmap.Size);
            hashCode.Add(Item.IsPolarToRect);
            hashCode.Add(Item.ForPreOrPostProcess);
            hashCode.Add(frame);

            var imageSize = new Vector2(width, height);
            var invertImageSize = new Vector2(height, width);
            var min = Math.Min(width, height);
            var max = Math.Max(width, height);
            Effect.Transform = transform;
            Effect.Scale = imageSize / (Item.IsPolarToRect ? max : min);
            Effect.Offset = (Item.IsPolarToRect ? (invertImageSize - new Vector2(min)) : (new Vector2(min) - imageSize)) / imageSize * 0.5F;
            Effect.IsPolarToRect = Item.IsPolarToRect;
            Effect.ForPreOrPostProcess = Item.ForPreOrPostProcess;
            Effect.UpdateKey = hashCode.ToHashCode();

            TransformEffect.TransformMatrix = Matrix3x2.CreateTranslation(rect.Left, rect.Top);

            return effectDescription.DrawDescription;
        }

        public void Dispose()
        {
            Output?.Dispose();
            if (Effect.IsEnabled)
            {
                Effect.SetInput(0, null, true);
            }
            TransformEffect.SetInput(0, null, true);
            Effect.Dispose();
            TransformEffect.Dispose();
            InputBitmap?.Dispose();
        }
    }
}
