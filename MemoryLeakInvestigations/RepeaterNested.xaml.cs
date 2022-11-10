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
using System.Collections.ObjectModel;
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
    public sealed partial class RepeaterNested : UserControl
    {
        /// <summary>
        /// The Dependency property backing the ViewModel property
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(ViewModel),
                typeof(RepeaterNested),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the viewmodel the chrome binds to.
        /// </summary>
        public ViewModel ViewModel
        {
            get { return (ViewModel)this.GetValue(ViewModelProperty); }
            set { this.SetValue(ViewModelProperty, value); }
        }

        public RepeaterNested()
        {
            this.InitializeComponent();

            this.ViewModel = new ViewModel()
            {
                Value = nameof(RepeaterFlat),
                Items = new ObservableCollection<ViewModel>()
                {
                    new ViewModel() { Value = nameof(RepeaterFlat), Items = new ObservableCollection<string> { "Value1", "Value2", "Value3" }  },
                    new ViewModel() { Value = nameof(RepeaterFlat), Items = new ObservableCollection<string> { "Value1", "Value2", "Value3" }  },
                }
            };
            this.Loaded += UserControl1_Loaded;
        }

        private void UserControl1_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("UserControl1_Loaded");
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ScrollViewer_Loaded");
        }

        private void UserControl2_ViewModelChanged(object sender, ViewModel e)
        {

        }

        private void ItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            Debug.WriteLine("ItemsRepeater_ElementPrepared");
        }

        private void NestedItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            Debug.WriteLine("ItemsRepeater_ElementPrepared");
        }
    }
}
