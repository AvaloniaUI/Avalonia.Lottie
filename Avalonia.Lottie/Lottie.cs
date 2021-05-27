using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Lottie.Animation.Content;
using Avalonia.Lottie.Manager;
using Avalonia.Lottie.Model;
using Avalonia.Lottie.Model.Layer;
using Avalonia.Lottie.Parser;
using Avalonia.Lottie.Utils;
using Avalonia.Lottie.Value;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Metadata;
using Avalonia.Platform;

#nullable enable

namespace Avalonia.Lottie
{
    /// <summary>
    ///     This can be used to show an lottie animation in any place that would normally take a drawable.
    ///     If there are masks or mattes, then you MUST call <seealso cref="RecycleBitmaps()" /> when you are done
    ///     or else you will leak bitmaps.
    /// </summary>
    [PseudoClasses(":animation-started", ":animation-ended")]
    public sealed class Lottie : Control, IAnimatable
    {
        private static readonly IAssetLoader s_AssetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();

        /// <summary>
        ///     This value used used with the <see cref="RepeatCount" /> property to repeat
        ///     the animation indefinitely.
        /// </summary>
        public const int Infinite = -1;

        private readonly Uri _baseUri;
        private readonly LottieValueAnimator _animator = new();

        private readonly List<Action<LottieComposition>> _lazyCompositionTasks = new();
        private LottieCanvas? _lottieCanvas;
        private LottieComposition? _composition;
        private CompositionLayer? _compositionLayer;
        private FontAssetDelegate? _fontAssetDelegate;
        private FontAssetManager? _fontAssetManager;
        private IImageAssetDelegate? _imageAssetDelegate;
        private ImageAssetManager? _imageAssetManager;
        private TextDelegate? _textDelegate;
        private IDisposable? _compositionTimerDisposable;
        private bool _isEnabled = true;
        private LottieCustomDrawOp _currentDrawOperation;

        static Lottie()
        {
            AffectsMeasure<Lottie>(SourceProperty);
            AffectsArrange<Lottie>(SourceProperty);
            AffectsRender<Lottie>(SourceProperty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lottie"/> class.
        /// </summary>
        /// <param name="baseUri">The base URL for the XAML context.</param>
        public Lottie(Uri baseUri)
        {
            _baseUri = baseUri;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lottie"/> class.
        /// </summary>
        /// <param name="serviceProvider">The XAML service provider.</param>
        public Lottie(IServiceProvider serviceProvider)
        {
            _baseUri = ((IUriContext) serviceProvider.GetService(typeof(IUriContext))).BaseUri;
            Initialize();
        }

        private void Initialize()
        {
            ClipToBounds = true;

            _animator.Update += delegate
            {
                if (_compositionLayer is not null)
                    _compositionLayer.Progress = _animator.AnimatedValueAbsolute;
            };

            _animator.AnimationStart += delegate
            {
                PseudoClasses.Set(":animation-ended", false);
                PseudoClasses.Set(":animation-started", true);
            };

            _animator.AnimationEnd += delegate
            {
                PseudoClasses.Set(":animation-ended", true);
                PseudoClasses.Set(":animation-started", false);
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
        public string? ImageAssetsFolder => null;

        /// <summary>
        ///     Gets or sets the minimum frame that the animation will start from when playing or looping.
        /// </summary>
        internal double MinFrame
        {
            get => 0;
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(_ => MinFrame = value);
                    return;
                }

                _animator.MinFrame = value;
            }
        }

        /// <summary>
        ///     Sets the minimum progress that the animation will start from when playing or looping.
        /// </summary>
        internal double MinProgress
        {
            get => 0;

            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(_ => { MinProgress = value; });
                    return;
                }

                MinFrame = MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, value);
            }
        }

        /// <summary>
        ///     Gets or sets the maximum frame that the animation will end at when playing or looping.
        /// </summary>
        internal double MaxFrame
        {
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(_ => MaxFrame = value);
                    return;
                }

                _animator.MaxFrame = value;
            }

            get => _animator.MaxFrame;
        }

        /// <summary>
        ///     Sets the maximum progress that the animation will end at when playing or looping.
        /// </summary>
        internal double MaxProgress
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
                    _lazyCompositionTasks.Add(_ => { MaxProgress = value; });
                    return;
                }

                MaxFrame = MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, value);
            }
        }

        /// <summary>
        ///     Sets the playback speed. If speed &lt; 0, the animation will play backwards.
        ///     Returns the current playback speed. This will be &lt; 0 if the animation is playing backwards.
        /// </summary>
        public double Speed
        {
            set => _animator.Speed = value;
            get => _animator.Speed;
        }

        internal double Frame
        {
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(_ => { Frame = value; });
                    return;
                }

                _animator.Frame = value;
            }
            get => _animator.Frame;
        }

        public double Progress
        {
            get => _animator.AnimatedValueAbsolute;
            set
            {
                if (_composition == null)
                {
                    _lazyCompositionTasks.Add(_ => { Progress = value; });
                    return;
                }

                Frame = MiscUtils.Lerp(_composition.StartFrame, _composition.EndFrame, value);
            }
        }

        /// <summary>
        ///     Defines what this animation should do when it reaches the end. This
        ///     setting is applied only when the repeat count is either greater than
        ///     0 or <see>
        ///         <cref>Lottie.RepeatMode.Infinite</cref>
        ///     </see>
        ///     . Defaults to <see>
        ///         <cref>Lottie.RepeatMode.Restart</cref>
        ///     </see>
        ///     .
        ///     Return either one of <see>
        ///         <cref>Lottie.RepeatMode.Reverse</cref>
        ///     </see>
        ///     or <see>
        ///         <cref>Lottie.RepeatMode.Restart</cref>
        ///     </see>
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
        ///     greater than 0 or <see>
        ///         <cref>Lottie.RepeatMode.Infinite</cref>
        ///     </see>
        ///     , the repeat mode will be taken
        ///     into account. The repeat count is 0 by default.
        ///     Count the number of times the animation should be repeated
        ///     Return the number of times the animation should repeat, or <see>
        ///         <cref>Lottie.RepeatMode.Infinite</cref>
        ///     </see>
        /// </summary>
        public int RepeatCount
        {
            set => _animator.RepeatCount = value;
            get => _animator.RepeatCount;
        }

        public double FrameRate
        {
            get => _animator.FrameRate;
            set => _animator.FrameRate = value;
        }

        public bool IsRunning => _animator.IsRunning;

        /// <summary>
        ///     Use this to manually set fonts.
        /// </summary>
        public FontAssetDelegate? FontAssetDelegate
        {
            get => null;

            set
            {
                _fontAssetDelegate = value;
                if (_fontAssetManager != null) _fontAssetManager.Delegate = value;
            }
        }

        public TextDelegate? TextDelegate
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
        internal IImageAssetDelegate? ImageAssetDelegate
        {
            get => null;
            set
            {
                _imageAssetDelegate = value;
                if (_imageAssetManager != null) _imageAssetManager.Delegate = value;
            }
        }

        public LottieComposition? Composition => _composition;

        private ImageAssetManager? ImageAssetManager
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

                    if (_composition?.Images != null)
                    {
                        foreach (var entry in _composition.Images)
                            clonedDict.Add(entry.Key, entry.Value);
                    }

                    _imageAssetManager = new ImageAssetManager(ImageAssetsFolder, _imageAssetDelegate, clonedDict);
                }

                return _imageAssetManager;
            }
        }

        private FontAssetManager FontAssetManager => _fontAssetManager ??= new FontAssetManager(_fontAssetDelegate);

        public void Start()
        {
            if (IsEnabled)
            {
                PlayAnimation();
            }
        }

        public void Stop()
        {
            EndAnimation();
        }

        public void ForceSoftwareRenderer(bool force)
        {
            //TODO: OID: Check can we do it or not
            //if (_canvasControl != null)
            //{
            //    _canvasControl.ForceSoftwareRenderer = force;
            //}
        }

        /// <summary>
        ///     Returns whether or not any layers in this composition has masks.
        /// </summary>
        public bool HasMasks()
        {
            return _compositionLayer != null && _compositionLayer.HasMasks();
        }

        /// <summary>
        ///     Returns whether or not any layers in this composition has a matte layer.
        /// </summary>
        public bool HasMatte()
        {
            return _compositionLayer != null && _compositionLayer.HasMatte();
        }


        /// <summary>
        ///     If you have image assets and use <seealso cref="Lottie" /> directly, you must call this yourself.
        ///     Calling recycleBitmaps() doesn't have to be final and <seealso cref="Lottie" />
        ///     will recreate the bitmaps if needed but they will leak if you don't recycle them.
        /// </summary>
        public void RecycleBitmaps()
        {
            _imageAssetManager?.RecycleBitmaps();
        }

        /// <summary>
        ///     Create a composition with <see cref="LottieCompositionFactory" />
        /// </summary>
        /// <param name="composition">The new composition.</param>
        /// <returns>True if the composition is different from the previously set composition, false otherwise.</returns>
        public bool SetComposition(LottieComposition composition)
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

                _compositionTimerDisposable =
                    (Clock ?? new Clock())
                    .Subscribe((_) =>
                    {
                        if (_animator.IsRunning && _isEnabled)
                            _animator.DoFrame();
                    });
            }

            InvalidateMeasure();
            InvalidateArrange();

            return true;
        }

        private void BuildCompositionLayer()
        {
            _compositionLayer =
                new CompositionLayer(this, LayerParser.Parse(_composition), _composition?.Layers, _composition);
        }

        public void ClearComposition()
        {
            RecycleBitmaps();
            if (_animator.IsRunning)
            {
                _compositionTimerDisposable?.Dispose();
                _animator.Cancel();
            }


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

        public static readonly StyledProperty<string?> SourceProperty =
            AvaloniaProperty.Register<Lottie, string?>(nameof(Source));

        [Content]
        public string? Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }


        public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
            AvaloniaProperty.Register<Lottie, StretchDirection>(nameof(StretchDirection), StretchDirection.Both);

        public StretchDirection StretchDirection
        {
            get => GetValue(StretchDirectionProperty);
            set => SetValue(StretchDirectionProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Stretch"/> property.
        /// </summary>
        public static readonly StyledProperty<Stretch> StretchProperty =
            AvaloniaProperty.Register<Lottie, Stretch>(nameof(Stretch), Stretch.Uniform);

        /// <summary>
        /// Gets or sets a value controlling how the image will be stretched.
        /// </summary>
        public Stretch Stretch
        {
            get => GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        private LottieComposition Load(string s, Uri? baseUri)
        {
            if (s is { })
            {
                var uri = s.StartsWith("/")
                    ? new Uri(s, UriKind.Relative)
                    : new Uri(s, UriKind.RelativeOrAbsolute);

                if (uri.IsAbsoluteUri && uri.IsFile)
                {
                    using (var file = File.Open(uri.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        return LottieCompositionFactory.FromJsonInputStreamSync(file, uri.AbsoluteUri).Value;
                    }
                }

                using (var asset = s_AssetLoader.Open(uri, baseUri))
                {
                    return LottieCompositionFactory.FromJsonInputStreamSync(asset, uri.ToString()).Value;
                }
            }

            return null;
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SourceProperty)
            {
                var newValue = change.NewValue.GetValueOrDefault<string>();
                var composition = Load(newValue, _baseUri);

                if (composition is { })
                {
                    SetComposition(composition);
                    Start();
                }
                else
                {
                    ClearComposition();
                }
            }
            else if (change.Property == IsEnabledProperty)
            {
                _isEnabled = change.NewValue.GetValueOrDefault<bool>();

                if (change.NewValue.GetValueOrDefault<bool>() && _composition != null)
                {
                    Start();
                }
            }
        }

        private Size SourceSize => (_composition?.Bounds.Size ?? Size.Empty) * VisualRoot.RenderScaling;

        /// <summary>
        /// Measures the control.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The desired size of the control.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            var source = _composition;
            var result = new Size();

            if (source != null)
            {
                result = Stretch.CalculateSize(availableSize, SourceSize, StretchDirection);
            }

            return result;
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var source = Source;

            if (source != null && _composition != null)
            {
                var result = Stretch.CalculateSize(finalSize, SourceSize, StretchDirection);

                if (_lottieCanvas != null && _compositionLayer != null)
                {
                    var viewPort = new Rect(result);

                    var sourceSize = _composition.Bounds.Size;
                    var scale = Stretch.CalculateScaling(viewPort.Size, sourceSize, StretchDirection);
                    var scaledSize = sourceSize * scale;

                    var matrix = Matrix.Identity;
                    matrix *= Matrix.CreateScale(scale.X, scale.Y);

                    var destRect = viewPort
                        .CenterRect(new Rect(scaledSize))
                        .Intersect(viewPort);

                    var sourceRect = new Rect(scaledSize)
                        .CenterRect(new Rect(finalSize));

                    matrix *= Matrix.CreateTranslation(-sourceRect.X, -sourceRect.Y);

                    _currentDrawOperation = new LottieCustomDrawOp(_lottieCanvas, _compositionLayer, destRect, matrix);
                }

                return result;
            }
            else
            {
                return new Size();
            }
        }

        public override void Render(DrawingContext renderCtx)
        {
            LottieLog.BeginSection("Drawable.Draw");

            var containerRect = Bounds;

            if (_lottieCanvas is null || _compositionLayer is null || containerRect.Width <= 0 ||
                containerRect.Height <= 0)
                return;

            if (_composition != null && _currentDrawOperation != null)
            {
                renderCtx.Custom(_currentDrawOperation);
            }

            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);

            LottieLog.EndSection("Drawable.Draw");
        }

        /// <summary>
        ///     Plays the animation from the beginning. If speed is &lt; 0, it will start at the end
        ///     and play towards the beginning
        /// </summary>
        public void PlayAnimation()
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(_ => { PlayAnimation(); });
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
        public void ResumeAnimation()
        {
            if (_compositionLayer == null)
            {
                _lazyCompositionTasks.Add(_ => { ResumeAnimation(); });
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
        public void SetMinAndMaxFrame(double minFrame, double maxFrame)
        {
            if (_composition == null)
            {
                _lazyCompositionTasks.Add(_ => SetMinAndMaxFrame(minFrame, maxFrame));
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
        public void SetMinAndMaxProgress(double minProgress, double maxProgress)
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
                _lazyCompositionTasks.Add(_ => { SetMinAndMaxProgress(minProgress, maxProgress); });
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

        internal bool UseTextGlyphs()
        {
            return _composition != null && _textDelegate == null && _composition.Characters.Count > 0;
        }

        private void UpdateBounds()
        {
            if (_composition == null) return;

            _lottieCanvas = new LottieCanvas(Width, Height);
        }

        public void CancelAnimation()
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
                _lazyCompositionTasks.Add(_ => { AddValueCallback(keyPath, property, callback); });
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
        public Bitmap? UpdateBitmap(string id, Bitmap bitmap)
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

        internal Bitmap? GetImageAsset(string id)
        {
            return ImageAssetManager?.BitmapForId(id);
        }

        internal Typeface? GetTypeface(string fontFamily, string style)
        {
            var assetManager = FontAssetManager;
            return assetManager.GetTypeface(fontFamily, style);
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

            public override bool Equals(object? obj)
            {
                if (this == obj) return true;

                if (!(obj is ColorFilterData)) return false;

                var other = (ColorFilterData) obj;

                return GetHashCode() == other.GetHashCode() && ColorFilter == other.ColorFilter;
            }
        }
    }
}