# RDCproject
University project showcasing client-server communication using sockets and mutual exclusion on common resources.
This chat application consists of a server accepting connections from multiple clients, that also logs the messages in a log file.
The clients have a GUI created with WPF. 
The server moves each client to its own thread. 
The messages are broadcasted to clients and also entered in a message queue, used for logging purposes. The logger is situated on a different thread.

Requirements:

- [x] Should work on multiple clients
- [x] The server should use one thread for each client
- [x] The server should broadcast to all clients (clients should see each other)
- [x] Should have a graphical interface
- [x] Objects should be transmitted between client and server, instead of primitives
- [x] Should use mutual exclusion on common resources
