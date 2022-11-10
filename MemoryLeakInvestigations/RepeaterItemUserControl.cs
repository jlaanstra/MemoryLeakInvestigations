// Copyright (c) Microsoft Corporation. All rights reserved.

using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace MemoryLeakInvestigations
{
    public partial class RepeaterItemUserControl : UserControl
    {
        public static readonly DependencyProperty IsInvokeEnabledProperty =
            DependencyProperty.Register(
                "IsInvokeEnabled",
                typeof(bool),
                typeof(RepeaterItemUserControl),
                new PropertyMetadata(false, OnIsInvokeEnabledPropertyChanged));

        public bool IsInvokeEnabled
        {
            get { return (bool)GetValue(IsInvokeEnabledProperty); }
            set { SetValue(IsInvokeEnabledProperty, value); }
        }

        private static void OnIsInvokeEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RepeaterItemUserControl)d).UpdateVisualState(true);
        }

        public delegate void RepeaterItemInvokedEventHandler(RepeaterItemUserControl sender, RoutedEventArgs args);

        public event RepeaterItemInvokedEventHandler Invoked;

        private bool isPressedByKey;
        private bool isPressedByPointer;
        private bool isPointerOver;
        private bool isHolding;

        public RepeaterItemUserControl()
        {
            IsTabStop = true;
            IsInvokeEnabled = true;
            UseSystemFocusVisuals = true;
            Margin = new Thickness(0);
            Padding = new Thickness(0);

            Loaded += OnLoaded;
            IsEnabledChanged += OnIsEnabledChanged;
        }

        protected bool IsPressedByKey => isPressedByKey;

        protected bool IsPressedByPointer => isPressedByPointer;

        protected bool IsPointerOver => isPointerOver;

        public void AutomationInvoked()
        {
            if (IsInvokeEnabled)
            {
                OnInvoked(new RoutedEventArgs());
            }
        }

        private void OnInvoked(RoutedEventArgs args)
        {
            Invoked?.Invoke(this, args);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled || !IsEnabled)
            {
                return;
            }

            var originalKey = e.OriginalKey;

            switch (originalKey)
            {
                case VirtualKey.Enter:
                case VirtualKey.GamepadA:
                    if (IsInvokeEnabled)
                    {
                        e.Handled = true;
                        isPressedByKey = true;
                        UpdateVisualState(true);
                    }
                    break;
                default:
                    break;
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);

            // Ignore event if already handled or if we are disabled
            if (e.Handled || !IsEnabled)
            {
                return;
            }

            var originalKey = e.OriginalKey;

            switch (originalKey)
            {
                case VirtualKey.Enter:
                case VirtualKey.GamepadA:

                    if (IsInvokeEnabled)
                    {
                        e.Handled = true;
                        if (isPressedByKey && !isPressedByPointer)
                        {
                            OnInvoked(e);
                        }
                        isPressedByKey = false;

                        UpdateVisualState(true);
                    }
                    break;
                default:
                    break;
            }
        }

        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            base.OnPointerCaptureLost(e);

            isPressedByPointer = false;

            // For touch, we can clear PointerOver when receiving PointerCaptureLost,
            // we get when the finger is lifted or from cancellation.
            if (e.GetCurrentPoint(null).PointerDeviceType == PointerDeviceType.Touch)
            {
                isPointerOver = false;
            }

            UpdateVisualState(true);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

            // Ignore event if already handled or if we are disabled
            if (e.Handled || !IsEnabled)
            {
                return;
            }

            if (IsInvokeEnabled)
            {
                bool isLeftButtonPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
                if (isLeftButtonPressed)
                {
                    isPressedByPointer = CapturePointer(e.Pointer);
                    UpdateVisualState(true);

                    // we don't set e.handled(true) here since that
                    // will regress drag and drop functionality.
                }
            }
        }

        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            base.OnHolding(e);

            // Ignore event if already handled or if we are disabled
            if (e.Handled || !IsEnabled)
            {
                return;
            }

            isHolding = true;
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);

            // Ignore event if already handled or if we are disabled
            if (e.Handled || !IsEnabled)
            {
                return;
            }

            if (IsInvokeEnabled)
            {
                e.Handled = true;

                var relativePosition = e.GetCurrentPoint(this).Position;
                bool withinBounds =
                    relativePosition.X >= 0 && relativePosition.X <= ActualWidth &&
                        relativePosition.Y >= 0 && relativePosition.Y <= ActualHeight;
                var properties = e.GetCurrentPoint(this).Properties;
                bool isInvokeAction = !isHolding && properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased;

                if (isInvokeAction && withinBounds && isPressedByPointer && !isPressedByKey)
                {
                    OnInvoked(e);
                }

                isHolding = false;
                isPressedByPointer = false;
                ReleasePointerCapture(e.Pointer);
                UpdateVisualState(true);
            }
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            if (IsInvokeEnabled)
            {
                isPointerOver = true;
                UpdateVisualState(true);
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            if (IsInvokeEnabled)
            {
                isPointerOver = false;
                UpdateVisualState(true);
            }
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState(true);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(false);
        }

        protected virtual void UpdateVisualState(bool useTransitions)
        {
            if (!IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }
            else if (isPressedByKey || isPressedByPointer)
            {
                VisualStateManager.GoToState(this, "Pressed", useTransitions);
            }
            else if (isPointerOver)
            {
                VisualStateManager.GoToState(this, "PointerOver", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
        }
    }
}
