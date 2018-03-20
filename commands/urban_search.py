import requests
import random

from commands.abstract_command import AbstractCommand


class UrbanSearchCommand(AbstractCommand):
    def get_purpose(self):
        return "Tells you what something _really_ means."

    def execute(self, search_term):
        if search_term is None or search_term == '':
            return None

        base_url = "http://api.urbandictionary.com/v0/define?term="
        results = requests.get(base_url + search_term).json()

        if self._result_set_is_empty(results):
            return None

        return self._get_random_result(results)

    def _result_set_is_empty(self, results):
        return results['list'] == []

    def _get_random_result(self, results):
        list_total = len(results['list'])
        random_index = random.randint(0, list_total-1)
        return results['list'][random_index]['definition']

    def __init__(self):
        super(UrbanSearchCommand, self)
