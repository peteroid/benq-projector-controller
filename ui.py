import tkinter as tk
from Projector import Projector


class ProjectorComponent(tk.Frame):
    def __init__(self, master):
        super().__init__(master)
        self.pack()
        self.add_power_on_button()
        self.add_power_off_button()
        self.add_entry()
        self.add_label()

        print("projector")

    def add_label(self):
        self.label_string = tk.StringVar()
        self.label_string.set("Status: xxx")
        self.label = tk.Label(self, textvariable=self.label_string)
        self.label.pack(side='left')

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

    def power_off_handler(self):
        # print('text: %s' % (self.entry_string.get()))
        port_name = self.entry_string.get()
        p = Projector(port_name)
        p.power_off()
        p.close()

    def add_power_on_button(self):
        self.power_on = tk.Button(self, text="ON", fg="black", command=self.power_on_handler)
        self.power_on.pack(side="right")

    def add_power_off_button(self):
        self.power_off = tk.Button(self, text="OFF", fg="black", command=self.power_off_handler)
        self.power_off.pack(side="right")


class Application(tk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.pack()

        for _ in range(4):
            self.add_projector_component()

        self.add_quit_button()

    def add_projector_component(self):
        projector_frame = ProjectorComponent(self)
        projector_frame.pack(side="top")

    def on_application_quit(self):
        root.destroy()

    def add_quit_button(self):
        self.quit = tk.Button(self, text="QUIT", fg="red", command=self.on_application_quit)
        self.quit.pack(side="bottom")


root = tk.Tk()
app = Application(master=root)
app.mainloop()
