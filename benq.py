import logging, sys, time
from Projector import Projector

# logging.basicConfig(stream=sys.stderr, level=logging.DEBUG)

with open('config.ini') as config_file:
    num_pf_port = 0
    ports = []
    for line in config_file:
        if 'ports=' in line:
            # deal with different ports
            ports = line\
                .replace('ports=', '')\
                .replace('\n', '')\
                .replace(' ', '')\
                .split(',')

        elif 'num_of_port=' in line:
            num_pf_port = int(line.replace('num_of_port=', ''))
    print(num_pf_port)
    print(ports)

    # for port_name in ports:
    #     proj = Projector(port_name)
    #     print(proj.get_model_name())
        # proj.power_off()
        # if proj.get_power() is 'ON':
        #     proj.power_off()
        # else:
        #     proj.power_on()
        # proj.close()

# p1 = Projector('COM14')
# print(p1.get_model_name())
# p1.power_on()

# p1.open_menu()

# for _ in range(5):
#  p1.down()

# for _ in range(2):
#  p1.enter()

# p1.power_off()

# print(p1.get_power())
# print(p1.get_source())

# p1.close()
