import time
import serial
import re

class Projector:
  ''' Projector class '''
  _port = None

  def __init__(self, portName, buad_rate=115200, timeout=0.1, **kwargs):
    self._port = serial.Serial(portName, buad_rate, timeout=timeout, **kwargs)


  def close(self):
    self._port.close()


  def write_command(self, command):
    self._port.flush()
    serial_command = chr(13) + '*' + command + '#' + chr(13)
    self._port.write(serial_command.encode())


  def read_command_result(self):
    result = b''
    _r = self._port.read()
    while not _r is b'':
      result += _r
      _r = self._port.read()
    return result.decode().split('\n')[1]


  def send_command(self, command):
    self.write_command(command)
    self.read_command_result()


  def get_attr(self, attr):
    self.write_command(attr + '=?')
    result = self.read_command_result()
    return re.findall('=(.*)#', result)[0]

  
  def get_power(self):
    return self.get_attr('pow')


  def get_source(self):
    return self.get_attr('sour')

  def get_all_attrs(self):
    return [self.get_power(), self.get_source()]


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


p1 = Projector('/dev/tty.UC-232AC')

# p1.open_menu()

# p1.down()
# p1.down()
# p1.down()
# p1.down()
# p1.down()

# p1.enter()
# p1.enter()

# print(p1.get_power())
# print(p1.get_source())
# print(p1.get_all_attrs())

p1.close()




# with serial.Serial('/dev/tty.UC-232AC', 115200, timeout=0.25) as proj_port:
#   print(proj_port.name)
#   proj_port.write(chr(13) + '*pow=?#' + chr(13))
#   # proj_port.write()
#   # proj_port.write()
#   # time.sleep(1)
#   # '*pow=?#'
#   print(proj_port.read(100))

#   time.sleep(1)
#   proj_port.close()

