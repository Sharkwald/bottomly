import warnings

from commands.abstract_command import AbstractCommand
from config import ConfigKeys, Config
from googleapiclient.discovery import build


class GoogleSearchCommand(AbstractCommand):

    def get_purpose(self):
        return "Performs a google search and returns the top hit."

    def execute(self, search_term):
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            if search_term is None or search_term == '':
                return None
            service = build("customsearch", "v1", developerKey=self.api_key)
            results = service.cse().list(q=search_term, cx=self.cse_id, num=1).execute()
            if self._result_set_is_empty(results):
                return None
            return results['items'][0]

    def _result_set_is_empty(self, results):
        return results['searchInformation']['totalResults'] == '0'

    def __init__(self):
        super(GoogleSearchCommand, self)
        config = Config()
        self.api_key = config.get_config_value(ConfigKeys.google_api_key)
        self.cse_id = config.get_config_value(ConfigKeys.google_cse_id)
