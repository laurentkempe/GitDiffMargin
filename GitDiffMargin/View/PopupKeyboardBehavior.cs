/* The MIT License
 *
 * Copyright (C) 2013 Mike Ward (http://mike-ward.net)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GitDiffMargin.View
{
    public class PopupAllowKeyboardInput
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(PopupAllowKeyboardInput),
                new PropertyMetadata(default(bool), IsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject d)
        {
            return (bool) d.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject d, bool value)
        {
            d.SetValue(IsEnabledProperty, value);
        }

        private static void IsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs ea)
        {
            EnableKeyboardInput((Popup) sender, (bool) ea.NewValue);
        }

        private static void EnableKeyboardInput(Popup popup, bool enabled)
        {
            if (!enabled) return;

            IInputElement element = null;
            popup.Loaded += (sender, args) =>
            {
                popup.Child.Focusable = true;
                popup.Child.IsVisibleChanged += (o, ea) =>
                {
                    if (!popup.Child.IsVisible) return;

                    element = Keyboard.FocusedElement;
                    Keyboard.Focus(popup.Child);
                };
            };
        }
    }
}