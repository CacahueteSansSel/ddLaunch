﻿using mcLaunch.Core.Boxes;
using mcLaunch.Core.Boxes.Format;
using mcLaunch.Core.Managers;

namespace mcLaunch.Core.Mods.Packs;

public class BoxBinaryModificationPack : ModificationPack
{
    private SerializedBox boxBinary;
    
    public override string Name
    {
        get => boxBinary.Name;
        init => boxBinary.Name = value;
    }
    public override string Author
    {
        get => boxBinary.Author;
        init => boxBinary.Author = value;
    }
    public override string Version
    {
        get => string.Empty;
        init => throw new NotImplementedException();
    }
    public override string? Id
    {
        get => boxBinary.Id;
        init => boxBinary.Id = value;
    }
    public override string? Description
    {
        get => boxBinary.Description;
        init => boxBinary.Description = value;
    }
    public override string MinecraftVersion
    {
        get => boxBinary.Version;
        init => boxBinary.Version = value;
    }
    public override string ModloaderId
    {
        get => boxBinary.ModLoaderId;
        init => boxBinary.ModLoaderId = value;
    }
    public override string ModloaderVersion
    {
        get => boxBinary.ModLoaderVersion;
        init => boxBinary.ModLoaderVersion = value;
    }

    public override SerializedModification[] Modifications { get; set; }
    public override AdditionalFile[] AdditionalFiles { get; set; }
    public byte[] IconData => boxBinary.IconData;
    public byte[] BackgroundData => boxBinary.BackgroundData;

    public BoxBinaryModificationPack()
    {
        
    }
    
    public BoxBinaryModificationPack(string filename) : base(filename)
    {
        boxBinary = new SerializedBox(filename);

        List<SerializedModification> mods = new();
        foreach (Mod mod in boxBinary.Mods)
        {
            mods.Add(new SerializedModification
            {
                IsRequired = true,
                ModId = mod.Id,
                PlatformId = mod.Platform,
                VersionId = mod.Version
            });
        }
        Modifications = mods.ToArray();

        List<AdditionalFile> files = new();
        foreach (FSFile file in boxBinary.Files)
        {
            files.Add(new AdditionalFile
            {
                Path = file.AbsFilename,
                Data = file.Data
            });
        }
        AdditionalFiles = files.ToArray();
    }
    
    public override async Task InstallModificationAsync(Box targetBox, SerializedModification mod)
    {
        await ModPlatformManager.Platform.InstallModAsync(targetBox, new Modification
        {
            Id = mod.ModId,
            ModPlatformId = mod.PlatformId
        }, mod.VersionId, false);
    }

    public override async Task ExportAsync(Box box, string filename)
    {
        SerializedBox bb = new SerializedBox
        {
            Id = box.Manifest.Id,
            Name = box.Manifest.Name,
            Author = box.Manifest.Author,
            Version = box.Manifest.Version,
            Description = box.Manifest.Description,
            ModLoaderId = box.Manifest.ModLoaderId,
            ModLoaderVersion = box.Manifest.ModLoaderVersion
        };

        MemoryStream iconStream = new();
        MemoryStream backgroundStream = new();
        if (box.Manifest.Icon != null) box.Manifest.Icon?.IconLarge!.Save(iconStream);
        box.Manifest.Background?.Save(backgroundStream);

        bb.IconData = iconStream.ToArray();
        bb.BackgroundData = backgroundStream.ToArray();

        List<Mod> mods = new();
        foreach (BoxStoredModification mod in box.Manifest.Modifications)
        {
            mods.Add(new Mod
            {
                Id = mod.Id,
                Filenames = mod.Filenames,
                Platform = mod.PlatformId,
                Version = mod.VersionId
            });
        }
        bb.Mods = mods.ToArray();

        List<FSFile> files = new();
        foreach (string file in box.GetAdditionalFiles())
        {
            string completePath = $"{box.Path}/minecraft/{file}";
            if (!File.Exists(completePath)) continue;
            
            byte[] data = await File.ReadAllBytesAsync(completePath);
            
            files.Add(new FSFile
            {
                AbsFilename = file,
                Data = data
            });
        }
        foreach (string modFile in box.GetUnlistedMods())
        {
            string completePath = $"{box.Path}/minecraft/{modFile}";
            if (!File.Exists(completePath)) continue;
            
            byte[] data = await File.ReadAllBytesAsync(completePath);
            
            files.Add(new FSFile
            {
                AbsFilename = modFile,
                Data = data
            });
        }
        bb.Files = files.ToArray();
        
        bb.Save(filename);
    }
}