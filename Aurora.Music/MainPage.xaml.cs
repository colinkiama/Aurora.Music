﻿using System;
using Aurora.Music.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Aurora.Music
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            MainFrame.Navigate(typeof(HomePage));
        }

        private Type[] navigateOptions = { typeof(HomePage), typeof(LibraryPage) };

        private void Main_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            MainFrame.Navigate(navigateOptions[sender.MenuItems.IndexOf(args.SelectedItem)]);
        }

        public void Navigate(Type type)
        {
            MainFrame.Navigate(type);
        }

        public void Navigate(Type type, object parameter)
        {
            MainFrame.Navigate(type, parameter);
        }

        public Visibility BooltoVisibility(bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}