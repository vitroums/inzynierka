from xml.etree import ElementTree as ET
import xml.dom.minidom as MD
import dropbox
from utils.talker import Talker
from utils.configuration import Configuration
import os


class DropboxApi(object):
    """
    Obsługa połączenia serwera z chmurą. Klasa przeznaczona do wywoływania przy użyciu "with".
    Wszystkie pliki pobrane podczas życia instancji klasy zostaną na końcu usunięte.
    """
    def __init__(self, workDirectory=""):
        """
        Pobiera konfigurację serwera oraz zapisuje katalog roboczy. Tworzy też pustą listę plikó do usunięcia.

        Args:
            workDirectory (Optional(str)): Katalog roboczy (id użytkownika, nazwa grupy)
        """
        self.__configuration = Configuration()
        self.__dir = "".join(["/", workDirectory])
        self.__usedFiles = []


    def __enter__(self):
        """
        Tworzy połączenie z chmurą.
        """
        try:
            self.__client = dropbox.client.DropboxClient(self.__configuration.dropboxApiKey)
        except dropbox.rest.ErrorResponse:
            print("ERROR: Couldn't connect to cloud")
        return self

    def __exit__(self, type, value, tb):
        """
        Usuwa obiekt klienta chmury oraz wszystkie stworzone pliki.
        """
        del self.__client
        for _file in self.__usedFiles:
            os.remove(_file)

    def getUsersList(self):
        """
        Pobiera bazę użytkowników z chmury i zwraca ją w formie listy.
        """
        self.__getUserDatabase()
        return self.__usersList

    def getGroupList(self):
        """
        Pobiera bazę grup z chmury i zwraca ją w formie listy.
        """
        self.__getGroupDatabase()
        return self.__groupList

    def sendFile(self, name, newName):
        """
        Wysyła plik o wskazanej nazwie do katalogu roboczego na chmurze.
        
        Args:
            name (str): Nazwa pliku do wysłania.
            newName (Optional(str)): Nazwa pod jaką plik zostanie zapisany na chmurze.
        """
        self.__uploadFile(name, newName)

    def __getFile(self, name):
        """
        Pobiera plik o wskazanej nazwie z katalogu roboczego na chmurze.

        Args:
            name (str): Nazwa pliku do pobrania.
        """
        try:
            filePath = "/".join([self.__dir, name])
            fileData, metadata = self.__client.get_file_and_metadata(filePath)
            with open(name, "wb") as temp:
                temp.write(fileData.read())
        except dropbox.rest.ErrorResponse:
            print("ERROR: Couldn't get a file")
        except IOError:
            print("ERROR: Couldn't save a file")
        else:
            self.__usedFiles.append(name)

    def __uploadFile(self, name, newName=""):
        """
        Wysyła plik o wskazanej nazwie do katalogu roboczego na chmurze.

        Args:
            name (str): Nazwa pliku do wysłania.
            newName (Optional(str)): Nazwa pod jaką plik zostanie zapisany na chmurze.
        """
        try:
            filePath = "/".join([self.__dir, newName and newName or name])
            with open(name, "rb") as _file:
                response = self.__client.put_file(filePath, _file, True)
        except IOError:
            print("ERROR: Couldn't read a file")
        except dropbox.rest.ErrorResponse:
            print("ERROR: Couldn't upload a file")

    def __createDir(self, name):
        """
        Tworzy katalog w folderze roboczym na chmurze.

        Args:
            name (str): Nazwa tworzonego katalogu.
        """
        try:
            self.__client.file_create_folder("/".join([self.__dir, name]))
        except dropbox.rest.ErrorResponse:
            print("ERROR: Couldn't create a directory")

    def __getUserDatabase(self):
        """
        Pobiera z chmury listę użytkowników i zapisuje ją do listy.
        """
        self.__getFile(self.__configuration.userDatabaseFileName)
        xmldoc = MD.parse(self.__configuration.userDatabaseFileName)
        itemList = xmldoc.getElementsByTagName("User")
        self.__usersList = []
        for item in itemList:
            self.__addUserToList(item.attributes["guid"].value, item.attributes["nick"].value,
                                 item.attributes["mail"].value)

    def __createUserDatabase(self):
        """
        Zapisuje listę użytkowników do pliku xml.
        """
        root = ET.Element("Users")
        for item in self.__usersList:
            ET.SubElement(root, "User", guid=item.guid, nick=item.nick, mail=item.mail)
        tree = ET.ElementTree(root)
        tree.write(self.__configuration.userDatabaseFileName)
        xmlText = MD.parse(self.__configuration.userDatabaseFileName).toprettyxml()
        with open(self.__configuration.userDatabaseFileName, "w") as dataBaseFile:
            dataBaseFile.write(xmlText)

    def __getGroupDatabase(self):
        """
        Pobiera z chmury listę grup i zapisuje ją do listy
        """
        self.__getFile(self.__configuration.groupDatabaseFileName)
        xmldoc = MD.parse(self.__configuration.groupDatabaseFileName)
        itemList = xmldoc.getElementsByTagName("Group")
        self.__groupList = []
        for item in itemList:
            self.__addGroupToList(item.attributes["name"].value, item.attributes["password"].value)

    def __createGroupDatabase(self):
        """
        Zapisuje listę drup do pliku xml.
        """
        root = ET.Element("Groups")
        for group in self.__groupList:
            ET.SubElement(root, "Group", name=group.name, password=group.password)
        tree = ET.ElementTree(root)
        tree.write(self.__configuration.groupDatabaseFileName)
        xmlText = MD.parse(self.__configuration.groupDatabaseFileName).toprettyxml()
        with open(self.__configuration.groupDatabaseFileName, "w") as dataBaseFile:
            dataBaseFile.write(xmlText)
            

    def __addUserToList(self, guid, name, mail):
        """
        Dodaje użytkownika do listy.

        Args:
            guid (str): ID użykownika.
            name (str): Nazwa użytkownika.
            mail (str): E-mail użytkownika.
        """
        self.__usersList.append(User(guid, name, mail))

    def __addGroupToList(self, name, password):
        """
        Dodaje grupę do listy.

        Args:
            name (str): Nazwa grupy.
            password (str): Hash hasła.
        """
        self.__groupList.append(Group(name, password))

    def addNewUser(self, guid, name, mail):
        """
        Tworzy nowego użytkownika i zapisuje informacje do bazy danych.

        Args:
            guid (str): ID użykownika.
            name (str): Nazwa użytkownika.
            mail (str): E-mail użytkownika.
        """
        self.__getUserDatabase()
        self.__addUserToList(guid, name, mail)
        self.__createUserDatabase()
        self.__uploadFile(self.__configuration.userDatabaseFileName)

    def addNewGroup(self, name, password):
        """
        Tworzy nową grupę i zapisuje informacje do bazy danych.

        Args:
            name (str): Nazwa grupy.
            password (str): Hash hasła.
        """
        self.__getGroupDatabase()
        self.__addGroupToList(name, password)
        self.__createGroupDatabase()
        self.__uploadFile(self.__configuration.groupDatabaseFileName)
        self.__createDir(name)

    def createNewGroupUserList(self, guid, name, mail):
        """
        W nowoutworzonej grupie, tworzy listę użytkowników, oraz katalog pierwszego użytkownika.

        Args:
            guid (str): ID pierwszego użytkownika.
            name (str): Nazwa pierwszego użytkownika.
            mail (str): Adres e-mail pierwszego użytkownika.
        """
        self.__usersList = [User(guid, name, mail)]
        self.__createUserDatabase()
        self.__uploadFile(self.__configuration.userDatabaseFileName)
        self.__createDir(guid)

class User:
    def __init__(self, guid, nick, mail):
        self.__guid = guid
        self.__nick = nick
        self.__mail = mail

    @property
    def guid(self):
        return self.__guid

    @property
    def nick(self):
        return self.__nick

    @property
    def mail(self):
        return self.__mail

class Group(object):
    def __init__(self, name, password):
        self.__name = name
        self.__password = password

    @property
    def name(self):
        return self.__name

    @property
    def password(self):
        return self.__password
