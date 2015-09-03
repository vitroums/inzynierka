from xml.etree import ElementTree as ET
import xml.dom.minidom as MD
import dropbox
from talker import Talker


class DropboxApi:
    __DATA_BASE_FILE_NAME = "list.xml"
    __DROPBOX_API_KEY = "q7RSg2cm1vAAAAAAAAAAC7sy1AfF2zsSStdhuG0KdJs3ieupiQ6A2Izek-5r8DE-"

    def __init__(self, GroupHash=""):
        self.__client = None
        self.__dir = "".join(["/", GroupHash])
        self.__database = None
        self.__usersList = None
        self.__connect(GroupHash)

    @Talker(before="Connecting to cloud...", after="Connected to the cloud")
    def __connect(self, GroupHash):
        try:
            self.__client = dropbox.client.DropboxClient(self.__DROPBOX_API_KEY)
        except dropbox.rest.ErrorResponse:
            print("ERROR! Couldnt connect to Cloud")

    
    @Talker(before="Loading database...", after="Database loaded")
    def __getDataBase(self):
        try:
            self.__database = self.__getFile(self.__DATA_BASE_FILE_NAME)
        except dropbox.rest.ErrorResponse:
            print("ERROR! Couldnt load DataBase!")

    @Talker(before="Getting file...", after="File downloaded from cloud")
    def __getFile(self, name, guid=''):
        try:
            filePath = "/".join([self.__dir, guid and "/".join([guid, name]) or name])
            fileData, metadata = self.__client.get_file_and_metadata(filePath)
            with open(name, "wb") as temp:
                temp.write(fileData.read())
            print("Saved: ", name)
            print(metadata)
            return fileData
        except dropbox.rest.ErrorResponse:
            print("ERROR! Couldnt get a file!")

    @Talker(before="Sending file...", after="File sent to cloud")
    def __uploadFile(self, name, guid=""):
        try:
            with open(name, "rb") as _file:
                filePath = "/".join([self.__dir, guid and "/".join([guid, name]) or name])
                response = self.__client.put_file(filePath, _file, True)
            print("Uploaded: "), response
        except dropbox.rest.ErrorResponse:
            print("ERROR! Couldnt upload a file!")

    @Talker(before="Creating directory on cloud...", after="Directory created on cloud")
    def __createDir(self, guid="", group=""):
        try:
            if group:
                self.__client.file_create_folder("/".join([self.__dir, guid]))
                print("Created directory: ", guid)
            else:
                self.__client.file_create_folder("".join(["/", group]))
                print("Created directory: ", group)
        except dropbox.rest.ErrorResponse:
            print("ERROR! Couldnt create a directory!")

    @Talker(before="Parsing database file...", after="Database parsed")
    def __parseDatabase(self):
        self.__getDataBase()
        xmldoc = minidom.parse(self.__DATA_BASE_FILE_NAME)
        itemlist = xmldoc.getElementsByTagName("User")
        self.__usersList = []
        for item in itemlist:
            self.__addUserToList(item.attributes["guid"].value, item.attributes["nick"].value, item.attributes["mail"].value)

    @Talker(before="Creating database...", after="Database created")
    def __createDatabase(self):
        root = ET.Element("Users")
        for item in self.__usersList:
            ET.SubElement(root, "User", guid=item.guid, nick=item.nick, mail=item.mail)
        tree = ET.ElementTree(root)
        tree.write(self.__DATA_BASE_FILE_NAME)
        xmlText = MD.parse(self.__DATA_BASE_FILE_NAME).toprettyxml()
        with open(self.__DATA_BASE_FILE_NAME, "w") as dataBaseFile:
            dataBaseFile.write(xmlText)

    def __addUserToList(self, guid, nick, mail):
        self.__usersList.append(User(guid, nick, mail))
    
    @Talker(before="Adding new user...", after="New user added")
    def addNewUser(self, guid, nick, mail):
        self.__parseDatabase()
        self.__addUserToList(guid, nick, mail)
        self.__createDatabase()
        self.__uploadFile(self.__DATA_BASE_FILE_NAME)


class User:
    def __init__(self, guid, nick, mail):
        self.guid = guid
        self.nick = nick
        self.mail = mail