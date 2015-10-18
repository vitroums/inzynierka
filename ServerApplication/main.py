from server import ClientServer
from CA.certificate_authority import CertificateAuthority

def main():
    ca = CertificateAuthority()
    server = ClientServer()
    server.serve_forever()

if __name__ == "__main__":
    main()