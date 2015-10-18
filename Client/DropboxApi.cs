using System;
using System.Collections.Generic;
using System.Text;
using DropNet;
using DropNet.Models;
using System.Xml;
using System.IO;
using Client.ClientData;
using Client.ServerData;

namespace Client
{
    public class DropboxApi
    {
        public DropboxApi(string group = "")
        {
            ConnectToCloud(group);
        }
        DropNetClient _client;
        List<UserInfo> _userList = new List<UserInfo>();
        List<GroupInfo> _groupsList = new List<GroupInfo>();
        string _path = @"/";

        // łączenie do chmury - jeżeli jest podana grupa to ustawia to w ścieżce globalnej
        public void ConnectToCloud(string group = "")
        {
            try
            {
                Console.WriteLine("Connecting to cloud...");
                _client = new DropNetClient("grew37lbe3smob4", "dbtvg6rym3qbg2l", "q7RSg2cm1vAAAAAAAAAAC7sy1AfF2zsSStdhuG0KdJs3ieupiQ6A2Izek-5r8DE-");
                if (group != "")
                {
                    _path += group;
                    _path += "/";
                }
                Console.WriteLine("Successfully connected!");
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to ConnectToCloud!");
                throw;
            }
        }

        // ładowanie bazy danych użytkowników i zapisanie do listy _userList
        public void LoadDatabase()
        {
            try
            {
                Console.WriteLine("Loading db, path: {0} ", _path);
                var fileBytes = _client.GetFile(_path + "list.xml");
                File.WriteAllBytes("list.xml", fileBytes);
                XmlDocument xd = new XmlDocument();
                xd.Load("list.xml");
                XmlNode UsersListNode = xd.SelectSingleNode("/Users");
                XmlNodeList UsersNodeList = UsersListNode.SelectNodes("User");
                foreach (XmlNode node in UsersNodeList)
                {
                    UserInfo _user = new UserInfo();
                    _user.ID = node.Attributes.GetNamedItem("guid").Value;
                    _user.Name = node.Attributes.GetNamedItem("nick").Value;
                    _user.Email = node.Attributes.GetNamedItem("mail").Value;
                    _userList.Add(_user);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR! Couldnt load database");
            }
           
        }

        // analogicznie dla grup
        public void LoadGroups()
        {
            try
            {
                Console.WriteLine("Loading groups list, path: {0} ", _path);
                var fileBytes = _client.GetFile(_path + "group.xml");
                string groups = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(groups);
                XmlNode GroupListNode = xd.SelectSingleNode("/Groups");
                XmlNodeList GroupsNodeList = GroupListNode.SelectNodes("Group");
                foreach (XmlNode node in GroupsNodeList)
                {
                    GroupInfo _group = new GroupInfo();
                    _group.Name = node.Attributes.GetNamedItem("name").Value;
                    _group.Password = node.Attributes.GetNamedItem("password").Value;
                    _groupsList.Add(_group);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't load groups list!");
                throw;
            }
        }

        // pobieranie pliku, uniwersalna metoda, można i z katalogu głownego jak i bezposrednio od uzytkownika
        public void GetFile(string name, string guid = "")
        {
            try
            {
                var fileBytes = _client.GetFile(_path + guid + "/" + name);
                //string db = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);
                File.WriteAllBytes(name, fileBytes);
            }
            catch
            {
                Console.WriteLine("Failed to download {0}!", name);
            }
        }

        // analogicznie upload
        public void UploadFile(string name, string guid)
        {
            try
            {
                string path = Directory.GetCurrentDirectory() + "\\" + name;
                Console.WriteLine(path);
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                var result = _client.UploadFile(_path + guid + '/', name, bytes, true);
                Console.WriteLine(result);
   
            }
            catch (Exception)
            {
                Console.WriteLine("Couldnt upload {0} to path: {1}!", name, _path + guid + '/' + name);
            }       
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

        // getter do _groupList
        public List<string> GetGroupsNamesList()
        {
            LoadGroups();
            List<string> names = new List<string>();
            foreach (GroupInfo g in _groupsList)
            {
                names.Add(g.Name);
            }
            return names;
        }

        // getter do _usersList
        public List<string> GetUsersNamesList()
        {
            List<string> names = new List<string>();
            foreach (UserInfo u in _userList)
            {
                names.Add(u.Name);
            }
            return names;
        }

        // walidacja poprawnosci hasla
        public bool ValidatePassword(string pass, string groupName)
        {
            foreach(GroupInfo g in _groupsList)
            {
                if (g.Name == groupName && g.Password == pass)
                    return true;
            }
            return false;
        }

        // jeżeli nie istneje to dodanie uzytkowika
        public List<UserInfo> AddUserIfNotExist(UserInfo user)
        {
            bool exist = false;
            foreach (UserInfo u in _userList)
            {
                if (u.ID == user.ID)
                {
                    exist = true;
                    break;
                }
            }
            if (!exist)
            {
                _userList.Add(user);
            }
            UpdateGroupUserList();
            return _userList;
        }

        public void UpdateGroupUserList()
        {

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create("list.xml", settings))
            {
                writer.WriteStartElement("Users");
                foreach(UserInfo u in _userList)
                {
                    writer.WriteStartElement("User");
                    writer.WriteAttributeString("guid", u.ID);
                    writer.WriteAttributeString("nick", u.Name);
                    writer.WriteAttributeString("mail", u.Email);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();
            }
            byte[] bytes = System.IO.File.ReadAllBytes("list.xml");
            _client.UploadFile(_path, "list.xml", bytes);
        }

        // zaladowanie listy plikow uzytkownika 
        public List<string> GetFilesList(string guid)
        {
            List<string> files = new List<string>();
            SendWelcomeFile(guid);
            MetaData meta = _client.GetMetaData(_path+guid+"/", null, false, false);
            for (int i = 0; i < meta.Contents.Count; i++ )
            {
                if (meta.Contents[i].Is_Dir == false)
                {
                    files.Add(meta.Contents[i].Name);
                }
            }
            return files;
        }

        public void SendWelcomeFile(string guid)
        {
            string str = "Welcome!";
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            _client.UploadFile(_path + guid + "/", "welcome.txt", bytes);
        }

    }
}
