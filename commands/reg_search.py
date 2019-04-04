from commands.abstract_command import AbstractCommand


class RegSearchCommand(AbstractCommand):

    def get_purpose(self):
        return 'AutoTrader reg lookup, because Jamie is lazy.'

    def execute(self, search_term):
        if search_term is None or search_term == '':
            return 'Registration missing'

        search_term = search_term.replace(' ', '')
        search_term = search_term.lower()

        if len(search_term) > 7:
            return 'Registration too long.'

        if not search_term.isalnum():
            return 'Registration should not contain special characters'

        # Sanity checks done, now replacing common mistakes
        search_term = search_term.replace('i', '1')

        base_url = 'https://www.vehiclecheck.co.uk/?vrm='

        return base_url + search_term

    def __init__(self):
        super(RegSearchCommand, self)
