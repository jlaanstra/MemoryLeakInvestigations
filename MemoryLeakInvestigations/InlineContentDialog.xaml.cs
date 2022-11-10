// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RuntimeComponent1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MemoryLeakInvestigations
{
    public sealed partial class InlineContentDialog : UserControl
    {
        /// <summary>
        /// The Dependency property backing the ViewModel property
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(ViewModel),
                typeof(InlineContentDialog),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the viewmodel the chrome binds to.
        /// </summary>
        public ViewModel ViewModel
        {
            get { return (ViewModel)this.GetValue(ViewModelProperty); }
            set { this.SetValue(ViewModelProperty, value); }
        }

        public InlineContentDialog()
        {
            this.ViewModel = new ViewModel();
            this.InitializeComponent();
        }

        public static bool ConvertIsCheckedToBoolean(bool? isChecked) => isChecked.GetValueOrDefault(false);

        public static bool ConvertNotNullToBoolean(object value) => value != null;

        public static Visibility ConvertNotNullToVisibility(object value) => value != null ? Visibility.Visible : Visibility.Collapsed;

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Debug.WriteLine("ContentDialog_PrimaryButtonClick");
        }

        private void OnRemoveDialogConfirmClick(ContentDialog sender, ContentDialogButtonClickEventArgs e)
        {
            Debug.WriteLine("OnRemoveDialogConfirmClick");
        }

        private void OnRemoveDialogCancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs e)
        {
            Debug.WriteLine("OnRemoveDialogCancelClick");
        }

        private void OnRemoveDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (sender != null)
            {
                sender.DataContext = null;
            }
        }

        public static string GetRemoveDialogBodyText(object dataContext)
        {
            if (dataContext is object o)
            {
                return "Text";
            }

            return string.Empty;
        }
    }
}
