from commands.abstract_command import AbstractCommand

import logging.config
from os import path
import requests

log_file_path = path.join(path.dirname(path.abspath(__file__)), '../logging.conf')
logging.config.fileConfig(log_file_path)
logger = logging.getLogger('bottomly')

from model.cocktail import Cocktail


class CocktailOfTheWeekSearchCommand(AbstractCommand):
    base_url = 'https://cocktailflow.com'
    lang_query_param = '?lang=en-US'

    cocktail_collection_list = [
        '/ajax/collection/type-champagne',
        '/ajax/collection/type-classical',
        '/ajax/collection/type-creamy',
        '/ajax/collection/type-frozen',
        '/ajax/collection/type-hotdrink',
        '/ajax/collection/type-longdrink',
        '/ajax/collection/type-martini',
        '/ajax/collection/type-short',
        '/ajax/collection/type-tropical'
    ]

    def get_purpose(self):
        return 'Cocktail of the week, a reminder we were once fun.'

    def execute(self):
        return self.get_all_cocktails()

    def get_all_cocktails(self) -> []:
        try:
            cocktail_list = []
            for cocktail_collection in self.cocktail_collection_list:
                cocktail_list += self.get_cocktails_in_collection(cocktail_collection)
            return cocktail_list
        except:
            logger.exception("Error - Cocktail of the week - get_all_cocktails failed")
            return None

    def get_cocktails_in_collection(self, collection_name) -> []:
        try:
            cocktail_classics_url = self.base_url + collection_name + self.lang_query_param
            response = requests.get(cocktail_classics_url)
            # Parse json response
            json_response = response.json()
            # get cocktails into an array from json
            cocktail_json_list = json_response["collection"]["cocktails"]["items"]

            cocktail_list = []
            for cocktail in cocktail_json_list:
                new_cocktail = Cocktail()
                # Get what we can from the collection request
                new_cocktail.name = cocktail["name"]
                new_cocktail.url = self.base_url + "/ajax/cocktail/" + cocktail["key"] + self.lang_query_param
                new_cocktail.source = "Cocktail Flow"
                # Get the cocktails details from a secondary request
                new_cocktail = self.get_cocktail_details(new_cocktail)

                if cocktail is not None:
                    # Append the cocktail to the list
                    cocktail_list.append(new_cocktail)

            return cocktail_list

        except:
            logger.exception("Error - Cocktail of the week - get_cocktails_in_collection for: " + collection_name)
            return None

    def get_cocktail_details(self, cocktail) -> Cocktail:
        try:
            # eg. https://cocktailflow.com/ajax/cocktail/grasshopper?lang=en-US
            response = requests.get(cocktail.url)
            json_response = response.json()

            cocktail.image = json_response["cocktail"]["imageUrl"]
            cocktail.ingredients = self.get_cocktail_ingredients(json_response)
            cocktail.instructions = self.get_cocktail_instructions(json_response)

            return cocktail
        except:
            # Let's log this, but not stop getting the rest of the cocktails
            logger.exception("Error - Cocktail of the week - get_cocktail_details for: " + cocktail.name)
            return None

    def get_cocktail_ingredients(self, json_response) -> []:
        ingredients = []
        try:
            for ingredient in json_response["cocktail"]["ingredients"]["items"]:
                ingredients.append(ingredient["fullDescription"])

            return ingredients
        except:
            return ingredients

    def get_cocktail_instructions(self, json_response) -> []:
        instructions = []
        try:
            preparation_step_list = json_response["cocktail"]["preparationSteps"]["items"]

            for step in preparation_step_list:  # oO)-.
                step_words = ''                 # /__  _\  - Big-O Toad disapproves.
                for word in step["words"]:      # \  \(  |
                    step_words += word["word"]  # '  '--'

                if step_words.count(',') > 0:
                    # Replace the last comma with 'and'
                    # What are you doing step_words?!
                    step_words = ",".join(step_words.split(",")[:-1]) + " and " + step_words.split(",")[-1]

                instructions.append(step_words)

            return instructions
        except:
            return instructions


def __init__(self):
    super(CocktailOfTheWeekSearchCommand, self)
