﻿using ddLaunch.Core.Boxes;

namespace ddLaunch.Core.Mods;

public abstract class ModPlatform
{
    public abstract string Name { get; }

    public abstract Task<Modification[]> GetModsAsync(int page, Box box, string searchQuery);

    public abstract Task<bool> InstallModificationAsync(Box targetBox, Modification mod, string versionId);

    public abstract Task<Modification> DownloadAdditionalInfosAsync(Modification mod);
}