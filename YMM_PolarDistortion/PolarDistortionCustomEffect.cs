using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vortice;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace YMM_PolarDistortion
{
    class PolarDistortionCustomEffect(IGraphicsDevicesAndContext devices) : D2D1CustomShaderEffectBase(Create<EffectImpl>(devices))
    {
        public float Transform
        {
            get => GetFloatValue((int)EffectProperty.Transform);
            set => SetValue((int)EffectProperty.Transform, value);
        }

        public Vector2 Scale
        {
            get => GetVector2Value((int)EffectProperty.Scale);
            set => SetValue((int)EffectProperty.Scale, value);
        }

        public Vector2 Offset
        {
            get => GetVector2Value((int)EffectProperty.Offset);
            set => SetValue((int)EffectProperty.Offset, value);
        }

        public bool IsPolarToRect
        {
            get => GetBoolValue((int)EffectProperty.IsPolarToRect);
            set => SetValue((int)EffectProperty.IsPolarToRect, value);
        }

        public bool ForPreOrPostProcess
        {
            get => GetBoolValue((int)EffectProperty.ForPreOrPostProcess);
            set => SetValue((int)EffectProperty.ForPreOrPostProcess, value);
        }

        public int UpdateKey
        {
            get => GetIntValue((int)EffectProperty.UpdateKey);
            set => SetValue((int)EffectProperty.UpdateKey, value);
        }
    }

    [CustomEffect(1)]
    file class EffectImpl : D2D1CustomShaderEffectImplBase<EffectImpl>
    {
        [CustomEffectProperty(PropertyType.Float, (int)EffectProperty.Transform)]
        public float Transform { get; set; }

        [CustomEffectProperty(PropertyType.Vector2, (int)EffectProperty.Scale)]
        public Vector2 Scale { get; set; }

        [CustomEffectProperty(PropertyType.Vector2, (int)EffectProperty.Offset)]
        public Vector2 Offset { get; set; }

        [CustomEffectProperty(PropertyType.Bool, (int)EffectProperty.IsPolarToRect)]
        public bool IsPolarToRect { get; set; }

        [CustomEffectProperty(PropertyType.Bool, (int)EffectProperty.ForPreOrPostProcess)]
        public bool ForPreOrPostProcess { get; set; }

        // 更新用プロパティ
        int updateKey = 0;
        [CustomEffectProperty(PropertyType.Int32, (int)EffectProperty.UpdateKey)]
        public int UpdateKey
        {
            get => updateKey;
            set
            {
                if (updateKey != value)
                {
                    updateKey = value;
                    UpdateConstants();
                }
            }
        }

        public EffectImpl() : base(GetShader()) { }

        protected override void UpdateConstants()
        {
            drawInformation?.SetPixelShaderConstantBuffer(new EffectParameter(Transform, Scale, Offset, IsPolarToRect, ForPreOrPostProcess));
        }

        static byte[] GetShader()
        {
            var assembly = typeof(EffectImpl).Assembly;
            using var stream = assembly.GetManifestResourceStream("PolarDistortion_Shader.cso");
            if (stream != null)
            {
                var result = new byte[stream.Length];
                stream.Read(result, 0, result.Length);
                return result;
            }
            else
            {
                return [];
            }
        }
    }

    file enum EffectProperty : int
    {
        Transform = 0,
        Scale,
        Offset,
        IsPolarToRect,
        ForPreOrPostProcess,
        UpdateKey
    }

    [StructLayout(LayoutKind.Explicit)]
    file readonly record struct EffectParameter(
        float Transform,
        Vector2 Scale,
        Vector2 Offset,
        bool IsPolarToRect,
        bool ForPreOrPostProcess
    )
    {
        [FieldOffset(0)]
        public readonly float Transform = Transform;

        [FieldOffset(sizeof(float) * 2)]
        public readonly Vector2 Scale = Scale;

        [FieldOffset(sizeof(float) * 4)]
        public readonly Vector2 Offset = Offset;

        [FieldOffset(sizeof(float) * 6)]
        public readonly bool IsPolarToRect = IsPolarToRect;

        [FieldOffset(sizeof(float) * 6 + 4)]
        public readonly bool ForPreOrPostProcess = ForPreOrPostProcess;
    }
}
