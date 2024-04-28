using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace YMM_PolarDistortion
{
    [VideoEffect("Polar Distortion", ["加工"], [], IsAviUtlSupported = false)]
    public class PolarDistortion : VideoEffectBase
    {
        [Display(Name = "変換", Description = "極座標の変換率")]
        [AnimationSlider("F2", "%", 0.0, 100.0)]
        public Animation Transform { get; } = new Animation(100.0, 0.0, 100.0);

        bool isPolarToRect = false;
        [Display(Name = "極座標から長方形へ", Description = "極座標から長方形、または長方形から極座標に変換するかどうか")]
        [ToggleSlider]
        public bool IsPolarToRect
        {
            get => isPolarToRect;
            set => Set(ref isPolarToRect, value);
        }

        bool forPreOrPostProcess = false;
        [Display(Name = "前処理/後処理用", Description = "極座標から長方形に変換、間に別のエフェクトを挟み、その後極座標に戻すための処理を行うかどうか")]
        [ToggleSlider]
        public bool ForPreOrPostProcess
        {
            get => forPreOrPostProcess;
            set => Set(ref forPreOrPostProcess, value);
        }

        public override string Label => "Polar Distortion";

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return [];
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new PolarDistortionProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables()
        {
            return [Transform];
        }
    }
}
