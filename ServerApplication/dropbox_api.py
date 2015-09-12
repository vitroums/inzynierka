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

    def createUserDirectory(self, guid):
        """
        Tworzy katalog użytkownika (o nazwie jego ID).

        Args:
            guid (str): ID użytkownika.
        """
        self.__createDir(guid)

    def getUsersList(self):
        """
        Pobiera bazę użytkowników z chmury i zwraca ją w formie listy.
        """
        self.__getDatabase()
        return self.__usersList

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

    def __getDatabase(self):
        """
        Pobiera z chmury listę użytkowników i zapisuje ją do listy.
        """
        self.__getFile(self.__configuration.databaseFileName)
        xmldoc = MD.parse(self.__configuration.databaseFileName)
        itemList = xmldoc.getElementsByTagName("User")
        self.__usersList = []
        for item in itemList:
            self.__addUserToList(item.attributes["guid"].value, item.attributes["nick"].value,
                                 item.attributes["mail"].value)

    def __createDatabase(self):
        """
        Zapisuje listę użytkowników do pliku xml.
        """
        root = ET.Element("Users")
        for item in self.__usersList:
            ET.SubElement(root, "User", guid=item.guid, nick=item.nick, mail=item.mail)
        tree = ET.ElementTree(root)
        tree.write(self.__configuration.databaseFileName)
        xmlText = MD.parse(self.__configuration.databaseFileName).toprettyxml()
        with open(self.__configuration.databaseFileName, "w") as dataBaseFile:
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

    def addNewUser(self, guid, name, mail):
        """
        Tworzy nowego użytkownika i zapisuje informacje do bazy danych.

        Args:
            guid (str): ID użykownika.
            name (str): Nazwa użytkownika.
            mail (str): E-mail użytkownika.
        """
        self.__getDatabase()
        self.__addUserToList(guid, name, mail)
        self.__createDatabase()
        self.__uploadFile(self.__configuration.databaseFileName)

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
