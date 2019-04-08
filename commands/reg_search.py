from commands.abstract_command import AbstractCommand

import requests
from bs4 import BeautifulSoup


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
        target_url = base_url + search_term

        r = requests.get(target_url)
        soup = BeautifulSoup(r.content, 'html.parser')

        try:
            vehicle_colour = soup.find(id='VehicleColour')['value'].strip()
            vehicle_year = soup.find(id='RegistrationYear')['value'].strip()
            vehicle_make = soup.find(id='VehicleMake')['value'].strip().capitalize()
            vehicle_model = soup.find(id='VehicleModel')['value'].strip()
            vehicle_image = 'https://www.vehiclecheck.co.uk' + soup.find(id='searchResultCarImage')['src'].strip()

            return vehicle_colour + ' ' + vehicle_make + ' ' + vehicle_model + ' (' + vehicle_year + ')' + ' ' + vehicle_image

        except:
            error_element = soup.find('div', {'class': 'ErrorMessage'})
            error_message = error_element.findChild('h3').text

            return error_message


def __init__(self):
    super(RegSearchCommand, self)
