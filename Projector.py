import serial, serial.tools.list_ports
import logging, re, time

class Projector:
    ''' Projector class '''
    _port = None
    _model = ''

    @staticmethod
    def get_all_available_ports():
        return [p[0] for p in serial.tools.list_ports.comports()]

    def port_must_initialized(func):
        def wrapper(self, *args, **kwargs):
            if not self.is_initialized():
                print('Port not initialized. Called from Projector.%s()' % (func.__name__))
                return
            else:
                return func(self, *args, **kwargs)

        return wrapper

    def __init__(self, portName, baud_rate=115200, timeout=0.5, **kwargs):
        available_ports = Projector.get_all_available_ports()
        if not portName in available_ports:
            print("Port: %s not found. Please check the connection." % (portName))
            print('Available ports: %s' % (available_ports))
        else:
            self._port = serial.Serial(portName, baud_rate, timeout=timeout, **kwargs)
            self._model = self.get_model_name()

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
        logging.debug('serial read: %s' % _r)
        while not _r is b'':
            result += _r
            _r = self._port.read()
            logging.debug('serial read: %s' % _r)
        result = result.decode()
        logging.debug("serial result: %s" % result)
        try:
            if '#\r' in result:
                result = result[result.index('#\r') + 2 if '#\r' in result else 0:]
            else:
                result = result[result.index('\n') + 1 if '\n' in result else 0:]
        except IndexError:
            logging.debug("Error -> serial result: %s" % result)

        return result.replace('\r', '').replace('\n', '')

    @port_must_initialized
    def get_attr(self, attr, wait=0):
        logging.debug("get_attr: %s" % attr)
        self.write_command(attr + '=?')
        time.sleep(wait)
        result = self.read_command_result()
        try:
            result = re.findall('=(.*)#', result)[0]
        except IndexError:
            pass
        logging.debug("get_attr: respond: %s" % result)
        return result

    def is_initialized(self):
        return self._port != None

    def send_command(self, command):
        self.write_command(command)
        self.read_command_result()

    def get_power(self):
        return self.get_attr('pow', wait=1)

    def get_source(self):
        return self.get_attr('sour')

    ''' since some models need time to fetch this result, better to wait up to 8 seconds '''
    def get_model_name(self):
        if self._model is '':
            self._model = self.get_attr('modelname', wait=8)
            time.sleep(1)
        return self._model

    def get_3D_status(self):
        return self.get_attr('3d')

    def get_all_attrs(self):
        return [
            self.get_model_name(),
            self.get_power(),
            self.get_source(),
            self.get_3D_status()
        ]

    def power_on(self):
        self.send_command('pow=on')

    def power_off(self):
        time.sleep(0.5)
        self.send_command('pow=off')

    def enable_3d(self):
        if 'MX819ST' in self._model:
            self.open_menu()
            for _ in range(5):
                self.down()
            for _ in range(2):
                self.enter()
        elif 'MW853UST' in self._model:
            pass
        else:
            pass

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

class Config:
    num_pf_port = 0
    ports = []

    def __init__(self, config_file_name='./config.ini'):
        with open(config_file_name) as config_file:
            for line in config_file:
                if line.startswith('ports='):
                    # deal with different ports
                    self.ports = line \
                        .replace('ports=', '') \
                        .replace('\n', '') \
                        .replace(' ', '') \
                        .split(',')

                elif line.startswith('num_of_port='):
                    self.num_pf_port = int(line.replace('num_of_port=', ''))
            logging.debug("num_of_port: %d" % self.num_pf_port)
            logging.debug("ports: %s" % self.ports)