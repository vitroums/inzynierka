﻿from handlers.server_handler_helper import ServerHandlerHelper
from uuid import uuid1
from dropbox_api import DropboxApi
from CA.certificate_authority import CertificateAuthority
import dropbox
import os
from hashlib import md5
from handlers.errors.response_error import ResponseError
from handlers.errors.command_error import CommandError
from handlers.errors.authentication_error import AuthenticationError


class ClientServerHandler(ServerHandlerHelper):
    """
    Główna klasa do obsługi połączeń serwera.
    """
    def __init__(self, request, client_address, server):
        """
        Tworzy handler dla serwera. Pobiera instancje klasy konfiguracji
        """
        self.__ca = CertificateAuthority()
        self.__dropboxApiHandler = DropboxApi()
        super().__init__(request, client_address, server)

    def handle(self):
        """
        Obsługuje połączenie przychodzące
        """
        try:
            command = self._receiveString()
            if command == "new-user":
                self.__newUserCommand()
            elif command == "login":
                result, id, name, mail = self.__authenticateUser()
                if result:
                    command = self._receiveString()
                    if command == "new-group":
                        self.__newGroupCommand(id, name, mail)
                    else:
                        self._sendString("unknown-command")        
            else:
                self.__commandError(command)
        except ResponseError as error:
            print(error)
        except CommandError as error:
            print(error)
        except AuthenticationError as error:
            print(error)

    def __newUserCommand(self):
        """
        Obsługuję proces tworzenia nowego użytkownika
        """
        name, mail = self.__requestUserData()
        if self.__doesUserExist(name, mail):
            self._sendString("user-exists")
            return
        informations, keysPassword, rescuePassword = self.__requestUserInformations()
        uuid = str(uuid1())
        certificate, keys = self.__ca.newCertificate(informations, keysPassword, uuid)
        if self.__addUserToCloud(name, mail, uuid, certificate):
            self._sendString("user-added")
            self.__sendNewUserFiles(certificate, keys, uuid)
            response = self._receiveString()
            if response != "everything-ok":
                self.__responseError("everything-ok", response)
            with open("/".join([self._configuration.passwordsDir, uuid]), "w") as passwordFile:
                passwordFile.write(md5(rescuePassword.encode(self._configuration.encoding)).hexdigest())
        else:
            self._sendString("problem-while-adding-user")
            os.remove(certificate)
            os.remove(keys)

    def __requestUserData(self):
        """
        Żąda od użytkownika jego podstawowych danych.

        Return:
            str, str: Nazwa użytkownika i jego adres e-mail.
        """
        self._sendString("provide-user-name")
        response = self._receiveString()
        if response != "user-name":
            self.__responseError("user-name", response)
        name = self._receiveString()
        self._sendString("provide-user-mail")
        response = self._receiveString()
        if response != "user-mail":
            self.__responseError("user-name", response)
        mail = self._receiveString()
        return name, mail

    def __doesUserExist(self, name, mail, uuid=None):
        """
        Sprawdza w bazie danych czy istnieje podany użytkownik.

        Args:
            name (str): Nazwa użytkownika.
            mail (str): Adres e-mail.
            uuid (Optional(str)): ID użytkownika.

        Return:
            bool: True jeśli użytkownik istnieje, False jeżeli nie istnieje. 
        """
        with DropboxApi() as cloud:
            usersList = cloud.getUsersList()
            for user in usersList:
                if uuid:
                    if user.mail == mail and user.nick == name and user.guid == uuid:
                        return True
                else:
                    if user.mail == mail and user.nick == name:
                        return True
        return False

    def __requestUserInformations(self):
        """
        Żąda od użytkonika przesłania danych potrzebnych do stworzenia certyfikatu.
        Po otrzymaniu parsuje je do postaci potrzebnej dla CA.

        Return:
            dict: Słownik z danymi użytkownika.
        """
        self._sendString("provide-user-data")
        response = self._receiveString()
        if response != "user-data":
            self.__responseError("user-data", response)
        data = self._receiveString().split(";")
        
        self._sendString("provide-keys-password")
        response = self._receiveString()
        if response != "keys-password":
            self.__responseError("keys-password", response)
        keysPassword = self._receiveString()
        
        self._sendString("provide-rescue-password")
        response = self._receiveString()
        if response != "rescue-password":
            self.__responseError("rescue-password", response)
        rescuePassword = self._receiveString()

        return {self._configuration.informationsKeys[i]:data[i]
                for i in range(0, len(self._configuration.informationsKeys))}, keysPassword, rescuePassword

    def __generateCertificate(self, informations, password, uuid):
        """
        Generuje certyfikat i klucz dla nowego użytkownika.

        Retrun:
            str, str: Ścieżka do pliku certyfikatu, ścieżka do pliku klucza
        """
        try:
            return self.__ca.newCertificate(informations, password, uuid)
        except ValueError:
            self._sendString("wrong-information-format")
        except IOError:
            self._sendString("server-error")

    def __addUserToCloud(self, name, mail, uuid, certificate):
        """
        Dodaje użytkownika do bazy danych w chmurze i zapisuje tam jego certyfikat.

        Args:
            name (str): Nazwa użytkownika.
            mail (str): Mail użytkownika.
            uuid (str): ID użytkownika.
            certificate (str): Ścieżka do pliku certyfikatu

        Return:
            bool: True jeśli udały się wszystki operacje, False jeśli nie.
        """
        try:
            with DropboxApi() as cloud:
                cloud.addNewUser(uuid, name, mail)
                cloud.sendFile(certificate, ".".join([uuid, "crt"]))
        except dropbox.rest.ErrorResponse:
            return False
        else:
            return True

    def __sendNewUserFiles(self, certificate, keys, uuid):
        """
        Wysłanie plików certyfikatu i klucza do użytkownika.

        Args:
            certificate (str): Ścieżka do pliku certyfikatu.
            keys (str): Ścieżka do pliku kluczy.
            uuid (str): ID użytkownika.
        """
        self._sendString("certificate-file")
        self._sendFile(certificate)
        self._sendString("keys-file")
        self._sendFile(keys)
        self._sendString("user-id")
        self._sendString(uuid)
        
    def __authenticateUser(self):
        """
        Sprawdzanie tożsamości użytkownika.

        Return:
            bool, str, str, str: True jeśli użytkownik zostanie poprawnie zweryfikowany, False jeśli nie.
                Przy udanym sprawdzeniu toższsamości użytkownika, zwraca jego dane.
        """
        self._sendString("provide-user-id")
        response = self._receiveString()
        if response != "user-id":
            self.__responseError("user-id", response)
        id = self._receiveString()

        self._sendString("provide-user-name")
        response = self._receiveString()
        if response != "user-name":
            self.__responseError("user-name", response)
        name = self._receiveString()

        self._sendString("provide-user-mail")
        response = self._receiveString()
        if response != "user-mail":
            self.__responseError("user-mail", response)
        mail = self._receiveString()

        if not self.__doesUserExist(name, mail, id):
            self._sendString("user-doesnt-exist")
        orginalMessage = "message"
        certificateFile = "/".join([self._configuration.certificatesDir, ".".join([id, "crt"])])
        chiper = self.__ca.encryptStringWithPublicKey(orginalMessage, certificateFile)
        self._sendString("decrypt-message")
        self._sendString(chiper)

        response = self._receiveString()
        if response != "decrypted-message":
            self.__responseError("decrypted-message", response)
        decryptedMessage = self._receiveString()
        if decryptedMessage == orginalMessage:
            self._sendString("permission-granted")
        else:
            self._sendString("permission-denied")
            self.__authenticationError("Decryption failed")            
        return True, id, name, mail

    def __newGroupCommand(self, id, login, mail):
        """
        Obsługuje porces tworzenia nowej grupy.
        """
        name, password = self.__requestGroupData()
        if self.__doesGroupExist(name):
            self._sendString("group-exists")
            return
        if self.__addGroupToCloud(name, password, id, login, mail):
            self._sendString("group-added")
        else:
            self._sendString("problem-while-adding-group")

    def __requestGroupData(self):
        """
        Żąda od użytkownika danych o grupie.

        Return:
            str, str: Nazwa grupy i jej hasło.
        """
        self._sendString("provide-group-name")
        response = self._receiveString()
        if response != "group-name":
            self.__responseError("group-name", response)
        name = self._receiveString()

        self._sendString("provide-group-password")
        response = self._receiveString()
        if response != "group-password":
            self.__responseError("group-password", response)
        password = md5(self._receiveString().encode(self._configuration.encoding)).hexdigest()
        return name, password

    def __doesGroupExist(self, name):
        """
        Sprawdza w bazie danych czy istnieje podana grupa.

        Args:
            name (str): Nazwa grupy.

        Return:
            bool: True jeśli grupa istnieje, False jeśli gupa nie istnieje.
        """
        with DropboxApi() as cloud:
            groupList = cloud.getGroupList()
            for group in groupList:
                if group.name == name:
                    return True
        return False

    def __addGroupToCloud(self, name, password, uuid, login, mail):
        """
        Tworzy grupę na chmurze i zapisije infromacje do bazydanych. Tworzy listę użytkownikó grupy.

        Args:
            name (str): Nazwa grupy.
            password (str): Hash hasła.
            uuid (str): ID użytkownika tworzącego grupę.

        Return:
            bool: True jeśli udały się wszystki operacje, False jeśli nie.
        """
        try:
            # TODO Dodać obsługę tworzenia bazy danych uiżytkowników grupy
            with DropboxApi() as cloud:
                cloud.addNewGroup(name, password)
            with DropboxApi(name) as cloud:
                cloud.createNewGroupUserList(uuid, login, mail)
        except dropbox.rest.ErrorResponse:
            return False
        else:
            return True

    def __responseError(self, expected, response):
        message = " ".join(["Excepted message:", expected, "Received message:", response])
        raise ResponseError(message)

    def __commandError(self, command):
        message = " ".join([command, "is not valid command"])
        raise CommandError(message)

    def __authenticationError(self, message):
        raise AuthenticationError(message)