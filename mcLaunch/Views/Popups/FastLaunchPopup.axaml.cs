﻿using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.DataContexts;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

public partial class FastLaunchPopup : UserControl
{
    public FastLaunchPopup()
    {
        InitializeComponent();
        DataContext = new MinecraftVersionSelectionDataContext();

        VersionSelector.OnVersionChanged += version => { FetchModLoadersLatestVersions(version.Id); };

        FetchModLoadersLatestVersions(VersionSelector.Version.Id);
    }

    private async void FetchModLoadersLatestVersions(string versionId)
    {
        LaunchButton.IsEnabled = false;
        MinecraftVersionSelectionDataContext ctx = (MinecraftVersionSelectionDataContext) DataContext;
        List<ModLoaderSupport> all = new();

        foreach (ModLoaderSupport ml in ModLoaderManager.All)
        {
            ModLoaderVersion? version = await ml.FetchLatestVersion(versionId);
            if (version != null) all.Add(ml);
        }

        ctx.ModLoaders = all.Select(m => new DataContextModLoader(m)).ToArray();
        ctx.SelectedModLoader = ctx.ModLoaders[0];
        LaunchButton.IsEnabled = true;
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void LaunchButtonClicked(object? sender, RoutedEventArgs e)
    {
        ManifestMinecraftVersion minecraftVersion = VersionSelector.Version;
        ModLoaderSupport modloader = ((MinecraftVersionSelectionDataContext) DataContext).SelectedModLoader.ModLoader;
        string name = $"Minecraft {minecraftVersion.Id}";

        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup("Preparing launch",
            $"Preparing launching {name}..."));
        StatusPopup.Instance.ShowDownloadBanner = true;

        // We fetch automatically the latest version of the modloader for now
        // TODO: Allow the user to select a specific modloader version
        ModLoaderVersion[]? modloaderVersions = await modloader.GetVersionsAsync(minecraftVersion.Id);

        if (modloaderVersions == null || modloaderVersions.Length == 0)
        {
            Navigation.HidePopup();
            Navigation.ShowPopup(new MessageBoxPopup("Failed to initialize the mod loader",
                $"Failed to get any version of {modloader.Name} for Minecraft {minecraftVersion.Id}"));
            return;
        }

        BoxManifest newBoxManifest = new BoxManifest(name, null, "FastLaunch", modloader.Id, modloaderVersions[0].Name,
            null, minecraftVersion, BoxType.Temporary);

        string path = await BoxManager.Create(newBoxManifest);
        Box box = new Box(path);
        box.SetAndSaveIcon(new Bitmap(AssetLoader.Open(
            new Uri("avares://mcLaunch/resources/fastlaunch_box_logo.png"))));

        BoxDetailsPage detailsPage = new BoxDetailsPage(box);
        await detailsPage.RunAsync();
    }
}