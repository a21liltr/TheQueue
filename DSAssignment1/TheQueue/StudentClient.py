import tkinter as tk
import threading
import json

from TheQueue.SocketConnections import subSocket, reqSocket, subSocket2


class StudentClient:

    def __init__(self):

        # Window for the client
        self.window = tk.Tk()
        self.window.title("Student Client")
        self.window.geometry("500x500")

        name_var = tk.StringVar()
        name_label = tk.Label(self.window, text="Name:")
        queue_label = tk.Label(self.window, text="Queue:")
        supervisors_label = tk.Label(self.window, text="Supervisors:")
        self.name_entry = tk.Entry(self.window, textvariable=name_var)

        sub_btn = tk.Button(self.window, text="Submit", command=self.testfunc)

        self.list = []
        self.tlist = []
        self.slist = []

        self.queueText = tk.Text(self.window, height=20, width=30)
        self.supervisorsText = tk.Text(self.window, height=12, width=25)

        name_label.grid(row=0, column=0)
        self.name_entry.grid(row=1, column=0)
        sub_btn.grid(row=2, column=0)

        supervisors_label.grid(row=5, column=3)
        self.supervisorsText.grid(row=6, column=3)

        queue_label.grid(row=5, column=0)
        self.queueText.grid(row=6, column=0)

        t2 = threading.Thread(target=self.readQueue)
        t2.start()

        t3 = threading.Thread(target=self.readSupervisors)
        t3.start()
        self.window.mainloop()

    def submit(self):
        self.name = self.name_entry.get()
        messagestring = "{\"clientId\": \"123qwe\", \"enterQueue\": true, \"name\": " + "\"" + self.name + "\"}"
        reqSocket.send_string(messagestring)
        reqSocket.recv()
        print("Name is: ", self.name)
        self.name_entry.delete(0, tk.END)

    def hbthread(self):
        t1 = threading.Thread(target=self.heartbeat())
        t1.start()

    def heartbeat(self):
        hbmessage = "{\"clientId\": \"123qwe\"}"
        reqSocket.send_string(hbmessage)
        reqSocket.recv()
        self.window.after(1000, self.heartbeat)

    def testfunc(self):
        self.hbthread()
        self.submit()


    def readQueue(self):

        while True:
            self.list.clear()
            self.tlist.clear()
            msg = subSocket.recv_string()
            try:
                data = json.loads(msg)
                for element in data:
                    self.list.append(element.get('name'))
                    self.tlist.append(element.get('ticket'))
            except json.JSONDecodeError as e:
                print(f"JSON decode error: {e}")
                continue

            print(self.list)
            print(self.tlist)

            self.queueText.configure(state=tk.NORMAL)
            self.queueText.delete("1.0", tk.END)

            res = '\n'.join('% s | Ticket:  % s' % i for i in zip(self.list, self.tlist))
            self.queueText.insert(tk.END, res + "\n")

            #for x in self.list:
                #self.queueText.insert(tk.END, x + "\n")

            self.queueText.configure(state=tk.DISABLED)

    def readSupervisors(self):

        while True:
            self.slist.clear()
            msg = subSocket2.recv_string()
            try:
                data = json.loads(msg)
                for element in data:
                    self.slist.append(element.get('name'))

            except json.JSONDecodeError as e:
                print(f"JSON decode error: {e}")
                continue

            self.supervisorsText.configure(state=tk.NORMAL)
            self.supervisorsText.delete("1.0", tk.END)

            for x in self.slist:
                self.supervisorsText.insert(tk.END, x + "\n")

            self.supervisorsText.configure(state=tk.DISABLED)


if __name__ == '__main__':
    client = StudentClient()