import logging, sys, time
from Projector import Projector, Config

logging.basicConfig(stream=sys.stderr, level=logging.DEBUG)
config = Config()

for port_name in config.ports:
    proj = Projector(port_name)
    # print(proj.get_all_attrs())
    # print(proj.get_3D_status())
    # print(proj.get_model_name())
    # proj.power_on()
    # if proj.get_power() is 'ON':
    #     proj.power_off()
    # else:
    #     proj.power_on()
    proj.close()

# p1 = Projector('COM14')
# print(p1.get_model_name())
# p1.power_on()



# p1.power_off()

# print(p1.get_power())
# print(p1.get_source())

# p1.close()
