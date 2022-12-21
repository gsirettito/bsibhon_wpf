using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Multimedia {
    public static class Animations {
        public static void FadeIn(UIElement element, Duration duration) {
            element.Visibility = Visibility.Visible;
            DoubleAnimation anima = new DoubleAnimation {
                From = 0, To = 1,
                Duration = duration,
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(UIElement.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Begin();
        }

        public static void FadeIn(UIElement element, Duration duration, Action<object, EventArgs> function) {
            DoubleAnimation anima = new DoubleAnimation {
                From = 0, To = 1,
                Duration = duration,
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(UIElement.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Completed += new EventHandler(function);
            storyboard.Begin();
        }

        public static void FadeOut(UIElement element, Duration duration) {
            DoubleAnimation anima = new DoubleAnimation {
                From = 1, To = 0,
                Duration = duration,
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(UIElement.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Begin();
        }

        public static void FadeOut(UIElement element, Duration duration, Action<object, EventArgs> function) {
            DoubleAnimation anima = new DoubleAnimation {
                From = 1, To = 0,
                Duration = duration,
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(UIElement.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Completed += new EventHandler(function);
            storyboard.Begin();
        }

        public static void TextExpand(TextElement element, double from, double to, Duration duration) {
            DoubleAnimation anima = new DoubleAnimation {
                From = from, To = to,
                Duration = duration,
                EasingFunction = new CubicEase()
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(TextElement.FontSizeProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Begin();
        }

        public static void ImageExpand(ImageBrush element, double from, double to, Duration duration) {
            DoubleAnimation anima = new DoubleAnimation {
                From = from, To = to,
                Duration = duration,
                EasingFunction = new CubicEase()
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(TextElement.FontSizeProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Begin();
        }

        public static void ImageExpand(ImageBrush element, double from, double to, Duration duration, Action<object, EventArgs> function) {
            DoubleAnimation anima = new DoubleAnimation {
                From = from, To = to,
                Duration = duration,
                EasingFunction = new CubicEase()
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(TextElement.FontSizeProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Completed += new EventHandler(function);
            storyboard.Begin();
        }

        public static void TextExpand(TextElement element, double from, double to, Duration duration, Action<object, EventArgs> function) {
            DoubleAnimation anima = new DoubleAnimation {
                From = from, To = to,
                Duration = duration,
                EasingFunction = new CubicEase()
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(TextElement.FontSizeProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Completed += new EventHandler(function);
            storyboard.Begin();
        }

        public static void RightExpand(UIElement element, double from, double to, Duration duration) {
            element.Visibility = Visibility.Visible;
            DoubleAnimation anima = new DoubleAnimation {
                From = from, To = to,
                Duration = duration,
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(Canvas.WidthProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Begin();
        }

        public static void RightExpand(UIElement element, double from, double to, Duration duration, Action<object, EventArgs> function) {
            element.Visibility = Visibility.Visible;
            DoubleAnimation anima = new DoubleAnimation {
                From = from, To = to,
                Duration = duration,
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(anima, element);
            Storyboard.SetTargetProperty(anima, new PropertyPath(Canvas.WidthProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(anima);
            storyboard.Completed += new EventHandler(function);
            storyboard.Begin();
        }
    }

    /// <summary>
    /// A helper class to simplify animation.
    /// </summary>
    public static class ZoomPanAnimationHelper {
        /// <summary>
        /// Starts an animation to a particular value on the specified dependency property.
        /// </summary>
        public static void StartAnimation(UIElement animatableElement, DependencyProperty dependencyProperty,
            double toValue, double animationDurationSeconds) {
            StartAnimation(animatableElement, dependencyProperty, toValue, animationDurationSeconds, null);
        }

        /// <summary>
        /// Starts an animation to a particular value on the specified dependency property.
        /// You can pass in an event handler to call when the animation has completed.
        /// </summary>
        public static void StartAnimation(UIElement animatableElement, DependencyProperty dependencyProperty,
            double toValue, double animationDurationSeconds, EventHandler completedEvent) {
            double fromValue = (double)animatableElement.GetValue(dependencyProperty);

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = fromValue;
            animation.To = toValue;
            animation.Duration = TimeSpan.FromSeconds(animationDurationSeconds);

            animation.Completed += delegate(object sender, EventArgs e) {
                //
                // When the animation has completed bake final value of the animation
                // into the property.
                //
                animatableElement.SetValue(dependencyProperty, animatableElement.GetValue(dependencyProperty));
                CancelAnimation(animatableElement, dependencyProperty);

                if (completedEvent != null) {
                    completedEvent(sender, e);
                }
            };

            animation.Freeze();

            animatableElement.BeginAnimation(dependencyProperty, animation);
        }

        /// <summary>
        /// Cancel any animations that are running on the specified dependency property.
        /// </summary>
        public static void CancelAnimation(UIElement animatableElement, DependencyProperty dependencyProperty) {
            animatableElement.BeginAnimation(dependencyProperty, null);
        }
    }
}
