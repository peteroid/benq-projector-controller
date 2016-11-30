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
            if self._model == '':
                logging.debug("Cannot get model name. Unset the port")
                self._port = None


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
        time.sleep(0.5)
        result = b''
        logging.debug("data available: %d" % self._port.inWaiting())
        _r = self._port.read()
        logging.debug('serial read: %s' % _r)
        while not _r == b'':
            result += _r
            _r = self._port.read()
            logging.debug('serial read: %s' % _r)
        result = result.decode()
        logging.debug("serial result: %s" % result)
        try:
            if '?' in result:
                result = result[result.index('?') + 1:]
            elif '#\r' in result:
                result = result[result.index('#\r') + 2:]
            else:
                result = result[result.index('\n') + 1 if '\n' in result else 0:]
        except IndexError:
            logging.debug("Error -> serial result: %s" % result)

        return result\
            .replace('\r', '')\
            .replace('\n', '')

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
        if 'Block' in result:
            result = 'Unknown'
        elif 'Illegal' in result:
            result = 'Error'
        return result\
            .replace('#', '')\
            .replace('?', '')

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
        if self._model == '':
            self._model = self.get_attr('modelname', wait=8)
            time.sleep(1)
        return self._model

    ''' seems the model doesn't support for this. this will return as a block item '''
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

    def disable_3D(self):
        self.send_command('3d=off')

    def enable_3D(self):
        self.send_command('3d=fs')

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

    def projector_port(self):
        return self._port

class Config:
    num_of_port = 0
    ports = []
    role = ''

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
                    self.num_of_port = int(line.replace('num_of_port=', ''))

                elif line.startswith('user='):
                    user = line.replace('user=', '').replace('\n', '')
                    logging.debug("User: %s, is admin ? %s" % (user, user == '_ims'))
                    self.role = '_admin' if user == '_ims' else 'user'
            logging.debug("num_of_port: %d" % self.num_of_port)
            logging.debug("ports: %s" % self.ports)

    def is_admin(self):
        return self.role == '_admin'