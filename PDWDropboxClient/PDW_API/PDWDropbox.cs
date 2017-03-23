using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Users;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using System.Windows;
using System.Windows.Forms;
using System.IO;


namespace DropboxTest
{
    class PDWDropbox
    {
        string API_KEY = "JoNiHOiTCQAAAAAAAAAADyMEeIxlBwLx9Sdhx8dY7CnOhtyryqy-HDK5cns-zDQZ";
        DropboxClient client;
        public PDWDropbox()
        {
            client = new DropboxClient(API_KEY);
        }

        public async Task<UserInfo> GetAccountInfo()
        {
            FullAccount info = await client.Users.GetCurrentAccountAsync();
            UserInfo result = new UserInfo(info.Email,info.Name.GivenName,
                info.Name.Surname, info.ProfilePhotoUrl, info.ReferralLink, info.Locale, info.Country);
            return result;
        }

        public async Task<SpaceInfo> GetSpaceInfo()
        {

            SpaceUsage info = await client.Users.GetSpaceUsageAsync();
            SpaceInfo res = new SpaceInfo(info.Allocation.AsIndividual.Value.Allocated, info.Used);
            return res;
        }
        private ElementType GetItemType(string type)
        {
            ElementType result;
            switch (type)
            {
                case "pdf":
                    result = ElementType.pdf;
                    break;
                case "exe":
                    result = ElementType.exe;
                    break;
                case "jpg":
                case "png":
                case "bmp":
                    result = ElementType.img;
                    break;
                default:
                    result = ElementType.file;
                    break;
            }
            return result;
        }
        public async Task<ItemInformation[]> GetFilesList(string path)
        {
            try
            {
                ListFolderResult list = await client.Files.ListFolderAsync(path, recursive: false, includeMediaInfo: true);
                ItemInformation[] res = new ItemInformation[list.Entries.Count];
                Metadata element;
                for(int i=0;i<res.Length;i++)
                {
                    element=list.Entries[i];
                    res[i] = new ItemInformation();
                    res[i].Name = element.Name;
                    res[i].Fullpath = element.PathDisplay;
                    if (element.IsFolder) { res[i].Type = ElementType.dir; }
                    else
                    {
                        string[] temp = element.Name.Split('.');
                        res[i].Type = GetItemType(temp[temp.Length - 1]);
                        res[i].Size = element.AsFile.Size;
                        res[i].EditDate = element.AsFile.ServerModified; 
                    }
                }
                return res;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        public async Task<Metadata> DeleteAsync(string path)
        {
            var metadata = await client.Files.DeleteAsync(path);
            return metadata;
        }
        public async Task<ItemInformation> MoveAsync(string pathFrom, string pathTo)
        {
            var metadata = await client.Files.MoveAsync(
                pathFrom, pathTo,
                allowSharedFolder: true,
                autorename: true);
            ItemInformation result = new ItemInformation();
            result.Name = metadata.Name;
            result.Fullpath = metadata.PathDisplay;
            if (metadata.IsFolder) { result.Type = ElementType.dir; }
            else
            {
                string[] temp = metadata.Name.Split('.');
                result.Type = GetItemType(temp[temp.Length - 1]);
                result.Size = metadata.AsFile.Size;
                result.EditDate = metadata.AsFile.ServerModified;
            }
            return result;
        }
        public async Task<ItemInformation> CopyAsync(string pathFrom, string pathTo)
        {
            var metadata = await client.Files.CopyAsync(
                pathFrom, pathTo,
                allowSharedFolder: true,
                autorename: true);
            ItemInformation result = new ItemInformation();
            result.Name = metadata.Name;
            result.Fullpath = metadata.PathDisplay;
            if (metadata.IsFolder) { result.Type = ElementType.dir; }
            else
            {
                string[] temp = metadata.Name.Split('.');
                result.Type = GetItemType(temp[temp.Length - 1]);
                result.Size = metadata.AsFile.Size;
                result.EditDate = metadata.AsFile.ServerModified;
            }
            return result;
        }
        public async Task<ItemInformation> CreateDirectory(string path)
        {
            var metadata = await client.Files.CreateFolderAsync(
                path,
                autorename: true);
            ItemInformation result = new ItemInformation();
            result.Name = metadata.Name;
            result.Fullpath = metadata.PathDisplay;
            if (metadata.IsFolder) { result.Type = ElementType.dir; }
            else
            {
                string[] temp = metadata.Name.Split('.');
                result.Type = GetItemType(temp[temp.Length - 1]);
                result.Size = metadata.AsFile.Size;
                result.EditDate = metadata.AsFile.ServerModified;
            }
            return result;
        }
        public async Task<ItemInformation> UploadAsync(string path, Stream stream = null)
        {
            if (stream == null)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                if (dialog.ShowDialog() != DialogResult.OK) return null;
                stream = dialog.OpenFile();
            }
            FileMetadata metadata = await client.Files.UploadAsync(
            path,
            WriteMode.Overwrite.Instance,
            body: stream);
            ItemInformation result = new ItemInformation();
            result.Name = metadata.Name;
            result.Fullpath = metadata.PathDisplay;
            if (metadata.IsFolder) { result.Type = ElementType.dir; }
            else
            {
                string[] temp = metadata.Name.Split('.');
                result.Type = GetItemType(temp[temp.Length - 1]);
                result.Size = metadata.AsFile.Size;
                result.EditDate = metadata.AsFile.ServerModified;
            }
            return result;
        }
        public async Task DownloadAsync(string path, string SaveDirectory)
        {
            var fileMetadata = await client.Files.DownloadAsync(path);
            Stream stream = await fileMetadata.GetContentAsStreamAsync();
            SaveStreamToFile(SaveDirectory, stream);
        }

        public async Task<ItemInformation> ShareAsync(string path)
        {
            var metadata = await client.Sharing.CreateSharedLinkWithSettingsAsync(path);
            ItemInformation result = new ItemInformation();
            result.Name = metadata.Name;
            result.Fullpath = metadata.PathLower;
            result.Share_url = metadata.Url;
            if (metadata.IsFolder) { result.Type = ElementType.dir; }
            else
            {
                string[] temp = metadata.Name.Split('.');
                result.Type = GetItemType(temp[temp.Length - 1]);
                result.Size = metadata.AsFile.Size;
                result.EditDate = metadata.AsFile.ServerModified;
            }
            return result;
        }
        public async Task UnShareAsync(string url)
        {
            await client.Sharing.RevokeSharedLinkAsync(url);
        }
        public async Task<ItemInformation[]> ListOfSharedLinksAsync()
        {
            var list = await client.Sharing.ListSharedLinksAsync(null, null, false);
            ItemInformation[] res = new ItemInformation[list.Links.Count];
            SharedLinkMetadata element;
            for (int i = 0; i < res.Length; i++)
            {
                element = list.Links[i];
                res[i] = new ItemInformation();
                res[i].Name = element.Name;
                res[i].Fullpath = element.PathLower;
                if (element.IsFolder) { res[i].Type = ElementType.dir; }
                else
                {
                    string[] temp = element.Name.Split('.');
                    res[i].Type = GetItemType(temp[temp.Length - 1]);
                    res[i].Size = element.AsFile.Size;
                    res[i].EditDate = element.AsFile.ServerModified;
                }
                res[i].Share_url = element.Url;
            }
            return res;
        }
        public async Task<ItemInformation[]> SearchAsync(string path,string query,uint takeCount = 1000,bool includeContent = true)
        {
            SearchMode serachMode;
            if(includeContent) { serachMode = SearchMode.FilenameAndContent.Instance;}
            else {serachMode = SearchMode.Filename.Instance;}
            var list = await client.Files.SearchAsync(path,query,0,takeCount,serachMode);
            ItemInformation[] res = new ItemInformation[list.Matches.Count];
            Metadata element;
            for (int i = 0; i < res.Length; i++)
            {
                element = list.Matches[i].Metadata;
                res[i] = new ItemInformation();
                res[i].Name = element.Name;
                res[i].Fullpath = element.PathLower;
                if (element.IsFolder) { res[i].Type = ElementType.dir; }
                else
                {
                    string[] temp = element.Name.Split('.');
                    res[i].Type = GetItemType(temp[temp.Length - 1]);
                    res[i].Size = element.AsFile.Size;
                    res[i].EditDate = element.AsFile.ServerModified;
                }
            }
            return res;
        }	
        private void SaveStreamToFile(string filename, Stream stream)
        {
            using (FileStream fileStream = File.Create(filename))
            {
                int byte_value;
                while ((byte_value=stream.ReadByte())!= -1)
                {
                    fileStream.WriteByte((byte)byte_value);
                }
            }
        }
    }
    
}
