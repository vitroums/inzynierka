from server import ClientServer

def main():
    HOST, PORT = "", 9999
    _server = ClientServer((HOST, PORT))
    _server.serve_forever()

if __name__ == "__main__":
    main()
