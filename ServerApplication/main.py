from server import ClientServer

def main():
    _server = ClientServer()
    _server.serve_forever()

if __name__ == "__main__":
    main()
