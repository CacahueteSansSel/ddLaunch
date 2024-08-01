﻿using mcLaunch.Launchsite.Http;
using mcLaunch.Launchsite.Models;

namespace mcLaunch.Launchsite.Core.ModLoaders;

public class BabricModLoaderVersion : ModLoaderVersion
{
    public override async Task<Result<MinecraftVersion>> GetMinecraftVersionAsync(string minecraftVersionId)
    {
        string url =
            $"{BabricModLoaderSupport.Url}/v2/versions/loader/{minecraftVersionId}/{Name}/profile/json";

        return new Result<MinecraftVersion>(await Api.GetAsync<MinecraftVersion>(url, true));
    }
}