import os
from googleapiclient.discovery import build


class GoogleSearchCommand(object):
    api_key_envar="bottomly_google_api_key"
    cse_id_envar="bottomly_google_cse_id"

    def execute(self, search_term):
        if search_term == None or search_term == '':
            return None
        service = build("customsearch", "v1", developerKey=self.api_key)
        results = service.cse().list(q=search_term, cx=self.cse_id, num=1).execute()
        if results['queries']['request'][0]['totalResults'] == '0': # This seems garbage... better way?
            return None
        return results['items'][0]

    def __init__(self):
        super(GoogleSearchCommand, self)

        if GoogleSearchCommand.api_key_envar in os.environ:
            self.api_key = os.environ.get(GoogleSearchCommand.api_key_envar)
        else:
            raise EnvironmentError('Google API key is not configured')

        if GoogleSearchCommand.cse_id_envar in os.environ:
            self.cse_id = os.environ.get(GoogleSearchCommand.cse_id_envar)
        else:
            raise EnvironmentError('Google custom search engine ID is not configured')