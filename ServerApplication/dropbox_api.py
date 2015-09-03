from xml.dom import minidom
from xml.etree import ElementTree as ET
import xml.dom.minidom as MD
import dropbox


class DropboxApi:
    # wywo³anie:
    # obj = DropboxApi() -> pracujemy bez ustawionej grupy
    # obj = DropboxApi('group01') -> pracujemy w folderze group01
    def __init__(self, GroupHash=''):
        self._connect(GroupHash)

    # £¹czymy siê z chmur¹ po AuthToken, je¿eli by³a wpisana to ustawiana jest grupa w œcie¿ce
    def _connect(self, GroupHash):
        print("Connecting to Cloud.")
        try:
            self.__client = dropbox.client.DropboxClient('q7RSg2cm1vAAAAAAAAAAC7sy1AfF2zsSStdhuG0KdJs3ieupiQ6A2Izek-5r8DE-')
            self.__dir = '/' + GroupHash
            if GroupHash != '':
                self.__dir += '/'
                print("Group: " + GroupHash)
        except:
            print("ERROR! Couldnt connect to Cloud")

    # pobranie XML'a bazy
    def _getDataBase(self):
        print("Loading DataBase.")
        try:
            self.__database = self._getFile('list.xml')
        except:
            print("ERROR! Couldnt load DataBase!")

    # pobranie pliku o nazwie Name, mo¿na wpisaæ Guid u¿ytkownika (domyœlnie puste np. do pobrania z grupy bezpoœrednio)
    # wywo³anie:
    # f = self._getFile('/plik.txt') -> z g³ownego katalogu(grupy je¿eli by³a ustawiona) jest pobierany plik plik.txt
    # f = self._getFile(/plik.txt', 'JanKowalskiGuid') -> pobieramy z folderu JanaKowalskiego plik plik.txt
    def _getFile(self, Name, Guid=''):

        try:
            if Guid == '':
                _f, _metadata = self.__client.get_file_and_metadata(self.__dir + Name)
            else:
                _f, _metadata = self.__client.get_file_and_metadata(self.__dir + Guid + '/' + Name)

            _out = open(Name, 'wb')
            _out.write(_f.read())
            _out.close()
            print("Saved: ", Name)
            print(_metadata)
            return _f
        except:
            print("ERROR! Couldnt get a file!")

    # analogicznie wysy³anie, Guid opcjonalny
    def _uploadFile(self, Name, Guid=''):
        try:
            _f = open(Name, 'rb')
            if Guid == '':
                _response = self.__client.put_file(self.__dir + Name, _f, True)
            else:
                _response = self.__client.put_file(self.__dir + Guid + '/' + Name, _f, True)

            print("Uploaded: "), _response
        except:
            print("ERROR! Couldnt upload a file!")

    # stworzenie folderu, w zale¿noœci co podamy, grupy lub guid'a
    # wywo³anie:
    # client._createDir('JanKowalskiGuid') -> tworzy folder w folderze grupy
    # client._createDir('', 'group01') -> tworzy folder grupy w root '/' tj. /group01/
    #
    # UWAGA! Je¿eli uploadujemy certyfikat nowego usera do œcie¿ki /grupaXX/guidXX/cert.pem to 
    # folder guidXX jak i grupaXX utworzony zostanie automatycznie je¿eli nie istnieje
    def _createDir(self, Guid='', Group=''):
        try:
            if Group=='':
                self.__client.file_create_folder(self.__dir + Guid)
                print("Created directory: ", Guid)
            else:
                self.__client.file_create_folder('/' + Group)
                print("Created directory: ", Group)
        except:
            print("ERROR! Couldnt create a directory!")

    # parsujemy XML'a u¿ytkonwików do listy 
    def _parseDatabase(self):
        self._getDataBase()
        _xmldoc = minidom.parse('list.xml')
        _itemlist = _xmldoc.getElementsByTagName('User')
        _userslist = []
        for s in _itemlist:
            _newUser = User(s.attributes['guid'].value, s.attributes['nick'].value, s.attributes['mail'].value)
            _userslist.append(_newUser)
        self._usersList = _userslist

    # tworzymy nowy plik XML bazy na posdtawie listy
    #
    # jak chcemy dodac nowego uzytkownika to parsujemy stara baze, dodajemy do _self._userslist nowego uzytkownika,
    # tworzymy baze _createDatabse i zapisujemy na chmurze _uploadFile
    def _createDatabae(self):
        _root = ET.Element("Users")
        for s in self._usersList:
            ET.SubElement(_root, "User", guid=s._guid, nick=s._nick, mail=s._mail)
        _tree = ET.ElementTree(_root)
        _tree.write("list.xml")
        x = MD.parse("list.xml").toprettyxml()
        file_ = open("list.xml", 'w')
        file_.write(x)
        file_.close()

    def _addUserToList(self, guid, nick, mail):
        self._usersList.append(User(guid, nick, mail))

class User:
    def __init__(self, guid, nick, mail):
        self._guid = guid
        self._nick = nick
        self._mail = mail
