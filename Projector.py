import serial, serial.tools.list_ports
import logging, re, time

class Projector:
    ''' Projector class '''
    _port = None
    _model = ''

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
            print("Port: %s not found. Please check the connection." % (portName))
            print('Available ports: %s' % (availablePorts))
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
            if '?#\r' in result:
                result = result.split('?#\r')[1]
            else:
                result = result.split('\n')[1]
        except IndexError:
            logging.debug("Error -> serial result: %s" % result)
        return result

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
        return self.get_attr('pow')

    def get_source(self):
        return self.get_attr('sour')

    ''' since some models need time to fetch this result, better to wait up to 5 seconds '''
    def get_model_name(self):
        if self._model is '':
            self._model = self.get_attr('modelname', wait=5)
        return self._model

    def get_3D_status(self):
        return self.get_attr('3d')

    def get_all_attrs(self):
        return [
            self.get_model_name(),
            self.get_power(),
            self.get_source()
        ]

    def power_on(self):
        self.send_command('pow=on')

    def power_off(self):
        time.sleep(0.5)
        self.send_command('pow=off')

    # def enable_3d(self):
    #     if self._model is 'MX819ST'

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

