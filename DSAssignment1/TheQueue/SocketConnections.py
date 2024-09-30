import zmq

#ZMQ Sockets
context = zmq.Context()
context2 = zmq.Context()
context3 = zmq.Context()

#Request socket
reqSocket = context.socket(zmq.REQ)
reqSocket.connect("tcp://ds.iit.his.se:5556")

#Subscribe socket
subSocket = context2.socket(zmq.SUB)
subSocket.setsockopt_string(zmq.SUBSCRIBE, 'queue')
subSocket.connect("tcp://ds.iit.his.se:5555")

#Sub2 socket
subSocket2 = context3.socket(zmq.SUB)
subSocket2.setsockopt_string(zmq.SUBSCRIBE, 'supervisors')
subSocket2.connect("tcp://ds.iit.his.se:5555")