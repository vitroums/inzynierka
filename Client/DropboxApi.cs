using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropNet;
using DropNet.Models;
using System.Xml;
using System.IO;

namespace Client
{
    public class DropboxApi
    {
        public DropboxApi(string group = "")
        {
            ConnectToCloud(group);
        }
        DropNetClient _client;
        List<User> _userList = new List<User>();
        List<Group> _groupsList = new List<Group>();
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
                string db = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(db);
                XmlNode UsersListNode = xd.SelectSingleNode("/Users");
                XmlNodeList UsersNodeList = UsersListNode.SelectNodes("User");
                foreach (XmlNode node in UsersNodeList)
                {
                    User _user = new User();
                    _user.guid = node.Attributes.GetNamedItem("guid").Value;
                    _user.nick = node.Attributes.GetNamedItem("nick").Value;
                    _user.mail = node.Attributes.GetNamedItem("mail").Value;
                    _userList.Add(_user);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't load database!");
                throw;
            }
        }

        // analogicnzie dla grup
        public void LoadGroups()
        {
            try
            {
                Console.WriteLine("Loading groups list, path: {0} ", _path);
                var fileBytes = _client.GetFile(_path + "groups.xml");
                string groups = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(groups);
                XmlNode GroupListNode = xd.SelectSingleNode("/Groups");
                XmlNodeList GroupsNodeList = GroupListNode.SelectNodes("Group");
                foreach (XmlNode node in GroupsNodeList)
                {
                    Group _group = new Group();
                    _group.name = node.Attributes.GetNamedItem("name").Value;
                    _group.password = node.Attributes.GetNamedItem("password").Value;
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

        // getter do _groupList
        public List<string> GetGroupsNamesList()
        {
            LoadGroups();
            List<string> names = new List<string>();
            foreach (Group g in _groupsList)
            {
                names.Add(g.name);
            }
            //List<string> tmpList = new List<string>();
            //var metaData = _client.GetMetaData("/", false, false);
            //for (int i = 0; i < metaData.Contents.Count; i++)
            //{
            //    if (metaData.Contents[i].Is_Dir == true)
            //    {
            //        tmpList.Add(metaData.Contents[i].Name);
            //    }
            //}
            //return tmpList;
            return names;
        }

        // walidacja poprawnosci hasla
        public bool ValidatePassword(string pass, string groupName)
        {
            foreach(Group g in _groupsList)
            {
                if (g.name == groupName && g.password == pass)
                    return true;
            }
            return false;
        }

        // getter do _userList
        public List<User> GetUsersList()
        {
            return _userList;
        }
    }

    public class User
    {
        public string guid { get; set; }
        public string nick { get; set; }
        public string mail { get; set; }

        public override string ToString()
        {
            return nick;
        }
    }

    public class Group
    {
        public string password { get; set; }
        public string name { get; set; }
        public override string ToString()
        {
            return name;
        }
    }
}
