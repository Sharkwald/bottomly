import requests
import json

from commands.abstract_command import AbstractCommand


class WikipediaSearchCommand(AbstractCommand):
    def get_purpose(self):
        return "Performs a wikipedia search and returns the top hit."

    def execute(self, search_term):
        if search_term is None or search_term == '':
            return None

        base_url = "https://en.wikipedia.org/w/api.php?action=opensearch&format=json&search="
        response = requests.get(base_url + search_term)
        results = response.json()

        if self._result_set_is_empty(results[1]):
            return None

        top_hit = results[1][0]
        link_to_top_hit = results[3][0]

        result = {"link": link_to_top_hit, "text": top_hit}

        return result

    def _result_set_is_empty(self, results):
        return len(results) == 0


    def __init__(self):
        super(WikipediaSearchCommand, self)
