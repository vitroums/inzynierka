from OpenSSL import crypto, SSL
import os
from utils.configuration import Configuration


class CertificateAuthority(object):
    """
    Klasa obsługująca CA serwera. Tworzy i podpisuje certyfikaty dla użytkowników.
    """
    __INSTANCE = None

    def __new__(cls, *args, **kwargs):
        """
        Implementacja wzorca Singletone. W programie zawsze istnieć będzie tylko jedna istancja klasy.
        """
        if not cls.__INSTANCE:
            cls.__INSTANCE = super().__new__(cls)
        return cls.__INSTANCE

    def __init__(self):
        """
        Tworzy całe CA. Wczytuje konfiguracje CA z pliku. Ładuje lub (w przypadku gdy nie istnieją)
        tworzy certyfikat i klucz prywatny.

        Args:
            certificatesDir (str): Katalog, w którym będą trzymane certyfikaty użytkowników.
            keysDir (str): Katalog, w któym będą trzymane klucze publiczne użytkowników.

        Raises:
            IOError: Jeśli nie można utworzyć klucza, albo odczytać go z pliku.
        """
        self.__configuration = Configuration()
        if not self.__checkFiles():
            self.__certificate, self.__keys = self.__newCaCertificate()
        else:
            self.__certificate = self.__loadCertificateFromFile(self.__configuration.certificateFile)
            self.__keys = self.__loadPrivateKeyFromFile(self.__configuration.keyFile, self.__configuration.caPassword)
        if not self.__keys:
            raise IOError

    def __checkFiles(self):
        """
        Sprawdza czy pliki certyfikatu i klucza serwera istnieją.

        Return:
            bool: True jeśli istnieją False jeśli nie istnieją
        """
        return os.path.exists(self.__configuration.certificateFile) and os.path.exists(self.__configuration.keysFile)

    def __checkInformations(self, informations):
        """
        Sprawdza czy podane informacje są słownikiem i zawierają odpowiednie klucze.

        Args:
            informations (dict): Słownik do sprawdzenia.

        Return:
            bool: True jeśli spełnia wymagane warunki False jeśli nie spełnia.
        """
        if not type(informations) is dict:
            return False
        for data in self.__configuration.informationsKeys:
            if data not in informations.keys():
                return False
        return True

    def __loadDataFromFile(self, path):
        """
        Ładuje dane z pliku.

        Args:
            path (str): Ścieżka do pliku.

        Return:
            str: Dane z pliku.

        Raise:
            FileNotFoundError: Ścieżka do pliku nie istnieje
            IOError: Problem z odczytem z pliku
        """
        if os.path.exists(path):
            with open(path, "rb") as keyFile:
                return keyFile.read()
        else:
            raise FileNotFoundError

    def __loadPrivateKeyFromFile(self, path, password):
        """
        Ładuje klucz prywatny z pliku i zwraca jako obiekt.

        Args:
            path (str): Ścieżka do pliku klucza.

        Return:
            OpenSSL.crypto.PKey: Obiekt klucza.
        """
        # TODO Dodać obsługę hasła
        try:
            return crypto.load_privatekey(crypto.FILETYPE_PEM, self.__loadDataFromFile(path))
        except FileNotFoundError:
            print("Key file doesn't exist")
        except IOError:
            print("Problem with key file")

    def __loadCertificateFromFile(self, path):
        """
        Ładuje certyfikat z pliku i zwraca jako obiekt.

        Args:
            path (str): Ścieżka do pliku certyfikatu.

        Return:
            OpenSSL.crypto.X509: Obiekt certyfikatu.
        """
        try:
            return crypto.load_certificate(crypto.FILETYPE_PEM, self.__loadDataFromFile(path))
        except FileNotFoundError:
            print("Certificate file doesn't exist")
        except IOError:
            print("Problem with certificate file")

    def __newCaCertificate(self):
        """
        Tworzy nowy certyfikat dla CA na podstawie danych zapisanych w pliku konfiguracyjnym.

        Return:
            OpenSSL.crypto.X509, OpenSSL.crypto.PKey: Obiekt certyfikatu i klucza.
        """
        try:
            certificate = self.__generateCertificate(self.__configuration.caInformations)
            keys = self.__generateKeys()
            certificate.set_issuer(certificate.get_subject())
            certificate.set_pubkey(keys)
            certificate.sign(keys, self.__configuration.signMethod)
            self.__saveCertificate(self.__configuration.certificateFile, certificate)
            self.__saveKeys(self.__configuration.keysFile, keys, self.__configuration.caPassword)
            return certificate, keys
        except ValueError:
            print("CA's informations corrupted")
        except IOError:
            print("Problem with saving files")
 
    def __generateCertificate(self, informations):
        """
        Generuje certyfikat na podstawie podanych informacji.

        Args:
            information (dict): Słownik zawierający informacje o podmiocie.
                Musi zawierać wszystkie te klucze:
                    country, state, city, organization, unit, name

        Return:
            OpenSSL.crypto.X509: Wygenerowany certyfikat

        Raises:
            ValueError: Jeśli informacje nie zawierają wszystkich kluczy
        """
        if not self.__checkInformations(informations):
            raise ValueError
        certificate = crypto.X509()
        certificate.get_subject().C = informations["country"]
        certificate.get_subject().ST = informations["state"]
        certificate.get_subject().L = informations["city"]
        certificate.get_subject().O = informations["organization"]
        certificate.get_subject().OU = informations["unit"]
        certificate.get_subject().CN = informations["name"]
        certificate.set_serial_number(1000)
        certificate.gmtime_adj_notBefore(self.__configuration.notBefore)
        certificate.gmtime_adj_notAfter(self.__configuration.notAfter)
        return certificate

    def __generateKeys(self, bits=2048):
        """
        Generuje parę kluczy o zadanej długości.

        Args:
            bits (Optional(int)): Długość klucza.

        Return:
            OpenSSL.crypto.PKey: Obiekt wygenerowanej pary kluczy.
        """
        keys = crypto.PKey()
        keys.generate_key(crypto.TYPE_RSA, bits)
        return keys

    def __signCertificate(self, certificate):
        """
        Podpisuje certyfikat kluczem CA.
        """
        certificate.set_issuer(self.__certificate.get_subject())
        certificate.sign(self.__keys, self.__configuration.signMethod)
        return certificate

    def __saveFile(self, path, data):
        """
        Zapisuje danych do pliku.

        Args:
            path (str): Ścieżka, pod którą zostanie zapisany plik.
            data (byte[]): Dane do zapisu.

        Return:
            bool: True jeśli zapis się powiódł
        """
        with open(path, "wb") as file:
            file.write(data)
        return True

    def __saveCertificate(self, path, certificate):
        """
        Zapisuje certyfikat do pliku.

        Args:
            path (str): Ścieżka, pod którą zostanie zapisany certyfikat.
            certificate (OpenSSL.crypto.X509): Dane certyfikatu.

        Return:
            bool: True jeśli zapis się powiódł
        """
        return self.__saveFile(path, crypto.dump_certificate(crypto.FILETYPE_PEM, certificate))
    
    def __saveKeys(self, path, key, password):
        """
        Zapisuje certyfikat do pliku.

        Args:
            path (str): Ścieżka, pod którą zostanie zapisany klucz.
            certificate (OpenSSL.crypto.PKey): Klucze do zapisu.
            password (str): Hasło chroniące klucz prywatny.

        Return:
            bool: True jeśli zapis się powiódł
        """
        # TODO Dodać obsługę hasła
        return self.__saveFile(path, crypto.dump_privatekey(crypto.FILETYPE_PEM, key))

    def newCertificate(self, informations, password, name):
        """
        Tworzy nowy certyfikat wraz z parą kluczy.

        Args:
            information (dict): Słownik zawierający informacje o podmiocie.
                Musi zawierać wszystkie te klucze:
                    country, state, city, organization, unit, name
            password (str): Hasło chroniące klucz prywatny.
            name (str): Nazwa plików, do których zapisane zostaną dane.

        Return:
            str, str: Ścieżka do pliku certyfikatu, ścieżka do pliku klucza

        Raises:
            ValueError: Jeśli informacje nie zawierają wszystkich kluczy
            IOError: Jeśli wystąpiły problemy z zapisem plików
        """
        certificate = self.__generateCertificate(informations)
        keys = self.__generateKeys()
        certificate.set_pubkey(keys)
        certificate = self.__signCertificate(certificate)
        certificatePath = os.path.join(self.__configuration.certificatesDir, ".".join([name, "crt"]))
        keysPath = os.path.join(self.__configuration.keysDir, ".".join([name, "key"]))
        self.__saveCertificate(certificatePath, certificate)
        self.__saveKeys(keysPath, keys, password)
        return certificatePath, keysPath