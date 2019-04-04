from commands.abstract_command import AbstractCommand

class RegSearchCommand(AbstractCommand):


    def get_purpose(self):
        return "AutoTrader reg lookup, because Jamie is lazy."

    def execute(self, search_term):
        if search_term is None or search_term == '':
            return None

        base_url = "https://www.vehiclecheck.co.uk/?vrm="

        return base_url + search_term

    def __init__(self):
        super(RegSearchCommand, self)
