﻿using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Views.Pages.Settings;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class GameSettingsSubControl : SubControl
{
    public GameSettingsSubControl()
    {
        InitializeComponent();
    }

    public override async Task PopulateAsync()
    {
        Container.Children.Clear();

        foreach (var kv in Box.Options.Where(opt => Box.Options.CanOptionBeChanged(opt.Key)))
        {
            GameSettingElement element = new GameSettingElement(Box, kv.Key);

            Container.Children.Add(element);
        }
    }
}