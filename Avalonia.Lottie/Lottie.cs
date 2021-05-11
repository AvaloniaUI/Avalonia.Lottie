using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Manager;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Lottie.Parser;
using Avalonia.Lottie.Utils;
using Avalonia.Lottie.Value;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using static Avalonia.Media.MediaExtensions;

namespace Avalonia.Lottie
{
    /// <summary>
    ///     This can be used to show an lottie animation in any place that would normally take a drawable.
    ///     If there are masks or mattes, then you MUST call <seealso cref="RecycleBitmaps()" /> when you are done
    ///     or else you will leak bitmaps.
    ///     <para>
    ///         It is preferable to use <seealso cref="LottieAnimationView" /> when possible because it
    ///         handles bitmap recycling and asynchronous loading
    ///         of compositions.
    ///     </para>
    /// </summary>
    public class Lottie : Control, IAnimatable
    {
        /// <summary>
        ///     This value used used with the <see cref="RepeatCount" /> property to repeat
        ///     the animation indefinitely.
        /// </summary>
        public const int Infinite = -1;

        private readonly LottieValueAnimator _animator = new();

        private readonly List<Action<LottieComposition>> _lazyCompositionTasks = new();
        private byte _alpha = 255;
        private RenderTargetBitmap _backingBitmap;
        private BitmapCanvas _bitmapCanvas;
        private LottieComposition _composition;
        private CompositionLayer _compositionLayer;
        private bool _enableMergePaths;
        private FontAssetDelegate _fontAssetDelegate;
        private FontAssetManager _fontAssetManager;
        private bool _forceSoftwareRenderer;
        private IImageAssetDelegate _imageAssetDelegate;
        private ImageAssetManager _imageAssetManager;
        private bool _performanceTrackingEnabled;
        private float _scale = 1f;
        private TextDelegate _textDelegate;

        public Lottie()
        {
            _animator.Update += (sender, e) =>
            {
                if (_compositionLayer != null) _compositionLayer.Progress = _animator.AnimatedValueAbsolute;
            };
        }

        /// <summary>
        ///     If you use image assets, you must explicitly specify the folder in assets/ in which they are
        ///     located because bodymovin uses the name filenames across all compositions (img_#).
        ///     Do NOT rename the images themselves.
        ///     If your images are located in src/main/assets/airbnb_loader/ then call
        ///     `setImageAssetsFolder("airbnb_loader/");`.
        ///     If you use LottieDrawable directly, you MUST call <seealso cref="RecycleBitmaps()" /> when you
        ///     are done. Calling <seealso cref="RecycleBitmaps()" /> doesn't have to be final and
        ///     <seealso cref="Lottie" />
        ///     will recreate the bitmaps if needed but they will leak if you don't recycle them.
        ///     Be wary if you are using many images, however. Lottie is designed to work with vector shapes
        ///     from After Effects. If your images look like they could be represented with vector shapes,
        ///     see if it is possible to convert them to shape layers and re-export your animation. Check
        ///     the documentation at http://airbnb.io/lottie for more information about importing shapes from
        ///     Sketch or Illustrator to avoid this.
        /// </summary>
        public virtual string ImageAssetsFolder { get; set; }

        public virtual bool PerformanceTrackingEnabled
        {
            get { return _composition?.PerformanceTrackingEnabled ?? false; }
            set
            {
                _performanceTrackingEnabled = value;
                if (_composition != null) _composition.PerformanceTrackingEnabled = value;
            }
        }

        public virtual PerformanceTracker PerformanceTracker => _composition?.PerformanceTracker;

        /// <summary>
        ///     Gets or sets the minimum frame that the animation will start from when playing or looping.
        /// </summary>
        internal float MinFrame
        {
            get => 0;
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c => MinFrame = value);
                    return;
                }

                _animator.MinFrame = value;
            }
        }

        /// <summary>
        ///     Sets the minimum progress that the animation will start from when playing or looping.
        /// </summary>
        internal float MinProgress
        {
            get => 0;

            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c => { MinProgress = value; });
                    return;
                }

                MinFrame = MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, value);
            }
        }

        /// <summary>
        ///     Gets or sets the maximum frame that the animation will end at when playing or looping.
        /// </summary>
        internal float MaxFrame
        {
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c => MaxFrame = value);
                    return;
                }

                _animator.MaxFrame = value;
            }

            get => _animator.MaxFrame;
        }

        /// <summary>
        ///     Sets the maximum progress that the animation will end at when playing or looping.
        /// </summary>
        internal float MaxProgress
        {
            get => 0;

            set
            {
                if (value < 0)
                    value = 0;
                if (value > 1)
                    value = 1;

                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c => { MaxProgress = value; });
                    return;
                }

                MaxFrame = MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, value);
            }
        }

        /// <summary>
        ///     Sets the playback speed. If speed &lt; 0, the animation will play backwards.
        ///     Returns the current playback speed. This will be &lt; 0 if the animation is playing backwards.
        /// </summary>
        public virtual float Speed
        {
            set => _animator.Speed = value;
            get => _animator.Speed;
        }

        internal float Frame
        {
            /**
            * Sets the progress to the specified frame.
            * If the composition isn't set yet, the progress will be set to the frame when
            * it is.
            */
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c => { Frame = value; });
                    return;
                }

                _animator.Frame = value;
            }
            /**
            * Get the currently rendered frame.
            */
            get => _animator.Frame;
        }

        public virtual float Progress
        {
            get => _animator.AnimatedValueAbsolute;
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(c => { Progress = value; });
                    return;
                }

                Frame = MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, value);
            }
        }

        /// <summary>
        ///     Defines what this animation should do when it reaches the end. This
        ///     setting is applied only when the repeat count is either greater than
        ///     0 or <see cref="Lottie.RepeatMode.Infinite" />. Defaults to <see cref="Lottie.RepeatMode.Restart" />.
        ///     Return either one of <see cref="Lottie.RepeatMode.Reverse" /> or <see cref="Lottie.RepeatMode.Restart" />
        /// </summary>
        /// <param name="value">
        ///     <seealso cref="RepeatMode" />
        /// </param>
        public RepeatMode RepeatMode
        {
            set => _animator.RepeatMode = value;
            get => _animator.RepeatMode;
        }

        /// <summary>
        ///     Sets how many times the animation should be repeated. If the repeat
        ///     count is 0, the animation is never repeated. If the repeat count is
        ///     greater than 0 or <see cref="Lottie.RepeatMode.Infinite" />, the repeat mode will be taken
        ///     into account. The repeat count is 0 by default.
        ///     Count the number of times the animation should be repeated
        ///     Return the number of times the animation should repeat, or <see cref="Lottie.RepeatMode.Infinite" />
        /// </summary>
        public int RepeatCount
        {
            set => _animator.RepeatCount = value;
            get => _animator.RepeatCount;
        }

        public float FrameRate
        {
            get => _animator.FrameRate;
            set => _animator.FrameRate = value;
        }

        public virtual bool IsAnimating => _animator.IsRunning;

        /// <summary>
        ///     Use this to manually set fonts.
        /// </summary>
        public virtual FontAssetDelegate FontAssetDelegate
        {
            get => null;

            set
            {
                _fontAssetDelegate = value;
                if (_fontAssetManager != null) _fontAssetManager.Delegate = value;
            }
        }

        public virtual TextDelegate TextDelegate
        {
            set => _textDelegate = value;
            get => _textDelegate;
        }


        /// <summary>
        ///     Use this if you can't bundle images with your app. This may be useful if you download the
        ///     animations from the network or have the images saved to an SD Card. In that case, Lottie
        ///     will defer the loading of the bitmap to this delegate.
        ///     Be wary if you are using many images, however. Lottie is designed to work with vector shapes
        ///     from After Effects. If your images look like they could be represented with vector shapes,
        ///     see if it is possible to convert them to shape layers and re-export your animation. Check
        ///     the documentation at http://airbnb.io/lottie for more information about importing shapes from
        ///     Sketch or Illustrator to avoid this.
        /// </summary>
        internal virtual IImageAssetDelegate ImageAssetDelegate
        {
            get => null;
            set
            {
                _imageAssetDelegate = value;
                if (_imageAssetManager != null) _imageAssetManager.Delegate = value;
            }
        }

        public virtual LottieComposition Composition => _composition;
 
        private ImageAssetManager ImageAssetManager
        {
            get
            {
                if (_imageAssetManager != null)
                {
                    _imageAssetManager.RecycleBitmaps();
                    _imageAssetManager = null;
                }

                if (_imageAssetManager == null)
                {
                    var clonedDict = new Dictionary<string, LottieImageAsset>();
                    foreach (var entry in _composition.Images) clonedDict.Add(entry.Key, entry.Value);

                    _imageAssetManager = new ImageAssetManager(ImageAssetsFolder, _imageAssetDelegate, clonedDict);
                }

                return _imageAssetManager;
            }
        }

        private FontAssetManager FontAssetManager => _fontAssetManager ??= new FontAssetManager(_fontAssetDelegate);

        public void Start()
        {
            PlayAnimation();
        }

        public void Stop()
        {
            EndAnimation();
        }

        public bool IsRunning => IsAnimating;

        public void ForceSoftwareRenderer(bool force)
        {
            _forceSoftwareRenderer = force;
            //TODO: OID: Check can we do it or not
            //if (_canvasControl != null)
            //{
            //    _canvasControl.ForceSoftwareRenderer = force;
            //}
        }

        /// <summary>
        ///     Returns whether or not any layers in this composition has masks.
        /// </summary>
        public virtual bool HasMasks()
        {
            return _compositionLayer != null && _compositionLayer.HasMasks();
        }

        /// <summary>
        ///     Returns whether or not any layers in this composition has a matte layer.
        /// </summary>
        public virtual bool HasMatte()
        {
            return _compositionLayer != null && _compositionLayer.HasMatte();
        }

        internal virtual bool EnableMergePathsForKitKatAndAbove()
        {
            return _enableMergePaths;
        }

        /// <summary>
        ///     Enable this to get merge path support for devices running KitKat (19) and above.
        ///     Merge paths currently don't work if the the operand shape is entirely contained within the
        ///     first shape. If you need to cut out one shape from another shape, use an even-odd fill type
        ///     instead of using merge paths.
        /// </summary>
        public virtual void EnableMergePathsForKitKatAndAbove(bool enable)
        {
            _enableMergePaths = enable;
            if (_composition != null) BuildCompositionLayer();
        }

        public bool IsMergePathsEnabledForKitKatAndAbove()
        {
            return _enableMergePaths;
        }

        /// <summary>
        ///     If you have image assets and use <seealso cref="Lottie" /> directly, you must call this yourself.
        ///     Calling recycleBitmaps() doesn't have to be final and <seealso cref="Lottie" />
        ///     will recreate the bitmaps if needed but they will leak if you don't recycle them.
        /// </summary>
        public virtual void RecycleBitmaps()
        {
            _imageAssetManager?.RecycleBitmaps();
        }

        /// <summary>
        ///     Create a composition with <see cref="LottieCompositionFactory" />
        /// </summary>
        /// <param name="composition">The new composition.</param>
        /// <returns>True if the composition is different from the previously set composition, false otherwise.</returns>
        public virtual bool SetComposition(LottieComposition composition)
        {
            //if (Callback == null) // TODO: needed?
            //{
            //    throw new System.InvalidOperationException("You or your view must set a Drawable.Callback before setting the composition. This " + "gets done automatically when added to an ImageView. " + "Either call ImageView.setImageDrawable() before setComposition() or call " + "setCallback(yourView.getCallback()) first.");
            //}

            if (_composition == composition) return false;

            lock (this)
            {
                ClearComposition();
                _composition = composition;
                BuildCompositionLayer();
                _animator.Composition = composition;
                Progress = _animator.AnimatedFraction;
                UpdateBounds();

                // We copy the tasks to a new ArrayList so that if this method is called from multiple threads, 
                // then there won't be two iterators iterating and removing at the same time. 
                foreach (var t in _lazyCompositionTasks.ToList()) t.Invoke(composition);

                _lazyCompositionTasks.Clear();
                composition.PerformanceTrackingEnabled = _performanceTrackingEnabled;

                _backingBitmap =
                    new RenderTargetBitmap(
                        new PixelSize((int) _composition.Bounds.Width, (int) _composition.Bounds.Height),
                        new Vector(96, 96));
            }

            return true;
        }

        private void BuildCompositionLayer()
        {
            _compositionLayer =
                new CompositionLayer(this, LayerParser.Parse(_composition), _composition.Layers, _composition);
        }

        public void ClearComposition()
        {
            RecycleBitmaps();
            if (_animator.IsRunning) _animator.Cancel();

            lock (this)
            {
                _composition = null;
            }

            _compositionLayer = null;
            _imageAssetManager = null;
            _animator.ClearComposition();
            InvalidateSelf();
        }

        public void InvalidateSelf()
        {
            if (Dispatcher.UIThread.CheckAccess())
                InvalidateVisual();
            else
                Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
        }
        
        /// <summary>
        /// Measures the control.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The desired size of the control.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            return _composition is null ? Size.Empty : Stretch.CalculateSize(availableSize, _composition.Bounds.Size, StretchDirection);
        }
        
        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            return _composition is null ? Size.Empty : Stretch.CalculateSize(finalSize, _composition.Bounds.Size);
        }

        public static readonly StyledProperty<LottieCompositionSource> SourceProperty =
            AvaloniaProperty.Register<Lottie, LottieCompositionSource>(nameof(Source));

        [Content]
        public LottieCompositionSource Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        
        /// <summary>
        /// Defines the <see cref="Stretch"/> property.
        /// </summary>
        public static readonly StyledProperty<Stretch> StretchProperty =
            AvaloniaProperty.Register<Image, Stretch>(nameof(Stretch), Stretch.Uniform);

        /// <summary>
        /// Gets or sets a value controlling how the image will be stretched.
        /// </summary>
        public Stretch Stretch
        {
            get { return GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value controlling in what direction the image will be stretched.
        /// </summary>
        public StretchDirection StretchDirection
        {
            get { return GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }

        /// <summary>
        /// Defines the <see cref="StretchDirection"/> property.
        /// </summary>
        public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
            AvaloniaProperty.Register<Image, StretchDirection>(
                nameof(StretchDirection),
                StretchDirection.Both);


        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SourceProperty)
            {
                var newValue = change.NewValue.GetValueOrDefault<LottieCompositionSource>();

                if (newValue is { } && newValue.Composition is { })
                {
                    SetComposition(newValue.Composition);
                    Start();
                }
                else
                {
                    ClearComposition();
                }
            }
        }

        public override void Render(DrawingContext renderCtx)
        {
            lock (this)
            {
                if (_bitmapCanvas is null || _backingBitmap is null) return;

                if (_animator.IsRunning) _animator.DoFrame();

                var size = Bounds.Size;

                using (var ctxi = _backingBitmap.CreateDrawingContext(null))
                using (var ctx = new DrawingContext(ctxi, false))
                using (_bitmapCanvas.CreateSession(size.Width, size.Height,
                    renderCtx.PlatformImpl))
                {
                    _bitmapCanvas.Clear(Colors.Transparent);
                    LottieLog.BeginSection("Drawable.Draw");

                    if (_compositionLayer != null && Bounds.Width > 0 && Bounds.Height > 0)
                    {
                        Size sourceSize = _composition.Bounds.Size;

                        Vector scale = Stretch.CalculateScaling(size, sourceSize, StretchDirection);
                        Size scaledSize = sourceSize * scale;

                        var k = Matrix3X3.CreateIdentity();

                        ctx.PushClip(new Rect(scaledSize));

                        _compositionLayer.Draw(_bitmapCanvas, MatrixExt.PreScale(k, (float) scale.X, (float) scale.Y),
                            _alpha);
                    }


                    LottieLog.EndSection("Drawable.Draw");
                }
            }

            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        }

        private Matrix3X3 ToMatrix3X3(Matrix ctxCurrentTransform)
        {
            return new Matrix3X3()
            {
                M11 = (float) ctxCurrentTransform.M11,
                M12 = (float) ctxCurrentTransform.M12,
                M21 = (float) ctxCurrentTransform.M21,
                M22 = (float) ctxCurrentTransform.M22,
                M31 = (float) ctxCurrentTransform.M31,
                M32 = (float) ctxCurrentTransform.M32,
            };
        }

        /// <summary>
        ///     Plays the animation from the beginning. If speed is &lt; 0, it will start at the end
        ///     and play towards the beginning
        /// </summary>
        public virtual void PlayAnimation()
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(c => { PlayAnimation(); });
                return;
            }

            _animator.PlayAnimation();
        }

        public void EndAnimation()
        {
            _lazyCompositionTasks.Clear();
            _animator.EndAnimation();
        }

        /// <summary>
        ///     Continues playing the animation from its current position. If speed &lt; 0, it will play backwards
        ///     from the current position.
        /// </summary>
        public virtual void ResumeAnimation()
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(c => { ResumeAnimation(); });
                return;
            }

            _animator.ResumeAnimation();
        }

        /// <summary>
        ///     <see cref="MinFrame" />
        ///     <see cref="MaxFrame" />
        /// </summary>
        /// <param name="minFrame"></param>
        /// <param name="maxFrame"></param>
        public void SetMinAndMaxFrame(float minFrame, float maxFrame)
        {
            if (_composition == null)
            {
                _lazyCompositionTasks.Add(c => SetMinAndMaxFrame(minFrame, maxFrame));
                return;
            }

            _animator.SetMinAndMaxFrames(minFrame, maxFrame);
        }

        /// <summary>
        ///     <see cref="MinProgress" />
        ///     <see cref="MaxProgress" />
        /// </summary>
        /// <param name="minProgress"></param>
        /// <param name="maxProgress"></param>
        public void SetMinAndMaxProgress(float minProgress, float maxProgress)
        {
            if (minProgress < 0)
                minProgress = 0;
            if (minProgress > 1)
                minProgress = 1;
            if (maxProgress < 0)
                maxProgress = 0;
            if (maxProgress > 1)
                maxProgress = 1;

            if (_composition == null)
            {
                _lazyCompositionTasks.Add(c => { SetMinAndMaxProgress(minProgress, maxProgress); });
                return;
            }

            SetMinAndMaxFrame((int) MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, minProgress),
                (int) MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, maxProgress));
        }

        /// <summary>
        ///     Reverses the current animation speed. This does NOT play the animation.
        ///     <see cref="Speed" />
        ///     <see cref="PlayAnimation" />
        ///     <see cref="ResumeAnimation" />
        /// </summary>
        public void ReverseAnimationSpeed()
        {
            _animator.ReverseAnimationSpeed();
        }

        public event EventHandler<ValueAnimator.ValueAnimatorUpdateEventArgs> AnimatorUpdate
        {
            add => _animator.Update += value;
            remove => _animator.Update -= value;
        }

        public void RemoveAllUpdateListeners()
        {
            _animator.RemoveAllUpdateListeners();
        }

        public event EventHandler ValueChanged
        {
            add => _animator.ValueChanged += value;
            remove => _animator.ValueChanged -= value;
        }

        public void RemoveAllAnimatorListeners()
        {
            _animator.RemoveAllListeners();
        }

        internal virtual bool UseTextGlyphs()
        {
            return _textDelegate == null && _composition.Characters.Count > 0;
        }

        private void UpdateBounds()
        {
            if (_composition == null) return;

            _bitmapCanvas?.Dispose();
            _bitmapCanvas = new BitmapCanvas((float) Width, (float) Height);
        }

        public virtual void CancelAnimation()
        {
            _lazyCompositionTasks.Clear();
            _animator.Cancel();
        }

        public void PauseAnimation()
        {
            _lazyCompositionTasks.Clear();
            _animator.PauseAnimation();
        }

        /// <summary>
        ///     Takes a <see cref="KeyPath" />, potentially with wildcards or globstars and resolve it to a list of
        ///     zero or more actual <see cref="KeyPath" />s
        ///     that exist in the current animation.
        ///     If you want to set value callbacks for any of these values, it is recommend to use the
        ///     returned <see cref="KeyPath" /> objects because they will be internally resolved to their content
        ///     and won't trigger a tree walk of the animation contents when applied.
        /// </summary>
        /// <param name="keyPath"></param>
        /// <returns></returns>
        public List<KeyPath> ResolveKeyPath(KeyPath keyPath)
        {
            if (_compositionLayer == null)
            {
                Debug.WriteLine("Cannot resolve KeyPath. Composition is not set yet.", LottieLog.Tag);
                return new List<KeyPath>();
            }

            var keyPaths = new List<KeyPath>();
            _compositionLayer.ResolveKeyPath(keyPath, 0, keyPaths, new KeyPath());
            return keyPaths;
        }

        /// <summary>
        ///     Add an property callback for the specified <see cref="KeyPath" />. This <see cref="KeyPath" /> can resolve
        ///     to multiple contents. In that case, the callbacks's value will apply to all of them.
        ///     Internally, this will check if the <see cref="KeyPath" /> has already been resolved with
        ///     <see cref="ResolveKeyPath" /> and will resolve it if it hasn't.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyPath"></param>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        public void AddValueCallback<T>(KeyPath keyPath, LottieProperty property, ILottieValueCallback<T> callback)
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(c => { AddValueCallback(keyPath, property, callback); });
                return;
            }

            bool invalidate;
            if (keyPath.GetResolvedElement() != null)
            {
                keyPath.GetResolvedElement().AddValueCallback(property, callback);
                invalidate = true;
            }
            else
            {
                var elements = ResolveKeyPath(keyPath);

                for (var i = 0; i < elements.Count; i++)
                    elements[i].GetResolvedElement().AddValueCallback(property, callback);

                invalidate = elements.Any();
            }

            if (invalidate)
            {
                InvalidateSelf();
                if (property == LottieProperty.TimeRemap)
                    // Time remapping values are read in setProgress. In order for the new value 
                    // to apply, we have to re-set the progress with the current progress so that the 
                    // time remapping can be reapplied. 
                    Progress = Progress;
            }
        }

        /// <summary>
        ///     Overload of <see cref="AddValueCallback{T}(KeyPath, LottieProperty, ILottieValueCallback{T})" /> that takes an
        ///     interface. This allows you to use a single abstract
        ///     method code block in Kotlin such as:
        ///     drawable.AddValueCallback(yourKeyPath, LottieProperty.Color) { yourColor }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyPath"></param>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        public void AddValueCallback<T>(KeyPath keyPath, LottieProperty property, SimpleLottieValueCallback<T> callback)
        {
            AddValueCallback(keyPath, property, new SimpleImplLottieValueCallback<T>(callback));
        }

        /// <summary>
        ///     Allows you to modify or clear a bitmap that was loaded for an image either automatically
        ///     through <seealso cref="ImageAssetsFolder" /> or with an <seealso cref="ImageAssetDelegate" />.
        /// </summary>
        /// <returns>
        ///     the previous Bitmap or null.
        /// </returns>
        public virtual Bitmap UpdateBitmap(string id, Bitmap bitmap)
        {
            var bm = ImageAssetManager;
            if (bm == null)
            {
                Debug.WriteLine(
                    "Cannot update bitmap. Most likely the drawable is not added to a View " +
                    "which prevents Lottie from getting a Context.", LottieLog.Tag);
                return null;
            }

            var ret = bm.UpdateBitmap(id, bitmap);
            InvalidateSelf();
            return ret;
        }

        internal virtual Bitmap GetImageAsset(string id)
        {
            return ImageAssetManager?.BitmapForId(id);
        }

        //public Device Device => base.devic;

        internal virtual Typeface GetTypeface(string fontFamily, string style)
        {
            var assetManager = FontAssetManager;
            return assetManager?.GetTypeface(fontFamily, style);
        }

        /**
         * If there are masks or mattes, we can't scale the animation larger than the canvas or else 
         * the off screen rendering for masks and mattes after saveLayer calls will get clipped.
         */
        private float GetMaxScale(BitmapCanvas canvas)
        {
            var maxScaleX = (float) canvas.Width / (float) _composition.Bounds.Width;
            var maxScaleY = (float) canvas.Height / (float) _composition.Bounds.Height;
            return Math.Min(maxScaleX, maxScaleY);
        }

        protected void Dispose(bool disposing)
        {
            // base.Dispose(disposing);

            _imageAssetManager?.Dispose();
            _imageAssetManager = null;

            _composition = null;

            _bitmapCanvas?.Dispose();
            _bitmapCanvas = null;

            _compositionLayer = null;

            ClearComposition();
        }

        private class ColorFilterData
        {
            internal readonly ColorFilter ColorFilter;
            internal readonly string ContentName;
            internal readonly string LayerName;

            internal ColorFilterData(string layerName, string contentName, ColorFilter colorFilter)
            {
                LayerName = layerName;
                ContentName = contentName;
                ColorFilter = colorFilter;
            }

            public override int GetHashCode()
            {
                var hashCode = 17;
                if (!string.IsNullOrEmpty(LayerName)) hashCode = hashCode * 31 * LayerName.GetHashCode();

                if (!string.IsNullOrEmpty(ContentName)) hashCode = hashCode * 31 * ContentName.GetHashCode();

                return hashCode;
            }

            public override bool Equals(object obj)
            {
                if (this == obj) return true;

                if (!(obj is ColorFilterData)) return false;

                var other = (ColorFilterData) obj;

                return GetHashCode() == other.GetHashCode() && ColorFilter == other.ColorFilter;
            }
        }
    }
}