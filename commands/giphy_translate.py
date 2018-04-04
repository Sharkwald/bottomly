import requests

from commands.abstract_command import AbstractCommand
from config import Config, ConfigKeys


def _result_set_is_empty(results):
    return len(results["data"]) == 0


class GiphyTranslateCommand(AbstractCommand):
    def get_purpose(self):
        return "Uses Giphy to find a gif matching the given search term"

    def execute(self, search_term):
        if search_term is None or search_term == '':
            return None

        base_url = "http://api.giphy.com/v1/gifs/translate?limit=1&api_key=" + self.giphy_key + "&s="
        response = requests.get(base_url + search_term)
        results = response.json()

        if _result_set_is_empty(results):
            return None

        gif_url = results["data"]["url"]

        return gif_url

    def __init__(self):
        c = Config()
        self.giphy_key = c.get_config_value(ConfigKeys.giphy_api_key)
        super(GiphyTranslateCommand, self)
