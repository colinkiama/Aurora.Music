﻿// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Aurora.Music.ViewModels
{
    class AddFoldersViewViewModel : ViewModelBase
    {
        public DelegateCommand AddFolderCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    var folderPicker = new Windows.Storage.Pickers.FolderPicker
                    {
                        SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder
                    };
                    folderPicker.FileTypeFilter.Add(".mp3");
                    folderPicker.FileTypeFilter.Add(".m4a");
                    folderPicker.FileTypeFilter.Add(".wav");
                    folderPicker.FileTypeFilter.Add(".flac");

                    StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        var opr = SQLOperator.Current();
                        if (await opr.AddFolderAsync(folder))
                        {
                            var l = await opr.GetFolderAsync(folder.Path);
                            foreach (var item in l)
                            {
                                Folders.Add(new FolderViewModel(item));
                            }
                        }
                    }
                    else
                    {
                        return;
                    }

                    if (MainPageViewModel.Current != null)
                    {
                        var t = Task.Run(async () =>
                        {
                            await MainPageViewModel.Current.FindFileChanges();
                        });
                    }
                });
            }
        }

        internal async Task RemoveFolder(FolderViewModel folderViewModel)
        {
            var opr = SQLOperator.Current();
            await opr.RemoveFolderAsync(folderViewModel.ID);
            Folders.Remove(folderViewModel);
            if (MainPageViewModel.Current != null)
            {
                var t = Task.Run(async () =>
                {
                    await MainPageViewModel.Current.FindFileChanges();
                });
            }
        }

        private ElementTheme foreground = ElementTheme.Default;
        public ElementTheme Foreground
        {
            get { return foreground; }
            set { SetProperty(ref foreground, value); }
        }


        private bool includeMusicLibrary = Settings.Current.IncludeMusicLibrary;
        public bool IncludeMusicLibrary
        {
            get { return includeMusicLibrary; }
            set
            {
                SetProperty(ref includeMusicLibrary, value);
                Settings.Current.IncludeMusicLibrary = value;
                Settings.Current.Save();
            }
        }

        public ObservableCollection<FolderViewModel> Folders { get; set; }

        public AddFoldersViewViewModel()
        {
            Folders = new ObservableCollection<FolderViewModel>();
        }

        public void ChangeForeGround()
        {
            Foreground = ElementTheme.Dark;
        }

        public async Task Init()
        {
            var opr = SQLOperator.Current();
            var folders = await opr.GetAllAsync<FOLDER>();
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in folders)
                {
                    if (item.Path == KnownFolders.MusicLibrary.Path || item.Path == ApplicationData.Current.LocalFolder.Path)
                    {
                        continue;
                    }
                    if (!item.Path.IsNullorEmpty())
                        Folders.Add(new FolderViewModel(item));
                }
            });
        }
    }

    class FolderViewModel : ViewModelBase
    {
        public FolderViewModel(FOLDER item)
        {
            ID = item.ID;
            Folder = AsyncHelper.RunSync(async () => await item.GetFolderAsync());
            if (Folder == null)
            {
                NotAvaliable = true;
                Disk = "N/A";
                Path = item.Path;
                FolderName = Consts.Localizer.GetString("NotAvaliableText");
                SongsCount = 0;
                return;
            }
            Disk = item.Path.Split(':').FirstOrDefault();
            Path = item.Path;
            FolderName = Folder.DisplayName;
            SongsCount = item.SongsCount;
        }

        private bool notAva;
        public bool NotAvaliable
        {
            get { return notAva; }
            set { SetProperty(ref notAva, value); }
        }

        private bool isOpened;

        public bool IsOpened
        {
            get { return isOpened; }
            set { SetProperty(ref isOpened, value); }
        }

        public FolderViewModel() { }

        public string Disk { get; set; }

        public string FolderName { get; set; }

        public string Path { get; set; }

        public int SongsCount { get; set; }
        public int ID { get; }
        public StorageFolder Folder { get; set; }

        public string FormatCount(int count)
        {
            if (count < 0)
            {
                return Consts.Localizer.GetString("NeedScanText");
            }
            return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), count);
        }
    }
}
