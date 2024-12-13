# Python server: socket_server.py
import socket

# Set up the server parameters
HOST = '127.0.0.1'  # localhost
PORT = 65432        # Port to listen on

# Create a socket
with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))
    s.listen()
    print("Python server is listening on port", PORT)

    conn, addr = s.accept()
    with conn:
        print('Connected by', addr)
        while True:
            # Receive data from Unity
            data = conn.recv(1024)
            if not data:
                break
            print("Received from Unity:", data.decode())

            # Send a response to Unity
            response = "Hello from Python!"
            conn.sendall(response.encode())
