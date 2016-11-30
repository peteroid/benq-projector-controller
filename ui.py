import sys, logging
import tkinter as tk
from Projector import Projector, Config

logging.basicConfig(stream=sys.stderr, level=logging.DEBUG)


class ProjectorComponent(tk.Frame):
    _projector = None

    def __init__(self, master, projector_port=None):
        super().__init__(master)
        self._projector = Projector(projector_port)

        self.display_panel = tk.Frame(self)
        self.display_panel.grid(row=1, column=1)

        self.action_panel = tk.Frame(self)
        self.action_panel.grid(row=1, column=2)

        self.add_model_label()
        self.add_status_label()

        if not self._projector.is_initialized():
            self.update_status_label(text=('%s not initialized') % (projector_port))
            return

        self.add_power_on_button()
        self.add_power_off_button()

        if config.is_admin():
            self.add_3D_on_button()
            self.add_3D_off_button()

        self.update_model_label()
        self.update_status_label()

    def update_status_label(self, text=None):
        self.status_string.set(("Power: %s | 3D: %s" % (self._projector.get_power(), self._projector.get_3D_status())) if text is None else text)

    def add_status_label(self):
        self.status_string = tk.StringVar()
        self.status = tk.Label(self.display_panel, textvariable=self.status_string)
        self.status.pack(side='left')

    def update_model_label(self):
        self.model_string.set("%s : %s" % (self._projector.projector_port().port, self._projector.get_model_name()))

    def add_model_label(self):
        self.model_string = tk.StringVar()
        self.model = tk.Label(self.display_panel, textvariable=self.model_string)
        self.model.pack(side='left')

    def power_on_handler(self):
        # print('text: %s' % (self.entry_string.get()))
        self._projector.power_on()
        self.update_status_label()

    def power_off_handler(self):
        # print('text: %s' % (self.entry_string.get()))
        self._projector.power_off()
        self.update_status_label()

    def on_3D_enable_handler(self):
        self._projector.enable_3D()
        self.update_status_label()

    def on_3D_disable_handler(self):
        self._projector.disable_3D()
        self.update_status_label()

    def add_3D_on_button(self):
        on_3D = tk.Button(self.action_panel, text="3D ON", fg="black", command=self.on_3D_enable_handler)
        on_3D.pack(side="right")

    def add_3D_off_button(self):
        off_3D = tk.Button(self.action_panel, text="3D OFF", fg="black", command=self.on_3D_disable_handler)
        off_3D.pack(side="right")

    def add_power_on_button(self):
        self.power_on = tk.Button(self.action_panel, text="ON", fg="black", command=self.power_on_handler)
        self.power_on.pack(side="right")

    def add_power_off_button(self):
        self.power_off = tk.Button(self.action_panel, text="OFF", fg="black", command=self.power_off_handler)
        self.power_off.pack(side="right")

    def on_quit_handler(self):
        self._projector.close()


class Application(tk.Frame):
    _projectors = []

    def __init__(self, master=None):
        super().__init__(master)
        self.pack(padx=20, pady=10)

        self.projector_component_panel = tk.Frame(self)
        self.projector_component_panel.grid(row=0)

        self.menu_panel = tk.Frame(self)
        self.menu_panel.grid(row=2, pady=5)
        # self.menu_panel.pack(side="bottom")

        self.operation_panel = tk.Frame(self)
        self.operation_panel.grid(row=1, pady=5)
        # self.operation_panel.pack(side="bottom")

        self.add_quit_button()
        self.add_start_button()
        self.add_all_action_button("Power On All", "power_on_handler")
        self.add_all_action_button("Power Off All", "power_off_handler")
        self.add_all_action_button("Update All", "update_status_label")

        if config.is_admin():
            self.add_list_button()
            self.add_all_action_button("3D On All", "on_3D_enable_handler")
            self.add_all_action_button("3D Off All", "on_3D_disable_handler")


        if not master is None:
            master.protocol('WM_DELETE_WINDOW', self.on_application_quit)

    def add_projector_components(self):
        for port_name in config.ports:
            self.add_projector_component(port_name)

    def add_projector_component(self, port_name=None):
        projector_frame = ProjectorComponent(self.projector_component_panel, projector_port=port_name)
        projector_frame.grid(row=len(self._projectors), padx=10, pady=5)
        self._projectors.append(projector_frame)

    def add_all_action_button(self, text, action_name):
        all_action = tk.Button(self.operation_panel, text=text,
                               command=lambda: [getattr(proj, action_name)() for proj in self._projectors])
        all_action.pack(side="left")

    def on_list_handler(self):
        print(Projector.get_all_available_ports())

    def add_list_button(self):
        list = tk.Button(self.menu_panel, text="List all ports", command=self.on_list_handler)
        list.pack(side="left")

    def add_start_button(self):
        self.start = tk.Button(self.menu_panel, text="Start", fg="black", command=self.add_projector_components)
        self.start.pack(side="left")

    def on_application_quit(self):
        logging.debug("on quit")
        for p in self._projectors:
            p.on_quit_handler()
        root.destroy()

    def add_quit_button(self):
        self.quit = tk.Button(self.menu_panel, text="QUIT", fg="red", command=self.on_application_quit)
        self.quit.pack(side="right")

config = Config()
root = tk.Tk()
root.wm_title('CAVE projector controller')
app = Application(master=root)
app.mainloop()
