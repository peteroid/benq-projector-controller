import time
import serial, serial.tools.list_ports
import re


class Projector:
    ''' Projector class '''
    _port = None

    def port_must_initialized(func):
        def wrapper(self, *args, **kwargs):
            if not self.is_initialized():
                print('Port not initialized. Called from Projector.%s()' % (func.__name__))
                return
            else:
                return func(self, *args, **kwargs)

        return wrapper

    def __init__(self, portName, baud_rate=115200, timeout=0.1, **kwargs):
        availablePorts = [p[0] for p in serial.tools.list_ports.comports()]
        if not portName in availablePorts:
            print("Port:%s not found. Please check the connection." % (portName))
            print('Available ports: %s' % (availablePorts))
        else:
            self._port = serial.Serial(portName, baud_rate, timeout=timeout, **kwargs)

    @port_must_initialized
    def close(self):
        self._port.close()

    @port_must_initialized
    def write_command(self, command):
        self._port.flush()
        serial_command = chr(13) + '*' + command + '#' + chr(13)
        self._port.write(serial_command.encode())

    @port_must_initialized
    def read_command_result(self):
        result = b''
        _r = self._port.read()
        while not _r is b'':
            result += _r
            _r = self._port.read()
        return result.decode().split('\n')[1]

    @port_must_initialized
    def get_attr(self, attr):
        self.write_command(attr + '=?')
        result = self.read_command_result()
        try:
            result = re.findall('=(.*)#', result)[0]
        except IndexError:
            pass
        return result

    def is_initialized(self):
        return self._port != None

    def send_command(self, command):
        self.write_command(command)
        self.read_command_result()

    def get_power(self):
        return self.get_attr('pow')

    def get_source(self):
        return self.get_attr('sour')

    def get_model_name(self):
        return self.get_attr('modelname')

    def get_all_attrs(self):
        return [
            self.get_model_name(),
            self.get_power(),
            self.get_source()
        ]

    def power_on(self):
        self.send_command('pow=on')

    def power_off(self):
        self.send_command('pow=off')

    def open_menu(self):
        self.send_command('menu=on')

    def left(self):
        self.send_command('left')

    def right(self):
        self.send_command('right')

    def up(self):
        self.send_command('up')

    def down(self):
        self.send_command('down')

    def enter(self):
        self.send_command('down')


p1 = Projector('COM14')
p1.power_on()

# p1.open_menu()

# for _ in range(5):
#  p1.down()

# for _ in range(2):
#  p1.enter()

# p1.power_off()

# print(p1.get_power())
# print(p1.get_source())
# print(p1.get_all_attrs())

p1.close()
