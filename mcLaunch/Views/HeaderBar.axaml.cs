﻿using System;
using System.Diagnostics;
using Avalonia.Controls;
using mcLaunch.Launchsite.Http;
using mcLaunch.Utilities;

namespace mcLaunch.Views;

public partial class HeaderBar : UserControl
{
    public HeaderBar()
    {
        InitializeComponent();

        MacosButtonsMargin.IsVisible = OperatingSystem.IsMacOS();

        Api.OnNetworkError += OnApiNetworkError;
        Api.OnNetworkSuccess += OnApiNetworkSuccess;

        bool isLanuchEasterEgg = Random.Shared.Next(1, 1000) == 1;
        
#if DEBUG
        LogoDevelopment.IsVisible = !isLanuchEasterEgg;
        LogoDevelopmentLanuch.IsVisible = isLanuchEasterEgg;
        
        LogoBeta.IsVisible = false;
        LogoBetaLanuch.IsVisible = false;
#else
        LogoDevelopment.IsVisible = false;
        LogoDevelopmentLanuch.IsVisible = false;

        LogoBeta.IsVisible = !isLanuchEasterEgg;
        LogoBetaLanuch.IsVisible = isLanuchEasterEgg;
#endif
    }

    public void SetTitle(string title)
    {
        TitleText.Text = title;
    }

    private void OnApiNetworkSuccess(string url)
    {
        OfflineBadge.IsVisible = false;
    }

    private void OnApiNetworkError(string url)
    {
        OfflineBadge.IsVisible = true;
    }
}