from commands.abstract_command import AbstractCommand

import os
import requests
from bs4 import BeautifulSoup


class CocktailOfTheWeekSearchCommand(AbstractCommand):

    def get_purpose(self):
        return 'Cocktail of the week, a reminder we were once fun.'

    def execute(self):
        base_url = 'https://thebrassmonkeygla.co.uk/'

        response = requests.get(base_url)
        soup = BeautifulSoup(response.content, 'html.parser')

        try:
            # Header Row - Used to identify
            header_row = soup.find(text='Cocktail of the Week').parent.parent.parent

            # Blank Row - Because fuck restaurant websites
            blank_row = header_row.find_next('tr')

            # Name & Price Row
            name_row = blank_row.find_next('tr')
            name_row_data_cells = name_row.findAll('td')
            name = name_row_data_cells[0].text
            price = name_row_data_cells[1].text

            # Ingredients Row
            ingredients_row = name_row.find_next('tr')
            ingredients = ingredients_row.find('td').text

            return 'Cocktail of the Week' + os.linesep + name + ' - (' + price + ')' + os.linesep + ingredients

        except:
            error_element = soup.find('div', {'class': 'ErrorMessage'})
            error_message = error_element.findChild('h3').text

            return error_message


def __init__(self):
    super(CocktailOfTheWeekSearchCommand, self)
