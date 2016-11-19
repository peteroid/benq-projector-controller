import logging, sys
from Projector import Projector

# logging.basicConfig(stream=sys.stderr, level=logging.DEBUG)

p1 = Projector('COM15')
print(p1.get_all_attrs())
# p1.power_on()

# p1.open_menu()

# for _ in range(5):
#  p1.down()

# for _ in range(2):
#  p1.enter()

# p1.power_off()

# print(p1.get_power())
# print(p1.get_source())

p1.close()
