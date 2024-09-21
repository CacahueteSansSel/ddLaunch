﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Auth;
using mcLaunch.Managers;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;
using mcLaunch.Views.Windows;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class ToolButtonsBar : UserControl
{
    public ToolButtonsBar()
    {
        InitializeComponent();

        DataContext = new Data();

        if (Design.IsDesignMode)
        {
            UIDataContext.Progress = 60;
            UIDataContext.ResourceCount = "1/3";
            UIDataContext.ResourceName = "Test";
            UIDataContext.ResourceDetailsText = "file.txt";
        }

        RefreshButtons();
    }

    public Data UIDataContext => (Data) DataContext;

    public void RefreshButtons()
    {
        AdvancedFeaturesButton.IsVisible = Settings.Instance.ShowAdvancedFeatures;

#if DEBUG
        UnitTestsButton.IsVisible = true;
#else
        UnitTestsButton.IsVisible = false;
#endif
    }

    private async void NewBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new NewBoxPopup());
    }

    private void ImportBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ImportBoxPopup());
    }

    private async void DisconnectButtonClicked(object? sender, RoutedEventArgs e)
    {
        await AuthenticationManager.DisconnectAsync();

        MainWindowDataContext.Instance.Reset();
        MainWindowDataContext.Instance.Push<OnBoardingPage>(false);
    }

    private void SettingsButtonClicked(object? sender, RoutedEventArgs e)
    {
        MainWindowDataContext.Instance.Push<SettingsPage>();
    }

    private void BrowseModpacksButtonClicked(object? sender, RoutedEventArgs e)
    {
        MainWindowDataContext.Instance.Push<BrowseModpacksPage>();
    }

    private void DefaultsButtonClicked(object? sender, RoutedEventArgs e)
    {
        MainWindowDataContext.Instance.Push<DefaultsPage>();
    }

    private void BrowseModsButtonClicked(object? sender, RoutedEventArgs e)
    {
        MainWindowDataContext.Instance.Push<BrowseModsPage>();
    }

    private void FastLaunchButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (BackgroundManager.IsMinecraftRunning)
        {
            Navigation.ShowPopup(new MessageBoxPopup("FastLaunch not available",
                "Cannot use FastLaunch while another Minecraft instance is running", MessageStatus.Warning));
            return;
        }

        Navigation.ShowPopup(new FastLaunchPopup());
    }

    private void ViewBoxesButtonClicked(object? sender, RoutedEventArgs e)
    {
        MainWindowDataContext.Instance.Reset();
        MainWindowDataContext.Instance.Push<MainPage>();
    }

    public class Data : ReactiveObject
    {
        private MinecraftAuthenticationResult? account;
        private Bitmap head;

        private int progress;
        private string resourceCount;
        private string resourceDetailsText = "-";
        private string resourceName = "No pending download";

        public Data()
        {
            AuthenticationManager.OnLogin += async result =>
            {
                Account = result;
                string headIconCacheName = $"user-{account.Uuid}";

                if (CacheManager.HasBitmap(headIconCacheName))
                {
                    await Task.Run(() => { HeadIcon = CacheManager.LoadBitmap(headIconCacheName); });

                    return;
                }

                using (var imageStream = await LoadIconStreamAsync(result))
                {
                    if (imageStream == null) return;

                    HeadIcon = await Task.Run(() =>
                    {
                        try
                        {
                            return Bitmap.DecodeToWidth(imageStream, 400);
                        }
                        catch (Exception e)
                        {
                            return null;
                        }
                    });

                    CacheManager.Store(HeadIcon, headIconCacheName);
                }
            };
        }

        public int Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
        }

        public string ResourceName
        {
            get => resourceName;
            set => this.RaiseAndSetIfChanged(ref resourceName, value);
        }

        public string ResourceDetailsText
        {
            get => resourceDetailsText;
            set => this.RaiseAndSetIfChanged(ref resourceDetailsText, value);
        }

        public string ResourceCount
        {
            get => resourceCount;
            set => this.RaiseAndSetIfChanged(ref resourceCount, value);
        }

        public MinecraftAuthenticationResult? Account
        {
            get => account;
            set => this.RaiseAndSetIfChanged(ref account, value);
        }

        public Bitmap? HeadIcon
        {
            get => head;
            set => this.RaiseAndSetIfChanged(ref head, value);
        }

        private async Task<Stream> LoadIconStreamAsync(MinecraftAuthenticationResult? account)
        {
            if (account == null) return null;

            HttpClient client = new HttpClient();

            try
            {
                HttpResponseMessage resp = await client.GetAsync($"https://crafatar.com/renders/head/{account.Uuid}");
                if (!resp.IsSuccessStatusCode) return null;

                return await resp.Content.ReadAsStreamAsync();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }

    void ManageSkinsButtonClicked(object? sender, RoutedEventArgs e)
    {
        //Navigation.Push<SkinsPage>();
    }

    void AdvancedFeaturesButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.Push<AdvancedFeaturesPage>();
    }

    void UnitTestsButtonClicked(object? sender, RoutedEventArgs e)
    {
        new UnitTestsWindow().Show();
    }
}