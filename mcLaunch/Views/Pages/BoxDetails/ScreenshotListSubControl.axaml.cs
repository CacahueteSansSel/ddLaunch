﻿using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class ScreenshotListSubControl : SubControl
{
    public ScreenshotListSubControl()
    {
        InitializeComponent();
    }
    
    public override async Task PopulateAsync()
    {
        Container.Children.Clear();
        
        foreach (string path in Box.GetScreenshotPaths())
            Container.Children.Add(new PictureFrame(path, Box));
    }
}