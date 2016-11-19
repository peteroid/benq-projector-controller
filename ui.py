import tkinter as tk
from Projector import Projector

class Application(tk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.pack()
        self.add_quit_button()
        self.add_power_on_button()
        self.add_entry()

    # def create_widgets(self):
    #     self.hi_there = tk.Button(self)
    #     self.hi_there["text"] = "Hello World\n(click me)"
    #     self.hi_there["command"] = self.say_hi
    #     self.hi_there.pack(side="top")

    def add_entry(self):
        self.entry_string = tk.StringVar()
        self.entry = tk.Entry(self, textvariable=self.entry_string)
        self.entry.pack(side='left')

    def power_on_handler(self):
        # print('text: %s' % (self.entry_string.get()))
        port_name = self.entry_string.get()
        p = Projector(port_name)
        p.power_on()
        p.close()

    def add_power_on_button(self):
        self.power_on = tk.Button(self, text="ON", fg="black",
                                  command=self.power_on_handler)
        self.power_on.pack(side="right")

    def add_quit_button(self):
        self.quit = tk.Button(self, text="QUIT", fg="red",
                              command=root.destroy)
        self.quit.pack(side="bottom")


root = tk.Tk()
app = Application(master=root)
app.mainloop()
