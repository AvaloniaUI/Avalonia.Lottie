using System.Collections.Generic;
using Avalonia.Lottie.Model.Content;
using Avalonia.Lottie.Utils;
using Avalonia.Lottie.Value;

namespace Avalonia.Lottie.Animation.Keyframe
{
    internal class ShapeKeyframeAnimation : BaseKeyframeAnimation<ShapeData, Path>
    {
        private readonly Path _tempPath = new();
        private readonly ShapeData _tempShapeData = new();

        internal ShapeKeyframeAnimation(List<Keyframe<ShapeData>> keyframes) : base(keyframes)
        {
        }

        public override Path GetValue(Keyframe<ShapeData> keyframe, float keyframeProgress)
        {
            var startShapeData = keyframe.StartValue;
            var endShapeData = keyframe.EndValue;

            _tempShapeData.InterpolateBetween(startShapeData, endShapeData, keyframeProgress);
            MiscUtils.GetPathFromData(_tempShapeData, _tempPath);
            return _tempPath;
        }
    }
}