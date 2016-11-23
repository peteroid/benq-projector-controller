import sys, logging
import tkinter as tk
from Projector import Projector, Config

logging.basicConfig(stream=sys.stderr, level=logging.DEBUG)


class ProjectorComponent(tk.Frame):
    _projector = None

    def __init__(self, master, projector=None):
        super().__init__(master)
        self._projector = projector
        self.pack()
        self.add_power_on_button()
        self.add_power_off_button()
        self.add_model_label()
        self.add_status_label()

    def update_status_label(self):
        self.status_string.set("Status: %s" % (self._projector.get_power()))

    def add_status_label(self):
        self.status_string = tk.StringVar()
        self.status = tk.Label(self, textvariable=self.status_string)
        self.status.pack(side='left')
        self.update_status_label()

    def update_model_label(self):
        self.model_string.set(self._projector.get_model_name())

    def add_model_label(self):
        self.model_string = tk.StringVar()
        self.model = tk.Label(self, textvariable=self.model_string)
        self.model.pack(side='left')
        self.update_model_label()

    def power_on_handler(self):
        # print('text: %s' % (self.entry_string.get()))
        self._projector.power_on()
        self.update_status_label()

    def power_off_handler(self):
        # print('text: %s' % (self.entry_string.get()))
        self._projector.power_off()
        self.update_status_label()

    def add_power_on_button(self):
        self.power_on = tk.Button(self, text="ON", fg="black", command=self.power_on_handler)
        self.power_on.pack(side="right")

    def add_power_off_button(self):
        self.power_off = tk.Button(self, text="OFF", fg="black", command=self.power_off_handler)
        self.power_off.pack(side="right")

    def on_quit_handler(self):
        self._projector.close()


class Application(tk.Frame):
    _projectors = []

    def __init__(self, master=None):
        super().__init__(master)
        self.pack()
        self.add_quit_button()
        self.add_start_button()
        self.add_list_button()
        self.add_all_action_button("Power On All", "power_on_handler")
        self.add_all_action_button("Power Off All", "power_off_handler")
        if not master is None:
            master.protocol('WM_DELETE_WINDOW', self.on_application_quit)

    def add_projector_components(self):
        for port_name in config.ports:
            self.add_projector_component(port_name)

    def add_projector_component(self, port_name=None):
        projector_frame = ProjectorComponent(self, projector=Projector(port_name))
        projector_frame.pack(side="top")
        self._projectors.append(projector_frame)

    def add_all_action_button(self, text, action_name):
        all_action = tk.Button(self, text=text,
                               command=lambda: [getattr(proj, action_name)() for proj in self._projectors])
        all_action.pack(side="bottom")

    def on_list_handler(self):
        print(Projector.get_all_available_ports())

    def add_list_button(self):
        list = tk.Button(self, text="List all ports", command=self.on_list_handler)
        list.pack(side="bottom")

    def add_start_button(self):
        self.start = tk.Button(self, text="Start", fg="black", command=self.add_projector_components)
        self.start.pack(side="bottom")

    def on_application_quit(self):
        logging.debug("on quit")
        for p in self._projectors:
            p.on_quit_handler()
        root.destroy()

    def add_quit_button(self):
        self.quit = tk.Button(self, text="QUIT", fg="red", command=self.on_application_quit)
        self.quit.pack(side="bottom")

config = Config()
root = tk.Tk()
app = Application(master=root)
app.mainloop()
