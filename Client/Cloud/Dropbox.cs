using System;
using System.Collections.Generic;
using System.Text;
using DropNet;
using DropNet.Models;
using DropNet.Exceptions;
using System.Xml;
using System.IO;
using Client.Clients;
using Client.Groups;
using Client.Errors;
using System.Threading.Tasks;

namespace Client.Cloud
{
    public class Dropbox
    {
        private DropNetClient _client;
        private string _path;
        public List<User> _usersList;
        private Configuration _configuration;

        public Dropbox(string group = null)
        {
            _configuration = Configuration.Instance;
            _path = CreatePath(group);
            ConnectToCloud();
        }

        private string CreatePath(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "/";
            }
            else
            {
                var pathBuilder = new StringBuilder();
                pathBuilder.Append("/");
                pathBuilder.Append(name);
                pathBuilder.Append("/");
                return pathBuilder.ToString();
            }
        }

        private void ConnectToCloud()
        {
            try
            {
                _client = new DropNetClient(_configuration.ApiKey, _configuration.AppSecret, _configuration.AccessToken);
            }
            catch (Exception)
            {
                throw new CloudException("Couldn't connect to cloud");
            }
        }

        public async Task<List<User>> GetUsersListAsync(User newUser = null)
        {
            try
            {
                var usersList = new List<User>();
                var tempFile = Path.GetTempFileName();
                await DownloadFileAsync(tempFile, _configuration.UsersListFileName);
                XmlDocument document = new XmlDocument();
                document.Load(tempFile);
                XmlNode usersListNode = document.SelectSingleNode(_configuration.UsersNodeName);
                XmlNodeList usersNodes = usersListNode.SelectNodes(_configuration.UserNodeName);
                foreach (XmlNode user in usersNodes)
                {
                    var id = user.Attributes.GetNamedItem(_configuration.UserIdAttributeName).Value;
                    var commonName = user.Attributes.GetNamedItem(_configuration.UserCommonNameAttributeName).Value;
                    var email = user.Attributes.GetNamedItem(_configuration.UserEmailAttributeName).Value;
                    usersList.Add(new User(id, commonName, email));
                }
                if (!(ReferenceEquals(null, newUser)))
                {
                    await AddNewUserToGroup(usersList, newUser);
                }
                File.Delete(tempFile);
                return usersList;
            }
            catch (DropboxException)
            {
                throw new CloudException("Couldn't load users list.");
            }
        }

        private async Task AddNewUserToGroup(List<User> usersList, User newUser)
        {
            try
            {
                if (!DoesUserExist(usersList, newUser))
                {
                    usersList.Add(newUser);

                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.Async = true;
                    var tempFile = Path.GetTempFileName();
                    using (XmlWriter writer = XmlWriter.Create(tempFile, settings))
                    {

                        await writer.WriteStartElementAsync(null, _configuration.UsersNodeName, null);
                        foreach (User user in usersList)
                        {
                            await writer.WriteStartElementAsync(null, _configuration.UserNodeName, null);
                            await writer.WriteAttributeStringAsync(null, _configuration.UserIdAttributeName, null, user.ID);
                            await writer.WriteAttributeStringAsync(null, _configuration.UserCommonNameAttributeName, null, user.CommonName);
                            await writer.WriteAttributeStringAsync(null, _configuration.UserEmailAttributeName, null, user.Email);
                            await writer.WriteEndElementAsync();
                        }
                        await writer.WriteEndElementAsync();
                        await writer.FlushAsync();
                    }
                    await _client.CreateFolderTask(_path + "/" + newUser.ID, (succes) => { }, (failure) => { });
                    await UploadFileAsync(tempFile, _configuration.UsersListFileName);
                    File.Delete(tempFile);
                }
            }
            catch (DropboxException)
            {
                throw new CloudException("Couldn't add user to group.");
            }
        }

        private bool DoesUserExist(List<User> usersList, User user)
        {
            var test = usersList.Find(users => users == user);
            return !(ReferenceEquals(null, test));
        }

        public async Task<List<Group>> GetGroupListAsync()
        {
            try
            {
                var groupList = new List<Group>();
                var tempFile = Path.GetTempFileName();
                await DownloadFileAsync(tempFile, _configuration.GroupListFileName);
                XmlDocument document = new XmlDocument();
                document.Load(tempFile);
                XmlNode groupsListNode = document.SelectSingleNode(_configuration.GroupsNodeName);
                XmlNodeList groupsNodes = groupsListNode.SelectNodes(_configuration.GroupNodeName);
                foreach (XmlNode group in groupsNodes)
                {
                    var name = group.Attributes.GetNamedItem(_configuration.GroupNameAttributeName).Value;
                    var password = group.Attributes.GetNamedItem(_configuration.GroupPasswordAttributeName).Value;
                    groupList.Add(new Group(name, password));
                }
                return groupList;
            }
            catch (DropboxException)
            {
                throw new CloudException("Couldn't load groups list");
            }
        }

        public async Task DownloadFileAsync(string path, string file, string rootPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(rootPath))
                {
                    rootPath = _path;
                }
                var fileBytes = (await _client.GetFileTask(rootPath + file)).RawBytes;
                using (var fileStream = File.Open(path, FileMode.OpenOrCreate))
                {
                    await fileStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                }
            }
            catch(DropboxException)
            {
                throw new CloudException("Couldn't dowload file from cloud.");
            }
        }

        private async Task UploadFileAsync(string path, string file, string rootPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(rootPath))
                {
                    rootPath = _path;
                }
                var fileBytes = File.ReadAllBytes(path);
                await _client.UploadFileTask(rootPath, file, fileBytes);
            }
            catch (DropboxException)
            {
                throw new CloudException("Couldn't send file to cloud.");
            }
        }

        private async Task DeleteFileAsync(string file, string rootPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(rootPath))
                {
                    rootPath = _path;
                }
                await _client.DeleteTask(rootPath + "/" + file);
            }
            catch (DropboxException)
            {
                throw new CloudException("Couldn't delete file on cloud.");
            }
        }

        public async Task UploadFileAsync(string path, User user, string file)
        {
            await UploadFileAsync(path, file, _path + "/" + user.ID);
        }

        public async Task DownloadFileAsync(string path, User user, string file)
        {
            await DownloadFileAsync(path, user.ID + "/" + file);
        }

        public async Task DeleteFileAsync(User user, string file)
        {
            await DeleteFileAsync(file, _path + "/" + user.ID);
        }

        public async Task DownloadCertificateAsync(string path, User user)
        {
            await DownloadFileAsync(path, user.ID + ".crt", "/");
        }

        public void UploadFile(string fullPath,string name, string guid)
        {
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                var result = _client.UploadFile(_path + guid + '/', name, bytes, true);
                Console.WriteLine(result);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldnt upload {0} to path: {1}!", name, _path + guid + '/' + name);
            }
        }
 
        public async Task<List<CloudFile>> GetFilesListAsync(User user)
        {
            List<CloudFile> files = new List<CloudFile>();
            MetaData meta = await _client.GetMetaDataTask(_path + user.ID + "/", null, false, false);
            foreach (var file in meta.Contents)
            {
                if (!file.Is_Dir)
                {
                    files.Add(new CloudFile(file));
                }
            }
            return files;
        }
    }
}
